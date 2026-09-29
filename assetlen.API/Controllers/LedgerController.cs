using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>
/// The stage money ledger — funded → claimed → cleared → carried forward — the
/// claims behind it, and the variation register. Money seats only, per project.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LedgerController : ControllerBase
{
    private readonly ILedgerDAL _dal;
    private readonly ITenantProvider _tenant;

    public LedgerController(ILedgerDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    private ActionResult Answer<T>(ServiceResult<T> result) =>
        result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);

    [HttpGet]
    [ProducesResponseType(typeof(StageLedgerDto), 200)]
    public async Task<ActionResult> GetStageLedger([FromQuery][Required] string projectId)
        => Answer(await _dal.GetStageLedger(projectId, _tenant.GetUserId()));

    // ─── Claims ───────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<StageClaimDto>), 200)]
    public async Task<ActionResult> GetClaims([FromQuery][Required] string projectId, [FromQuery] string? stageId = null)
        => Answer(await _dal.GetClaims(projectId, stageId, _tenant.GetUserId()));

    [HttpPost]
    [ProducesResponseType(typeof(StageClaimDto), 200)]
    public async Task<ActionResult> AddClaim([FromBody] StageClaimCreateDto dto)
        => Answer(await _dal.AddClaim(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(StageClaimDto), 200)]
    public async Task<ActionResult> DecideClaim([FromBody] StageClaimDecisionDto dto)
        => Answer(await _dal.DecideClaim(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(StageClaimDto), 200)]
    public async Task<ActionResult> WithdrawClaim([FromQuery][Required] string claimId)
        => Answer(await _dal.WithdrawClaim(claimId, _tenant.GetUserId()));

    /// <summary>What a claim on this stage could carry, so it is paid without a phone call.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ClaimEvidenceOptionsDto), 200)]
    public async Task<ActionResult> GetClaimEvidenceOptions([FromQuery][Required] string projectId, [FromQuery][Required] string stageId)
        => Answer(await _dal.GetClaimEvidenceOptions(projectId, stageId, _tenant.GetUserId()));

    // ─── Variations ───────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<VariationDto>), 200)]
    public async Task<ActionResult> GetVariations([FromQuery][Required] string projectId)
        => Answer(await _dal.GetVariations(projectId, _tenant.GetUserId()));

    [HttpPost]
    [ProducesResponseType(typeof(VariationDto), 200)]
    public async Task<ActionResult> AddVariation([FromBody] VariationCreateDto dto)
        => Answer(await _dal.AddVariation(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(VariationDto), 200)]
    public async Task<ActionResult> DecideVariation([FromBody] VariationDecisionDto dto)
        => Answer(await _dal.DecideVariation(dto, _tenant.GetUserId()));
}
