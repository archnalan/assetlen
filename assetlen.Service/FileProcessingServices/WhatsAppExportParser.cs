using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices;

/// <summary>One transcript line, parsed. Pure data — no database, no ids.</summary>
public sealed record WhatsAppMessage(
    int SequenceNo,
    DateTime SentAt,
    string Author,
    string Body,
    string? MediaFileName,
    bool HasMediaMarker,
    bool IsSystemMessage);

/// <summary>What one pass over an export transcript produced.</summary>
public sealed record WhatsAppParseResult(
    List<WhatsAppMessage> Messages,
    List<string> Participants,
    ExportDateOrder DateOrder,
    List<string> Warnings);

/// <summary>
/// Reads a WhatsApp chat export into messages (plan.md P3, assetlen.md D3).
/// <para>
/// Deliberately a <b>pure function over text</b>: no database, no storage, no
/// access checks. Every hard problem in ingest is in here — date ambiguity,
/// invisible direction marks, four media-marker dialects — and each one silently
/// corrupts a year of history rather than throwing. Keeping it pure is what
/// makes those failures reproducible from a file instead of from a live import.
/// </para>
/// <para>
/// Nothing is dropped. A line that cannot be understood is attached to the
/// previous message rather than discarded, because the corpus is the evidence
/// and a parser that quietly loses 3% of it is worse than one that admits it.
/// </para>
/// </summary>
public static class WhatsAppExportParser
{
    /// <summary>Matches <c>tbl_IngestedMessage.Body</c>. Longer bodies are kept, truncated, and warned about.</summary>
    public const int MaxBodyLength = 8000;

    private const int MaxAuthorLength = 100;

    // ─── Line shapes ─────────────────────────────────────────────────────
    //
    // iOS brackets the stamp, Android follows it with " - ". Both are matched
    // non-greedily against the *whole* line; anything that matches neither is a
    // continuation, which is how multi-line messages survive.

