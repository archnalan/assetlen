using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>
/// Extraction (plan.md P5): the review queue, progress readings, and the text
/// read out of files. The server decides what the reader may see and do; the
/// client only renders it.
/// </summary>
public interface IExtractionApi
{
    [Post("/api/Extraction/Run")]
    Task<IApiResponse<ExtractionRunDto>> Run([Body] ExtractionRunRequestDto dto);

    [Get("/api/Extraction/GetQueue")]
    Task<IApiResponse<ExtractionQueueDto>> GetQueue([Query] string projectId, [Query] ProposalStatus? status = null);

    [Post("/api/Extraction/Decide")]
    Task<IApiResponse<ProposalDecisionResultDto>> Decide([Body] ProposalDecisionDto dto);

    [Put("/api/Extraction/Edit")]
    Task<IApiResponse<ExtractionProposalDto>> Edit([Body] ProposalEditDto dto);

    [Get("/api/Extraction/GetReadings")]
    Task<IApiResponse<List<ProgressReadingDto>>> GetReadings([Query] string projectId, [Query] string? stageId = null);

    [Get("/api/Extraction/GetArtifactText")]
    Task<IApiResponse<ArtifactTextDto>> GetArtifactText([Query] string artifactId);

    [Post("/api/Extraction/QueueOcr")]
    Task<IApiResponse<OcrQueueResultDto>> QueueOcr([Query] string projectId);
}
