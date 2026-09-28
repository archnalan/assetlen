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
/// Extraction — the forwarded pile read into proposals, a review queue cleared
/// in bulk, progress readings, and the text read out of files (plan.md P5).
/// Every role reaches the controller; standing on the project decides the rest.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ExtractionController : ControllerBase
{
    private readonly IExtractionDAL _dal;
    private readonly ITenantProvider _tenant;

    public ExtractionController(IExtractionDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    private ActionResult Answer<T>(ServiceResult<T> result) =>
        result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);

    /// <summary>Read the project's ingested record again. Safe to repeat: nothing already proposed is proposed twice.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExtractionRunDto), 200)]
    public async Task<ActionResult> Run([FromBody] ExtractionRunRequestDto dto, CancellationToken ct)
        => Answer(await _dal.RunAsync(dto, _tenant.GetUserId(), ct));

    [HttpGet]
    [ProducesResponseType(typeof(ExtractionQueueDto), 200)]
    public async Task<ActionResult> GetQueue([FromQuery][Required] string projectId, [FromQuery] ProposalStatus? status, CancellationToken ct)
        => Answer(await _dal.GetQueueAsync(projectId, status, _tenant.GetUserId(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(ProposalDecisionResultDto), 200)]
    public async Task<ActionResult> Decide([FromBody] ProposalDecisionDto dto, CancellationToken ct)
        => Answer(await _dal.DecideAsync(dto, _tenant.GetUserId(), ct));

    [HttpPut]
    [ProducesResponseType(typeof(ExtractionProposalDto), 200)]
    public async Task<ActionResult> Edit([FromBody] ProposalEditDto dto, CancellationToken ct)
        => Answer(await _dal.EditAsync(dto, _tenant.GetUserId(), ct));

    [HttpGet]
    [ProducesResponseType(typeof(List<ProgressReadingDto>), 200)]
    public async Task<ActionResult> GetReadings([FromQuery][Required] string projectId, [FromQuery] string? stageId, CancellationToken ct)
        => Answer(await _dal.GetReadingsAsync(projectId, stageId, _tenant.GetUserId(), ct));

    [HttpGet]
    [ProducesResponseType(typeof(ArtifactTextDto), 200)]
    public async Task<ActionResult> GetArtifactText([FromQuery][Required] string artifactId, CancellationToken ct)
        => Answer(await _dal.GetArtifactTextAsync(artifactId, _tenant.GetUserId(), ct));

    /// <summary>Queue every unread file on the project for OCR — the backfill once an engine exists.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OcrQueueResultDto), 200)]
    public async Task<ActionResult> QueueOcr([FromQuery][Required] string projectId, CancellationToken ct)
        => Answer(await _dal.QueueProjectOcrAsync(projectId, _tenant.GetUserId(), ct));
}
