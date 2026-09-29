using System.Globalization;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Modules.Report.Components;

/// <summary>
/// How the works report writes dates, money and states. Dates are the project's
/// wall-clock days as the server computed them — never shifted to the browser's zone.
/// </summary>
public static class ReportFormat
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Day(DateTime? d) => d is { } v ? v.ToString("d MMM", Inv) : "—";
    public static string DayYear(DateTime? d) => d is { } v ? v.ToString("d MMM yyyy", Inv) : "—";
    public static string Pct(decimal? p) => p is { } v ? v.ToString("0.#", Inv) + "%" : "—";

    public static string Money(decimal amount, string? currency) => $"{currency ?? "UGX"} {Short(amount)}";

    public static string Short(decimal amount) => amount switch
    {
        >= 1_000_000_000 => (amount / 1_000_000_000m).ToString("0.##", Inv) + "B",
        >= 1_000_000 => (amount / 1_000_000m).ToString("0.##", Inv) + "M",
        >= 1_000 => (amount / 1_000m).ToString("0.##", Inv) + "K",
        _ => amount.ToString("0", Inv)
    };

    public static string ToneClass(ReportTone tone) => tone switch
    {
        ReportTone.Good => "report-tone--good",
        ReportTone.Watch => "report-tone--watch",
        ReportTone.Late => "report-tone--late",
        _ => "report-tone--neutral"
    };

    public static string Phase(StageGroup? phase) => phase is { } p ? $"al-stage--{(int)p}" : "";

    public static string Duration(double? seconds) => seconds is { } s
        ? TimeSpan.FromSeconds(Math.Round(s)).ToString(s >= 3600 ? @"h\:mm\:ss" : @"m\:ss", Inv)
        : "length unknown";

    public static string SourceIcon(ReportSourceKind kind) => kind switch
    {
        ReportSourceKind.Message => "message",
        ReportSourceKind.Photo => "image",
        ReportSourceKind.Video => "camera",
        ReportSourceKind.Capture => "camera",
        ReportSourceKind.Release or ReportSourceKind.Claim => "money",
        ReportSourceKind.Commitment => "commitment",
        ReportSourceKind.Variation => "variation",
        ReportSourceKind.Blocker => "blocked",
        ReportSourceKind.Deliverable => "check",
        _ => "stage"
    };

    public static string SourceWord(ReportSourceKind kind) => kind switch
    {
        ReportSourceKind.Message => "Message",
        ReportSourceKind.Photo => "Photo",
        ReportSourceKind.Video => "Video",
        ReportSourceKind.Capture => "Capture",
        ReportSourceKind.Release => "Release",
        ReportSourceKind.Claim => "Claim",
        ReportSourceKind.Commitment => "Register",
        ReportSourceKind.Variation => "Variation",
        ReportSourceKind.Blocker => "Blocker",
        ReportSourceKind.Reading => "Reading",
        ReportSourceKind.Deliverable => "Deliverable",
        _ => "Record"
    };

    public static List<NarrativeSentenceDto> Words(WorksReportDto report, string targetId) =>
        report.Narrative.Targets.FirstOrDefault(t => t.TargetId == targetId)?.Sentences ?? new();

    public static bool HasPicture(ReportFrameDto f) => !f.IsVideo || f.PosterStatus == PosterStatus.Done;
}

/// <summary>The snapshot's sources, handed down so any chip can resolve its id to a printed reference.</summary>
public sealed class ReportSources
{
    private readonly Dictionary<string, ReportSourceDto> _byId;

    public ReportSources(IEnumerable<ReportSourceDto> sources) =>
        _byId = sources.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());

    public ReportSourceDto? Get(string id) => _byId.GetValueOrDefault(id);
}
