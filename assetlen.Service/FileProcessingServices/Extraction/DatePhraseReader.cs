using System.Globalization;
using System.Text.RegularExpressions;

namespace assetlen.Service.FileProcessingServices.Extraction;

/// <summary>A date found in a sentence, and the words it was read from.</summary>
public sealed record DateHit(DateTime Date, string Text, bool IsRelative, bool IsToday);

/// <summary>
/// Reads a promised date out of a sentence, anchored on when the message was sent.
/// <para>
/// The thread's dates are mostly relative — "tomorrow", "by Tuesday", "this week"
/// — and several of them lapsed (works-report.md §1). A relative promise is only
/// checkable once it is pinned to a calendar day, and the only honest anchor is
/// the moment it was said, never the moment it was imported.
/// </para>
/// </summary>
public static class DatePhraseReader
{
    private const RegexOptions Opts = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant;

    private const string MonthNames =
        "january|february|march|april|may|june|july|august|september|october|november|december|" +
        "jan|feb|mar|apr|jun|jul|aug|sept|sep|oct|nov|dec";

    private static readonly Regex EndOfMonth = new(
        $@"\b(?:by\s+)?(?:the\s+)?end\s+of\s+(?<m>{MonthNames})\b(?:\s+(?<y>\d{{4}}))?", Opts);

    private static readonly Regex DayMonth = new(
        $@"\b(?<d>\d{{1,2}})(?:st|nd|rd|th)?\s+(?:of\s+)?(?<m>{MonthNames})\b\.?(?:,?\s+(?<y>\d{{4}}))?", Opts);

    private static readonly Regex MonthDay = new(
        $@"\b(?<m>{MonthNames})\.?\s+(?<d>\d{{1,2}})(?:st|nd|rd|th)?\b(?:,?\s+(?<y>\d{{4}}))?", Opts);

    private static readonly Regex DayAfterTomorrow = new(@"\bday\s+after\s+(?:tomorrow|2morrow)\b", Opts);

    private static readonly Regex Tomorrow = new(@"\b(?:(?:by\s+)?(?:the\s+)?end\s+of\s+)?(?:tomorrow|2morrow|tmrw|tomorow|tommorow|tommorrow)\b", Opts);

    // Full names only: "sat" and "wed" are ordinary English words in a site thread.
    private static readonly Regex Weekday = new(
        @"\b(?<pre>by|on|this|next|coming|before|until|till)?\s*(?<w>monday|tuesday|wednesday|thursday|friday|saturday|sunday)\b", Opts);

    private static readonly Regex ThisWeek = new(@"\b(?:this\s+week|end\s+of\s+(?:the\s+|this\s+)?week|this\s+weekend|by\s+the\s+weekend)\b", Opts);

    private static readonly Regex NextWeek = new(@"\bnext\s+week\b", Opts);

    private static readonly Regex EndOfThisMonth = new(@"\b(?:end\s+of\s+(?:the\s+|this\s+)month|this\s+month)\b", Opts);

    private static readonly Regex InN = new(@"\bin\s+(?<n>\d{1,2}|a|one|two|three|four|five|six)\s+(?<u>days?|weeks?)\b", Opts);

    private static readonly Regex Today = new(@"\b(?:today|tonight|this\s+evening|this\s+afternoon)\b", Opts);

