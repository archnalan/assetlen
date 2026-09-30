using System.Globalization;

namespace assetlen.Shared.Models.Scheduling;

/// <summary>An activity whose dates a change moves.</summary>
public sealed record ActivityMove(string Key, string? Title, DateOnly StartBefore, DateOnly StartAfter, DateOnly FinishBefore, DateOnly FinishAfter);

/// <summary>
/// What a proposed change does to the plan, in the words the contractor reads
/// before saving: "Moves works complete from Wed 2 Dec to Fri 4 Dec. Reserve 1 → 0 days."
/// </summary>
public sealed record PlanComparison
{
    public DateOnly? WorksCompleteBefore { get; init; }
    public DateOnly? WorksCompleteAfter { get; init; }
    public int? ReserveBefore { get; init; }
    public int? ReserveAfter { get; init; }
    public int DaysOverAfter { get; init; }
    public bool CriticalPathChanged { get; init; }
    public IReadOnlyList<ActivityMove> Moved { get; init; } = Array.Empty<ActivityMove>();
    public IReadOnlyList<string> Sentences { get; init; } = Array.Empty<string>();

    public static string Day(DateOnly d) => d.ToString("ddd d MMM", CultureInfo.InvariantCulture);

    private static string Days(int n) => n == 1 ? "1 day" : $"{n} days";

    public static PlanComparison Of(PlanOutcome before, PlanOutcome after)
    {
        var moved = before.Activities
            .Join(after.Activities, b => b.Key, a => a.Key, (b, a) => (b, a))
            .Where(x => x.b.Start != x.a.Start || x.b.Fin != x.a.Fin)
            .Select(x => new ActivityMove(x.a.Key, x.a.Title, x.b.Start, x.a.Start, x.b.Fin, x.a.Fin))
            .ToList();

        var sentences = new List<string>();
        if (before.WorksComplete is { } wb && after.WorksComplete is { } wa)
            sentences.Add(wb == wa
                ? $"Works complete stays {Day(wa)}."
                : $"Moves works complete from {Day(wb)} to {Day(wa)}.");

        if (before.ReserveDays is { } rb && after.ReserveDays is { } ra)
            sentences.Add(rb == ra ? $"Reserve stays {Days(ra)}." : $"Reserve {rb} → {Days(ra)}.");

        if (after.DaysOver > 0 && after.Handover is { } h)
            sentences.Add($"Handover {Day(h)} no longer holds: the works run {Days(after.DaysOver)} into it.");

        var pathChanged = !before.CriticalPath.SequenceEqual(after.CriticalPath);
        if (pathChanged)
        {
            var names = after.CriticalPath.Select(k => after[k]?.Title ?? k);
            sentences.Add($"What sets the date changes: {string.Join(" → ", names)}.");
        }

        if (moved.Count > 0)
            sentences.Add(moved.Count == 1 ? "1 activity moves." : $"{moved.Count} activities move.");
        else if (before.WorksComplete == after.WorksComplete)
            sentences.Add("No activity moves.");

        return new PlanComparison
        {
            WorksCompleteBefore = before.WorksComplete,
            WorksCompleteAfter = after.WorksComplete,
            ReserveBefore = before.ReserveDays,
            ReserveAfter = after.ReserveDays,
            DaysOverAfter = after.DaysOver,
            CriticalPathChanged = pathChanged,
            Moved = moved,
            Sentences = sentences
        };
    }
}
