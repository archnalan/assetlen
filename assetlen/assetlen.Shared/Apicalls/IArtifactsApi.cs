using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>
/// The canonical file store and the exposure gate (assetlen.md Law 2).
/// <para>
/// Upload is multipart — never base64 in JSON. Bytes are read from
/// <c>ArtifactDto.Url</c> / <c>ThumbnailUrl</c> by the browser directly, so no
/// method here returns file content.
/// </para>
/// </summary>
public interface IArtifactsApi
{
    /// <summary>
    /// Store a file. An identical hash returns the existing artifact with
    /// <c>WasDeduplicated</c> set — show *"already on this project"* rather than
    /// creating a second copy.
    /// </summary>
    [Multipart]
    [Post("/api/Artifacts/Upload")]
    Task<IApiResponse<ArtifactDto>> Upload(
        [AliasAs("file")] StreamPart file,
        [AliasAs("projectId")] string projectId,
        [AliasAs("capturedAt")] DateTime? capturedAt = null);

    [Get("/api/Artifacts/Get")]
    Task<IApiResponse<ArtifactDto>> Get([Query] string artifactId);

    [Post("/api/Artifacts/AddRef")]
    Task<IApiResponse<ArtifactRefDto>> AddRef([Body] ArtifactRefCreateDto dto);

    [Get("/api/Artifacts/GetRefs")]
    Task<IApiResponse<List<ArtifactRefDto>>> GetRefs(
        [Query] ArtifactTargetType targetType, [Query] string targetId);

    /// <summary>Expose or withdraw one use. Mediator, owner or manager only.</summary>
    [Put("/api/Artifacts/SetRefChannel")]
    Task<IApiResponse<ArtifactRefDto>> SetRefChannel([Body] ArtifactExposureDto dto);

    /// <summary>Batch form — three of eighteen, in one action.</summary>
    [Put("/api/Artifacts/SetRefChannelBatch")]
    Task<IApiResponse<List<ArtifactRefDto>>> SetRefChannelBatch([Body] ArtifactExposureBatchDto dto);

    [Delete("/api/Artifacts/RemoveRef")]
    Task<IApiResponse<bool>> RemoveRef([Query] string refId);

    // ─── Controlled documents ────────────────────────────────────────────

    [Post("/api/Artifacts/CreateDocument")]
    Task<IApiResponse<DocumentDto>> CreateDocument([Body] DocumentCreateDto dto);

    /// <summary>Reissue. Pins the new revision; the previous one is archived.</summary>
    [Post("/api/Artifacts/AddRevision")]
    Task<IApiResponse<DocumentDto>> AddRevision([Body] ArtifactRevisionCreateDto dto);

    [Get("/api/Artifacts/GetDocuments")]
    Task<IApiResponse<List<DocumentDto>>> GetDocuments([Query] string projectId);

    [Get("/api/Artifacts/GetDocument")]
    Task<IApiResponse<DocumentDto>> GetDocument([Query] string documentId);
}
