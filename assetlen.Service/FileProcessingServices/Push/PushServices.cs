using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices.Push;

/// <summary>
/// The server's VAPID identity. Read from <c>Push:VapidPublicKey</c> /
/// <c>Push:VapidPrivateKey</c> when set, otherwise generated once and kept in a
/// key file — a key that changed on every restart would silently unsubscribe
/// every phone.
/// </summary>
public sealed class VapidKeys
{
    public string PublicKey { get; }
    public ECDsa SigningKey { get; }
    public string Subject { get; }

    public VapidKeys(IConfiguration config, string keyFile)
    {
        Subject = config["Push:Subject"] ?? "mailto:push@assetlen.app";

        var pub = config["Push:VapidPublicKey"];
        var priv = config["Push:VapidPrivateKey"];
        if (string.IsNullOrWhiteSpace(pub) || string.IsNullOrWhiteSpace(priv))
        {
            if (File.Exists(keyFile))
            {
                var stored = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(keyFile))!;
                pub = stored["publicKey"];
                priv = stored["privateKey"];
            }
            else
            {
                using var fresh = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                var p = fresh.ExportParameters(true);
                pub = WebPushCrypto.B64Url(WebPushCrypto.PublicPoint(p));
                priv = WebPushCrypto.B64Url(p.D!);
                Directory.CreateDirectory(Path.GetDirectoryName(keyFile)!);
                File.WriteAllText(keyFile, JsonSerializer.Serialize(new Dictionary<string, string> { ["publicKey"] = pub, ["privateKey"] = priv }));
            }
        }

        var point = WebPushCrypto.FromB64Url(pub!);
        var parameters = WebPushCrypto.FromPoint(point);
        parameters.D = WebPushCrypto.FromB64Url(priv!);
        SigningKey = ECDsa.Create(parameters);
        PublicKey = pub!;
    }
}

/// <summary>Deliveries waiting to go out. In memory for speed; the table is the durable record.</summary>
public sealed class PushQueue
{
    private readonly System.Threading.Channels.Channel<string> _channel = System.Threading.Channels.Channel.CreateUnbounded<string>();
    public void Enqueue(string deliveryId) => _channel.Writer.TryWrite(deliveryId);
    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}

/// <summary>
/// Who to wake for an event, decided by each person's standing on this project —
/// never a tenant-level role. Queues one delivery per device and returns at once.
/// </summary>
public interface INotifier
{
    Task<int> NotifyAsync(string projectId, Func<ProjectAccess, bool> who, string? exceptUserId,
        PushKind kind, string title, string body, string url, CancellationToken ct = default);

    Task<int> NotifyUsersAsync(IEnumerable<string> userIds, string? projectId,
        PushKind kind, string title, string body, string url, CancellationToken ct = default);
}

public sealed class Notifier : INotifier
{
    private readonly AssetlenDbContext _context;
    private readonly IProjectAccessService _access;
    private readonly PushQueue _queue;
    private readonly ILogger<Notifier> _logger;

    public Notifier(AssetlenDbContext context, IProjectAccessService access, PushQueue queue, ILogger<Notifier> logger)
    {
        _context = context;
        _access = access;
        _queue = queue;
        _logger = logger;
    }

    public async Task<int> NotifyAsync(string projectId, Func<ProjectAccess, bool> who, string? exceptUserId,
        PushKind kind, string title, string body, string url, CancellationToken ct = default)
    {
        try
        {
            var parentId = await _context.tbl_Projects_RS.IgnoreQueryFilters()
                .Where(p => p.Id == projectId).Select(p => p.ParentProjectId).FirstOrDefaultAsync(ct);
            var candidates = await _context.tbl_ProjectMembers.IgnoreQueryFilters().AsNoTracking()
                .Where(m => (m.ProjectId == projectId || (parentId != null && m.ProjectId == parentId))
                            && m.IsActive && m.UserId != null && m.IsDeleted != true)
                .Select(m => m.UserId!).Distinct().ToListAsync(ct);

            var recipients = new List<string>();
            foreach (var userId in candidates.Where(u => u != exceptUserId))
                if (who(await _access.ResolveUnscopedAsync(projectId, userId, ct))) recipients.Add(userId);

            return await NotifyUsersAsync(recipients, projectId, kind, title, body, url, ct);
        }
        catch (Exception ex)
        {
            // A notification is never worth failing the act that caused it.
            _logger.LogWarning(ex, "Could not queue {Kind} push for {ProjectId}", kind, projectId);
            return 0;
        }
    }

