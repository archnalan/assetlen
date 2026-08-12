using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

/// <summary>
/// A project's size and what it bills at. Billing is **per project, by area** —
/// never per seat.
/// </summary>
public class ProjectSizingDto
{
    public string? ProjectId { get; set; }

    /// <summary>The project that actually bills — the top-level parent.</summary>
    public string? BillableProjectId { get; set; }

    /// <summary>This project's own declared area, excluding sub-projects.</summary>
    public decimal? OwnAreaSqm { get; set; }

    /// <summary>The billable project's area including every sub-project.</summary>
    public decimal? TotalAreaSqm { get; set; }

    public ProjectSizeTier Tier { get; set; }
    public ProjectSizeSource Source { get; set; }

    /// <summary>e.g. <c>"250–750 m²"</c>. For the project header and the pricing page.</summary>
    public string? Band { get; set; }

    /// <summary>
    /// Set when the measured area implies a **higher** tier than the project is
    /// billing at. The increase is not applied until someone confirms it, so a
    /// bill never rises without a person saying yes.
    /// </summary>
    public ProjectSizeTier? PendingTier { get; set; }

    public bool RequiresConfirmation => PendingTier is not null;

    /// <summary>Band the pending tier would move the project into.</summary>
    public string? PendingBand { get; set; }

    public string? ConfirmedById { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>Sub-projects contributing to the roll-up, for the breakdown.</summary>
    public List<ProjectAreaContributionDto> Contributions { get; set; } = new();
}

public class ProjectAreaContributionDto
{
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public decimal? AreaSqm { get; set; }
    public bool IsParent { get; set; }
}

public class ProjectAreaUpdateDto
{
    [Required]
    public string? ProjectId { get; set; }

    /// <summary>Gross internal floor area in square metres. Null clears it.</summary>
    [Range(0, 1_000_000)]
    public decimal? FloorAreaSqm { get; set; }

    /// <summary>
    /// Where the number came from. <c>DerivedFromDrawing</c> is reserved for the
    /// drawing-reading step and must not be set by a client.
    /// </summary>
    public ProjectSizeSource Source { get; set; } = ProjectSizeSource.Declared;
}