    /// <summary>The first date the sentence commits to, or null.</summary>
    public static DateHit? Find(string sentence, DateTime anchor)
    {
        if (string.IsNullOrWhiteSpace(sentence)) return null;
        var day = anchor.Date;

        Match m;

        if ((m = EndOfMonth.Match(sentence)).Success && TryMonth(m.Groups["m"].Value, out var em))
        {
            var year = Year(m.Groups["y"].Value, day, em, DateTime.DaysInMonth(day.Year, em));
            return new DateHit(new DateTime(year, em, DateTime.DaysInMonth(year, em)), m.Value.Trim(), false, false);
        }

        if ((m = DayMonth.Match(sentence)).Success && TryAbsolute(m, day, out var dm))
            return new DateHit(dm, m.Value.Trim(), false, false);

        if ((m = MonthDay.Match(sentence)).Success && TryAbsolute(m, day, out var md))
            return new DateHit(md, m.Value.Trim(), false, false);

        if ((m = DayAfterTomorrow.Match(sentence)).Success)
            return new DateHit(day.AddDays(2), m.Value.Trim(), true, false);

        if ((m = Tomorrow.Match(sentence)).Success)
            return new DateHit(day.AddDays(1), m.Value.Trim(), true, false);

        if ((m = Weekday.Match(sentence)).Success)
        {
            var target = Enum.Parse<DayOfWeek>(m.Groups["w"].Value, ignoreCase: true);
            var ahead = ((int)target - (int)day.DayOfWeek + 7) % 7;
            if (ahead == 0) ahead = 7;
            var date = day.AddDays(ahead);

            // "Next Tuesday" said on a Monday means the Tuesday after the one tomorrow.
            if (m.Groups["pre"].Value.Equals("next", StringComparison.OrdinalIgnoreCase)
                && date <= EndOfWeek(day))
                date = date.AddDays(7);

            return new DateHit(date, m.Value.Trim(), true, false);
        }

        if ((m = ThisWeek.Match(sentence)).Success)
            return new DateHit(EndOfWeek(day), m.Value.Trim(), true, false);

        if ((m = NextWeek.Match(sentence)).Success)
            return new DateHit(EndOfWeek(day).AddDays(7), m.Value.Trim(), true, false);

        if ((m = EndOfThisMonth.Match(sentence)).Success)
            return new DateHit(new DateTime(day.Year, day.Month, DateTime.DaysInMonth(day.Year, day.Month)), m.Value.Trim(), true, false);

        if ((m = InN.Match(sentence)).Success)
        {
            var n = Number(m.Groups["n"].Value);
            var weeks = m.Groups["u"].Value.StartsWith("week", StringComparison.OrdinalIgnoreCase);
            return new DateHit(day.AddDays(weeks ? n * 7 : n), m.Value.Trim(), true, false);
        }

        if ((m = Today.Match(sentence)).Success)
            return new DateHit(day, m.Value.Trim(), true, true);

        return null;
    }

    /// <summary>A construction week runs Monday to Saturday; "this week" is kept until the Sunday that ends it.</summary>
    public static DateTime EndOfWeek(DateTime day)
    {
        var toSunday = ((int)DayOfWeek.Sunday - (int)day.DayOfWeek + 7) % 7;
        return day.Date.AddDays(toSunday);
    }

    private static bool TryAbsolute(Match m, DateTime anchor, out DateTime date)
    {
        date = default;
        if (!TryMonth(m.Groups["m"].Value, out var month)) return false;
        if (!int.TryParse(m.Groups["d"].Value, out var d) || d < 1 || d > 31) return false;

        var year = Year(m.Groups["y"].Value, anchor, month, d);
        if (d > DateTime.DaysInMonth(year, month)) return false;
        date = new DateTime(year, month, d);
        return true;
    }

    /// <summary>
    /// No year was said: take the anchor's, unless that puts the date more than a
    /// month in the past, in which case it is next year's — nobody promises a date
    /// that has already gone.
    /// </summary>
    private static int Year(string said, DateTime anchor, int month, int day)
    {
        if (int.TryParse(said, out var y) && y is > 2000 and < 2100) return y;
        var candidate = new DateTime(anchor.Year, month, Math.Min(day, DateTime.DaysInMonth(anchor.Year, month)));
        return candidate < anchor.AddDays(-31) ? anchor.Year + 1 : anchor.Year;
    }

    private static bool TryMonth(string text, out int month)
    {
        month = 0;
        var t = text.ToLowerInvariant();
        if (t == "sept") t = "sep";
        for (var i = 1; i <= 12; i++)
        {
            var name = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(i).ToLowerInvariant();
            if (name == t || name.StartsWith(t) && t.Length >= 3)
            {
                month = i;
                return true;
            }
        }
        return false;
    }

    private static int Number(string n) => n.ToLowerInvariant() switch
    {
        "a" or "one" => 1,
        "two" => 2,
        "three" => 3,
        "four" => 4,
        "five" => 5,
        "six" => 6,
        _ => int.TryParse(n, out var v) ? v : 1
    };
}
