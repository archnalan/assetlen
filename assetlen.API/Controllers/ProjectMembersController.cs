using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberDAL _dal;
    private readonly ITenantProvider _tenantProvider;

    public ProjectMembersController(IProjectMemberDAL dal, ITenantProvider tenantProvider)
    {
        _dal = dal;
        _tenantProvider = tenantProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectMemberDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AddMember([FromBody] ProjectMemberCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.AddMember(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectMemberDto>), 200)]
    public async Task<ActionResult> GetMembersByProject([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.GetMembersByProject(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeactivateMember([FromQuery][Required] string memberId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.DeactivateMember(memberId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
