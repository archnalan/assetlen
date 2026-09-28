using System.Globalization;

namespace assetlen.Shared.Modules.Search.Components;

/// <summary>Formatting shared by the search page and its results.</summary>
public static class SearchFormat
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// A date as it was said. Thread times are the sender's wall clock, stored
    /// without a zone, so they are shown as they are — converting them would
    /// move a 16:14 message to 19:14.
    /// </summary>
    public static string When(DateTime at) =>
        at.TimeOfDay == TimeSpan.Zero ? at.ToString("d MMM yyyy", Inv) : at.ToString("d MMM yyyy, HH:mm", Inv);

    public static string Day(DateTime at) => at.ToString("d MMM yyyy", Inv);

    public readonly record struct Segment(string Text, bool Hit);

    /// <summary>Split text so every occurrence of a term can be marked.</summary>
    public static IEnumerable<Segment> Segments(string? text, IReadOnlyList<string> terms)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        if (terms.Count == 0)
        {
            yield return new Segment(text, false);
            yield break;
        }

        var i = 0;
        while (i < text.Length)
        {
            var next = -1;
            var len = 0;
            foreach (var t in terms)
            {
                if (string.IsNullOrEmpty(t)) continue;
                var at = text.IndexOf(t, i, StringComparison.OrdinalIgnoreCase);
                if (at >= 0 && (next < 0 || at < next || (at == next && t.Length > len)))
                {
                    next = at;
                    len = t.Length;
                }
            }

            if (next < 0)
            {
                yield return new Segment(text[i..], false);
                yield break;
            }

            if (next > i) yield return new Segment(text[i..next], false);
            yield return new Segment(text.Substring(next, len), true);
            i = next + len;
        }
    }
}
