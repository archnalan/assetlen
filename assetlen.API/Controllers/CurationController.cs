using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>
/// Curation by exception (plan.md P9). The role gate is coarse; the real check is
/// the mediator's standing on the project, resolved in <see cref="ICurationDAL"/>.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CurationController : ControllerBase
{
    private readonly ICurationDAL _dal;
    private readonly ITenantProvider _tenant;
    private readonly IWebHostEnvironment _env;

    public CurationController(ICurationDAL dal, ITenantProvider tenant, IWebHostEnvironment env)
    {
        _dal = dal;
        _tenant = tenant;
        _env = env;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CurationDraftDto), 200)]
    public async Task<ActionResult> Draft([FromQuery][Required] string projectId, [FromQuery] DateTime? day, CancellationToken ct)
    {
        var r = await _dal.GetDraft(projectId, day, _tenant.GetUserId(), ct);
        return r.IsSuccess ? Ok(r.Data) : StatusCode(r.StatusCode, r.Error.Message);
    }

    [HttpPut]
    [ProducesResponseType(typeof(CurationDraftDto), 200)]
    public async Task<ActionResult> Mark([FromBody] CurationMarkDto dto, CancellationToken ct)
    {
        var r = await _dal.Mark(dto, _tenant.GetUserId(), ct);
        return r.IsSuccess ? Ok(r.Data) : StatusCode(r.StatusCode, r.Error.Message);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CurationPublishResultDto), 200)]
    public async Task<ActionResult> Publish([FromQuery][Required] string projectId, [FromQuery] DateTime? day, CancellationToken ct)
    {
        var r = await _dal.Publish(projectId, day, _tenant.GetUserId(), ct);
        return r.IsSuccess ? Ok(r.Data) : StatusCode(r.StatusCode, r.Error.Message);
    }

    /// <summary>
    /// Development only: run the cutoff as if the clock said <paramref name="now"/>,
    /// with nobody's authority but the schedule's. Stands in for waiting until 20:00.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> RunCutoff([FromQuery] DateTime? now, [FromQuery] string? projectId, CancellationToken ct)
    {
        if (!_env.IsDevelopment()) return NotFound();
        return Ok(await _dal.RunCutoffAsync(now, projectId, ct));
    }
}
