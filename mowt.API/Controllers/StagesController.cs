using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using mowt.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace mowt.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Investor},{UserRoles.ProjectManager}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class StagesController : ControllerBase
{
    private readonly IStageDAL _stageDAL;
    private readonly ITenantProvider _tenantProvider;

    public StagesController(IStageDAL stageDAL, ITenantProvider tenantProvider)
    {
        _stageDAL = stageDAL;
        _tenantProvider = tenantProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(StageDto), 200)]
    public async Task<ActionResult> CreateStage(
        [FromQuery][Required] string projectId,
        [FromBody] StageCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _stageDAL.CreateStage(projectId, dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(StageDto), 200)]
    public async Task<ActionResult> UpdateStage([FromBody] StageDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _stageDAL.UpdateStage(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = UserRoles.Investor, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeleteStage([FromQuery][Required] string stageId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _stageDAL.DeleteStage(stageId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<StageDto>), 200)]
    public async Task<ActionResult> GetStagesByProjectId([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _stageDAL.GetStagesByProjectId(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(StageDto), 200)]
    public async Task<ActionResult> GetStageById([FromQuery][Required] string stageId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _stageDAL.GetStageById(stageId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
