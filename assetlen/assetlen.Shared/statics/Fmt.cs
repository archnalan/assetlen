using System.Globalization;

namespace assetlen.Shared.statics;

/// <summary>
/// Display formatting, in one place.
/// <para>
/// The figures on this product are Ugandan shillings and they are large — a
/// stage runs to nine digits. Two forms are needed and they must never be
/// mixed up: a <b>compact</b> form for glanceable positions, and an
/// <b>exact</b> form for anything a reader might reconcile against a bank
/// statement. Rounding a number somebody is checking is how a ledger loses its
/// authority.
/// </para>
/// </summary>
public static class Fmt
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Exact, grouped, no decimals: <c>UGX 450,000,000</c>. Use in ledgers and totals.</summary>
    public static string Money(decimal amount, string? currency = "UGX")
        => $"{currency} {amount.ToString("N0", Inv)}".Trim();

    /// <summary>Exact figure with no currency prefix, for table columns that carry the unit in the header.</summary>
    public static string Amount(decimal amount) => amount.ToString("N0", Inv);

    /// <summary>
    /// Compact: <c>UGX 450M</c>. Only for cards and stat tiles where the exact
    /// figure is one tap away. Never inside a reconciliation view.
    /// </summary>
    public static string MoneyShort(decimal amount, string? currency = "UGX")
    {
        var sign = amount < 0 ? "-" : "";
        var v = Math.Abs(amount);

        var body = v switch
        {
            >= 1_000_000_000 => (v / 1_000_000_000m).ToString("0.##", Inv) + "B",
            >= 1_000_000     => (v / 1_000_000m).ToString("0.##", Inv) + "M",
            >= 1_000         => (v / 1_000m).ToString("0.#", Inv) + "K",
            _                => v.ToString("0", Inv)
        };

        return string.IsNullOrEmpty(currency) ? sign + body : $"{currency} {sign}{body}";
    }

    public static string Percent(decimal value) => value.ToString("0.#", Inv) + "%";

    /// <summary>Absolute date: <c>21 May 2026</c>. Dates on commitments are evidence and are never relative.</summary>
    public static string Date(DateTime? d) => d is null ? "—" : d.Value.ToLocalTime().ToString("d MMM yyyy", Inv);

    public static string DateShort(DateTime? d) => d is null ? "—" : d.Value.ToLocalTime().ToString("d MMM", Inv);

    public static string DateTimeFull(DateTime? d)
        => d is null ? "—" : d.Value.ToLocalTime().ToString("d MMM yyyy, HH:mm", Inv);

    /// <summary>
    /// Relative, for "what moved" surfaces only. Anything older than a week
    /// falls back to the absolute date — "43 days ago" is not a date anyone can
    /// hold a commitment against.
    /// </summary>
    public static string Ago(DateTime? d)
    {
        if (d is null) return "—";

        var span = DateTime.UtcNow - d.Value.ToUniversalTime();

        if (span.TotalSeconds < 60) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} h ago";
        if (span.TotalDays < 2) return "yesterday";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";

        return Date(d);
    }

    /// <summary>Floor area, always with the unit — it is what the bill is computed from.</summary>
    public static string Area(decimal? sqm)
        => sqm is null or 0 ? "not declared" : $"{sqm.Value.ToString("N0", Inv)} m²";

    /// <summary>Initials for an avatar. Falls back to the first letter of whatever is present.</summary>
    public static string Initials(string? first, string? last, string? fallback = null)
    {
        var a = first?.Trim();
        var b = last?.Trim();

        var initials =
            (a is { Length: > 0 } ? a[0].ToString() : "") +
            (b is { Length: > 0 } ? b[0].ToString() : "");

        if (initials.Length > 0) return initials.ToUpperInvariant();

        var f = fallback?.Trim();
        return f is { Length: > 0 } ? f[0].ToString().ToUpperInvariant() : "?";
    }

    /// <summary>Splits an enum name at its capitals: <c>QueryRaised</c> → <c>Query raised</c>.</summary>
    public static string Humanise(string? pascal)
    {
        if (string.IsNullOrWhiteSpace(pascal)) return "";

        var sb = new System.Text.StringBuilder(pascal.Length + 4);
        for (var i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(pascal[i - 1])) sb.Append(' ').Append(char.ToLowerInvariant(c));
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
