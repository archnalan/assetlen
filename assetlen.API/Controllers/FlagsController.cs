using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FlagsController : ControllerBase
{
    private readonly IFlagDAL _dal;
    private readonly ITenantProvider _tenantProvider;

    public FlagsController(IFlagDAL dal, ITenantProvider tenantProvider)
    {
        _dal = dal;
        _tenantProvider = tenantProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FlagDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AddFlag([FromBody] FlagCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.AddFlag(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(FlagDto), 200)]
    public async Task<ActionResult> GetFlag([FromQuery][Required] string flagId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.GetFlag(flagId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FlagDto>), 200)]
    public async Task<ActionResult> GetFlagsByProject(
        [FromQuery][Required] string projectId,
        [FromQuery] FlagStatus? status = null)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.GetFlagsByProject(projectId, status, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FlagDto>), 200)]
    public async Task<ActionResult> GetFlagsByEntry([FromQuery][Required] string progressUpdateId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.GetFlagsByEntry(progressUpdateId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(FlagDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> UpdateFlag([FromBody] FlagUpdateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.UpdateFlag(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(FlagDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> ResolveFlag([FromQuery][Required] string flagId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.ResolveFlag(flagId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(FlagDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> NudgeFlag([FromQuery][Required] string flagId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.NudgeFlag(flagId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
