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
    private readonly ITenantProvider _tenantProvider;

    public ProjectsRSController(IProjectDAL projectDAL, ITenantProvider tenantProvider)
    {
        _projectDAL = projectDAL;
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
}
