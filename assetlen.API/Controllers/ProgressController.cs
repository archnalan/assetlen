using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>The multipart body of <see cref="ProgressController.Capture"/>. Server-side because IFormFile is.</summary>
public class CaptureRequest
{
    [Required] public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? DeliverableId { get; set; }
    public string? Description { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public bool HasIssues { get; set; }
    public Channel Channel { get; set; } = Channel.Crew;
    public string? ClientCaptureId { get; set; }
    public DateTime? CapturedAt { get; set; }
    public List<IFormFile> Files { get; set; } = new();
    public List<string> Captions { get; set; } = new();
    public IFormFile? Voice { get; set; }
}

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProgressController : ControllerBase
{
    private readonly IProgressDAL _progressDAL;
    private readonly IPMDashboardDAL _pmDashboardDAL;
    private readonly ITenantProvider _tenantProvider;

    public ProgressController(
        IProgressDAL progressDAL,
        IPMDashboardDAL pmDashboardDAL,
        ITenantProvider tenantProvider)
    {
        _progressDAL = progressDAL;
        _pmDashboardDAL = pmDashboardDAL;
        _tenantProvider = tenantProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AddProgressUpdate([FromBody] ProgressUpdateCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.AddProgressUpdate(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Three taps: the deliverable, the camera roll, post. Multipart, never base64 —
    /// eighteen phone photographs inflated by a third is a failed post on a site
    /// connection. Safe to repeat: the offline queue resends the same ClientCaptureId
    /// until it hears back.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(300_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 300_000_000)]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Capture([FromForm] CaptureRequest request, CancellationToken ct)
    {
        var userId = _tenantProvider.GetUserId();
        var frames = new List<CaptureFile>();
        CaptureFile? voice = null;
        try
        {
            // Each part is copied out before the next is read: form-file streams share
            // one request body, and reading them interleaved throws (see RejoinMedia).
            for (var i = 0; i < request.Files.Count; i++)
            {
                var caption = i < request.Captions.Count && !string.IsNullOrWhiteSpace(request.Captions[i]) ? request.Captions[i] : null;
                frames.Add(new CaptureFile(await CopyAsync(request.Files[i], ct), request.Files[i].FileName, request.Files[i].ContentType, caption));
            }
            if (request.Voice is { Length: > 0 } v)
                voice = new CaptureFile(await CopyAsync(v, ct), v.FileName, v.ContentType);

            var meta = new ProgressUpdateCreateDto
            {
                ProjectId = request.ProjectId,
                StageId = request.StageId,
                DeliverableId = request.DeliverableId,
                Description = request.Description,
                CompletionPercentage = request.CompletionPercentage,
                HasIssues = request.HasIssues,
                Channel = request.Channel,
                ClientCaptureId = request.ClientCaptureId,
                CapturedAt = request.CapturedAt
            };
            var result = await _progressDAL.Capture(meta, frames, voice, userId, ct);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
            return Ok(result.Data);
        }
        finally
        {
            foreach (var f in frames) await f.Content.DisposeAsync();
            if (voice is not null) await voice.Content.DisposeAsync();
        }
    }

    private static async Task<Stream> CopyAsync(IFormFile file, CancellationToken ct)
    {
        var copy = new MemoryStream();
        await using (var source = file.OpenReadStream()) await source.CopyToAsync(copy, ct);
        copy.Position = 0;
        return copy;
    }

    /// <summary>Today's deliverables, so the first tap names the work.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CaptureTodayDto), 200)]
    public async Task<ActionResult> GetCaptureToday([FromQuery][Required] string projectId, CancellationToken ct)
    {
        var result = await _progressDAL.GetCaptureToday(projectId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    public async Task<ActionResult> GetProgressUpdate([FromQuery][Required] string updateId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.GetProgressUpdate(updateId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetApprovalStatus([FromBody] ProgressApprovalDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.SetApprovalStatus(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetChannel(
        [FromQuery][Required] string updateId,
        [FromQuery][Required] Channel channel)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.SetChannel(updateId, channel, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Expose or withdraw individual frames on an entry — the mediator picks
    /// three of eighteen. The role gate here is coarse; the real check is
    /// <c>IProjectAccessService.CanExposeToClientAsync</c>, which admits the
    /// project's mediator whichever side they sit on.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProgressUpdateDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> SetImageChannel([FromBody] ProgressImageExposureDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.SetImageChannel(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginationDetails<ProgressUpdateDto>), 200)]
    public async Task<ActionResult> GetProgressUpdates(
        [FromQuery][Required] string projectId,
        [FromQuery] string? stageId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.GetProgressUpdates(projectId, stageId, offset, limit, userId, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProgressCommentDto), 200)]
    public async Task<ActionResult> AddComment([FromBody] ProgressCommentCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _progressDAL.AddComment(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── PM Dashboard ─────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(PMDashboardDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> GetPMDashboard()
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _pmDashboardDAL.GetPMDashboard(userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
