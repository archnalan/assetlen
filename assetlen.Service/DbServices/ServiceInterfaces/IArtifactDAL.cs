using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// The canonical file store (assetlen.md Law 2) and the exposure gate.
/// <para>
/// Ingest is hash-first: identical bytes always resolve to the same artifact,
/// so the same receipt sent five times is one row with five pointers. Exposure
/// is per-<em>ref</em>, never per-entry, so a mediator promotes three frames
/// out of eighteen rather than the whole batch.
/// </para>
/// </summary>
public interface IArtifactDAL
{
    /// <summary>
    /// Store a file and return its artifact, creating one only if this hash is
    /// new to the tenant. <c>WasDeduplicated</c> tells the caller which
    /// happened so the UI can say <em>"already on this project"</em>.
    /// </summary>
    Task<ServiceResult<ArtifactDto>> IngestAsync(
        Stream content,
        string? fileName,
        string? contentType,
        string projectId,
        string userId,
        DateTime? capturedAt = null,
        CancellationToken ct = default);

    /// <summary>Read an artifact's metadata. Requires read access to its project.</summary>
    Task<ServiceResult<ArtifactDto>> GetAsync(string artifactId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Open the bytes for streaming. Enforces that the caller may see the
    /// artifact <em>through at least one visible ref</em> — a client-side user
    /// holding a guessed artifact id gets nothing.
    /// </summary>
    Task<ServiceResult<ArtifactContent>> OpenContentAsync(
        string artifactId, bool thumbnail, string userId, CancellationToken ct = default);

    /// <summary>Point an existing artifact at a target. Always lands Crew-only.</summary>
    Task<ServiceResult<ArtifactRefDto>> AddRefAsync(
        ArtifactRefCreateDto dto, string userId, CancellationToken ct = default);

    /// <summary>
    /// Refs for one target, filtered to what this caller may see. Client-side
    /// callers get <see cref="Channel.Client"/> refs only.
    /// </summary>
    Task<ServiceResult<List<ArtifactRefDto>>> GetRefsAsync(
        ArtifactTargetType targetType, string targetId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Expose or withdraw one ref. Gated on
    /// <c>IProjectAccessService.CanExposeToClientAsync</c> — mediators and
    /// owner/manager authority only.
    /// </summary>
    Task<ServiceResult<ArtifactRefDto>> SetRefChannelAsync(
        ArtifactExposureDto dto, string userId, CancellationToken ct = default);

    /// <summary>Batch form of <see cref="SetRefChannelAsync"/>. All-or-nothing.</summary>
    Task<ServiceResult<List<ArtifactRefDto>>> SetRefChannelBatchAsync(
        ArtifactExposureBatchDto dto, string userId, CancellationToken ct = default);

    /// <summary>Remove a pointer. The artifact itself is never deleted.</summary>
    Task<ServiceResult<bool>> RemoveRefAsync(string refId, string userId, CancellationToken ct = default);

    // ─── Controlled documents (amendment E2) ─────────────────────────────

    Task<ServiceResult<DocumentDto>> CreateDocumentAsync(
        DocumentCreateDto dto, string userId, CancellationToken ct = default);

    /// <summary>
    /// Add a revision and repoint the document at it. The previous revision is
    /// stamped <c>SupersededByRevisionId</c> and archived, never deleted.
    /// </summary>
    Task<ServiceResult<DocumentDto>> AddRevisionAsync(
        ArtifactRevisionCreateDto dto, string userId, CancellationToken ct = default);

    Task<ServiceResult<List<DocumentDto>>> GetDocumentsAsync(
        string projectId, string userId, CancellationToken ct = default);

    Task<ServiceResult<DocumentDto>> GetDocumentAsync(
        string documentId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Release a controlled document to the client side, or withdraw it. Same
    /// gate as frame exposure: this is the mediator's decision, and it carries
    /// every revision of the document with it.
    /// </summary>
    Task<ServiceResult<DocumentDto>> SetDocumentChannelAsync(
        string documentId, Channel channel, string userId, CancellationToken ct = default);
}

/// <summary>A stream of artifact bytes plus what the caller needs to serve it.</summary>
public sealed record ArtifactContent(Stream Content, string MimeType, string FileName, long ByteSize);
