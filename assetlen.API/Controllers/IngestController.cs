using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>
/// The multipart body of <see cref="IngestController.UploadArchive"/>.
/// <para>
/// Lives here rather than in Shared.Models because <see cref="IFormFile"/> is a
/// server type and Shared.Models is referenced by the WASM client. Bound as one
/// model, not as loose <c>[FromForm]</c> parameters — Swashbuckle cannot
/// describe an action mixing a bare <see cref="IFormFile"/> with sibling form
/// scalars and drops the entire API document when it tries.
/// </para>
/// </summary>
public class IngestArchiveRequest
{
    /// <summary>The export: a <c>.zip</c> with media, or the bare <c>.txt</c> transcript.</summary>
    [Required]
    public IFormFile? File { get; set; }

    [Required]
    public string? ProjectId { get; set; }
}

/// <summary>The multipart body of <see cref="IngestController.CaptureShare"/>.</summary>
public class ShareCaptureRequest
{
    /// <summary>Optional — a share may be text only.</summary>
    public IFormFile? File { get; set; }

    [Required]
    public string? ProjectId { get; set; }

    public string? Text { get; set; }

    public DateTime? SentAt { get; set; }
}

/// <summary>
/// The front door (assetlen.md D3, plan.md P3).
/// <para>
/// Nothing here requires the contractor to exist. Peter uploads his own WhatsApp
/// export, from his own phone, and gets a searchable, de-duplicated record —
/// that is Law 0 made concrete, and it is the only endpoint set in the product
/// that has to work with every other participant silent.
/// </para>
/// <para>
/// Import is two calls. <c>UploadArchive</c> reads and reports; <c>CommitImport</c>
/// writes. The split exists so attribution — the one expensive thing to get
/// wrong — is a decision somebody makes rather than a guess the server commits.
/// </para>
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class IngestController : ControllerBase
{
    private readonly IIngestDAL _dal;
    private readonly ITenantProvider _tenantProvider;
    private readonly IConfiguration _config;

    public IngestController(IIngestDAL dal, ITenantProvider tenantProvider, IConfiguration config)
    {
        _dal = dal;
        _tenantProvider = tenantProvider;
        _config = config;
    }

    // ─── WhatsApp export ─────────────────────────────────────────────────

    /// <summary>
    /// Upload an export and see what is in it. <b>Writes no messages.</b>
    /// Returns the participant list to map, the date range, and how much of the
    /// export this project already holds.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(IngestDAL.MaxArchiveBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = IngestDAL.MaxArchiveBytes)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IngestPreviewDto), 200)]
    public async Task<ActionResult> UploadArchive([FromForm] IngestArchiveRequest request, CancellationToken ct)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");
        if (string.IsNullOrWhiteSpace(request.ProjectId))
            return BadRequest("projectId is required.");

        await using var stream = file.OpenReadStream();

        var result = await _dal.PreviewArchiveAsync(
            stream, file.FileName, file.ContentType, request.ProjectId, _tenantProvider.GetUserId(), ct);

        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Apply a previewed import. Idempotent — messages already present are
    /// counted and skipped, never duplicated.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IngestBatchDto), 200)]
    public async Task<ActionResult> CommitImport([FromBody] IngestCommitDto dto, CancellationToken ct)
    {
        var result = await _dal.CommitImportAsync(dto, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── The ongoing trickle ─────────────────────────────────────────────

    /// <summary>
    /// One item from a phone's share sheet — the Web Share Target lands here.
    /// Takes no options: parity is measured against forwarding inside WhatsApp
    /// (assetlen.md §9), and a question costs more than the feature is worth.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IngestedMessageDto), 200)]
    public async Task<ActionResult> CaptureShare([FromForm] ShareCaptureRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectId))
            return BadRequest("projectId is required.");

        Stream? stream = null;
        if (request.File is { Length: > 0 }) stream = request.File.OpenReadStream();

        try
        {
            var result = await _dal.CaptureShareAsync(
                stream, request.File?.FileName, request.File?.ContentType,
                new ShareCaptureDto
                {
                    ProjectId = request.ProjectId,
                    Text = request.Text,
                    SentAt = request.SentAt
                },
                _tenantProvider.GetUserId(), ct);

            if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
            return Ok(result.Data);
        }
        finally
        {
            if (stream is not null) await stream.DisposeAsync();
        }
    }

    /// <summary>
    /// Webhook for a mail relay delivering to a project's inbound address.
    /// <para>
    /// <b>Anonymous by necessity</b> — the caller is a mail provider, not a
    /// signed-in person — and therefore authenticated by a shared secret instead.
    /// The project is resolved from the recipient key, so a forged body cannot
    /// select a project whose address it does not already hold. When no secret is
    /// configured the endpoint refuses everything rather than standing open.
    /// </para>
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IngestBatchDto), 200)]
    public async Task<ActionResult> InboundEmail(
        [FromBody] InboundEmailDto dto,
        [FromHeader(Name = "X-Assetlen-Ingest-Secret")] string? secret,
        CancellationToken ct)
    {
        var expected = _config["Ingest:InboundSecret"];
        if (string.IsNullOrEmpty(expected))
            return StatusCode(503, "Inbound email is not configured.");

        // Fixed-time compare: the secret is long-lived and guessable one byte at
        // a time if the comparison short-circuits.
        if (string.IsNullOrEmpty(secret) ||
            !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(secret),
                System.Text.Encoding.UTF8.GetBytes(expected)))
            return Unauthorized();

        var result = await _dal.ReceiveEmailAsync(dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>The project's inbound address, minted on first request.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProjectInboxDto), 200)]
    public async Task<ActionResult> GetInbox([FromQuery][Required] string projectId, CancellationToken ct)
    {
        var result = await _dal.GetInboxAsync(projectId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>Revoke the current address and mint a new one. Owner or manager.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProjectInboxDto), 200)]
    public async Task<ActionResult> ResetInbox([FromQuery][Required] string projectId, CancellationToken ct)
    {
        var result = await _dal.ResetInboxAsync(projectId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── Reading the raw record ──────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<IngestBatchDto>), 200)]
    public async Task<ActionResult> GetBatches([FromQuery][Required] string projectId, CancellationToken ct)
    {
        var result = await _dal.GetBatchesAsync(projectId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IngestBatchDto), 200)]
    public async Task<ActionResult> GetBatch([FromQuery][Required] string batchId, CancellationToken ct)
    {
        var result = await _dal.GetBatchAsync(batchId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// The raw record, paged. Filtered to the runs this caller may read — a
    /// delivery-side import is not visible to the client side, and the reverse.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IngestedMessagePageDto), 200)]
    public async Task<ActionResult> GetMessages([FromQuery] IngestedMessageQueryDto query, CancellationToken ct)
    {
        var result = await _dal.GetMessagesAsync(query, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
