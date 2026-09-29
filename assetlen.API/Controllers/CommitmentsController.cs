using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>
/// The commitments register — deliverables, commitments, links, accountability.
/// Every role reaches the controller; standing on the project decides the rest.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CommitmentsController : ControllerBase
{
    private readonly ICommitmentDAL _dal;
    private readonly ITenantProvider _tenant;

    public CommitmentsController(ICommitmentDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    private ActionResult Answer<T>(ServiceResult<T> result) =>
        result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);

    // ─── Deliverables ─────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<DeliverableDto>), 200)]
    public async Task<ActionResult> GetDeliverables([FromQuery][Required] string projectId, [FromQuery] string? stageId = null)
        => Answer(await _dal.GetDeliverables(projectId, stageId, _tenant.GetUserId()));

    [HttpPost]
    [ProducesResponseType(typeof(DeliverableDto), 200)]
    public async Task<ActionResult> AddDeliverable([FromBody] DeliverableCreateDto dto)
        => Answer(await _dal.AddDeliverable(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(DeliverableDto), 200)]
    public async Task<ActionResult> UpdateDeliverable([FromBody] DeliverableUpdateDto dto)
        => Answer(await _dal.UpdateDeliverable(dto, _tenant.GetUserId()));

    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult> DeleteDeliverable([FromQuery][Required] string deliverableId)
        => Answer(await _dal.DeleteDeliverable(deliverableId, _tenant.GetUserId()));

    // ─── Commitments ──────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<CommitmentDto>), 200)]
    public async Task<ActionResult> GetCommitments(
        [FromQuery][Required] string projectId,
        [FromQuery] string? stageId = null,
        [FromQuery] bool includeSuperseded = false)
        => Answer(await _dal.GetCommitments(projectId, stageId, includeSuperseded, _tenant.GetUserId()));

    [HttpGet]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> GetCommitment([FromQuery][Required] string commitmentId)
        => Answer(await _dal.GetCommitment(commitmentId, _tenant.GetUserId()));

    [HttpGet]
    [ProducesResponseType(typeof(List<CommitmentDto>), 200)]
    public async Task<ActionResult> GetChain([FromQuery][Required] string commitmentId)
        => Answer(await _dal.GetChain(commitmentId, _tenant.GetUserId()));

    [HttpPost]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> AddCommitment([FromBody] CommitmentCreateDto dto)
        => Answer(await _dal.AddCommitment(dto, _tenant.GetUserId()));

    /// <summary>"Agreed on the call: …" — one tap, attributed to both parties, awaiting the other side.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> LogDecision([FromBody] CommitmentCreateDto dto)
        => Answer(await _dal.LogDecision(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> Confirm([FromQuery][Required] string commitmentId)
        => Answer(await _dal.Confirm(commitmentId, _tenant.GetUserId()));

    /// <summary>"That's not what we said."</summary>
    [HttpPut]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> Dispute([FromBody] CommitmentNoteDto dto)
        => Answer(await _dal.Dispute(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> RaiseQuery([FromBody] CommitmentNoteDto dto)
        => Answer(await _dal.RaiseQuery(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> ResolveQuery([FromBody] CommitmentResolveDto dto)
        => Answer(await _dal.ResolveQuery(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> SetMaturity([FromBody] CommitmentMaturityDto dto)
        => Answer(await _dal.SetMaturity(dto, _tenant.GetUserId()));

    [HttpPut]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> Clear([FromQuery][Required] string commitmentId)
        => Answer(await _dal.Clear(commitmentId, _tenant.GetUserId()));

    [HttpPost]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> Restate([FromBody] CommitmentRestateDto dto)
        => Answer(await _dal.Restate(dto, _tenant.GetUserId()));

    // ─── Parked ideas ─────────────────────────────────────────

    [HttpPut]
    [ProducesResponseType(typeof(CommitmentDto), 200)]
    public async Task<ActionResult> Park([FromBody] CommitmentParkDto dto)
        => Answer(await _dal.Park(dto, _tenant.GetUserId()));

    [HttpPost]
    [ProducesResponseType(typeof(CommitmentEstimateDto), 200)]
    public async Task<ActionResult> AddEstimate([FromBody] CommitmentEstimateCreateDto dto)
        => Answer(await _dal.AddEstimate(dto, _tenant.GetUserId()));

    [HttpGet]
    [ProducesResponseType(typeof(List<CommitmentEstimateDto>), 200)]
    public async Task<ActionResult> GetEstimates([FromQuery][Required] string commitmentId)
        => Answer(await _dal.GetEstimates(commitmentId, _tenant.GetUserId()));

    // ─── Links ────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(CommitmentLinkDto), 200)]
    public async Task<ActionResult> AddLink([FromBody] CommitmentLinkCreateDto dto)
        => Answer(await _dal.AddLink(dto, _tenant.GetUserId()));

    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult> RemoveLink([FromQuery][Required] string linkId)
        => Answer(await _dal.RemoveLink(linkId, _tenant.GetUserId()));

    [HttpGet]
    [ProducesResponseType(typeof(List<CommitmentLinkDto>), 200)]
    public async Task<ActionResult> GetLinks([FromQuery][Required] string commitmentId)
        => Answer(await _dal.GetLinks(commitmentId, _tenant.GetUserId()));

    [HttpGet]
    [ProducesResponseType(typeof(List<CommitmentLinkDto>), 200)]
    public async Task<ActionResult> GetBacklinks(
        [FromQuery][Required] string projectId,
        [FromQuery][Required] CommitmentLinkTarget targetType,
        [FromQuery][Required] string targetId)
        => Answer(await _dal.GetBacklinks(projectId, targetType, targetId, _tenant.GetUserId()));

    // ─── Accountability ───────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(AccountabilityDto), 200)]
    public async Task<ActionResult> GetAccountability([FromQuery][Required] string projectId)
        => Answer(await _dal.GetAccountability(projectId, _tenant.GetUserId()));
}
