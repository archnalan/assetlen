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

/// <summary>The multipart body of <see cref="WorkPlanController.Tick"/>: the line and its one photo.</summary>
public class TickRequest
{
    [Required] public string? DeliverableId { get; set; }
    public IFormFile? Photo { get; set; }
}

/// <summary>
/// The work plan and knocking lines off it (works-report.md §4.5). Every role
/// reaches the controller; standing on the project decides the rest.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class WorkPlanController : ControllerBase
{
    private readonly IWorkPlanDAL _dal;
    private readonly ITenantProvider _tenant;

    public WorkPlanController(IWorkPlanDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    private ActionResult Answer<T>(ServiceResult<T> result) =>
        result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);

    [HttpGet]
    [ProducesResponseType(typeof(WorkPlanDto), 200)]
    public async Task<ActionResult> GetPlan([FromQuery][Required] string projectId, [FromQuery] string? stageId, CancellationToken ct)
        => Answer(await _dal.GetPlan(projectId, stageId, _tenant.GetUserId(), ct));

    /// <summary>Knock a line off on one photo. No photo, no tick.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(60_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 60_000_000)]
    [ProducesResponseType(typeof(DeliverableDto), 200)]
    public async Task<ActionResult> Tick([FromForm] TickRequest request, CancellationToken ct)
    {
        if (Request.Form.Files.Count > 1)
            return BadRequest("A tick takes exactly one photo.");

        CaptureFile? photo = null;
        try
        {
            if (request.Photo is { Length: > 0 } p)
            {
                var copy = new MemoryStream();
                await using (var source = p.OpenReadStream()) await source.CopyToAsync(copy, ct);
                copy.Position = 0;
                photo = new CaptureFile(copy, p.FileName, p.ContentType);
            }
            return Answer(await _dal.Tick(request.DeliverableId!, photo, _tenant.GetUserId(), ct));
        }
        finally
        {
            if (photo is not null) await photo.Content.DisposeAsync();
        }
    }

    /// <summary>Take a tick back. The photo and the earlier tick stay on record.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(DeliverableDto), 200)]
    public async Task<ActionResult> Reopen([FromQuery][Required] string deliverableId, CancellationToken ct)
        => Answer(await _dal.Reopen(deliverableId, _tenant.GetUserId(), ct));

    /// <summary>The house's plan: dates computed from days, waits and order (works-report.md §4.6).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ScheduleDto), 200)]
    public async Task<ActionResult> GetSchedule([FromQuery][Required] string projectId, [FromQuery] DateOnly? asAt, CancellationToken ct)
        => Answer(await _dal.GetSchedule(projectId, asAt, _tenant.GetUserId(), ct));

    /// <summary>What a change would do to the handover, before anyone saves it.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SchedulePreviewDto), 200)]
    public async Task<ActionResult> PreviewSchedule([FromBody] SchedulePreviewRequestDto dto, CancellationToken ct)
        => Answer(await _dal.PreviewSchedule(dto, _tenant.GetUserId(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(ScheduleDto), 200)]
    public async Task<ActionResult> SaveSchedule([FromBody] ScheduleChangeDto dto, CancellationToken ct)
        => Answer(await _dal.SaveSchedule(dto, _tenant.GetUserId(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(ScheduleDto), 200)]
    public async Task<ActionResult> Recompute([FromQuery][Required] string projectId, CancellationToken ct)
        => Answer(await _dal.Recompute(projectId, _tenant.GetUserId(), ct));
}