    public async Task<int> NotifyUsersAsync(IEnumerable<string> userIds, string? projectId,
        PushKind kind, string title, string body, string url, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return 0;

        var subs = await _context.tbl_PushSubscriptions.IgnoreQueryFilters()
            .Where(s => s.UserId != null && ids.Contains(s.UserId) && s.IsDeleted != true)
            .ToListAsync(ct);
        if (subs.Count == 0) return 0;

        var now = DateTime.UtcNow;
        var rows = subs.Select(s => new tbl_PushDelivery
        {
            SubscriptionId = s.Id,
            UserId = s.UserId,
            ProjectId = projectId,
            TenantId = s.TenantId,
            Kind = kind,
            Title = Clip(title, 120),
            Body = Clip(body, 300),
            Url = Clip(url, 300),
            Status = PushDeliveryStatus.Pending,
            QueuedAt = now
        }).ToList();
        _context.tbl_PushDeliveries.AddRange(rows);
        await _context.SaveChangesAsync(ct);

        foreach (var r in rows) _queue.Enqueue(r.Id!);
        return rows.Count;
    }

    private static string? Clip(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..(max - 1)] + "…";
}

/// <summary>
/// Sends queued pushes as they arrive — not on a polling job, because a
/// notification that lands fifteen seconds after WhatsApp's is a notification
/// people stop waiting for (assetlen.md §9). Anything left pending by a restart
/// is picked up on start.
/// </summary>
public sealed class PushDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly PushQueue _queue;
    private readonly VapidKeys _keys;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<PushDispatcher> _logger;

    public PushDispatcher(IServiceScopeFactory scopes, PushQueue queue, VapidKeys keys, IHttpClientFactory http, ILogger<PushDispatcher> logger)
    {
        _scopes = scopes;
        _queue = queue;
        _keys = keys;
        _http = http;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AssetlenDbContext>();
            var since = DateTime.UtcNow.AddDays(-1);
            var pending = await db.tbl_PushDeliveries.IgnoreQueryFilters()
                .Where(d => d.Status == PushDeliveryStatus.Pending && d.QueuedAt > since)
                .Select(d => d.Id!).ToListAsync(stoppingToken);
            foreach (var id in pending) _queue.Enqueue(id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not reload pending pushes");
        }

        try
        {
            await foreach (var id in _queue.ReadAllAsync(stoppingToken))
            {
                try { await SendAsync(id, stoppingToken); }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested) { _logger.LogWarning(ex, "Push {DeliveryId} failed", id); }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is stopping; undelivered pushes stay Pending in the table and are reloaded on the next start.
        }
    }

    private async Task SendAsync(string deliveryId, CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AssetlenDbContext>();
        var delivery = await db.tbl_PushDeliveries.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == deliveryId, ct);
        if (delivery is null || delivery.Status != PushDeliveryStatus.Pending) return;
        var sub = await db.tbl_PushSubscriptions.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == delivery.SubscriptionId, ct);
        if (sub is null || sub.IsDeleted == true)
        {
            delivery.Status = PushDeliveryStatus.Gone;
            await db.SaveChangesAsync(ct);
            return;
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            title = delivery.Title,
            body = delivery.Body,
            url = delivery.Url,
            tag = $"{delivery.Kind}:{delivery.ProjectId}",
            kind = delivery.Kind.ToString()
        });

        try
        {
            var body = WebPushCrypto.Encrypt(payload, WebPushCrypto.FromB64Url(sub.P256dh!), WebPushCrypto.FromB64Url(sub.Auth!));
            using var request = new HttpRequestMessage(HttpMethod.Post, sub.Endpoint) { Content = new ByteArrayContent(body) };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            request.Content.Headers.ContentEncoding.Add("aes128gcm");
            request.Headers.TryAddWithoutValidation("TTL", "86400");
            request.Headers.TryAddWithoutValidation("Urgency", "high");
            request.Headers.TryAddWithoutValidation("Authorization", WebPushCrypto.VapidHeader(sub.Endpoint!, _keys.SigningKey, _keys.PublicKey, _keys.Subject));

            using var response = await _http.CreateClient("webpush").SendAsync(request, ct);
            delivery.HttpStatus = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                delivery.Status = PushDeliveryStatus.Sent;
                delivery.SentAt = DateTime.UtcNow;
                sub.LastSuccessAt = delivery.SentAt;
                sub.ConsecutiveFailures = 0;
            }
            else if ((int)response.StatusCode is 404 or 410)
            {
                // The browser threw the subscription away; so do we.
                delivery.Status = PushDeliveryStatus.Gone;
                sub.IsDeleted = true;
            }
            else
            {
                delivery.Status = PushDeliveryStatus.Failed;
                delivery.Error = Clip(await response.Content.ReadAsStringAsync(ct), 500);
                sub.ConsecutiveFailures++;
            }
        }
        catch (Exception ex)
        {
            delivery.Status = PushDeliveryStatus.Failed;
            delivery.Error = Clip(ex.Message, 500);
            sub.ConsecutiveFailures++;
        }

        await db.SaveChangesAsync(ct);
    }

    private static string? Clip(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..max];
}

