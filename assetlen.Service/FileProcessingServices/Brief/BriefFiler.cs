using System.Text.RegularExpressions;
using assetlen.Service.FileProcessingServices.Extraction;

namespace assetlen.Service.FileProcessingServices.Brief;

public sealed record DeliverableRef(string Id, string StageId, string Title);

public readonly record struct Filing(string? StageId, string? DeliverableId)
{
    public static readonly Filing None = new(null, null);
    public bool IsFiled => StageId is not null;
}

/// <summary>
/// Files a line of the thread under the deliverable it is about, so the brief can
/// be grouped by the work rather than by posting order (assetlen.md §5). Only an
/// unambiguous match files to a deliverable; otherwise the stage, otherwise
/// nothing — a brief that files a photo under the wrong funded item is worse than
/// one that says it could not tell.
/// </summary>
public sealed class BriefFiler
{
    private static readonly HashSet<string> Stop = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "works", "work", "stage", "phase", "of", "to", "in", "on", "main", "house",
        "all", "are", "was", "has", "have", "been", "this", "that", "from", "into", "onto", "will", "done",
        "today", "tomorrow", "complete", "completed", "site", "its", "our", "your", "part", "first", "second"
    };

    private readonly IReadOnlyList<StageRef> _stages;
    private readonly List<(DeliverableRef D, List<string> Parts)> _deliverables;

    public BriefFiler(IReadOnlyList<StageRef> stages, IReadOnlyList<DeliverableRef> deliverables)
    {
        _stages = stages;
        _deliverables = deliverables
            .Select(d => (d, Tokens(d.Title).Where(p => p.Length >= 3 && !Stop.Contains(p)).Distinct().ToList()))
            .Where(x => x.Item2.Count > 0)
            .ToList();
    }

    public Filing File(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Filing.None;
        var words = Tokens(text);

        (DeliverableRef D, int Score)? best = null;
        var tie = new List<DeliverableRef>();

        foreach (var (d, parts) in _deliverables)
        {
            var matched = parts.Where(p => Has(words, p)).ToList();

            // Two words of the title, or the whole of a one-word title. One shared
            // word — "wall", "plaster" — names a trade, not a deliverable.
            if (matched.Count < Math.Min(2, parts.Count)) continue;

            var score = matched.Sum(p => p.Length) * 10 + (matched.Count == parts.Count ? 5 : 0);
            if (best is null || score > best.Value.Score)
            {
                best = (d, score);
                tie.Clear();
            }
            else if (score == best.Value.Score)
            {
                tie.Add(d);
            }
        }

        if (best is { } b)
        {
            if (tie.Count == 0) return new Filing(b.D.StageId, b.D.Id);

            // Tied deliverables on one stage still name the stage unambiguously.
            if (tie.All(t => t.StageId == b.D.StageId)) return new Filing(b.D.StageId, null);
        }

        var stage = StageMatcher.Match(text, _stages);
        return stage is null ? Filing.None : new Filing(stage, null);
    }

    private static bool Has(List<string> words, string part)
    {
        var stem = part.Length > 5 ? part[..5] : part;
        return words.Any(w => w.StartsWith(stem, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> Tokens(string text) =>
        Regex.Split(text.ToLowerInvariant(), @"[^a-z0-9]+").Where(t => t.Length > 0).ToList();
}
