using assetlen.Shared.Apicalls;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.statics;
using Microsoft.Extensions.Logging;

namespace assetlen.Shared.Services;

/// <summary>
/// What is waiting on the person reading, and where.
///
/// <para>
/// Two signals, deliberately only two, because a chrome that lights up for
/// everything is a chrome nobody reads:
/// </para>
/// <list type="bullet">
///   <item><b>Owed</b> — a number. Something is assigned to this reader and
///   waiting. It clears when the thing is <i>resolved</i>, never by being
///   looked at, and it is the same set of items the Needs-you page lists.</item>
///   <item><b>Unseen</b> — a dot. Something moved on a project since the reader
///   last opened it. It clears on the visit, because a marker that survives the
///   visit teaches people to ignore markers (Law 4: speak only when waiting
///   costs something).</item>
/// </list>
///
/// <para>
/// A project never shows both. If you owe something on it, the number is the
/// stronger statement and the dot would only add noise.
/// </para>
///
/// <para>
/// This service also owns the fetch. Home and the Needs-you page each ran their
/// own copy of the same per-project loop, so the two screens could disagree
/// about how many decisions Peter owed — and the rail could not show a count at
/// all without adding a third copy. One loader, three readers.
/// </para>
/// </summary>
public sealed class AttentionState
{
    private const string SeenKey = "al-seen";

    private readonly IFlagsApi _flags;
    private readonly IFundingApi _funding;
    private readonly IStorageService _storage;
    private readonly ISD _sd;
    private readonly ILogger<AttentionState> _logger;

    private readonly List<AttentionItem> _owed = new();
    private readonly Dictionary<string, DateTime> _seen = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> _moved = new(StringComparer.OrdinalIgnoreCase);

    private bool _seenLoaded;
    private bool _refreshing;

    public AttentionState(
        IFlagsApi flags,
        IFundingApi funding,
        IStorageService storage,
        ISD sd,
        ILogger<AttentionState> logger)
    {
        _flags = flags;
        _funding = funding;
        _storage = storage;
        _sd = sd;
        _logger = logger;
    }

    public event Action? OnChange;

    /// <summary>False until the first scan finishes, so badges render as nothing rather than as zero.</summary>
    public bool Loaded { get; private set; }

    /// <summary>Everything owed by the signed-in reader, soonest deadline first.</summary>
    public IReadOnlyList<AttentionItem> Owed => _owed;

    public int Count => _owed.Count;

    public int OverdueCount => _owed.Count(i => i.IsOverdue);

    /// <summary>How many items this reader owes on one project. Exact — a parent does not absorb its sub-project's count, because the rail lists the sub-project directly beneath it.</summary>
    public int OwedOn(string? projectId)
        => string.IsNullOrEmpty(projectId) ? 0 : _owed.Count(i => i.ProjectId == projectId);

    /// <summary>
    /// The same count narrowed to one kind, so the strip inside a project can
    /// say *which* section the work is in. A number on the project and then
    /// nothing on any of its nine tabs is a scavenger hunt.
    /// </summary>
    public int OwedOn(string? projectId, AttentionKind kind)
        => string.IsNullOrEmpty(projectId) ? 0 : _owed.Count(i => i.ProjectId == projectId && i.Kind == kind);

    /// <summary>
    /// True when something landed on this project after the reader last opened
    /// it. Not a count: a number here would read as a to-do list, and none of
    /// these are things the reader owes.
    /// </summary>
    public bool HasUnseen(string? projectId)
    {
        if (string.IsNullOrEmpty(projectId)) return false;
        if (OwedOn(projectId) > 0) return false;
        if (!_moved.TryGetValue(projectId, out var moved)) return false;

        return !_seen.TryGetValue(projectId, out var seen) || moved > seen;
    }

    /// <summary>Plain-language reason for the dot, for the title and aria-label. A marker nobody can interpret is decoration.</summary>
    public string? UnseenReason(string? projectId)
        => HasUnseen(projectId) && projectId is not null && _moved.TryGetValue(projectId, out var when)
            ? $"Something moved here {Fmt.Ago(when)} — you have not opened it since"
            : null;

    // ── Reading the world ───────────────────────────────────────────────────

