using System.Globalization;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Modules.Brief.Components;

/// <summary>Formatting shared by the brief and Peter's home.</summary>
public static class BriefFormat
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Thread times are the sender's wall clock, stored without a zone, so they
    /// are shown as they are — converting them would move a 16:14 message to 19:14.
    /// </summary>
    public static string When(DateTime? at) => at is not { } d ? "—"
        : d.TimeOfDay == TimeSpan.Zero ? d.ToString("d MMM", Inv) : d.ToString("d MMM, HH:mm", Inv);

    public static string Day(DateTime? at) => at is { } d ? d.ToString("d MMM", Inv) : "—";

    public static string Time(DateTime at) => at.ToString("HH:mm", Inv);

    public static string Icon(TruthFloorKind kind) => kind switch
    {
        TruthFloorKind.Money => "money",
        TruthFloorKind.Date => "calendar",
        TruthFloorKind.Spec => "commitment",
        TruthFloorKind.Blocker => "blocked",
        _ => "decision"
    };

    /// <summary>What an empty section says. Absence is information: "no money moved" is worth knowing.</summary>
    public static string Nothing(TruthFloorKind kind) => kind switch
    {
        TruthFloorKind.Money => "No money moved.",
        TruthFloorKind.Date => "No date moved.",
        TruthFloorKind.Spec => "No spec was agreed or changed.",
        TruthFloorKind.Blocker => "Nothing is blocked.",
        _ => "Nothing is waiting on you."
    };

    public static string EmphasisLabel(ReaderEmphasis e) => e switch
    {
        ReaderEmphasis.Funder => "Weighted for the funder — progress, money and dates first",
        ReaderEmphasis.Representative => "Weighted for the representative — choices, specs and finishes first",
        _ => "As the client side reads it"
    };

    public static string DueBadge(DateTime? due, bool overdue) => due is null ? "No deadline"
        : overdue ? "Overdue" : $"By {Day(due)}";

    public static string DueClass(DateTime? due, bool overdue) => due is null ? "al-badge--neutral"
        : overdue ? "al-badge--danger"
        : (due.Value.Date - DateTime.Now.Date).TotalDays <= 7 ? "al-badge--warning"
        : "al-badge--neutral";
}
