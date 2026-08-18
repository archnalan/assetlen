using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class PMDashboardDAL : IPMDashboardDAL
{
    private readonly AssetlenDbContext _context;
    private readonly IProjectHealthService _healthService;
    private readonly IFundingDAL _fundingDAL;
    private readonly IProgressDAL _progressDAL;
    private readonly ILogger<PMDashboardDAL> _logger;

    public PMDashboardDAL(
        AssetlenDbContext context,
        IProjectHealthService healthService,
        IFundingDAL fundingDAL,
        IProgressDAL progressDAL,
        ILogger<PMDashboardDAL> logger)
    {
        _context = context;
        _healthService = healthService;
        _fundingDAL = fundingDAL;
        _progressDAL = progressDAL;
        _logger = logger;
    }

    public async Task<ServiceResult<PMDashboardDto>> GetPMDashboard(string managerId)
    {
        try
        {
            // Assigned projects
            var projects = await _context.tbl_Projects_RS
                .Include(p => p.Stages)
                .Include(p => p.FundingEntries.Where(f => (f.Status == FundingStatus.Confirmed || f.Status == FundingStatus.Settled)))
                .Where(p => p.ProjectManagerId == managerId && p.Status == ProjectStatus.Active)
                .OrderByDescending(p => p.DateTimeCreated)
                .AsNoTracking()
                .ToListAsync();

            var cards = projects.Select(project =>
            {
                var totalFunded = project.FundingEntries.Sum(f => f.ReceivedAmount ?? f.Amount);
                var fundedPct = _healthService.CalculateFundingPercentage(project.TotalBudget, totalFunded);
                var stageDtos = project.Stages.Select(s => new StageDto
                {
                    CompletionPercentage = s.CompletionPercentage,
                    BudgetAmount = s.BudgetAmount
                }).ToList();
                var completedPct = _healthService.CalculateCompletionPercentage(stageDtos);
                var currentStage = project.Stages
                    .Where(s => s.Status == StageStatus.InProgress)
                    .OrderBy(s => s.DisplayOrder)
                    .FirstOrDefault();

                return new ProjectCardDto
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName ?? string.Empty,
                    Location = project.Location,
                    FundedPercentage = fundedPct,
                    CompletedPercentage = completedPct,
                    CurrentStageName = currentStage?.StageName ?? "Not Started",
                    RiskLevel = _healthService.CalculateRiskLevel(fundedPct, completedPct,
                        project.RevisedCompletionDate ?? project.ExpectedCompletionDate, null),
                    IsSubscriptionActive = project.IsSubscriptionActive,
                    Status = project.Status,
                    Currency = project.Currency,
                    TotalBudget = project.TotalBudget ?? 0,
                    TotalFunded = totalFunded
                };
            }).ToList();

            // Pending confirmations
            var pendingResult = await _fundingDAL.GetPendingConfirmations(managerId);
            var pending = pendingResult.IsSuccess ? pendingResult.Data : new();

            // Recent comments needing response
            var commentsResult = await _progressDAL.GetRecentComments(managerId, 10);
            var comments = commentsResult.IsSuccess ? commentsResult.Data : new();

            var dashboard = new PMDashboardDto
            {
                AssignedProjects = cards,
                PendingConfirmations = pending,
                RecentComments = comments,
                TotalAssigned = cards.Count,
                PendingCount = pending.Count,
                CommentsNeedingResponse = comments.Count
            };

            return ServiceResult<PMDashboardDto>.Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PM dashboard for {ManagerId}", managerId);
            return ServiceResult<PMDashboardDto>.Failure(new ServerErrorException(ex.Message));
        }
    }
}
