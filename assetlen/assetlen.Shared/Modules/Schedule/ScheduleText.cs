using System.Globalization;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.Scheduling;

namespace assetlen.Shared.Modules.Schedule;

/// <summary>
/// How the plan reads (works-report.md §4.6): each activity one sentence, in the
/// project's own voice — trades and kinds of thing, never people.
/// </summary>
public static class ScheduleText
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Day(DateOnly d) => d.ToString("ddd d MMM", Inv);
    public static string Day(DateOnly? d) => d is { } v ? Day(v) : "—";
    public static string DayShort(DateOnly d) => d.ToString("d MMM", Inv);
    public static string DayLong(DateOnly d) => d.ToString("dddd d MMMM", Inv);

    public static string Days(int n) => n == 1 ? "1 day" : $"{n} days";
    public static string WorkingDays(int n) => n == 1 ? "1 working day" : $"{n} working days";

    public static string Kind(ArrivalKind k) => k switch
    {
        ArrivalKind.SignOff => "Sign-off",
        _ => k.ToString()
    };

    /// <summary>One wait, as the sentence says it: "after boards on site 7 Oct".</summary>
    public static string Wait(ScheduleWaitDto w) => w.Kind switch
    {
        WaitKind.Activity when w.Link == WaitLink.StartToStart => $"with {w.PredecessorTitle ?? "another activity"}",
        WaitKind.Activity when w.AfterMaking => $"fixed after {w.PredecessorTitle ?? "another activity"}",
        WaitKind.Activity => $"after {w.PredecessorTitle ?? "another activity"}",
        WaitKind.Drying => $"{(string.IsNullOrWhiteSpace(w.Title) ? "drying" : w.Title!.ToLowerInvariant())} {Days(w.Days)}",
        _ => $"{(string.IsNullOrWhiteSpace(w.Title) ? Kind(w.Arrival ?? ArrivalKind.Delivery).ToLowerInvariant() : Lower(w.Title!))} {(w.End is { } e ? DayShort(e) : "")}".Trim()
    };

    private static string Lower(string s) => s.Length > 1 && char.IsUpper(s[0]) && !char.IsUpper(s[1]) ? char.ToLowerInvariant(s[0]) + s[1..] : s;

    /// <summary>"14 days", "made 10 · 3 on site", "cures 3".</summary>
    public static string Duration(ScheduleActivityDto a)
    {
        var parts = new List<string>();
        if (a.MakeDays > 0) parts.Add($"made {a.MakeDays}");
        if (a.WorkDays > 0) parts.Add(a.MakeDays > 0 ? $"{a.WorkDays} on site" : Days(a.WorkDays));
        if (a.CureDays > 0) parts.Add($"cures {a.CureDays}");
        return parts.Count == 0 ? "no days" : string.Join(" · ", parts);
    }

    /// <summary>What the line can absorb, or that it sets the date.</summary>
    public static string Slack(ScheduleActivityDto a, bool hasHandover)
    {
        if (a.Done) return "Done";
        if (a.Critical) return "sets the date";
        var n = hasHandover ? a.SlipDays : a.FloatDays;
        return n switch
        {
            null => "",
            >= 250 => "can slip weeks",
            _ => $"can slip {Days(n.Value)}"
        };
    }

    /// <summary>The area's accent: the same four the issued plan used.</summary>
    public static string AreaAccent(string? area) => (area ?? "").Trim().ToLowerInvariant() switch
    {
        "main house" => "var(--al-stage-7)",
        "guest wing" => "var(--al-stage-5)",
        "external works" => "var(--al-stage-8)",
        "handover" or "close-out" => "var(--al-stage-9)",
        _ => "var(--al-stage-0)"
    };

    /// <summary>Areas in the order the work reaches them; handover, and anything unplaced, last.</summary>
    public static int AreaRank(string? area) => (area ?? "").Trim().ToLowerInvariant() switch
    {
        "main house" => 0,
        "guest wing" => 1,
        "external works" => 2,
        "handover" or "close-out" => 8,
        "" => 9,
        _ => 3
    };

    public static string AreaName(string? area) => string.IsNullOrWhiteSpace(area) ? "Other lines" : area!;

    /// <summary>A line's title, with its area when another line shares the title — two sets of doors, two areas.</summary>
    public static string TitleIn(ScheduleDto plan, string id)
    {
        var a = plan.Activities.FirstOrDefault(x => x.Id == id);
        if (a is null) return "An activity";
        var twin = plan.Activities.Any(x => x.Id != id && string.Equals(x.Title, a.Title, StringComparison.OrdinalIgnoreCase));
        return twin ? $"{a.Title} · {AreaName(a.Area)}" : a.Title ?? "An activity";
    }
}
