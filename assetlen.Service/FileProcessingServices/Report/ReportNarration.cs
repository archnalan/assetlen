using System.Globalization;
using System.Text.RegularExpressions;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.FileProcessingServices.Report;

public enum NarrativeTargetKind
{
    Cover,
    Answer,
    Stage,
    Decision,
    Variation,
    Blocker
}

/// <summary>
/// One thing on the page that gets words, with everything the words may draw
/// on. The facts are final before any model sees them; the template is what
/// the page says when no model is used or when the model's draft is refused.
/// </summary>
public sealed class NarrativeRequest
{
    public required string TargetId { get; init; }
    public required NarrativeTargetKind Kind { get; init; }

    /// <summary>The facts in words, every figure already computed.</summary>
    public List<string> Facts { get; } = new();

    /// <summary>What each fact came from — the only source ids a sentence may cite.</summary>
    public HashSet<string> SourceIds { get; } = new(StringComparer.Ordinal);

    /// <summary>The words of the sources themselves: message bodies, captions, capture notes.</summary>
    public List<(string SourceId, string Text)> Snippets { get; } = new();

    public List<NarrativeSentenceDto> Template { get; } = new();

    public int MaxSentences => Kind switch
    {
        NarrativeTargetKind.Cover => 3,
        NarrativeTargetKind.Answer => 3,
        NarrativeTargetKind.Stage => 2,
        _ => 1
    };

    public int MaxWords => Kind switch
    {
        NarrativeTargetKind.Cover => 90,
        NarrativeTargetKind.Answer => 60,
        NarrativeTargetKind.Stage => 40,
        _ => 30
    };

    public NarrativeRequest Say(string text, params string[] sourceIds)
    {
        var ids = sourceIds.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
        foreach (var id in ids) SourceIds.Add(id);
        Template.Add(new NarrativeSentenceDto { Text = text, SourceIds = ids });
        Facts.Add(text);
        return this;
    }
}

/// <summary>
/// Writes the page's few sentences (works-report.md §6). The facts it is given
/// are already filtered to the reader's side; it may describe them, never add to them.
/// </summary>
public interface IReportNarrator
{
    string Engine { get; }

    /// <summary>True when this narrator can run at all on this server.</summary>
    bool IsAvailable { get; }

    Task<List<NarrativeTargetDto>> DraftAsync(IReadOnlyList<NarrativeRequest> targets, CancellationToken ct = default);
}

/// <summary>The fallback that is always there: the sentences built from the facts themselves.</summary>
public sealed class TemplateReportNarrator : IReportNarrator
{
    public string Engine => "template";
    public bool IsAvailable => true;

    public Task<List<NarrativeTargetDto>> DraftAsync(IReadOnlyList<NarrativeRequest> targets, CancellationToken ct = default) =>
        Task.FromResult(targets.Select(t => new NarrativeTargetDto
        {
            TargetId = t.TargetId,
            Templated = true,
            Sentences = t.Template.Select(s => new NarrativeSentenceDto { Text = s.Text, SourceIds = s.SourceIds.ToList() }).ToList()
        }).ToList());
}

/// <summary>
/// The guard between a drafting model and the person paying (works-report.md §6.1).
/// A sentence is refused when it cites nothing, cites what it was not given,
/// carries a number, date or month the facts do not, forecasts, sells or
/// blames beyond the record, or runs over its word budget.
/// </summary>
public static class NarrativeValidator
{
    private static readonly Regex Number = new(@"\d+(?:[.,]\d+)*", RegexOptions.CultureInvariant);
    private static readonly Regex Word = new(@"[A-Za-z']+", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> Months = new(StringComparer.OrdinalIgnoreCase)
    {
        "jan", "january", "feb", "february", "mar", "march", "apr", "april", "may", "jun", "june", "jul", "july",
        "aug", "august", "sep", "sept", "september", "oct", "october", "nov", "november", "dec", "december",
        "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday", "tomorrow", "yesterday"
    };

    private static readonly HashSet<string> NumberWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve",
        "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty", "thirty",
        "forty", "fifty", "sixty", "seventy", "eighty", "ninety", "hundred", "hundreds", "thousand", "thousands",
        "million", "millions", "billion", "billions", "dozen", "dozens", "half", "halfway", "quarter", "double",
        "doubled", "twice", "triple", "tripled", "fortnight", "fortnights", "percent"
    };

