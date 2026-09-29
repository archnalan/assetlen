using System.Text;
using System.Text.RegularExpressions;

namespace assetlen.Service.FileProcessingServices.Search;

/// <summary>
/// A question as Peter would say it, reduced to the words worth looking for.
/// <para>
/// <em>"What did I approve on the balustrade?"</em> is a search for
/// <c>balustrade</c>, biased toward agreements. The question words and the
/// verbs of agreeing are set aside rather than matched: nobody wrote
/// "approve" in the message that approved the balustrade.
/// </para>
/// </summary>
public sealed record SearchTerms(IReadOnlyList<string> Terms, IReadOnlyList<string> SetAside, bool AsksForAgreement)
{
    public const int MaxTerms = 6;

    /// <summary>
    /// Accept a word that is only nearly the term — an OCR misread, ZENTAHA for
    /// ZENTARA. Set when the database can find such words (pg_trgm), so what
    /// the query found and what scoring keeps follow one rule.
    /// </summary>
    public bool Fuzzy { get; init; }

    /// <summary>pg_trgm <c>word_similarity</c> at or above which a word counts as the term.</summary>
    public const double FuzzyThreshold = 0.6;

    /// <summary>
    /// Short words and numbers are matched exactly: "tile" is three trigrams from
    /// "title", and a receipt number that is nearly right is a different receipt.
    /// </summary>
    public static bool FuzzyEligible(string term) => term.Length >= 6 && !term.All(char.IsDigit);

    private static readonly HashSet<string> Function = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "are", "as", "at", "be", "been", "by", "can", "could", "did", "do", "does",
        "for", "from", "had", "has", "have", "how", "i", "if", "in", "into", "is", "it", "its", "me",
        "my", "of", "on", "or", "our", "so", "that", "the", "their", "them", "then", "there", "this",
        "to", "up", "us", "was", "we", "were", "what", "whats", "when", "where", "which", "who", "why",
        "will", "with", "you", "your", "about", "any", "all", "get", "got", "please", "show", "find",
        "tell", "there", "those", "these", "they", "he", "she", "him", "her", "his", "ever", "last",
        "again", "was", "wasnt", "didnt", "dont"
    };

    /// <summary>The verbs of agreeing. Dropped from matching, kept as a hint that the reader wants the commitment.</summary>
    private static readonly HashSet<string> Agreement = new(StringComparer.OrdinalIgnoreCase)
    {
        "approve", "approved", "approval", "agree", "agreed", "agreement", "decide", "decided",
        "decision", "say", "said", "promise", "promised", "commit", "committed", "commitment",
        "confirm", "confirmed", "choose", "chose", "chosen", "pick", "picked", "settle", "settled"
    };

    private static readonly Regex Word = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);

    public static SearchTerms Parse(string? query)
    {
        var words = Word.Matches(query ?? "")
            .Select(m => m.Value.ToLowerInvariant())
            .Where(w => w.Length >= 2 || w.All(char.IsDigit))
            .Distinct()
            .ToList();

        var asks = words.Any(Agreement.Contains);
        var kept = words.Where(w => !Function.Contains(w) && !Agreement.Contains(w)).ToList();
        var setAside = words.Where(w => !kept.Contains(w)).ToList();

        // "approved" alone is still a search — for the word, since nothing else is left.
        if (kept.Count == 0)
        {
            kept = words.Where(w => !Function.Contains(w)).ToList();
            setAside = words.Where(w => !kept.Contains(w)).ToList();
        }

        return new SearchTerms(kept.Take(MaxTerms).Select(Stem).Distinct().ToList(), setAside, asks);
    }

    /// <summary>
    /// A crude singular, so "receipts" finds "receipt". Matching is by substring,
    /// so the stem must be a prefix of every form it stands for — never an
    /// ending added, only one taken away.
    /// </summary>
    public static string Stem(string w)
    {
        if (w.Length > 4 && w.EndsWith("ies")) return w[..^3];
        if (w.Length > 4 && w.EndsWith("es") && (w.EndsWith("ches") || w.EndsWith("shes") || w.EndsWith("xes"))) return w[..^2];
        if (w.Length > 3 && w.EndsWith('s') && !w.EndsWith("ss")) return w[..^1];
        return w;
    }

    /// <summary>How many of the terms occur in any of the texts.</summary>
    public int CountIn(params string?[] texts)
    {
        var n = 0;
        foreach (var t in Terms)
            if (texts.Any(x => Occurs(t, x))) n++;
        return n;
    }

    private bool Occurs(string term, string? text) =>
        text is not null
        && (text.Contains(term, StringComparison.OrdinalIgnoreCase)
            || (Fuzzy && FuzzyEligible(term) && Word.Matches(text).Any(w => WordSimilarity(term, w.Value) >= FuzzyThreshold)));

    /// <summary>
    /// pg_trgm's <c>word_similarity</c> for a single word: the share of the term's
    /// trigrams (lower-cased, padded two spaces before and one after) found in it.
    /// </summary>
    public static double WordSimilarity(string term, string word)
    {
        var a = Trigrams(term);
        if (a.Count == 0) return 0;
        var b = Trigrams(word);
        return (double)a.Count(b.Contains) / a.Count;
    }

    private static HashSet<string> Trigrams(string w)
    {
        var padded = "  " + w.ToLowerInvariant() + " ";
        var set = new HashSet<string>();
        for (var i = 0; i + 3 <= padded.Length; i++) set.Add(padded.Substring(i, 3));
        return set;
    }

    /// <summary>
    /// The fewest terms a result must contain. All of them for a short query;
    /// all but one once the reader has typed three or more words, because the
    /// fourth word of a remembered phrase is the one most often misremembered.
    /// </summary>
    public int Required => Terms.Count <= 2 ? Terms.Count : Terms.Count - 1;

    /// <summary>The passage around the first match, whitespace collapsed, about <paramref name="width"/> characters.</summary>
    public string? Snippet(string? text, int width = 180)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var flat = Regex.Replace(text, @"\s+", " ").Trim();

        var first = Terms
            .Select(t => flat.IndexOf(t, StringComparison.OrdinalIgnoreCase))
            .Where(i => i >= 0)
            .DefaultIfEmpty(0)
            .Min();

        if (flat.Length <= width) return flat;

        var start = Math.Max(0, first - width / 3);
        var end = Math.Min(flat.Length, start + width);
        start = Math.Max(0, end - width);

        var sb = new StringBuilder();
        if (start > 0) sb.Append('…');
        sb.Append(flat, start, end - start);
        if (end < flat.Length) sb.Append('…');
        return sb.ToString();
    }
}
