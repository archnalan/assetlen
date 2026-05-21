using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls
{
    public interface IProjectsRSApi
    {
        [Get("/api/ProjectsRS/GetPortfolioDashboard")]
        Task<IApiResponse<PortfolioSummaryDto>> GetPortfolioDashboard();

        [Post("/api/ProjectsRS/CreateProject")]
        Task<IApiResponse<ProjectDto>> CreateProject([Body] ProjectCreateDto dto);

        [Get("/api/ProjectsRS/GetProjectById")]
        Task<IApiResponse<ProjectDto>> GetProjectById([Query] string projectId);

        [Put("/api/ProjectsRS/UpdateProject")]
        Task<IApiResponse<ProjectDto>> UpdateProject([Body] ProjectDto dto);

        [Delete("/api/ProjectsRS/DeleteProject")]
        Task<IApiResponse<bool>> DeleteProject([Query] string projectId);

        [Get("/api/ProjectsRS/SearchProjects")]
        Task<IApiResponse<PaginationDetails<ProjectCardDto>>> SearchProjects(
            [Query] string? keywords = null,
            [Query] int offset = 0,
            [Query] int limit = 12,
            [Query] string? sortByColumn = null,
            [Query] bool sortAscending = false,
            [Query] CancellationToken cancellationToken = default);

        [Put("/api/ProjectsRS/AssignProjectManager")]
        Task<IApiResponse<ProjectDto>> AssignProjectManager(
            [Query] string projectId,
            [Query] string managerId);

        [Get("/api/ProjectsRS/GetProjectTimeline")]
        Task<IApiResponse<List<TimelineEntryDto>>> GetProjectTimeline([Query] string projectId);

        [Get("/api/ProjectsRS/GetProjectAnalytics")]
        Task<IApiResponse<ProjectAnalyticsDto>> GetProjectAnalytics([Query] string projectId);
    }

}
