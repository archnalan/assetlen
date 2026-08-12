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
    }
}
