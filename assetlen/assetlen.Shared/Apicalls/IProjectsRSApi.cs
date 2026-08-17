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

        /// <summary>Empty one project out of the bin early. Fails unless it is archived already.</summary>
        [Delete("/api/ProjectsRS/DeleteProject")]
        Task<IApiResponse<bool>> DeleteProject([Query] string projectId);

        // ─── The bin ───
        // Deleting a project archives it for 30 days. Nothing underneath is
        // touched until the sweep runs.

        [Put("/api/ProjectsRS/ArchiveProject")]
        Task<IApiResponse<bool>> ArchiveProject([Query] string projectId);

        [Put("/api/ProjectsRS/RestoreProject")]
        Task<IApiResponse<bool>> RestoreProject([Query] string projectId);

        [Get("/api/ProjectsRS/GetArchivedProjects")]
        Task<IApiResponse<List<ProjectCardDto>>> GetArchivedProjects();

        // ─── This reader's arrangement ───
        // Per user, so two people on one engagement each order their own screen.

        [Put("/api/ProjectsRS/ReorderProjects")]
        Task<IApiResponse<bool>> ReorderProjects([Body] ProjectOrderUpdateDto dto);

        [Put("/api/ProjectsRS/SetProjectPinned")]
        Task<IApiResponse<bool>> SetProjectPinned([Query] string projectId, [Query] bool pinned);

        /// <summary>Set a project's cover, or clear it with a null artifact id.</summary>
        [Put("/api/ProjectsRS/SetProjectCover")]
        Task<IApiResponse<ProjectDto>> SetProjectCover([Body] ProjectCoverUpdateDto dto);

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

        // ─── Sizing and billing tier ───
        // Billing is per project by floor area, never per seat.

        [Get("/api/ProjectsRS/GetProjectSizing")]
        Task<IApiResponse<ProjectSizingDto>> GetProjectSizing([Query] string projectId);

        [Put("/api/ProjectsRS/SetProjectArea")]
        Task<IApiResponse<ProjectSizingDto>> SetProjectArea([Body] ProjectAreaUpdateDto dto);

        /// <summary>Accept a pending tier increase — it changes the bill.</summary>
        [Put("/api/ProjectsRS/ConfirmProjectTier")]
        Task<IApiResponse<ProjectSizingDto>> ConfirmProjectTier([Query] string projectId);
    }
}
