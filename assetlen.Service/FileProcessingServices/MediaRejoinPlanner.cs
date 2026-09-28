using System.Globalization;
using System.Text.RegularExpressions;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices;

/// <summary>What a loose media file's name says about it.</summary>
public sealed record MediaFileName(
    string FileName,
    DateTime? StampedAt,
    string? Kind,
    int CopyOrdinal,
    string? Caption);

/// <summary>
/// Reads the name WhatsApp gives a saved file — <c>WhatsApp Image 2026-09-28 at 7.58.25 PM.jpeg</c>
/// — and the copies a phone or a person makes of it (<c>WhatsApp Image2 …</c>,
/// <c>WhatsApp2 Image …</c>, <c>… (1).jpeg</c>).
/// <para>
/// The stamp is the only reliable fact about the file. Folder names are ignored on
/// purpose: a dozen of the real week's files sat in the wrong day's folder, and
/// the stamp was right where the folder was wrong (works-report.md §5).
/// </para>
/// </summary>
public static class WhatsAppMediaName
{
    private static readonly Regex Stamped = new(
        @"^WhatsApp(?<o1>\d*)\s+(?<kind>Image|Video|Audio|Document|Sticker|GIF|PTT)(?<o2>\d*)\s+(?<date>\d{4}-\d{2}-\d{2})\s+at\s+(?<h>\d{1,2})[.:](?<mi>\d{2})[.:](?<s>\d{2})(?:\s*(?<ap>[AaPp][Mm]))?(?:\s*\((?<o3>\d+)\))?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static MediaFileName Parse(string path)
    {
        var fileName = Path.GetFileName(path.Replace('\\', '/'));
        var stem = Path.GetFileNameWithoutExtension(fileName).Trim();

        var m = Stamped.Match(stem);
        if (!m.Success)
            return new MediaFileName(fileName, null, null, 0, Caption(stem));

        var date = DateTime.ParseExact(m.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var hour = int.Parse(m.Groups["h"].Value, CultureInfo.InvariantCulture);
        var ap = m.Groups["ap"].Value.ToUpperInvariant();
        if (ap == "PM" && hour < 12) hour += 12;
        if (ap == "AM" && hour == 12) hour = 0;

        var stamp = date.AddHours(hour)
                        .AddMinutes(int.Parse(m.Groups["mi"].Value, CultureInfo.InvariantCulture))
                        .AddSeconds(int.Parse(m.Groups["s"].Value, CultureInfo.InvariantCulture));

        var ordinal = Ordinal(m.Groups["o1"].Value) + Ordinal(m.Groups["o2"].Value) + Ordinal(m.Groups["o3"].Value);
        return new MediaFileName(fileName, stamp, m.Groups["kind"].Value, ordinal, null);
    }

    private static int Ordinal(string digits) => int.TryParse(digits, out var n) ? n : 0;

    /// <summary>
    /// "intended stoppage line" is a person naming what the photo shows — the
    /// best caption the record will ever get, so it is kept as one.
    /// </summary>
    private static string? Caption(string stem)
    {
        var text = Regex.Replace(stem, @"[_]+", " ");
        text = Regex.Replace(text, @"(?<=[A-Za-z])\d{1,2}$", "");
        text = Regex.Replace(text, @"\s*\(\d+\)$", "");
        text = Regex.Replace(text, @"\s+", " ").Trim();

        // Camera and messenger names carry nothing a reader can use.
        if (text.Length < 3 || Regex.IsMatch(text, @"^(?:IMG|VID|PXL|DSC|DCIM|Screenshot|image|video|photo)[\s_-]*\d", RegexOptions.IgnoreCase))
            return null;

        return char.ToUpperInvariant(text[0]) + text[1..];
    }
}

/// <summary>A <c>&lt;Media omitted&gt;</c> line still waiting for its file.</summary>
public sealed record OpenMediaLine(string MessageId, DateTime SentAt, string Author, int SequenceNo);

/// <summary>A loose file offered to the re-join, identified by its bytes.</summary>
public sealed record LooseFile(string Key, MediaFileName Name, string Sha256);

public sealed record RejoinDecision(
    LooseFile File,
    MediaRejoinOutcome Outcome,
    string Reason,
    OpenMediaLine? Line);

/// <summary>
/// Binds loose files to the transcript lines they belong to (works-report.md §5).
/// <para>
/// Pure: no database, no storage. Android stamps a message to the minute and the
/// file to the second, so within one minute the files are put in seconds order
/// and paired with that minute's media lines in transcript order — the same
/// ordering the import's occurrence ordinal preserves. Nothing is dropped: a
/// file that binds to nothing is reported as unbound and still kept.
/// </para>
/// </summary>
public static class MediaRejoinPlanner
{
    public static List<RejoinDecision> Plan(IReadOnlyList<OpenMediaLine> openLines, IReadOnlyList<LooseFile> files)
    {
        var decisions = new List<RejoinDecision>();

        // Law 2 inside one upload: "Image2" is usually the same bytes saved twice.
        var distinct = new List<LooseFile>();
        foreach (var file in files
                     .OrderBy(f => f.Name.StampedAt ?? DateTime.MaxValue)
                     .ThenBy(f => f.Name.CopyOrdinal)
                     .ThenBy(f => f.Name.FileName, StringComparer.OrdinalIgnoreCase))
        {
            var first = distinct.FirstOrDefault(d => d.Sha256 == file.Sha256);
            if (first is not null)
            {
                decisions.Add(new RejoinDecision(file, MediaRejoinOutcome.Duplicate,
                    $"Same bytes as {first.Name.FileName}.", null));
                continue;
            }
            distinct.Add(file);
        }

        foreach (var file in distinct.Where(f => f.Name.StampedAt is null))
            decisions.Add(new RejoinDecision(file, MediaRejoinOutcome.Unbound,
                file.Name.Caption is null
                    ? "No WhatsApp stamp in the name, so there is no moment to bind it to."
                    : "No WhatsApp stamp in the name — kept, with its name as the caption.",
                null));

        var linesByMinute = openLines
            .GroupBy(l => Minute(l.SentAt))
            .ToDictionary(g => g.Key, g => new Queue<OpenMediaLine>(g.OrderBy(l => l.SequenceNo)));

        var leftovers = new List<LooseFile>();

        foreach (var group in distinct.Where(f => f.Name.StampedAt is not null)
                     .GroupBy(f => Minute(f.Name.StampedAt!.Value))
                     .OrderBy(g => g.Key))
        {
            linesByMinute.TryGetValue(group.Key, out var queue);
            foreach (var file in group.OrderBy(f => f.Name.StampedAt).ThenBy(f => f.Name.CopyOrdinal))
            {
                if (queue is { Count: > 0 })
                    decisions.Add(new RejoinDecision(file, MediaRejoinOutcome.Bound,
                        "Bound by the minute in its name.", queue.Dequeue()));
                else
                    leftovers.Add(file);
            }
        }

        // A file saved a few seconds after the minute turned belongs to the
        // neighbouring minute's line. Tried only once every exact match is taken.
        foreach (var file in leftovers)
        {
            var minute = Minute(file.Name.StampedAt!.Value);
            var near = new[] { minute.AddMinutes(-1), minute.AddMinutes(1) }
                .Select(k => linesByMinute.GetValueOrDefault(k))
                .FirstOrDefault(q => q is { Count: > 0 });

            if (near is not null)
            {
                decisions.Add(new RejoinDecision(file, MediaRejoinOutcome.Bound,
                    "Bound to the neighbouring minute — every line in its own minute was taken.", near.Dequeue()));
                continue;
            }

            decisions.Add(new RejoinDecision(file, MediaRejoinOutcome.Unbound,
                linesByMinute.ContainsKey(minute)
                    ? "More files than media lines in that minute."
                    : "No media line in the transcript at that minute.",
                null));
        }

        return decisions;
    }

    private static DateTime Minute(DateTime t) => new(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
}
