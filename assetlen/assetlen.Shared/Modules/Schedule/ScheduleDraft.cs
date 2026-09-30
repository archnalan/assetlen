using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Modules.Schedule;

/// <summary>
/// The contractor's edits since the last save. Nothing here reaches the plan
/// until Save: every edit is previewed first (works-report.md §4.6).
/// </summary>
public sealed class ScheduleDraft
{
    private readonly Dictionary<string, ActivityChangeDto> _activities = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TeamQueueDto> _queues = new(StringComparer.OrdinalIgnoreCase);

    public DateOnly? Handover { get; private set; }

    public bool IsEmpty => _activities.Count == 0 && _queues.Count == 0 && Handover is null;

    public int Count => _activities.Count + _queues.Count + (Handover is null ? 0 : 1);

    private ActivityChangeDto For(string id)
    {
        if (!_activities.TryGetValue(id, out var a)) _activities[id] = a = new ActivityChangeDto { Id = id };
        return a;
    }

    public void SetDays(string id, int work, int make, int cure)
    {
        var a = For(id);
        a.WorkDays = work;
        a.MakeDays = make;
        a.CureDays = cure;
    }

    public void SetTrade(string id, string trade) => For(id).Trade = trade;

    public void SetFinish(string id, DateOnly? pinned)
    {
        var a = For(id);
        a.NeedsMoreDays = null;
        a.PinnedFinish = pinned;
        a.ClearPinnedFinish = pinned is null;
    }

    public void SetEarliest(string id, DateOnly? earliest)
    {
        var a = For(id);
        a.EarliestStart = earliest;
        a.ClearEarliestStart = earliest is null;
    }

    public void NeedsMore(string id, int days)
    {
        var a = For(id);
        a.PinnedFinish = null;
        a.ClearPinnedFinish = false;
        a.NeedsMoreDays = days;
    }

    public void SetWaits(string id, List<WaitInputDto> waits) => For(id).Waits = waits;

    public void ClearWait(string id, string waitId)
    {
        var a = For(id);
        a.ClearWaitIds ??= new();
        if (!a.ClearWaitIds.Contains(waitId)) a.ClearWaitIds.Add(waitId);
    }

    public void SetTeam(string id, string? team)
    {
        var a = For(id);
        a.TeamKey = team;
        a.ClearTeam = string.IsNullOrWhiteSpace(team);
    }

    public void SetQueue(string team, List<string> orderedIds) =>
        _queues[team] = new TeamQueueDto { TeamKey = team, OrderedIds = orderedIds.ToList() };

    public void SetHandover(DateOnly? day) => Handover = day;

    public void Clear()
    {
        _activities.Clear();
        _queues.Clear();
        Handover = null;
    }

    public ScheduleChangeDto ToDto(string projectId) => new()
    {
        ProjectId = projectId,
        Activities = _activities.Values.ToList(),
        Queues = _queues.Values.ToList(),
        Handover = Handover
    };

    /// <summary>A plan's waits, as the inputs an edit starts from.</summary>
    public static List<WaitInputDto> Inputs(ScheduleActivityDto a) => a.Waits.Select(w => new WaitInputDto
    {
        Id = w.Id,
        Kind = w.Kind,
        Arrival = w.Arrival,
        Title = w.Title,
        PredecessorId = w.PredecessorId,
        Link = w.Link,
        Days = w.Days,
        CalendarDays = w.CalendarDays,
        From = w.From,
        Until = w.Until,
        AfterMaking = w.AfterMaking
    }).ToList();
}
