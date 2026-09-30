namespace assetlen.Shared.Models.Scheduling;

/// <summary>One wait as it falls on the calendar.</summary>
public sealed record WaitSpan
{
    public int Index { get; init; }
    public WaitKind Kind { get; init; }
    public ArrivalKind? Arrival { get; init; }
    public string? Title { get; init; }
    public string? ActivityKey { get; init; }
    public WaitLink Link { get; init; }
    public bool AfterMaking { get; init; }

    /// <summary>For an arrival or drying: when it starts counting and the day it clears.</summary>
    public DateOnly? Start { get; init; }
    public DateOnly? End { get; init; }

    public bool Cleared { get; init; }

    /// <summary>Its day has passed and it has not been cleared.</summary>
    public bool Overdue { get; init; }

    /// <summary>The day it was due before a late extension, when it was extended.</summary>
    public DateOnly? DueWas { get; init; }

    /// <summary>This wait, and not another, decided when the work could start.</summary>
    public bool Binding { get; init; }
}

public sealed record ActivityPlan
{
    public required string Key { get; init; }
    public string? Title { get; init; }
    public string? Area { get; init; }
    public string? Trade { get; init; }

    public DateOnly Start { get; init; }
    public DateOnly Finish { get; init; }
    public DateOnly? MakeStart { get; init; }
    public DateOnly? MakeEnd { get; init; }
    public DateOnly? CureEnd { get; init; }

    /// <summary>When whatever follows it may begin: the finish, or the end of curing.</summary>
    public DateOnly Fin => CureEnd ?? Finish;

    public IReadOnlyList<WaitSpan> Waits { get; init; } = Array.Empty<WaitSpan>();

    public bool Done { get; init; }
    public bool Started { get; init; }

    /// <summary>Not done, and its planned finish has passed. Its finish is held at today so what follows it moves.</summary>
    public bool Late { get; init; }
    public DateOnly? LateSince { get; init; }

    /// <summary>On the chain that sets works complete.</summary>
    public bool Critical { get; init; }

    /// <summary>Working days it can run over before works complete moves. Null when done.</summary>
    public int? FloatDays { get; init; }

    /// <summary>Working days it can run over before the committed handover moves. Null without a handover.</summary>
    public int? SlipDays { get; init; }
}

/// <summary>Something that must happen, and by when, for the dates to hold.</summary>
public sealed record PlanAction
{
    public DateOnly By { get; init; }
    public string? What { get; init; }
    public ArrivalKind Kind { get; init; }
    public required string ActivityKey { get; init; }
    public string? ActivityTitle { get; init; }
    public bool SetsTheDate { get; init; }
    public bool Overdue { get; init; }
}

public sealed record PlanOutcome
{
    public DateOnly Today { get; init; }
    public DateOnly? WorksComplete { get; init; }
    public DateOnly? Handover { get; init; }

    /// <summary>Working days strictly between works complete and handover; never below zero.</summary>
    public int? ReserveDays { get; init; }

    /// <summary>Working days from the handover to works complete, both counted, once works run to or past it.</summary>
    public int DaysOver { get; init; }

    /// <summary>The chain that sets the date, first activity first.</summary>
    public IReadOnlyList<string> CriticalPath { get; init; } = Array.Empty<string>();

    public IReadOnlyList<ActivityPlan> Activities { get; init; } = Array.Empty<ActivityPlan>();
    public IReadOnlyList<PlanAction> Actions { get; init; } = Array.Empty<PlanAction>();

    public ActivityPlan? this[string key] => Activities.FirstOrDefault(a => a.Key == key);
}

/// <summary>An order that loops back on itself, or a wait on an activity that is not on the plan.</summary>
public sealed class PlanShapeException(string message) : Exception(message);
