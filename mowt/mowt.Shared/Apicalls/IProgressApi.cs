using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface IProgressApi
    {
        [Post("/api/Progress/AddProgressUpdate")]
        Task<IApiResponse<ProgressUpdateDto>> AddProgressUpdate([Body] ProgressUpdateCreateDto dto);

        [Put("/api/Progress/SetApprovalStatus")]
        Task<IApiResponse<ProgressUpdateDto>> SetApprovalStatus([Body] ProgressApprovalDto dto);

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
