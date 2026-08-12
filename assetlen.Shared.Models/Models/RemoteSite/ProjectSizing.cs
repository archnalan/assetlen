namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// Billing tier for a project, derived from its floor area.
/// <para>
/// Assetlen bills the developer <b>per project, by size</b> — never per seat.
/// Seat-based pricing would make Peter's bill grow every time his contractor
/// hired a labourer, which punishes exactly the behaviour the product needs
/// (get the whole delivery side capturing). Area is contractor-independent,
/// verifiable from the drawings, and does not move when the crew does.
/// </para>
/// </summary>
public enum ProjectSizeTier
{
    /// <summary>A single modest residence.</summary>
    Small = 0,

    /// <summary>A substantial residence, or one with a secondary building.</summary>
    Medium = 1,

    /// <summary>Multiple buildings, multi-unit or commercial development.</summary>
    Large = 2
}

/// <summary>How a project's floor area came to be known. Drives how much we trust it.</summary>
public enum ProjectSizeSource
{
    /// <summary>Nobody has said. The project bills at the lowest tier until it does.</summary>
    Unknown = 0,

    /// <summary>Typed in by the developer at project creation.</summary>
    Declared = 1,

    /// <summary>Read off an uploaded drawing. Reserved — see <c>ProjectSizingPolicy</c>.</summary>
    DerivedFromDrawing = 2,

    /// <summary>Set by hand after a dispute or a re-measure. Never overwritten automatically.</summary>
    Manual = 3
}

/// <summary>
/// The one place tier thresholds live. Changing a boundary here changes what
/// every project is billed, so it does not get duplicated into a controller,
/// a seed file or a pricing page.
/// </summary>
public static class ProjectSizingPolicy
{
    /// <summary>Upper bound of <see cref="ProjectSizeTier.Small"/>, in square metres, inclusive.</summary>
    public const decimal SmallMaxSqm = 250m;

    /// <summary>Upper bound of <see cref="ProjectSizeTier.Medium"/>, in square metres, inclusive.</summary>
    public const decimal MediumMaxSqm = 750m;

    /// <summary>
    /// Tier for a given total floor area. A project with no declared area bills
    /// at <see cref="ProjectSizeTier.Small"/> — we do not guess upward, because
    /// a wrong guess in our favour is the fastest way to lose the account.
    /// </summary>
    public static ProjectSizeTier TierFor(decimal? totalFloorAreaSqm) => totalFloorAreaSqm switch
    {
        null or <= 0 => ProjectSizeTier.Small,
        <= SmallMaxSqm => ProjectSizeTier.Small,
        <= MediumMaxSqm => ProjectSizeTier.Medium,
        _ => ProjectSizeTier.Large
    };

    /// <summary>Human-readable band, for the pricing page and the project header.</summary>
    public static string DescribeBand(ProjectSizeTier tier) => tier switch
    {
        ProjectSizeTier.Small => $"up to {SmallMaxSqm:N0} m²",
        ProjectSizeTier.Medium => $"{SmallMaxSqm:N0}–{MediumMaxSqm:N0} m²",
        _ => $"over {MediumMaxSqm:N0} m²"
    };

    /// <summary>
    /// A tier change only ever takes effect through this check, so a project
    /// cannot silently jump a band mid-stage.
    /// <para>
    /// Moving <em>up</em> costs the developer money and must be surfaced before
    /// it bills. Moving <em>down</em> is applied immediately — we never keep
    /// billing a higher tier than the drawings justify.
    /// </para>
    /// </summary>
    public static bool RequiresConfirmation(ProjectSizeTier current, ProjectSizeTier proposed) =>
        proposed > current;
}
