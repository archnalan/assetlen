using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Scheduling;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── The scheduler (works-report.md §4.6) ────────────────────────
// Days are the site's calendar days (DateOnly), never instants: a plan date
// means the same day to every reader in every zone.

/// <summary>The house's plan, computed for one reader.</summary>
public class ScheduleDto
{
    public string? ProjectId { get; set; }

    /// <summary>The day the plan was computed from.</summary>
    public DateOnly Today { get; set; }

    public DateOnly? WorksComplete { get; set; }

    /// <summary>The committed handover — the head of its Date commitment's restatement chain.</summary>
    public DateOnly? Handover { get; set; }
    public string? HandoverCommitmentId { get; set; }

    /// <summary>Every statement of the handover, oldest first. Nothing is dropped.</summary>
    public List<HandoverStatementDto> HandoverHistory { get; set; } = new();

    public int? ReserveDays { get; set; }
    public int DaysOver { get; set; }

    /// <summary>Where works complete stood before it last moved, and when — read first (truth floor).</summary>
    public DateOnly? PreviousWorksComplete { get; set; }
    public DateTime? WorksCompleteMovedAt { get; set; }
    public DateTime? LastSavedAt { get; set; }

    /// <summary>Line ids that set the date, first first.</summary>
    public List<string> CriticalPath { get; set; } = new();

    public List<ScheduleActivityDto> Activities { get; set; } = new();
    public List<ScheduleActionDto> Actions { get; set; } = new();

    public DayOfWeek RestDay { get; set; } = DayOfWeek.Saturday;
    public List<DateOnly> Holidays { get; set; } = new();
    public bool ExtendLateWaits { get; set; }

    /// <summary>The teams on this plan, each in queue order.</summary>
    public List<TeamQueueDto> Teams { get; set; } = new();

    /// <summary>Stamped by the server for this reader.</summary>
    public bool CanEdit { get; set; }
    public bool CanTick { get; set; }

    /// <summary>Whether the house has a schedule at all, or its lines still carry typed dates.</summary>
    public bool IsScheduled { get; set; }
}

public class HandoverStatementDto
{
    public string? CommitmentId { get; set; }
    public string? Title { get; set; }
    public DateOnly? Date { get; set; }
    public DateTime? AgreedAt { get; set; }
    public bool IsCurrent { get; set; }
}

public class ScheduleActivityDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Area { get; set; }
    public string? Trade { get; set; }
    public string? StageName { get; set; }
    public StageGroup? StagePhase { get; set; }
    public DeliverableStatus Status { get; set; }

    // Inputs
    public int WorkDays { get; set; }
    public int MakeDays { get; set; }
    public int CureDays { get; set; }
    public DateOnly? EarliestStart { get; set; }
    public string? TeamKey { get; set; }
    public int QueueOrder { get; set; }
    public DateOnly? ActualStart { get; set; }
    public DateOnly? PinnedFinish { get; set; }
    public DateOnly? DoneOn { get; set; }
    public List<ScheduleWaitDto> Waits { get; set; } = new();

    // Computed
    public DateOnly Start { get; set; }
    public DateOnly Finish { get; set; }
    public DateOnly? MakeStart { get; set; }
    public DateOnly? MakeEnd { get; set; }
    public DateOnly? CureEnd { get; set; }
    public bool Critical { get; set; }
    public int? FloatDays { get; set; }
    public int? SlipDays { get; set; }
    public bool Late { get; set; }
    public DateOnly? LateSince { get; set; }
    public bool Done { get; set; }
    public bool Started { get; set; }

    /// <summary>The line as the checklist reads it — the tick, its photo and its history.</summary>
    public DeliverableDto? Line { get; set; }
}

public class ScheduleWaitDto
{
    public string? Id { get; set; }
    public WaitKind Kind { get; set; }
    public ArrivalKind? Arrival { get; set; }
    public string? Title { get; set; }
    public string? PredecessorId { get; set; }
    public string? PredecessorTitle { get; set; }
    public WaitLink Link { get; set; }
    public int Days { get; set; }
    public bool CalendarDays { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? Until { get; set; }
    public bool AfterMaking { get; set; }
    public bool Cleared { get; set; }
    public DateOnly? ClearedOn { get; set; }

    // Computed
    public DateOnly? Start { get; set; }
    public DateOnly? End { get; set; }
    public bool Overdue { get; set; }
    public DateOnly? DueWas { get; set; }
    public bool Binding { get; set; }
}

public class ScheduleActionDto
{
    public DateOnly By { get; set; }
    public string? What { get; set; }
    public ArrivalKind Kind { get; set; }
    public string? ActivityId { get; set; }
    public string? ActivityTitle { get; set; }
    public bool SetsTheDate { get; set; }
    public bool Overdue { get; set; }
}

public class TeamQueueDto
{
    [Required, MaxLength(80)] public string? TeamKey { get; set; }
    public List<string> OrderedIds { get; set; } = new();
}

// ─── Proposing a change ─────────────────────────────────────────

/// <summary>A proposed change to the plan. Null means unchanged.</summary>
public class ScheduleChangeDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    public List<ActivityChangeDto> Activities { get; set; } = new();
    public List<TeamQueueDto> Queues { get; set; } = new();

