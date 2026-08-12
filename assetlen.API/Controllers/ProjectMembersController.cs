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

    // Client is on the route gate deliberately. The developer who funds and owns
    // the project may hold the tenant-level Client role, and D1 says the project
    // is theirs — locking them out of staffing it would make the coarse gate
    // contradict the per-project authority. tbl_ProjectMember, resolved by
    // IProjectAccessService inside the DAL, is what actually decides.
    [HttpPost]
    [ProducesResponseType(typeof(ProjectMemberDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
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

    /// <summary>
    /// Change a member's side, mediator seat, specialization or title. The
    /// mediator cap (two) and the last-mediator rule are enforced in the DAL.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProjectMemberDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> UpdateMember([FromBody] ProjectMemberUpdateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.UpdateMember(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// The caller's own standing on this project. The UI reads it to decide
    /// which surface to render; the server never trusts it back.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProjectAccessDto), 200)]
    public async Task<ActionResult> GetMyStanding([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.GetMyStanding(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeactivateMember([FromQuery][Required] string memberId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.DeactivateMember(memberId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