    /// <summary>
    /// Rescan. Safe to call from several screens at once; a second call while
    /// one is in flight is dropped rather than queued, because the answer would
    /// be the same and the loop is one request per project.
    /// </summary>
    public async Task RefreshAsync(IEnumerable<ProjectCardDto> projects)
    {
        if (_refreshing) return;
        _refreshing = true;

        // Snapshot before the first await. Callers pass ShellState's live list,
        // which another screen's dashboard read replaces underneath a scan that
        // is one request per project long.
        var roots = projects.ToList();

        try
        {
            await EnsureSeenLoadedAsync();

            var me = _sd.CurrentUser?.Id;
            var flat = roots
                .Concat(roots.SelectMany(p => p.SubProjects))
                .ToList();

            var found = new List<AttentionItem>();

            foreach (var project in flat)
            {
                // The dashboard already knows when work was last posted, so the
                // movement signal costs no extra request.
                if (project.LastUpdateDate is { } last) Remember(project.Id, last);

                try
                {
                    var response = await _flags.GetFlagsByProject(project.Id, FlagStatus.Open);
                    if (!response.IsSuccessStatusCode || response.Content is null) continue;

                    foreach (var flag in response.Content)
                    {
                        if (flag.AssignedToId == me && me is not null)
                        {
                            found.Add(new AttentionItem(
                                Key: $"flag:{flag.Id}",
                                Kind: AttentionKind.Decision,
                                Title: flag.Title ?? "Open question",
                                ProjectId: project.Id,
                                ProjectName: project.ProjectName,
                                Detail: flag.StageName,
                                Consequence: Trim(flag.Description),
                                Href: $"/project/{project.Id}/register#{flag.Id}",
                                Icon: "decision",
                                Due: flag.DueDate));
                        }
                        else if (flag.DateTimeCreated is { } raised)
                        {
                            // Someone else owes this one. It is still a live
                            // query on the reader's project, which is the
                            // "something out of the ordinary" the dot is for.
                            Remember(project.Id, raised);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read open questions on {ProjectId}", project.Id);
                }
            }

            // A release stalls just as badly at either end — unacknowledged by
            // the delivery side, or reported short and unanswered by the funder —
            // so one queue serves both and the server decides which rows name
            // this reader. The earlier version asked only the delivery side's
            // question and had to be role-gated to avoid a refusal.
            try
            {
                var waiting = await _funding.GetFundingNeedingMe();
                if (waiting.IsSuccessStatusCode && waiting.Content is not null)
                {
                    foreach (var entry in waiting.Content)
                    {
                        var mine = entry.HasGap;

                        found.Add(new AttentionItem(
                            Key: $"funding:{entry.Id}",
                            Kind: AttentionKind.Money,
                            Title: mine
                                ? $"{Fmt.Money(entry.Shortfall)} less arrived than you sent on {entry.StageName ?? "this project"}"
                                : $"Confirm {Fmt.Money(entry.Amount)} on {entry.StageName ?? "this project"}",
                            ProjectId: entry.ProjectId ?? "",
                            ProjectName: entry.ProjectName ?? "—",
                            Detail: mine
                                ? $"They received {Fmt.Money(entry.SettledAmount)}"
                                : $"Recorded {Fmt.Date(entry.PaymentDate)}",
                            Consequence: Trim(mine ? entry.ReceiptNote ?? entry.Notes : entry.Notes),
                            Href: $"/project/{entry.ProjectId}/money",
                            Icon: "money",
                            Due: null));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read the funding waiting on this reader");
            }

            // Soonest first, undated last — an item with a real deadline
            // outranks one someone can sit on.
            found.Sort((a, b) => (a.Due ?? DateTime.MaxValue).CompareTo(b.Due ?? DateTime.MaxValue));

            _owed.Clear();
            _owed.AddRange(found);
            Loaded = true;
            Notify();
        }
        finally
        {
            _refreshing = false;
        }
    }

    private void Remember(string projectId, DateTime when)
    {
        if (!_moved.TryGetValue(projectId, out var existing) || when > existing)
            _moved[projectId] = when;
    }

    // ── Clearing, in real time ──────────────────────────────────────────────

    /// <summary>
    /// Drop one item the moment it is dealt with, without waiting for a rescan.
    /// Resolving a question and then still seeing "3" is the badge telling the
    /// reader it is not to be trusted.
    /// </summary>
    public void Retire(AttentionKind kind, string? id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var key = $"{(kind == AttentionKind.Decision ? "flag" : "funding")}:{id}";
        if (_owed.RemoveAll(i => i.Key == key) > 0) Notify();
    }

    /// <summary>
    /// The reader opened this project, so nothing on it is unseen any more.
    /// Persisted, because the marker must not come back on the next boot.
    /// </summary>
    public async Task MarkSeenAsync(string? projectId)
    {
        if (string.IsNullOrEmpty(projectId)) return;

        await EnsureSeenLoadedAsync();

        var before = HasUnseen(projectId);
        _seen[projectId] = DateTime.UtcNow;

        try { await _storage.SetItemAsync(SeenKey, _seen); }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not persist the seen-marker for {ProjectId}", projectId); }

        if (before) Notify();
    }

    /// <summary>Forget everything on sign-out — one account's markers must not appear inside another's chrome.</summary>
    public void Clear()
    {
        _owed.Clear();
        _moved.Clear();
        _seen.Clear();
        _seenLoaded = false;
        Loaded = false;
        Notify();
    }

    private async Task EnsureSeenLoadedAsync()
    {
        if (_seenLoaded) return;
        _seenLoaded = true;

        try
        {
            var stored = await _storage.GetItemAsync<Dictionary<string, DateTime>>(SeenKey);
            if (stored is null) return;
            foreach (var (id, when) in stored) _seen[id] = when;
        }
        catch (Exception ex)
        {
            // Without the markers every project reads as unseen once. That is
            // the safe direction to fail: over-reporting movement is a nuisance,
            // under-reporting it hides work.
            _logger.LogWarning(ex, "Could not read seen-markers");
        }
    }

    private static string? Trim(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var clean = text.Trim();
        return clean.Length <= 90 ? clean : clean[..87].TrimEnd() + "…";
    }

    private void Notify() => OnChange?.Invoke();
}

public enum AttentionKind { Decision, Money }

public sealed record AttentionItem(
    string Key,
    AttentionKind Kind,
    string Title,
    string ProjectId,
    string ProjectName,
    string? Detail,
    string? Consequence,
    string Href,
    string Icon,
    DateTime? Due)
{
    public bool IsOverdue => Due is not null && Due.Value.Date < DateTime.Now.Date;

    public string DueLabel => Due is null
        ? "No deadline"
        : IsOverdue ? "Overdue" : $"By {Fmt.DateShort(Due)}";

    public string? DueLabelShort => Due is null ? null : DueLabel;

    public string BadgeClass => Due is null
        ? "al-badge--neutral"
        : IsOverdue ? "al-badge--danger"
        : (Due.Value - DateTime.Now).TotalDays <= 7 ? "al-badge--warning"
        : "al-badge--neutral";
}