    /// <summary>Restates the committed handover. The old statement is kept.</summary>
    public DateOnly? Handover { get; set; }

    /// <summary>The site calendar: its rest day, and its holidays in full.</summary>
    public DayOfWeek? RestDay { get; set; }
    public List<DateOnly>? Holidays { get; set; }
}

public class ActivityChangeDto
{
    [Required, MaxLength(40)] public string? Id { get; set; }

    [Range(0, 400)] public int? WorkDays { get; set; }
    [Range(0, 400)] public int? MakeDays { get; set; }
    [Range(0, 120)] public int? CureDays { get; set; }

    public DateOnly? EarliestStart { get; set; }
    public bool ClearEarliestStart { get; set; }

    /// <summary>A known finish.</summary>
    public DateOnly? PinnedFinish { get; set; }
    public bool ClearPinnedFinish { get; set; }

    /// <summary>"Needs N more days": the finish is pinned N working days from tomorrow.</summary>
    [Range(1, 200)] public int? NeedsMoreDays { get; set; }

    [MaxLength(80)] public string? TeamKey { get; set; }
    public bool ClearTeam { get; set; }

    [MaxLength(80)] public string? Trade { get; set; }

    /// <summary>The line's waits, in full; null leaves them as they are.</summary>
    public List<WaitInputDto>? Waits { get; set; }

    /// <summary>Waits dealt with. A cleared wait keeps its row.</summary>
    public List<string>? ClearWaitIds { get; set; }
}

public class WaitInputDto
{
    /// <summary>An existing wait kept as it is or edited; null for a new one.</summary>
    [MaxLength(40)] public string? Id { get; set; }
    public WaitKind Kind { get; set; }
    public ArrivalKind? Arrival { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    [MaxLength(40)] public string? PredecessorId { get; set; }
    public WaitLink Link { get; set; }
    [Range(0, 400)] public int Days { get; set; }
    public bool CalendarDays { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? Until { get; set; }
    public bool AfterMaking { get; set; }
}

public class SchedulePreviewRequestDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }

    /// <summary>Compute as at this day instead of today. Reading only; a save always plans from today.</summary>
    public DateOnly? AsAt { get; set; }

    [Required] public ScheduleChangeDto? Proposed { get; set; }

    /// <summary>A second sequence to set beside the first.</summary>
    public ScheduleChangeDto? Alternative { get; set; }
}

public class ScheduleSummaryDto
{
    public DateOnly? WorksComplete { get; set; }
    public DateOnly? Handover { get; set; }
    public int? ReserveDays { get; set; }
    public int DaysOver { get; set; }
    public List<string> CriticalPath { get; set; } = new();
    public List<string> CriticalTitles { get; set; } = new();
}

public class ActivityMoveDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public DateOnly StartBefore { get; set; }
    public DateOnly StartAfter { get; set; }
    public DateOnly FinishBefore { get; set; }
    public DateOnly FinishAfter { get; set; }
}

/// <summary>What a change would do. Nothing is saved.</summary>
public class SchedulePreviewDto
{
    public ScheduleSummaryDto Current { get; set; } = new();
    public ScheduleSummaryDto Proposed { get; set; } = new();
    public ScheduleSummaryDto? Alternative { get; set; }

    /// <summary>"Moves works complete from Wed 2 Dec to Fri 4 Dec. Reserve 1 → 0 days."</summary>
    public List<string> Sentences { get; set; } = new();
    public List<string>? AlternativeSentences { get; set; }

    public bool CriticalPathChanged { get; set; }
    public List<ActivityMoveDto> Moved { get; set; } = new();

    /// <summary>The proposed plan in full, so the lanes can redraw before anything is saved.</summary>
    public ScheduleDto? Plan { get; set; }
    public ScheduleDto? AlternativePlan { get; set; }
}
