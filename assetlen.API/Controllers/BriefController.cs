using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;

namespace assetlen.API.Controllers;

/// <summary>
/// Peter's surfaces (plan.md P7): the home across every project, what he owes,
/// and the daily brief. Every role reaches the controller; standing on each
/// project decides what comes back.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BriefController : ControllerBase
{
    private readonly IBriefDAL _dal;
    private readonly ITenantProvider _tenant;

    public BriefController(IBriefDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HomeDto), 200)]
    public async Task<ActionResult> Home([FromQuery] int days = 7, CancellationToken ct = default)
    {
        var result = await _dal.GetHomeAsync(_tenant.GetUserId(), days, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OwedItemDto>), 200)]
    public async Task<ActionResult> Owed(CancellationToken ct = default)
    {
        var result = await _dal.GetOwedAsync(_tenant.GetUserId(), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DailyBriefDto), 200)]
    public async Task<ActionResult> Day([FromQuery] string projectId, [FromQuery] DateTime? day, [FromQuery] int days = 1, CancellationToken ct = default)
    {
        var result = await _dal.GetBriefAsync(projectId, day, days, _tenant.GetUserId(), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);
    }
}
