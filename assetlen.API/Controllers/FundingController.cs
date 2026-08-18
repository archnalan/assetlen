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
// Funding is on the Finance row of the §5.5 matrix: Contractor/Manager
// /Client read. Crew + Guest have no financial visibility.
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FundingController : ControllerBase
{
    private readonly IFundingDAL _fundingDAL;
    private readonly ITenantProvider _tenantProvider;

    public FundingController(IFundingDAL fundingDAL, ITenantProvider tenantProvider)
    {
        _fundingDAL = fundingDAL;
        _tenantProvider = tenantProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FundingEntryDto), 200)]
    // Client raises a deposit (capital coming in from the investor side);
    // Contractor/Manager record one on a client's behalf.
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AddFundingEntry([FromBody] FundingEntryCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _fundingDAL.AddFundingEntry(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(FundingEntryDto), 200)]
    // Confirmation lives with the project owner / PM — they're the ones
    // who acknowledge receipt of funds.
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> ConfirmFunding([FromBody] FundingConfirmDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _fundingDAL.ConfirmFunding(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FundingEntryDto>), 200)]
    public async Task<ActionResult> GetFundingByProject([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _fundingDAL.GetFundingByProject(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FundingEntryDto>), 200)]
    public async Task<ActionResult> GetFundingByStage([FromQuery][Required] string stageId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _fundingDAL.GetFundingByStage(stageId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FundingEntryDto>), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> GetPendingConfirmations()
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _fundingDAL.GetPendingConfirmations(userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(FundingEntryDto), 200)]
    // Writing off a shortfall is the funder's call and nobody else's, so this is
    // deliberately open to the client role the confirm endpoint excludes. The
    // DAL still checks that this caller is the one whose money it was.
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SettleFunding([FromBody] FundingSettleDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _fundingDAL.SettleFunding(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FundingEntryDto>), 200)]
    // Both ends of the exchange read this one — the delivery side for releases
    // to acknowledge, the funder for shortfalls to answer — so it carries no
    // role gate beyond being signed in. The query only ever returns rows that
    // name this caller.
    public async Task<ActionResult> GetFundingNeedingMe()
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _fundingDAL.GetFundingNeedingMe(userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
