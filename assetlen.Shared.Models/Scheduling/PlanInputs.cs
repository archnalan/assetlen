namespace assetlen.Shared.Models.Scheduling;

/// <summary>
/// What an activity waits on — the three plain choices the contractor picks
/// from (works-report.md §4.6).
/// </summary>
public enum WaitKind
{
    /// <summary>Another activity finishing (or starting, for work done alongside it).</summary>
    Activity = 0,

    /// <summary>Something arriving by a date: an order, a delivery, a sign-off, a purchase, a booking, money.</summary>
    Arrival = 1,

    /// <summary>Drying or curing for a number of calendar days.</summary>
    Drying = 2
}

/// <summary>
/// What is arriving. These become the "what must happen, and by when" list,
/// labelled by kind rather than by party (works-report.md §4.5).
/// </summary>
public enum ArrivalKind
{
    Order = 0,
    Delivery = 1,
    SignOff = 2,
    Purchase = 3,
    Booking = 4,
    Funding = 5
}

public enum WaitLink
{
    /// <summary>Starts once the other activity has finished.</summary>
    FinishToStart = 0,

    /// <summary>Goes in alongside the other activity: starts when it starts.</summary>
    StartToStart = 1
}

/// <summary>One thing an activity waits on.</summary>
public sealed record PlanWait
{
    public WaitKind Kind { get; init; }
    public string? Title { get; init; }

    /// <summary>For <see cref="WaitKind.Activity"/>: the activity waited on.</summary>
    public string? ActivityKey { get; init; }
    public WaitLink Link { get; init; } = WaitLink.FinishToStart;

    public ArrivalKind? Arrival { get; init; }

    /// <summary>How long the wait runs: working days for an arrival's lead time, calendar days for drying.</summary>
    public int Days { get; init; }

    /// <summary>Counts every day rather than working days. Always true for drying.</summary>
    public bool CalendarDays { get; init; }

    /// <summary>When the wait starts counting — screed laid, order placed. Defaults to the activity's own earliest start.</summary>
    public DateOnly? From { get; init; }

    /// <summary>A fixed day it arrives by, in place of a duration.</summary>
    public DateOnly? Until { get; init; }

    /// <summary>
    /// Holds only the work on site, not the making: the fabricator can make the
    /// railing now, but fixes it once the doors are in.
    /// </summary>
    public bool AfterMaking { get; init; }

    /// <summary>Dealt with. A cleared wait ends on the day it cleared.</summary>
    public bool Cleared { get; init; }
    public DateOnly? ClearedOn { get; init; }
}

/// <summary>One activity on the plan, as the engine reads it.</summary>
public sealed record PlanActivity
{
    public required string Key { get; init; }
    public string? Title { get; init; }
    public string? Area { get; init; }
    public string? Trade { get; init; }

    /// <summary>Working days on site. Zero for an activity that is only made off site.</summary>
    public int WorkDays { get; init; }

    /// <summary>Working days made off site before the site work can start.</summary>
    public int MakeDays { get; init; }

    /// <summary>Calendar days the finished work cures before anything follows it.</summary>
    public int CureDays { get; init; }

    /// <summary>Not before this day.</summary>
    public DateOnly? EarliestStart { get; init; }

    /// <summary>Activities sharing a team run one after another, in <see cref="QueueOrder"/>.</summary>
    public string? TeamKey { get; init; }
    public int QueueOrder { get; init; }

    /// <summary>Started on site on this day: what it waited on no longer holds it.</summary>
    public DateOnly? ActualStart { get; init; }

    /// <summary>A known finish — "the doors run to 21 Oct", or "needs two more days".</summary>
    public DateOnly? PinnedFinish { get; init; }

    /// <summary>Ticked off on this day.</summary>
    public DateOnly? DoneOn { get; init; }

    public IReadOnlyList<PlanWait> Waits { get; init; } = Array.Empty<PlanWait>();
}

/// <summary>
/// What a wait that outlives its date is extended by, per cause (works-report.md §4.4
/// calibration). A late wait is never frozen at today: the one already late is the
/// likeliest to stay late.
/// </summary>
public sealed record LateWaitRule
{
    public bool Enabled { get; init; }

    /// <summary>Working days added from today to an arrival that has not come.</summary>
    public IReadOnlyDictionary<ArrivalKind, int> ArrivalLag { get; init; } = Defaults;

    /// <summary>Calendar days added to drying that has not been called dry.</summary>
    public int DryingLag { get; init; } = 5;

    /// <summary>
    /// From the 29 Sep calibration: a third-party booking not yet confirmed runs ~3 weeks,
    /// a decision ~4 weeks, money 2–5 days. Orders, deliveries and purchases were not
    /// measured there; a working week stands in until the project's own history replaces it.
    /// </summary>
    public static readonly IReadOnlyDictionary<ArrivalKind, int> Defaults = new Dictionary<ArrivalKind, int>
    {
        [ArrivalKind.Order] = 5,
        [ArrivalKind.Delivery] = 5,
        [ArrivalKind.Purchase] = 5,
        [ArrivalKind.SignOff] = 20,
        [ArrivalKind.Booking] = 15,
        [ArrivalKind.Funding] = 3
    };

    public static readonly LateWaitRule Off = new() { Enabled = false };
    public static readonly LateWaitRule Calibrated = new() { Enabled = true };
}

public sealed record PlanInput
{
    /// <summary>Nothing not yet started can start before this day.</summary>
    public required DateOnly Today { get; init; }
    public required SiteCalendar Calendar { get; init; }
    public required IReadOnlyList<PlanActivity> Activities { get; init; }

    /// <summary>The committed handover day — the head of its Date commitment chain.</summary>
    public DateOnly? Handover { get; init; }

    public LateWaitRule Lateness { get; init; } = LateWaitRule.Off;

    /// <summary>Work out how far each activity can slip. Off for a quick preview of many alternatives.</summary>
    public bool WithSlack { get; init; } = true;
}
