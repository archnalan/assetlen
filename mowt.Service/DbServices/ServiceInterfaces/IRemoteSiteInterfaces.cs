using mowt.ServiceHandler;
using mowt.Shared.Models.Models.RemoteSite;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace mowt.Service.DbServices.ServiceInterfaces;

public interface IProjectDAL
{
    // ─── Portfolio ────────────────────────────────────────────
    Task<ServiceResult<PortfolioSummaryDto>> GetPortfolioDashboard(string investorId);

    // ─── CRUD ─────────────────────────────────────────────────
    Task<ServiceResult<ProjectDto>> CreateProject(ProjectCreateDto dto, string investorId);
    Task<ServiceResult<ProjectDto>> GetProjectById(string projectId, string userId);
    Task<ServiceResult<ProjectDto>> UpdateProject(ProjectDto dto, string userId);
    Task<ServiceResult<bool>> DeleteProject(string projectId, string userId);

    // ─── Search / List ────────────────────────────────────────
    Task<ServiceResult<PaginationDetails<ProjectCardDto>>> SearchProjects(
        string userId, string? keywords, int offset, int limit,
        string? sortByColumn, bool sortAscending, CancellationToken ct);

    // ─── Manager Assignment ───────────────────────────────────
    Task<ServiceResult<ProjectDto>> AssignProjectManager(string projectId, string managerId, string investorId);

    // ─── Timeline ─────────────────────────────────────────────
    Task<ServiceResult<List<TimelineEntryDto>>> GetProjectTimeline(string projectId, string userId);

    // ─── Analytics ────────────────────────────────────────────
    Task<ServiceResult<ProjectAnalyticsDto>> GetProjectAnalytics(string projectId, string userId);
}

public interface IStageDAL
{
    Task<ServiceResult<StageDto>> CreateStage(string projectId, StageCreateDto dto, string userId);
    Task<ServiceResult<StageDto>> UpdateStage(StageDto dto, string userId);
    Task<ServiceResult<bool>> DeleteStage(string stageId, string userId);
    Task<ServiceResult<List<StageDto>>> GetStagesByProjectId(string projectId, string userId);
    Task<ServiceResult<StageDto>> GetStageById(string stageId, string userId);
}

public interface IFundingDAL
{
    Task<ServiceResult<FundingEntryDto>> AddFundingEntry(FundingEntryCreateDto dto, string investorId);
    Task<ServiceResult<FundingEntryDto>> ConfirmFunding(FundingConfirmDto dto, string managerId);
    Task<ServiceResult<List<FundingEntryDto>>> GetFundingByProject(string projectId, string userId);
    Task<ServiceResult<List<FundingEntryDto>>> GetFundingByStage(string stageId, string userId);
    Task<ServiceResult<List<FundingEntryDto>>> GetPendingConfirmations(string managerId);
}

public interface IProgressDAL
{
    Task<ServiceResult<ProgressUpdateDto>> AddProgressUpdate(ProgressUpdateCreateDto dto, string userId);
    Task<ServiceResult<ProgressUpdateDto>> SetApprovalStatus(ProgressApprovalDto dto, string investorId);
    Task<ServiceResult<PaginationDetails<ProgressUpdateDto>>> GetProgressUpdates(
        string projectId, string? stageId, int offset, int limit, string userId, CancellationToken ct);
    Task<ServiceResult<ProgressCommentDto>> AddComment(ProgressCommentCreateDto dto, string userId);
    Task<ServiceResult<List<ProgressCommentDto>>> GetRecentComments(string managerId, int count);
}

public interface IProjectHealthService
{
    decimal CalculateFundingPercentage(decimal? totalBudget, decimal totalFunded);
    decimal CalculateCompletionPercentage(IEnumerable<StageDto> stages);
    int CalculateTimelineVariance(DateTime? expectedEnd, DateTime? revisedEnd);
    RiskLevel CalculateRiskLevel(decimal fundedPct, decimal completedPct,
        DateTime? expectedEnd, DateTime? lastUpdateDate);
}

public interface IPMDashboardDAL
{
    Task<ServiceResult<PMDashboardDto>> GetPMDashboard(string managerId);
}
