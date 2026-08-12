using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// Owns a project's floor area and the billing tier that follows from it.
/// <para>
/// Assetlen bills the developer **per project, by size** — never per seat, so
/// Peter's bill does not grow when his contractor hires a labourer. Area is
/// contractor-independent and verifiable from the drawings.
/// </para>
/// <para>
/// The billable unit is the **top-level project**. A guest wing is a
/// sub-project: it enlarges the parent's area rather than becoming a second
/// invoice.
/// </para>
/// </summary>
public interface IProjectSizingService
{
    /// <summary>Current area, tier, band and whether a change is awaiting confirmation.</summary>
    Task<ServiceResult<ProjectSizingDto>> GetAsync(string projectId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Declare or correct the floor area of one project (or sub-project) and
    /// recompute the billable parent's tier.
    /// <para>
    /// A tier that moves **up** costs money, so it is proposed rather than
    /// applied: the response carries <c>RequiresConfirmation</c> and the tier is
    /// unchanged until <see cref="ConfirmTierAsync"/>. A tier that moves
    /// **down** applies immediately — we never keep billing a higher band than
    /// the measurements justify.
    /// </para>
    /// </summary>
    Task<ServiceResult<ProjectSizingDto>> SetAreaAsync(
        ProjectAreaUpdateDto dto, string userId, CancellationToken ct = default);

    /// <summary>Accept a pending tier increase. Owner or manager only — it changes the bill.</summary>
    Task<ServiceResult<ProjectSizingDto>> ConfirmTierAsync(
        string projectId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Recompute the billable parent's rolled-up area and tier after a
    /// sub-project is added, removed or resized. Never applies an upgrade
    /// silently — it reports one as pending.
    /// </summary>
    Task<ServiceResult<ProjectSizingDto>> RecomputeAsync(
        string projectId, string userId, CancellationToken ct = default);
}
