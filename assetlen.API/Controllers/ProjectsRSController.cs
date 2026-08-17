using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
// Outer gate matches the Projects row in the §5.5 access matrix —
// every signed-in ASSETLEN role can READ; write paths are narrowed
// per-action below.
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProjectsRSController : ControllerBase
{
    private readonly IProjectDAL _projectDAL;
    private readonly IProjectSizingService _sizing;
    private readonly ITenantProvider _tenantProvider;

    public ProjectsRSController(
        IProjectDAL projectDAL,
        IProjectSizingService sizing,
        ITenantProvider tenantProvider)
    {
        _projectDAL = projectDAL;
        _sizing = sizing;
        _tenantProvider = tenantProvider;
    }

    // ─── Portfolio Dashboard ──────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(PortfolioSummaryDto), 200)]
    public async Task<ActionResult> GetPortfolioDashboard()
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.GetPortfolioDashboard(userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── CRUD ─────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> CreateProject([FromBody] ProjectCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.CreateProject(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProjectDto), 200)]
    public async Task<ActionResult> GetProjectById([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.GetProjectById(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProjectDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> UpdateProject([FromBody] ProjectDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.UpdateProject(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Empty one project out of the bin ahead of its thirty days. Rejected
    /// unless the project is already archived — deletion goes through the bin so
    /// it can be undone.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = UserRoles.Contractor,
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeleteProject([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.DeleteProject(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── The bin ──────────────────────────────────────────────
    // Archiving is what the delete gesture does. Thirty days later a sweep
    // soft-deletes the contents; until then the project is one call from being
    // back on the rail.

    [HttpPut]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> ArchiveProject([FromQuery][Required] string projectId)
    {
        var result = await _projectDAL.ArchiveProject(projectId, _tenantProvider.GetUserId());
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> RestoreProject([FromQuery][Required] string projectId)
    {
        var result = await _projectDAL.RestoreProject(projectId, _tenantProvider.GetUserId());
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectCardDto>), 200)]
    public async Task<ActionResult> GetArchivedProjects()
    {
        var result = await _projectDAL.GetArchivedProjects(_tenantProvider.GetUserId());
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── This reader's arrangement ────────────────────────────
    // Ordering and pins are per user. Every signed-in role may arrange their
    // own screen — a guest who cannot change a figure can still decide which
    // project they want to see first.

    [HttpPut]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult> ReorderProjects([FromBody] ProjectOrderUpdateDto dto)
    {
        var result = await _projectDAL.ReorderProjects(dto, _tenantProvider.GetUserId());
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult> SetProjectPinned(
        [FromQuery][Required] string projectId, [FromQuery] bool pinned)
    {
        var result = await _projectDAL.SetProjectPinned(projectId, pinned, _tenantProvider.GetUserId());
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Give a project a face, or clear it. A cleared sub-project falls back to
    /// its parent's cover; a cleared top-level project falls back to its mark.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProjectDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetProjectCover([FromBody] ProjectCoverUpdateDto dto)
    {
        var result = await _projectDAL.SetProjectCover(dto, _tenantProvider.GetUserId());
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── Search ───────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(PaginationDetails<ProjectCardDto>), 200)]
    public async Task<ActionResult> SearchProjects(
        [FromQuery] string? keywords,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 12,
        [FromQuery] string? sortByColumn = null,
        [FromQuery] bool sortAscending = false,
        CancellationToken ct = default)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.SearchProjects(userId, keywords, offset, limit, sortByColumn, sortAscending, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── PM Assignment ────────────────────────────────────────

    [HttpPut]
    [ProducesResponseType(typeof(ProjectDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AssignProjectManager(
        [FromQuery][Required] string projectId,
        [FromQuery][Required] string managerId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.AssignProjectManager(projectId, managerId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── Timeline ─────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<TimelineEntryDto>), 200)]
    public async Task<ActionResult> GetProjectTimeline([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.GetProjectTimeline(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── Analytics ────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(ProjectAnalyticsDto), 200)]
    public async Task<ActionResult> GetProjectAnalytics([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _projectDAL.GetProjectAnalytics(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── Sizing and billing tier ──────────────────────────────
    // Assetlen bills the developer per project, by floor area — never per
    // seat, so the bill does not grow when the contractor hires a labourer.

    [HttpGet]
    [ProducesResponseType(typeof(ProjectSizingDto), 200)]
    public async Task<ActionResult> GetProjectSizing([FromQuery][Required] string projectId, CancellationToken ct)
    {
        var result = await _sizing.GetAsync(projectId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Declare or correct floor area. A tier that moves **up** comes back as
    /// <c>PendingTier</c> and is not applied until <see cref="ConfirmProjectTier"/>;
    /// a tier that moves down applies at once.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProjectSizingDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetProjectArea([FromBody] ProjectAreaUpdateDto dto, CancellationToken ct)
    {
        var result = await _sizing.SetAreaAsync(dto, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>Accept a pending tier increase. It changes the bill, so a person says yes.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProjectSizingDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> ConfirmProjectTier([FromQuery][Required] string projectId, CancellationToken ct)
    {
        var result = await _sizing.ConfirmTierAsync(projectId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