/// <summary>
/// A stand-in push service for development: it holds the browser's half of the
/// keys, decrypts what arrives and checks the VAPID signature, so the whole path
/// — encryption, signing, delivery, latency — is tested without a phone.
/// </summary>
public static class DevPushSink
{
    public sealed record Sink(ECDiffieHellman Key, byte[] Auth, ConcurrentQueue<Received> Inbox);
    public sealed record Received(DateTime At, string? Payload, bool VapidValid, string? Encoding, string? Error);

    private static readonly ConcurrentDictionary<string, Sink> Sinks = new();

    public static (string Id, string P256dh, string Auth) Create()
    {
        var id = Guid.NewGuid().ToString("N");
        var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var auth = RandomNumberGenerator.GetBytes(16);
        Sinks[id] = new Sink(key, auth, new ConcurrentQueue<Received>());
        return (id, WebPushCrypto.B64Url(WebPushCrypto.PublicPoint(key.ExportParameters(false))), WebPushCrypto.B64Url(auth));
    }

    public static bool Receive(string id, byte[] body, string? encoding, string? authorization, string vapidPublicKey)
    {
        if (!Sinks.TryGetValue(id, out var sink)) return false;
        try
        {
            var payload = Encoding.UTF8.GetString(WebPushCrypto.Decrypt(body, sink.Key, sink.Auth));
            sink.Inbox.Enqueue(new Received(DateTime.UtcNow, payload, VerifyVapid(authorization, vapidPublicKey), encoding, null));
        }
        catch (Exception ex)
        {
            sink.Inbox.Enqueue(new Received(DateTime.UtcNow, null, false, encoding, ex.Message));
        }
        return true;
    }

    public static IReadOnlyList<Received> Inbox(string id) =>
        Sinks.TryGetValue(id, out var sink) ? sink.Inbox.ToList() : new List<Received>();

    private static bool VerifyVapid(string? header, string publicKey)
    {
        if (header is null || !header.StartsWith("vapid ")) return false;
        var parts = header[6..].Split(',', StringSplitOptions.TrimEntries)
            .Select(p => p.Split('=', 2)).Where(p => p.Length == 2).ToDictionary(p => p[0], p => p[1]);
        if (!parts.TryGetValue("t", out var jwt) || parts.GetValueOrDefault("k") != publicKey) return false;
        var pieces = jwt.Split('.');
        if (pieces.Length != 3) return false;
        using var key = ECDsa.Create(WebPushCrypto.FromPoint(WebPushCrypto.FromB64Url(publicKey)));
        return key.VerifyData(Encoding.ASCII.GetBytes($"{pieces[0]}.{pieces[1]}"), WebPushCrypto.FromB64Url(pieces[2]), HashAlgorithmName.SHA256);
    }
}
