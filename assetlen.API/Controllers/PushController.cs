using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>
/// Web push for this person's devices (assetlen.md §9 — notification speed equal
/// to WhatsApp). Which events may wake them is decided per project by standing.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PushController : ControllerBase
{
    private readonly IPushDAL _dal;
    private readonly ITenantProvider _tenant;

    public PushController(IPushDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PushStatusDto), 200)]
    public async Task<ActionResult> Status(CancellationToken ct) => Ok(await _dal.GetStatus(_tenant.GetUserId(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(PushStatusDto), 200)]
    public async Task<ActionResult> Subscribe([FromBody] PushSubscriptionCreateDto dto, CancellationToken ct)
    {
        var r = await _dal.Subscribe(dto, _tenant.GetUserId(), ct);
        return r.IsSuccess ? Ok(r.Data) : StatusCode(r.StatusCode, r.Error.Message);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(PushStatusDto), 200)]
    public async Task<ActionResult> Unsubscribe([FromQuery][Required] string endpoint, CancellationToken ct)
    {
        var r = await _dal.Unsubscribe(endpoint, _tenant.GetUserId(), ct);
        return r.IsSuccess ? Ok(r.Data) : StatusCode(r.StatusCode, r.Error.Message);
    }

    [HttpPost]
    public async Task<ActionResult> Test(CancellationToken ct) => Ok(new { queued = await _dal.SendTest(_tenant.GetUserId(), ct) });

    [HttpGet]
    [ProducesResponseType(typeof(List<PushDeliveryDto>), 200)]
    public async Task<ActionResult> Deliveries([FromQuery] int take = 20, CancellationToken ct = default) =>
        Ok(await _dal.GetMyDeliveries(take, _tenant.GetUserId(), ct));
}