    private static readonly Regex IosHeader = new(
        @"^\[(?<ts>[^\]]{6,40})\]\s*(?<rest>.*)$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex AndroidHeader = new(
        @"^(?<ts>\d{1,4}[./-]\d{1,2}[./-]\d{2,4},?\s+\d{1,2}:\d{2}(?::\d{2})?(?:\s*[APap]\.?[Mm]\.?)?)\s+-\s+(?<rest>.*)$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Pulls the parts of a stamp out without committing to day/month order —
    /// that decision needs the whole file and is made once, afterwards.
    /// </summary>
    private static readonly Regex Stamp = new(
        @"^(?<a>\d{1,4})[./-](?<b>\d{1,2})[./-](?<c>\d{2,4}),?\s+(?<h>\d{1,2}):(?<mi>\d{2})(?::(?<s>\d{2}))?\s*(?<ap>[APap]\.?[Mm]\.?)?\s*$",
        RegexOptions.Compiled);

    private static readonly Regex AuthorSplit = new(
        @"^(?<author>[^:\r\n]{1,100}): ?(?<body>.*)$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    // ─── Media markers ───────────────────────────────────────────────────
    //
    // Four dialects, and only two of them name the file. An export "without
    // media" — which is what the corpus is — names nothing at all, so the
    // message still has to be recorded as carrying an attachment or 47% of the
    // thread reads as empty text.

    /// <summary>Android, media included: <c>IMG-20260716-WA0009.jpg (file attached)</c>.</summary>
    private static readonly Regex AndroidAttachment = new(
        @"^\s*(?<name>[^\r\n<>|:*?""]{1,255}?)\s*\((?:file attached|archivo adjunto|fichier joint)\)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    /// <summary>iOS, media included: <c>&lt;attached: 00000042-PHOTO-2026-07-16-21-56-03.jpg&gt;</c>.</summary>
    private static readonly Regex IosAttachment = new(
        @"<attached:\s*(?<name>[^>]{1,255})>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Android, media stripped: the literal <c>&lt;Media omitted&gt;</c>.</summary>
    private static readonly Regex OmittedAngle = new(
        @"<\s*(?:media|medios|média)\s+(?:omitted|omitidos|omis)\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>iOS, media stripped: a bare <c>image omitted</c> / <c>audio omitted</c> line.</summary>
    private static readonly Regex OmittedBare = new(
        @"^\s*(?:image|video|audio|sticker|document|GIF|contact card)\s+omitted\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// WhatsApp's own notices. These have no author, so the author split would
    /// otherwise mis-read the first clause of one as a person's name.
    /// </summary>
    private static readonly string[] SystemPhrases =
    {
        "end-to-end encrypted",
        "created group",
        "added you",
        "changed the subject",
        "changed this group's icon",
        "changed their phone number",
        "security code changed",
        "joined using this group's invite link",
        "left",
        "removed",
        "you were added",
        "messages you send to this chat",
        "waiting for this message",
        "changed the group description",
        "turned on disappearing messages",
        "pinned a message"
    };

    /// <summary>
    /// Parse a transcript. Never throws on malformed input — a file that is not
    /// a WhatsApp export simply yields no messages and a warning saying so.
    /// </summary>
    public static WhatsAppParseResult Parse(string text)
    {
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return new WhatsAppParseResult(new(), new(), ExportDateOrder.AssumedDayFirst,
                new() { "The transcript was empty." });

        text = Normalize(text);

        // Pass 1 — split into raw records, keeping the stamp unresolved.
        var raw = new List<RawMessage>();
        foreach (var line in text.Split('\n'))
        {
            var header = IosHeader.Match(line);
            if (!header.Success) header = AndroidHeader.Match(line);

            if (header.Success)
            {
                var stamp = Stamp.Match(header.Groups["ts"].Value.Trim());
                if (stamp.Success)
                {
                    raw.Add(new RawMessage(stamp, header.Groups["rest"].Value));
                    continue;
                }
            }

            // Not a header. A continuation of the message above — WhatsApp keeps
            // hard line breaks inside a single message, and a specification list
            // ("Cement: Tororo CEM II", one material per line) is exactly the
            // high-value shape that arrives that way.
            if (raw.Count > 0)
                raw[^1].Continuations.Add(line);
            else if (line.Trim().Length > 0)
                warnings.Add($"Ignored a leading line with no timestamp: \"{Clip(line, 60)}\"");
        }

        if (raw.Count == 0)
            return new WhatsAppParseResult(new(), new(), ExportDateOrder.AssumedDayFirst,
                new() { "No WhatsApp messages were found. Is this the chat's .txt transcript?" });

        // Pass 2 — resolve day/month order across the whole file, once.
        var order = ResolveDateOrder(raw);
        if (order == ExportDateOrder.AssumedDayFirst)
            warnings.Add("No date in this export distinguishes day from month, so day-first was assumed. " +
                         "If the history looks shifted, the export came from a month-first phone.");

        // Pass 3 — materialise.
        var messages = new List<WhatsAppMessage>(raw.Count);
        var authorCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var badStamps = 0;

        foreach (var r in raw)
        {
            if (!TryBuildTimestamp(r.Stamp, order, out var sentAt))
            {
                badStamps++;
                continue;
            }

            var full = r.Continuations.Count == 0
                ? r.Rest
                : r.Rest + "\n" + string.Join("\n", r.Continuations);

            var (author, body, isSystem) = SplitAuthor(full);
            var (mediaName, hasMedia, cleanBody) = ExtractMedia(body);

            if (cleanBody.Length > MaxBodyLength)
            {
                warnings.Add($"A message from {(author.Length == 0 ? "the system" : author)} " +
                             $"on {sentAt:yyyy-MM-dd} was truncated to {MaxBodyLength} characters.");
                cleanBody = cleanBody[..MaxBodyLength];
            }

            if (!isSystem && author.Length > 0)
                authorCounts[author] = authorCounts.GetValueOrDefault(author) + 1;

            messages.Add(new WhatsAppMessage(
                messages.Count, sentAt, author, cleanBody, mediaName, hasMedia, isSystem));
        }

        if (badStamps > 0)
            warnings.Add($"{badStamps} line(s) carried a timestamp that is not a real date and were skipped.");

        // Busiest first: the participant who posts most is the one whose mapping
        // matters, and in the corpus that is the contractor at 69% of the thread.
        var participants = authorCounts
            .OrderByDescending(p => p.Value)
            .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
            .Select(p => p.Key)
            .ToList();

        return new WhatsAppParseResult(messages, participants, order, warnings);
    }

    // ─── Normalisation ───────────────────────────────────────────────────

    /// <summary>
    /// Strip what is invisible and would otherwise break every regex in this file.
    /// <para>
    /// WhatsApp wraps stamps and system notices in <b>directional marks</b>
    /// (U+200E/U+200F) and, on newer iOS builds, separates the time from AM/PM
    /// with a <b>narrow no-break space</b> (U+202F). None of them render, all of
    /// them defeat <c>\s</c> or a leading <c>^\[</c>, and the symptom is a file
    /// that parses to zero messages while looking perfectly correct in an editor.
    /// </para>
    /// </summary>
    private static string Normalize(string text)
    {
        var sb = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            switch (ch)
            {
                case '\uFEFF':   // BOM — also appears mid-file after some transfers
                case '\u200B':   // zero-width space
                case '\u200E':   // left-to-right mark
                case '\u200F':   // right-to-left mark
                case '\u061C':   // arabic letter mark
                case '\r':
                    continue;
                case '\u00A0':   // no-break space
                case '\u202F':   // narrow no-break space — iOS puts it before AM/PM
                    sb.Append(' ');
                    continue;
                default:
                    sb.Append(ch);
                    continue;
            }
        }

        return sb.ToString();
    }

    // ─── Dates ───────────────────────────────────────────────────────────

    /// <summary>
    /// Decide day-first vs month-first from the evidence in the file.
    /// <para>
    /// A single date with a first component above 12 proves day-first outright;
    /// one with a second component above 12 proves month-first. A year-long
    /// export contains hundreds of both kinds, so this is decisive in practice —
    /// but the default matters, because getting it wrong moves most of a
    /// project's history by months without any visible error.
    /// </para>
    /// </summary>
    private static ExportDateOrder ResolveDateOrder(List<RawMessage> raw)
    {
        foreach (var r in raw)
        {
            // A four-digit leading component is a year — ISO order, no ambiguity
            // to resolve and nothing this decision applies to.
            if (r.Stamp.Groups["a"].Value.Length == 4) continue;

            var a = int.Parse(r.Stamp.Groups["a"].Value, CultureInfo.InvariantCulture);
            var b = int.Parse(r.Stamp.Groups["b"].Value, CultureInfo.InvariantCulture);

            if (a > 12) return ExportDateOrder.DayFirst;
            if (b > 12) return ExportDateOrder.MonthFirst;
        }

        return ExportDateOrder.AssumedDayFirst;
    }

    private static bool TryBuildTimestamp(Match stamp, ExportDateOrder order, out DateTime result)
    {
        result = default;

        var a = int.Parse(stamp.Groups["a"].Value, CultureInfo.InvariantCulture);
        var b = int.Parse(stamp.Groups["b"].Value, CultureInfo.InvariantCulture);
        var c = int.Parse(stamp.Groups["c"].Value, CultureInfo.InvariantCulture);

        int year, month, day;

        if (stamp.Groups["a"].Value.Length == 4)
        {
            (year, month, day) = (a, b, c);       // 2026-07-16
        }
        else
        {
            year = c < 100 ? 2000 + c : c;
            (day, month) = order == ExportDateOrder.MonthFirst ? (b, a) : (a, b);
        }

        var hour = int.Parse(stamp.Groups["h"].Value, CultureInfo.InvariantCulture);
        var minute = int.Parse(stamp.Groups["mi"].Value, CultureInfo.InvariantCulture);
        var second = stamp.Groups["s"].Success
            ? int.Parse(stamp.Groups["s"].Value, CultureInfo.InvariantCulture)
            : 0;

        if (stamp.Groups["ap"].Success)
        {
            var isPm = stamp.Groups["ap"].Value.TrimStart().StartsWith("p", StringComparison.OrdinalIgnoreCase);
            // 12 AM is hour 0 and 12 PM is hour 12 — the one pair that does not
            // follow the +12 rule, and midnight is a busy hour in this corpus.
            if (isPm && hour < 12) hour += 12;
            else if (!isPm && hour == 12) hour = 0;
        }

        if (month is < 1 or > 12 || day < 1 || year is < 1970 or > 2200) return false;
        if (day > DateTime.DaysInMonth(year, month)) return false;
        if (hour > 23 || minute > 59 || second > 59) return false;

        // Unspecified, not Utc: the export carries the sender's wall-clock time
        // with no zone. Asserting Utc would silently shift a day's work across
        // a date boundary — and the brief is grouped by day.
        result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
        return true;
    }

    // ─── Author and body ─────────────────────────────────────────────────

    private static (string Author, string Body, bool IsSystem) SplitAuthor(string rest)
    {
        var trimmed = rest.TrimStart();

        var match = AuthorSplit.Match(trimmed);
        if (!match.Success)
            return (string.Empty, trimmed.Trim(), true);

        var author = match.Groups["author"].Value.Trim();
        var body = match.Groups["body"].Value;

        // "Messages and calls are end-to-end encrypted. No one outside of this
        // chat…" splits cleanly at a colon that is not an author separator on
        // some locales. Known notices are classified before the split is trusted.
        if (LooksLikeSystemNotice(trimmed))
            return (string.Empty, trimmed.Trim(), true);

        if (author.Length is 0 or > MaxAuthorLength)
            return (string.Empty, trimmed.Trim(), true);

        return (author, body.Trim(), false);
    }

    private static bool LooksLikeSystemNotice(string line)
    {
        foreach (var phrase in SystemPhrases)
            if (line.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    // ─── Media ───────────────────────────────────────────────────────────

    /// <summary>
    /// Pull an attachment out of a body. Returns the file name when the export
    /// carried one, whether a marker was present at all, and the body with the
    /// marker removed so a caption survives on its own.
    /// </summary>
    private static (string? Name, bool HasMedia, string Body) ExtractMedia(string body)
    {
        var ios = IosAttachment.Match(body);
        if (ios.Success)
            return (CleanFileName(ios.Groups["name"].Value),
                    true,
                    IosAttachment.Replace(body, string.Empty).Trim());

        var android = AndroidAttachment.Match(body);
        if (android.Success)
            return (CleanFileName(android.Groups["name"].Value),
                    true,
                    AndroidAttachment.Replace(body, string.Empty).Trim());

        // Marker present, file absent — an "export without media". The message
        // must still count as carrying one: 723 of the corpus's 1,529 messages
        // are exactly this, and treating them as empty text loses half the thread.
        if (OmittedAngle.IsMatch(body))
            return (null, true, OmittedAngle.Replace(body, string.Empty).Trim());

        if (OmittedBare.IsMatch(body))
            return (null, true, string.Empty);

        return (null, false, body);
    }

    private static string? CleanFileName(string raw)
    {
        var name = raw.Trim();
        if (name.Length == 0) return null;

        // Only ever the leaf. An archive may nest media in a folder, and a name
        // carrying a path would let a crafted export write outside the store.
        name = name.Replace('\\', '/');
        var slash = name.LastIndexOf('/');
        if (slash >= 0) name = name[(slash + 1)..];

        return name.Length == 0 ? null : name;
    }

    private static string Clip(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";

    /// <summary>A header line plus the un-prefixed lines that belong to it.</summary>
    private sealed class RawMessage
    {
        public RawMessage(Match stamp, string rest)
        {
            Stamp = stamp;
            Rest = rest;
        }

        public Match Stamp { get; }
        public string Rest { get; }
        public List<string> Continuations { get; } = new();
    }
}
