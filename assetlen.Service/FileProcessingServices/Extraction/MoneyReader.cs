using System.Globalization;
using System.Text.RegularExpressions;

namespace assetlen.Service.FileProcessingServices.Extraction;

/// <summary>A figure found in text, normalised to units, with the words it was read from.</summary>
public sealed record MoneyHit(decimal Amount, string Currency, string Text, bool Explicit);

/// <summary>
/// Reads money out of how it is actually written on a site thread — "UGX 12M",
/// "12M", "UGX 100 M", "5,250,000/=". A figure is the one thing extraction must
/// never invent, so every amount a proposal carries comes from here and nowhere else.
/// </summary>
public static class MoneyReader
{
    private const RegexOptions Opts = RegexOptions.Compiled | RegexOptions.CultureInvariant;

    private static readonly Regex WithCurrency = new(
        @"(?<cur>(?i:ugx|ushs|ush|shs|usd|kes|ksh)|US\$|\$)\s?(?<num>\d{1,3}(?:,\d{3})+(?:\.\d+)?|\d+(?:\.\d+)?)\s?(?<mul>million|Million|mn|M|m|k|K|bn|B)?\b",
        Opts);

    /// <summary>Bare "12M". Uppercase or spelled out only — a lowercase "m" on a site is metres.</summary>
    private static readonly Regex Bare = new(
        @"(?<![\w.,])(?<num>\d+(?:\.\d+)?)\s?(?<mul>M|million|Million|mn)\b(?!\s*(?:m2|m3|²|³))",
        Opts);

    /// <summary>"53,823,000" with no currency — only ever read where the sentence is about money.</summary>
    private static readonly Regex Grouped = new(@"(?<![\d,.])(?<num>\d{1,3}(?:,\d{3}){2,})(?![\d,])", Opts);

    private static readonly Regex Slashed = new(
        @"(?<num>\d{1,3}(?:,\d{3})+)\s*(?:/=|/-)", Opts);

    public static List<MoneyHit> FindAll(string text)
    {
        var hits = new List<MoneyHit>();
        if (string.IsNullOrWhiteSpace(text)) return hits;

        var taken = new List<(int Start, int End)>();

        foreach (Match m in WithCurrency.Matches(text))
        {
            var amount = Scale(m.Groups["num"].Value, m.Groups["mul"].Value);
            if (amount is null or <= 0) continue;
            hits.Add(new MoneyHit(amount.Value, CurrencyCode(m.Groups["cur"].Value), m.Value.Trim(), true));
            taken.Add((m.Index, m.Index + m.Length));
        }

        foreach (Match m in Bare.Matches(text))
        {
            if (taken.Any(t => m.Index >= t.Start && m.Index < t.End)) continue;
            var amount = Scale(m.Groups["num"].Value, m.Groups["mul"].Value);
            if (amount is null or <= 0) continue;
            hits.Add(new MoneyHit(amount.Value, "UGX", m.Value.Trim(), false));
            taken.Add((m.Index, m.Index + m.Length));
        }

        foreach (Match m in Slashed.Matches(text))
        {
            if (taken.Any(t => m.Index >= t.Start && m.Index < t.End)) continue;
            var amount = Scale(m.Groups["num"].Value, "");
            if (amount is null or <= 0) continue;
            hits.Add(new MoneyHit(amount.Value, "UGX", m.Value.Trim(), false));
            taken.Add((m.Index, m.Index + m.Length));
        }

        foreach (Match m in Grouped.Matches(text))
        {
            if (taken.Any(t => m.Index >= t.Start && m.Index < t.End)) continue;
            var amount = Scale(m.Groups["num"].Value, "");
            if (amount is null or <= 0) continue;
            hits.Add(new MoneyHit(amount.Value, "UGX", m.Value.Trim(), false));
        }

        return hits;
    }

    private static decimal? Scale(string number, string multiplier)
    {
        if (!decimal.TryParse(number.Replace(",", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return null;

        return multiplier switch
        {
            "M" or "m" or "million" or "Million" or "mn" => value * 1_000_000m,
            "k" or "K" => value * 1_000m,
            "bn" or "B" => value * 1_000_000_000m,
            _ => value
        };
    }

    private static string CurrencyCode(string written) => written.ToUpperInvariant() switch
    {
        "USD" or "US$" or "$" => "USD",
        "KES" or "KSH" => "KES",
        _ => "UGX"
    };
}
