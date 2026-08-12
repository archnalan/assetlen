using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>
/// The front door (assetlen.md D3, plan.md P3).
/// <para>
/// Import is deliberately two calls: <see cref="UploadArchive"/> reports what an
/// export contains and writes nothing, then <see cref="CommitImport"/> applies
/// it with the author mapping the user chose. A single-call import would have to
/// guess attribution, and filing a year of history against the wrong person is
/// the one part of this that is expensive to undo.
/// </para>
/// </summary>
public interface IIngestApi
{
    /// <summary>
    /// Upload a WhatsApp export and read back what is in it. Multipart, never
    /// base64 — an export is measured in hundreds of megabytes and a data URI
    /// inflates it by a third.
    /// </summary>
    [Multipart]
    [Post("/api/Ingest/UploadArchive")]
    Task<IApiResponse<IngestPreviewDto>> UploadArchive(
        [AliasAs("file")] StreamPart file,
        [AliasAs("projectId")] string projectId);

    /// <summary>Apply a previewed import. Safe to repeat — duplicates are counted, not created.</summary>
    [Post("/api/Ingest/CommitImport")]
    Task<IApiResponse<IngestBatchDto>> CommitImport([Body] IngestCommitDto dto);

    /// <summary>One item from the share sheet: a file, some text, or both.</summary>
    [Multipart]
    [Post("/api/Ingest/CaptureShare")]
    Task<IApiResponse<IngestedMessageDto>> CaptureShare(
        [AliasAs("projectId")] string projectId,
        [AliasAs("file")] StreamPart? file = null,
        [AliasAs("text")] string? text = null,
        [AliasAs("sentAt")] DateTime? sentAt = null);

    [Get("/api/Ingest/GetInbox")]
    Task<IApiResponse<ProjectInboxDto>> GetInbox([Query] string projectId);

    /// <summary>Revoke the inbound address and mint a new one. Owner or manager.</summary>
    [Put("/api/Ingest/ResetInbox")]
    Task<IApiResponse<ProjectInboxDto>> ResetInbox([Query] string projectId);

    [Get("/api/Ingest/GetBatches")]
    Task<IApiResponse<List<IngestBatchDto>>> GetBatches([Query] string projectId);

    [Get("/api/Ingest/GetBatch")]
    Task<IApiResponse<IngestBatchDto>> GetBatch([Query] string batchId);

    /// <summary>The raw record, paged and filtered server-side by what this caller may read.</summary>
    [Get("/api/Ingest/GetMessages")]
    Task<IApiResponse<IngestedMessagePageDto>> GetMessages([Query] IngestedMessageQueryDto query);
}
