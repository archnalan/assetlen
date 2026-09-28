using System.Text.RegularExpressions;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices.Extraction;

public sealed record StageRef(string Id, string Name, string? CatalogueKey);

/// <summary>
/// Suggests which of a project's stages a sentence is about — "Guest wing
/// interior plastering" to the stage called "Guest wing plaster". Only an
/// unambiguous best match is returned; a guess would file a commitment against
/// the wrong funded stage, and the active stage is a safer default than that.
/// </summary>
public static class StageMatcher
{
    private static readonly HashSet<string> Stop = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "works", "work", "stage", "phase", "of", "to", "in", "on", "main", "house"
    };

    public static string? Match(string? text, IReadOnlyList<StageRef> stages)
    {
        if (string.IsNullOrWhiteSpace(text) || stages.Count == 0) return null;

        var words = Tokens(text);
        (string Id, int Score)? best = null;
        var tie = false;

        foreach (var stage in stages)
        {
            var score = Score(stage.Name, words);
            var item = StageCatalogue.Find(stage.CatalogueKey);
            if (item is { } known)
            {
                score = Math.Max(score, Score(known.Name, words));
                foreach (var alias in known.Aliases)
                    score = Math.Max(score, Score(alias, words));
            }

            if (score == 0) continue;
            if (best is null || score > best.Value.Score)
            {
                best = (stage.Id, score);
                tie = false;
            }
            else if (score == best.Value.Score)
            {
                tie = true;
            }
        }

        return best is null || tie ? null : best.Value.Id;
    }

    /// <summary>Every meaningful word of the name must appear (as a word or its stem) in the text.</summary>
    private static int Score(string name, List<string> words)
    {
        var parts = Tokens(name).Where(p => p.Length >= 3 && !Stop.Contains(p)).ToList();
        if (parts.Count == 0) return 0;

        foreach (var part in parts)
        {
            var stem = part.Length > 5 ? part[..5] : part;
            if (!words.Any(w => w.StartsWith(stem, StringComparison.OrdinalIgnoreCase))) return 0;
        }

        return parts.Sum(p => p.Length);
    }

    private static List<string> Tokens(string text) =>
        Regex.Split(text.ToLowerInvariant(), @"[^a-z0-9]+").Where(t => t.Length > 0).ToList();
}
