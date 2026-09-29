using assetlen.Shared.Apicalls;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.statics;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Refit;
using System.Globalization;

namespace assetlen.Shared.Services;

/// <summary>A capture as the phone holds it until it is sent.</summary>
public sealed class CaptureDraft
{
    public string ProjectId { get; set; } = "";
    public string? DeliverableId { get; set; }
    public string? StageId { get; set; }

    /// <summary>What the outbox shows while it waits — "Rear wall plaster".</summary>
    public string? Label { get; set; }
    public string? Description { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public bool HasIssues { get; set; }
    public Channel Channel { get; set; } = Channel.Crew;
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public List<CaptureDraftFile> Files { get; } = new();
    public CaptureDraftFile? Voice { get; set; }
}

public sealed record CaptureDraftFile(string Name, string ContentType, byte[] Data, string? Caption = null);

/// <summary>One capture waiting in the outbox.</summary>
public sealed class OutboxItem
{
    public string Id { get; set; } = "";
    public string? ProjectId { get; set; }
    public string? DeliverableId { get; set; }
    public string? StageId { get; set; }
    public string? Label { get; set; }
    public string? Description { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public bool HasIssues { get; set; }
    public string? Channel { get; set; }
    public string? CapturedAt { get; set; }
    public int FrameCount { get; set; }
    public bool HasVoice { get; set; }
    public bool Ready { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public List<string> Captions { get; set; } = new();
    public List<string> Names { get; set; } = new();
    public List<string> Types { get; set; } = new();
    public string? VoiceName { get; set; }
    public string? VoiceType { get; set; }
}

/// <summary>
/// Capture is written to the phone first and sent when there is signal
/// (assetlen.md §9: works on a bad connection; queues and syncs). Posting never
/// waits on the network. The server keeps one entry per outbox id, so a retry after
/// a reply that never arrived does not post twice.
/// </summary>
public sealed class CaptureOutbox : IAsyncDisposable
{
    private const string ModulePath = "./_content/assetlen.Shared/outbox.js";

    private readonly IJSRuntime _js;
    private readonly IProgressApi _progress;
    private readonly ILogger<CaptureOutbox> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IJSObjectReference? _module;
    private DotNetObjectReference<CaptureOutbox>? _self;
    private Timer? _timer;
    private bool? _available;

    public CaptureOutbox(IJSRuntime js, IProgressApi progress, ILogger<CaptureOutbox> logger)
    {
        _js = js;
        _progress = progress;
        _logger = logger;
    }

    public IReadOnlyList<OutboxItem> Pending { get; private set; } = Array.Empty<OutboxItem>();

    /// <summary>Raised when the queue changes, and when a capture lands (with its project).</summary>
    public event Action? Changed;
    public event Action<string>? Posted;

    private async ValueTask<IJSObjectReference?> ModuleAsync()
    {
        if (_available == false) return null;
        try
        {
            _module ??= await _js.InvokeAsync<IJSObjectReference>("import", ModulePath);
            _available ??= await _module.InvokeAsync<bool>("available");
            return _available == true ? _module : null;
        }
        catch (Exception ex)
        {
            // A private window or a host with no IndexedDB: capture still posts, just not offline.
            _logger.LogWarning(ex, "The capture outbox is unavailable");
            _available = false;
            return null;
        }
    }

    /// <summary>Start listening for the signal coming back. Safe to call more than once.</summary>
    public async Task StartAsync()
    {
        if (_self is not null) return;
        var module = await ModuleAsync();
        if (module is null) return;
        _self = DotNetObjectReference.Create(this);
        await module.InvokeVoidAsync("watch", _self);
        _timer = new Timer(_ => _ = SyncAsync(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
        await RefreshAsync();
    }

    [JSInvokable]
    public Task Kick() => SyncAsync();

    public async Task RefreshAsync()
    {
        var module = await ModuleAsync();
        Pending = module is null ? Array.Empty<OutboxItem>() : (await module.InvokeAsync<List<OutboxItem>>("list")).Where(i => i.Ready).ToList();
        Changed?.Invoke();
    }

    /// <summary>
    /// Keep a capture and try to send it. Returns true when it has already been
    /// posted, false when it is waiting for signal.
    /// </summary>
    public async Task<bool> EnqueueAsync(CaptureDraft draft)
    {
        var id = Guid.NewGuid().ToString("N");
        var module = await ModuleAsync();
        if (module is null)
            return await PostDirectAsync(id, draft);

        await module.InvokeVoidAsync("begin", new
        {
            id,
            projectId = draft.ProjectId,
            deliverableId = draft.DeliverableId,
            stageId = draft.StageId,
            label = draft.Label,
            description = draft.Description,
            completionPercentage = draft.CompletionPercentage,
            hasIssues = draft.HasIssues,
            channel = draft.Channel.ToString(),
            capturedAt = draft.CapturedAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
        });
        foreach (var f in draft.Files)
            await module.InvokeVoidAsync("addFile", id, f.Name, f.ContentType, f.Caption ?? "", f.Data);
        if (draft.Voice is { } v)
            await module.InvokeVoidAsync("setVoice", id, v.Name, v.ContentType, v.Data);
        await module.InvokeVoidAsync("commit", id);

        await RefreshAsync();
        await SyncAsync();
        return Pending.All(p => p.Id != id);
    }

    /// <summary>Send whatever is waiting, oldest first. Stops at the first network failure.</summary>
    public async Task SyncAsync()
    {
        if (!await _gate.WaitAsync(0)) return;
        try
        {
            var module = await ModuleAsync();
            if (module is null) return;
            if (!await module.InvokeAsync<bool>("isOnline")) return;

            var items = (await module.InvokeAsync<List<OutboxItem>>("list"))
                .Where(i => i.Ready).OrderBy(i => i.CapturedAt).ToList();
            foreach (var item in items)
            {
                var files = new List<ByteArrayPart>();
                for (var i = 0; i < item.FrameCount; i++)
                {
                    var bytes = await module.InvokeAsync<byte[]?>("fileBytes", item.Id, i);
                    if (bytes is null) continue;
                    files.Add(new ByteArrayPart(bytes, item.Names.ElementAtOrDefault(i) ?? $"frame-{i + 1}.jpg",
                        item.Types.ElementAtOrDefault(i) ?? "image/jpeg"));
                }
                ByteArrayPart? voice = null;
                if (item.HasVoice && await module.InvokeAsync<byte[]?>("voiceBytes", item.Id) is { } vb)
                    voice = new ByteArrayPart(vb, item.VoiceName ?? "voice.webm", item.VoiceType ?? "audio/webm");

                var outcome = await SendAsync(item, files, voice);
                if (outcome == Outcome.Sent)
                {
                    await module.InvokeVoidAsync("remove", item.Id);
                    Posted?.Invoke(item.ProjectId ?? "");
                }
                else if (outcome == Outcome.Offline)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Outbox sync stopped");
        }
        finally
        {
            _gate.Release();
            await RefreshAsync();
        }
    }

    /// <summary>Throw away a capture the server refused, once the person has seen why.</summary>
    public async Task DiscardAsync(string id)
    {
        var module = await ModuleAsync();
        if (module is not null) await module.InvokeVoidAsync("remove", id);
        await RefreshAsync();
    }

    private enum Outcome { Sent, Offline, Refused }

    private async Task<Outcome> SendAsync(OutboxItem item, List<ByteArrayPart> files, ByteArrayPart? voice)
    {
        try
        {
            var response = await _progress.Capture(item.ProjectId!, files, item.Captions.Take(files.Count).ToList(),
                item.DeliverableId, item.StageId, item.Description,
                item.CompletionPercentage?.ToString(CultureInfo.InvariantCulture), item.HasIssues,
                item.Channel ?? "Crew", item.Id, item.CapturedAt, voice);

            if (response.IsSuccessStatusCode) return Outcome.Sent;

            var status = (int)response.StatusCode;
            var module = await ModuleAsync();
            var message = response.Error?.Content ?? response.ReasonPhrase ?? $"HTTP {status}";
            if (module is not null) await module.InvokeVoidAsync("markFailed", item.Id, message);
            // A timeout, a throttle or a server fault is worth another try; a refusal is not.
            return status is 408 or 429 or >= 500 ? Outcome.Offline : Outcome.Refused;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or ApiException)
        {
            return Outcome.Offline;
        }
    }

    private async Task<bool> PostDirectAsync(string id, CaptureDraft draft)
    {
        var response = await _progress.Capture(draft.ProjectId,
            draft.Files.Select(f => new ByteArrayPart(f.Data, f.Name, f.ContentType)).ToList(),
            draft.Files.Select(f => f.Caption ?? "").ToList(),
            draft.DeliverableId, draft.StageId, draft.Description,
            draft.CompletionPercentage?.ToString(CultureInfo.InvariantCulture), draft.HasIssues,
            draft.Channel.ToString(), id, draft.CapturedAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
            draft.Voice is { } v ? new ByteArrayPart(v.Data, v.Name, v.ContentType) : null);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(response.Error?.Content ?? "The capture was refused.");
        Posted?.Invoke(draft.ProjectId);
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        _self?.Dispose();
        if (_module is not null)
        {
            try { await _module.DisposeAsync(); } catch (JSDisconnectedException) { }
        }
    }
}
