using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProgressController : ControllerBase
{
    private readonly IProgressDAL _progressDAL;
    private readonly IPMDashboardDAL _pmDashboardDAL;
    private readonly ITenantProvider _tenantProvider;

    public ProgressController(
        IProgressDAL progressDAL,
        IPMDashboardDAL pmDashboardDAL,
        ITenantProvider tenantProvider)
    {
        _progressDAL = progressDAL;
        _pmDashboardDAL = pmDashboardDAL;
        _tenantProvider = tenantProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AddProgressUpdate([FromBody] ProgressUpdateCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.AddProgressUpdate(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    public async Task<ActionResult> GetProgressUpdate([FromQuery][Required] string updateId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.GetProgressUpdate(updateId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetApprovalStatus([FromBody] ProgressApprovalDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.SetApprovalStatus(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetChannel(
        [FromQuery][Required] string updateId,
        [FromQuery][Required] Channel channel)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.SetChannel(updateId, channel, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Expose or withdraw individual frames on an entry — the mediator picks
    /// three of eighteen. The role gate here is coarse; the real check is
    /// <c>IProjectAccessService.CanExposeToClientAsync</c>, which admits the
    /// project's mediator whichever side they sit on.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetImageChannel([FromBody] ProgressImageExposureDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.SetImageChannel(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginationDetails<ProgressUpdateDto>), 200)]
    public async Task<ActionResult> GetProgressUpdates(
        [FromQuery][Required] string projectId,
        [FromQuery] string? stageId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.GetProgressUpdates(projectId, stageId, offset, limit, userId, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProgressCommentDto), 200)]
    public async Task<ActionResult> AddComment([FromBody] ProgressCommentCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.AddComment(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── PM Dashboard ─────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(PMDashboardDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> GetPMDashboard()
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _pmDashboardDAL.GetPMDashboard(userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
