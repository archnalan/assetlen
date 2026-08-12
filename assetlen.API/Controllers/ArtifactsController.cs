using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>
/// The multipart body of <see cref="ArtifactsController.Upload"/>.
/// <para>
/// Lives in the API project, not in Shared.Models: <see cref="IFormFile"/> is
/// an ASP.NET Core server type and Shared.Models is referenced by the WASM
/// client, which must never take that dependency. The client sends the same
/// three parts through Refit's <c>StreamPart</c>.
/// </para>
/// </summary>
public class ArtifactUploadRequest
{
    /// <summary>The bytes. Part name <c>file</c>.</summary>
    [Required]
    public IFormFile? File { get; set; }

    /// <summary>Which project owns the artifact. Access is resolved against it.</summary>
    [Required]
    public string? ProjectId { get; set; }

    /// <summary>
    /// When the photo was taken, if the caller knows — for a camera-roll import
    /// the capture date is the fact that matters, not the upload date.
    /// </summary>
    public DateTime? CapturedAt { get; set; }
}

/// <summary>
/// The canonical file store (assetlen.md Law 2) and the exposure gate.
/// <para>
/// Upload is <b>multipart</b>, not base64 — a JSON data-URI inflates bytes by a
/// third, and the previous base64 path corrupted every JPEG it ever handled.
/// Download streams; nothing here returns file bytes inside JSON.
/// </para>
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ArtifactsController : ControllerBase
{
    private readonly IArtifactDAL _dal;
    private readonly ITenantProvider _tenantProvider;

    /// <summary>Matches <c>ArtifactDAL.MaxUploadBytes</c>; rejected here first so a huge body is not buffered.</summary>
    private const long MaxUploadBytes = 40L * 1024 * 1024;

    public ArtifactsController(IArtifactDAL dal, ITenantProvider tenantProvider)
    {
        _dal = dal;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Store a file. An identical hash returns the artifact that already exists
    /// with <c>WasDeduplicated</c> set — *"this is already Receipt R-014."*
    /// <para>
    /// The form fields arrive as one bound model rather than as loose
    /// <c>[FromForm]</c> parameters: Swashbuckle cannot describe an action that
    /// mixes a bare <see cref="IFormFile"/> parameter with sibling form
    /// scalars, and threw on the whole document — every endpoint in the API
    /// disappeared from /swagger/v1/swagger.json, not just this one. Wire names
    /// are unchanged, so the multipart part names stay <c>file</c>,
    /// <c>projectId</c>, <c>capturedAt</c>.
    /// </para>
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(MaxUploadBytes)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ArtifactDto), 200)]
    public async Task<ActionResult> Upload([FromForm] ArtifactUploadRequest request, CancellationToken ct)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");
        if (string.IsNullOrWhiteSpace(request.ProjectId))
            return BadRequest("projectId is required.");

        var userId = _tenantProvider.GetUserId();
        await using var stream = file.OpenReadStream();

        var result = await _dal.IngestAsync(
            stream, file.FileName, file.ContentType, request.ProjectId, userId, request.CapturedAt, ct);

        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ArtifactDto), 200)]
    public async Task<ActionResult> Get([FromQuery][Required] string artifactId, CancellationToken ct)
    {
        var result = await _dal.GetAsync(artifactId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Stream the original. Routed through the DAL rather than served statically
    /// because visibility is per caller: a client-side reader must not reach an
    /// artifact simply by knowing its id.
    /// </summary>
    [HttpGet("/api/Artifacts/{artifactId}/content")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public Task<ActionResult> Content(string artifactId, CancellationToken ct) =>
        StreamAsync(artifactId, thumbnail: false, ct);

    [HttpGet("/api/Artifacts/{artifactId}/thumbnail")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public Task<ActionResult> Thumbnail(string artifactId, CancellationToken ct) =>
        StreamAsync(artifactId, thumbnail: true, ct);

    private async Task<ActionResult> StreamAsync(string artifactId, bool thumbnail, CancellationToken ct)
    {
        var result = await _dal.OpenContentAsync(artifactId, thumbnail, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);

        var content = result.Data;

        // Content-addressed: the bytes at an id can never change, so cache hard.
        // Private, because whether *this* caller may see it is an authorization
        // decision that must not be made by a shared cache.
        Response.Headers.CacheControl = "private, max-age=31536000, immutable";

        return File(content.Content, content.MimeType, enableRangeProcessing: true);
    }

    // ─── Pointers and exposure ───────────────────────────────────────────

    /// <summary>Point an artifact at a target. Always lands Crew-only.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ArtifactRefDto), 200)]
    public async Task<ActionResult> AddRef([FromBody] ArtifactRefCreateDto dto, CancellationToken ct)
    {
        var result = await _dal.AddRefAsync(dto, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ArtifactRefDto>), 200)]
    public async Task<ActionResult> GetRefs(
        [FromQuery][Required] ArtifactTargetType targetType,
        [FromQuery][Required] string targetId,
        CancellationToken ct)
    {
        var result = await _dal.GetRefsAsync(targetType, targetId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>
    /// Expose or withdraw one use of an artifact. **Mediator, owner or manager
    /// only** — this is the decision that puts something in front of the client.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ArtifactRefDto), 200)]
    public async Task<ActionResult> SetRefChannel([FromBody] ArtifactExposureDto dto, CancellationToken ct)
    {
        var result = await _dal.SetRefChannelAsync(dto, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>Batch form — the mediator's real gesture: three of eighteen, in one action.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(List<ArtifactRefDto>), 200)]
    public async Task<ActionResult> SetRefChannelBatch([FromBody] ArtifactExposureBatchDto dto, CancellationToken ct)
    {
        var result = await _dal.SetRefChannelBatchAsync(dto, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>Remove a pointer. The artifact itself is never deleted.</summary>
    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult> RemoveRef([FromQuery][Required] string refId, CancellationToken ct)
    {
        var result = await _dal.RemoveRefAsync(refId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    // ─── Controlled documents and revisions ──────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), 200)]
    public async Task<ActionResult> CreateDocument([FromBody] DocumentCreateDto dto, CancellationToken ct)
    {
        var result = await _dal.CreateDocumentAsync(dto, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>Reissue. Pins the new revision; the previous one is archived, never deleted.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), 200)]
    public async Task<ActionResult> AddRevision([FromBody] ArtifactRevisionCreateDto dto, CancellationToken ct)
    {
        var result = await _dal.AddRevisionAsync(dto, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DocumentDto>), 200)]
    public async Task<ActionResult> GetDocuments([FromQuery][Required] string projectId, CancellationToken ct)
    {
        var result = await _dal.GetDocumentsAsync(projectId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DocumentDto), 200)]
    public async Task<ActionResult> GetDocument([FromQuery][Required] string documentId, CancellationToken ct)
    {
        var result = await _dal.GetDocumentAsync(documentId, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    /// <summary>Issue a document to the client side, or withdraw it. Mediator, owner or manager.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(DocumentDto), 200)]
    public async Task<ActionResult> SetDocumentChannel(
        [FromQuery][Required] string documentId, [FromQuery] Channel channel, CancellationToken ct)
    {
        var result = await _dal.SetDocumentChannelAsync(documentId, channel, _tenantProvider.GetUserId(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
