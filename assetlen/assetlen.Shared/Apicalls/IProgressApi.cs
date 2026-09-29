using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls
{
    public interface IProgressApi
    {
        [Post("/api/Progress/AddProgressUpdate")]
        Task<IApiResponse<ProgressUpdateDto>> AddProgressUpdate([Body] ProgressUpdateCreateDto dto);

        [Get("/api/Progress/GetProgressUpdate")]
        Task<IApiResponse<ProgressUpdateDto>> GetProgressUpdate([Query] string updateId);

        [Put("/api/Progress/SetApprovalStatus")]
        Task<IApiResponse<ProgressUpdateDto>> SetApprovalStatus([Body] ProgressApprovalDto dto);

        [Put("/api/Progress/SetChannel")]
        Task<IApiResponse<ProgressUpdateDto>> SetChannel([Query] string updateId, [Query] Channel channel);

        /// <summary>
        /// Expose or withdraw individual frames — the mediator picks three of
        /// eighteen rather than flipping the whole batch across.
        /// </summary>
        [Put("/api/Progress/SetImageChannel")]
        Task<IApiResponse<ProgressUpdateDto>> SetImageChannel([Body] ProgressImageExposureDto dto);

        [Get("/api/Progress/GetProgressUpdates")]
        Task<IApiResponse<PaginationDetails<ProgressUpdateDto>>> GetProgressUpdates(
            [Query] string projectId,
            [Query] string? stageId = null,
            [Query] int offset = 0,
            [Query] int limit = 10,
            [Query] CancellationToken cancellationToken = default);

        [Post("/api/Progress/AddComment")]
        Task<IApiResponse<ProgressCommentDto>> AddComment([Body] ProgressCommentCreateDto dto);

        [Get("/api/Progress/GetPMDashboard")]
        Task<IApiResponse<PMDashboardDto>> GetPMDashboard();

        /// <summary>
        /// The capture path: frames as parts, never base64. The offline queue sends
        /// the same clientCaptureId until it hears back, and the server keeps one.
        /// </summary>
        [Multipart]
        [Post("/api/Progress/Capture")]
        Task<IApiResponse<ProgressUpdateDto>> Capture(
            [AliasAs("projectId")] string projectId,
            [AliasAs("files")] IEnumerable<ByteArrayPart> files,
            [AliasAs("captions")] IEnumerable<string> captions,
            [AliasAs("deliverableId")] string? deliverableId = null,
            [AliasAs("stageId")] string? stageId = null,
            [AliasAs("description")] string? description = null,
            [AliasAs("completionPercentage")] string? completionPercentage = null,
            [AliasAs("hasIssues")] bool hasIssues = false,
            [AliasAs("channel")] string channel = "Crew",
            [AliasAs("clientCaptureId")] string? clientCaptureId = null,
            [AliasAs("capturedAt")] string? capturedAt = null,
            [AliasAs("voice")] ByteArrayPart? voice = null);

        [Get("/api/Progress/GetCaptureToday")]
        Task<IApiResponse<CaptureTodayDto>> GetCaptureToday([Query] string projectId);
    }
}
