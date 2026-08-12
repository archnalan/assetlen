using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

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
    Task<ServiceResult<ProgressUpdateDto>> GetProgressUpdate(string updateId, string userId);
    Task<ServiceResult<ProgressUpdateDto>> SetApprovalStatus(ProgressApprovalDto dto, string investorId);
    Task<ServiceResult<ProgressUpdateDto>> SetChannel(string updateId, Channel channel, string userId);

    /// <summary>
    /// Expose or withdraw **individual frames** on an entry. The mediator's core
    /// curation gesture: share three of eighteen rather than flipping the whole
    /// batch across, which is what forwarding already does badly.
    /// </summary>
    Task<ServiceResult<ProgressUpdateDto>> SetImageChannel(ProgressImageExposureDto dto, string userId);
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

public interface IProjectMemberDAL
{
    Task<ServiceResult<ProjectMemberDto>> AddMember(ProjectMemberCreateDto dto, string actingUserId);
    Task<ServiceResult<List<ProjectMemberDto>>> GetMembersByProject(string projectId, string actingUserId);
    Task<ServiceResult<bool>> DeactivateMember(string memberId, string actingUserId);
}

public interface IFlagDAL
{
    Task<ServiceResult<FlagDto>> AddFlag(FlagCreateDto dto, string actingUserId);
    Task<ServiceResult<FlagDto>> GetFlag(string flagId, string actingUserId);
    Task<ServiceResult<List<FlagDto>>> GetFlagsByProject(string projectId, FlagStatus? status, string actingUserId);
    Task<ServiceResult<List<FlagDto>>> GetFlagsByEntry(string progressUpdateId, string actingUserId);
    Task<ServiceResult<FlagDto>> UpdateFlag(FlagUpdateDto dto, string actingUserId);
    Task<ServiceResult<FlagDto>> ResolveFlag(string flagId, string actingUserId);
    Task<ServiceResult<FlagDto>> NudgeFlag(string flagId, string actingUserId);
}

public interface IBudgetDAL
{
    Task<ServiceResult<ProjectBudgetSummaryDto>> GetSummary(string projectId, string actingUserId);
    Task<ServiceResult<BudgetLineItemDto>> AddLineItem(BudgetLineItemCreateDto dto, string actingUserId);
    Task<ServiceResult<BudgetLineItemDto>> UpdateLineItem(BudgetLineItemUpdateDto dto, string actingUserId);
    Task<ServiceResult<bool>> DeleteLineItem(string lineItemId, string actingUserId);
    Task<ServiceResult<ReceiptDto>> AddReceipt(ReceiptCreateDto dto, string actingUserId);
    Task<ServiceResult<List<ReceiptDto>>> GetReceiptsByLineItem(string lineItemId, string actingUserId);
    Task<ServiceResult<bool>> DeleteReceipt(string receiptId, string actingUserId);
}