    /// <summary>
    /// Words that forecast, promise, sell or blame. Allowed only when the
    /// record itself says them — "complete this week" is a quote, not a claim.
    /// </summary>
    private static readonly string[] Forbidden =
    {
        "will", "shall", "going to", "expect", "expected", "expects", "likely", "should", "soon", "promise to",
        "guarantee", "guaranteed", "on track", "on schedule", "ahead of schedule", "great", "excellent",
        "fantastic", "impressive", "good progress", "well done", "amazing", "fault", "blame", "blamed",
        "responsible for the delay", "negligent", "unfortunately", "hopefully"
    };

    public static List<NarrativeSentenceDto> Accept(NarrativeRequest target, NarrativeTargetDto? draft, List<string> reasons)
    {
        if (draft is null || draft.Sentences.Count == 0)
        {
            reasons.Add($"{target.TargetId}: nothing drafted");
            return new();
        }
        if (draft.Sentences.Count > target.MaxSentences)
        {
            reasons.Add($"{target.TargetId}: {draft.Sentences.Count} sentences, the budget is {target.MaxSentences}");
            return new();
        }

        var corpus = string.Join("\n", target.Facts.Concat(target.Snippets.Select(s => s.Text)));
        var numbers = Number.Matches(corpus).Select(m => Norm(m.Value)).ToHashSet();
        var lowerCorpus = corpus.ToLowerInvariant();
        var corpusWords = Word.Matches(lowerCorpus).Select(m => m.Value).ToHashSet();

        var words = draft.Sentences.Sum(s => Word.Matches(s.Text).Count + Number.Matches(s.Text).Count);
        if (words > target.MaxWords)
        {
            reasons.Add($"{target.TargetId}: {words} words, the budget is {target.MaxWords}");
            return new();
        }

        var accepted = new List<NarrativeSentenceDto>();
        foreach (var s in draft.Sentences)
        {
            var why = Refuse(s, target, numbers, corpusWords, lowerCorpus);
            if (why is not null)
            {
                reasons.Add($"{target.TargetId}: {why} — \"{s.Text}\"");
                return new();
            }
            accepted.Add(s);
        }
        return accepted;
    }

    private static string? Refuse(NarrativeSentenceDto s, NarrativeRequest target, HashSet<string> numbers,
        HashSet<string> corpusWords, string lowerCorpus)
    {
        if (string.IsNullOrWhiteSpace(s.Text)) return "empty sentence";
        if (s.SourceIds.Count == 0) return "cites no source";
        var stray = s.SourceIds.FirstOrDefault(id => !target.SourceIds.Contains(id));
        if (stray is not null) return $"cites {stray}, which it was not given";

        foreach (Match m in Number.Matches(s.Text))
            if (!numbers.Contains(Norm(m.Value))) return $"the figure {m.Value} is not in the facts";

        var lower = s.Text.ToLowerInvariant();
        foreach (Match m in Word.Matches(lower))
            if (Months.Contains(m.Value) && !corpusWords.Contains(m.Value)) return $"the date word '{m.Value}' is not in the facts";

        // "Two weeks late" is a figure as much as "14 days late" is (works-report.md §6.1).
        foreach (Match m in Word.Matches(lower))
            if (NumberWords.Contains(m.Value) && !corpusWords.Contains(m.Value)) return $"the figure '{m.Value}' is not in the facts";

        foreach (var f in Forbidden)
            if (Regex.IsMatch(lower, $@"\b{Regex.Escape(f)}\b") && !Regex.IsMatch(lowerCorpus, $@"\b{Regex.Escape(f)}\b"))
                return $"'{f}' forecasts, sells or blames beyond the record";
        if (s.Text.Contains('!')) return "an exclamation";
        return null;
    }

    private static string Norm(string n)
    {
        var t = n.Replace(",", "");
        return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
            ? d.ToString("0.####", CultureInfo.InvariantCulture) : t;
    }
}
