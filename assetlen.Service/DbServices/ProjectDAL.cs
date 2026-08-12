using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class ProjectDAL : IProjectDAL
{
    private readonly AssetlenDbContext _context;
    private readonly IProjectHealthService _healthService;
    private readonly ILogger<ProjectDAL> _logger;
    private readonly ITenantProvider _tenantProvider;
    private readonly IProjectAccessService _access;

    public ProjectDAL(
        AssetlenDbContext context,
        IProjectHealthService healthService,
        ILogger<ProjectDAL> logger,
        ITenantProvider tenantProvider,
        IProjectAccessService access)
    {
        _context = context;
        _healthService = healthService;
        _logger = logger;
        _tenantProvider = tenantProvider;
        _access = access;
    }

    // ─── Portfolio Dashboard ──────────────────────────────────

    public async Task<ServiceResult<PortfolioSummaryDto>> GetPortfolioDashboard(string userId)
    {
        try
        {
            // Stakeholder set: project owner (InvestorId), assigned PM, or an
            // active project member. Sub-projects inherit visibility from their
            // parent via the OR-on-ParentProject branch.
            var projects = await _context.tbl_Projects_RS
                .Include(p => p.Stages)
                .Include(p => p.FundingEntries.Where(f => f.Status == FundingStatus.Confirmed))
                .Include(p => p.ProgressUpdates.OrderByDescending(u => u.DateTimeCreated).Take(3))
                    .ThenInclude(u => u.Images.OrderByDescending(i => i.DisplayOrder).Take(3))
                .Where(p =>
                    p.InvestorId == userId
                    || p.ProjectManagerId == userId
                    || (p.ParentProject != null
                        && (p.ParentProject.InvestorId == userId || p.ParentProject.ProjectManagerId == userId))
                    || _context.tbl_ProjectMembers.Any(m =>
                        m.ProjectId == p.Id && m.UserId == userId && m.IsActive))
                .OrderByDescending(p => p.DateTimeCreated)
                .AsNoTracking()
                .ToListAsync();

            var cards = new List<ProjectCardDto>();

            foreach (var project in projects)
            {
                var recentImages = project.ProgressUpdates
                    .SelectMany(u => u.Images)
                    .Select(i => i.ThumbnailUrl ?? i.ImageUrl)
                    .Where(u => !string.IsNullOrEmpty(u))
                    .Cast<string>()
                    .Distinct()
                    .Take(5)
                    .ToList();

                if (!string.IsNullOrEmpty(project.CoverImageUrl))
                    recentImages.Insert(0, project.CoverImageUrl);

                var totalFunded = project.FundingEntries.Sum(f => f.Amount);
                var fundedPct = _healthService.CalculateFundingPercentage(project.TotalBudget, totalFunded);

                var stageDtos = project.Stages.Select(s => new StageDto
                {
                    CompletionPercentage = s.CompletionPercentage,
                    BudgetAmount = s.BudgetAmount
                }).ToList();

                var completedPct = _healthService.CalculateCompletionPercentage(stageDtos);
                var lastUpdate = project.ProgressUpdates.FirstOrDefault();
                var currentStage = project.Stages
                    .Where(s => s.Status == StageStatus.InProgress)
                    .OrderBy(s => s.DisplayOrder)
                    .FirstOrDefault();

                var riskLevel = _healthService.CalculateRiskLevel(
                    fundedPct, completedPct,
                    project.RevisedCompletionDate ?? project.ExpectedCompletionDate,
                    lastUpdate?.DateTimeCreated);

                var timelineStatus = riskLevel switch
                {
                    RiskLevel.Green => "On Track",
                    RiskLevel.Yellow => "Needs Attention",
                    RiskLevel.Red => "At Risk",
                    _ => "On Track"
                };

                cards.Add(new ProjectCardDto
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName ?? string.Empty,
                    Location = project.Location,
                    FundedPercentage = fundedPct,
                    CompletedPercentage = completedPct,
                    TimelineStatus = timelineStatus,
                    LastUpdateDate = lastUpdate?.DateTimeCreated,
                    LatestImageUrl = recentImages.FirstOrDefault(),
                    RecentImageUrls = recentImages,
                    CurrentStageName = currentStage?.StageName ?? "Not Started",
                    RiskLevel = riskLevel,
                    IsSubscriptionActive = project.IsSubscriptionActive,
                    Status = project.Status,
                    Currency = project.Currency,
                    TotalBudget = project.TotalBudget ?? 0,
                    TotalFunded = totalFunded,
                    ParentProjectId = project.ParentProjectId
                });
            }

            // Nest sub-projects under their parent (one level only)
            var byId = cards.ToDictionary(c => c.Id);
            var topLevel = new List<ProjectCardDto>();
            foreach (var card in cards)
            {
                if (card.ParentProjectId != null && byId.TryGetValue(card.ParentProjectId, out var parent))
                {
                    parent.SubProjects.Add(card);
                    parent.SubProjectCount = parent.SubProjects.Count;
                }
                else
                {
                    topLevel.Add(card);
                }
            }

            var summary = new PortfolioSummaryDto
            {
                TotalCapitalDeployed = cards.Sum(c => c.TotalFunded),
                ActiveProjectsCount = cards.Count(c => c.Status == ProjectStatus.Active),
                ProjectsAtRiskCount = cards.Count(c => c.RiskLevel == RiskLevel.Red),
                TotalPortfolioCompletion = cards.Count > 0
                    ? Math.Round(cards.Average(c => c.CompletedPercentage), 1)
                    : 0,
                Projects = topLevel
            };

            return ServiceResult<PortfolioSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio dashboard for user {UserId}", userId);
            return ServiceResult<PortfolioSummaryDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Create Project ───────────────────────────────────────

    public async Task<ServiceResult<ProjectDto>> CreateProject(ProjectCreateDto dto, string investorId)
    {
        try
        {
            // One-level nesting: a Sub-project's parent must itself be a root project.
            if (!string.IsNullOrEmpty(dto.ParentProjectId))
            {
                var parent = await _context.tbl_Projects_RS
                    .Where(p => p.Id == dto.ParentProjectId)
                    .Select(p => new { p.Id, p.ParentProjectId })
                    .FirstOrDefaultAsync();
                if (parent is null)
                    return ServiceResult<ProjectDto>.Failure(new NotFoundException("Parent project not found."));
                if (!string.IsNullOrEmpty(parent.ParentProjectId))
                    return ServiceResult<ProjectDto>.Failure(new BadRequestException(
                        "Sub-projects cannot be nested. The selected parent is already a Sub-project."));
            }

            // Sub-projects don't get a separate free-quota slot — they share their parent's subscription.
            var isSubProject = !string.IsNullOrEmpty(dto.ParentProjectId);
            var existingCount = isSubProject ? 1 : await _context.tbl_Projects_RS
                .CountAsync(p => p.InvestorId == investorId && p.ParentProjectId == null);

            var isFirstFree = !isSubProject && existingCount == 0;

            var project = new tbl_Project
            {
                ProjectName = dto.ProjectName,
                Description = dto.Description,
                Location = dto.Location,
                TotalBudget = dto.TotalBudget,
                ExpectedStartDate = dto.ExpectedStartDate,
                ExpectedCompletionDate = dto.ExpectedCompletionDate,
                InvestorId = investorId,
                ProjectManagerId = dto.ProjectManagerId,
                ParentProjectId = dto.ParentProjectId,
                Currency = dto.Currency,
                IsFirstFreeProject = isFirstFree,
                IsSubscriptionActive = isFirstFree || isSubProject,
                Status = ProjectStatus.Active
            };

            _context.tbl_Projects_RS.Add(project);
            await _context.SaveChangesAsync();

            // Create stages if provided
            if (dto.Stages?.Any() == true)
            {
                int order = 1;
                foreach (var stageDto in dto.Stages)
                {
                    var stage = new tbl_Stage
                    {
                        ProjectId = project.Id,
                        StageName = stageDto.StageName,
                        Description = stageDto.Description,
                        BudgetAmount = stageDto.BudgetAmount,
                        StartDate = stageDto.StartDate,
                        ExpectedEndDate = stageDto.ExpectedEndDate,
                        DisplayOrder = stageDto.DisplayOrder > 0 ? stageDto.DisplayOrder : order++,
                        Status = StageStatus.NotStarted
                    };
                    _context.tbl_Stages.Add(stage);
                }
                await _context.SaveChangesAsync();
            }

            // Sub-projects inherit the parent's subscription; no separate record.
            if (isFirstFree && !isSubProject)
            {
                _context.tbl_ProjectSubscriptions.Add(new tbl_ProjectSubscription
                {
                    ProjectId = project.Id,
                    InvestorId = investorId,
                    Status = SubscriptionStatus.Free,
                    MonthlyAmount = 0,
                    Currency = dto.Currency
                });
                await _context.SaveChangesAsync();
            }

            return await GetProjectById(project.Id, investorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return ServiceResult<ProjectDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Get Project By ID ────────────────────────────────────

    public async Task<ServiceResult<ProjectDto>> GetProjectById(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.Investor)
                .Include(p => p.ProjectManager)
                .Include(p => p.ParentProject)
                .Include(p => p.SubProjects)
                .Include(p => p.Stages.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.FundingEntries.Where(f => f.Status == FundingStatus.Confirmed))
                .Include(p => p.FundingEntries)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ServiceResult<ProjectDto>.Failure(new NotFoundException("Project not found"));

            // Owner, manager, or an active member. Sub-projects inherit the parent's.
            if (!await _access.CanReadAsync(project, userId))
                return ServiceResult<ProjectDto>.Failure(new ForbiddenException("Access denied"));

            var totalFunded = project.FundingEntries
                .Where(f => f.Status == FundingStatus.Confirmed)
                .Sum(f => f.Amount);

            var stageDtos = project.Stages.Select(s =>
            {
                var stageFunded = s.FundingEntries.Sum(f => f.Amount);
                return new StageDto
                {
                    Id = s.Id,
                    ProjectId = s.ProjectId,
                    StageName = s.StageName,
                    Description = s.Description,
                    BudgetAmount = s.BudgetAmount,
                    StartDate = s.StartDate,
                    ExpectedEndDate = s.ExpectedEndDate,
                    ActualEndDate = s.ActualEndDate,
                    CompletionPercentage = s.CompletionPercentage,
                    DisplayOrder = s.DisplayOrder,
                    Status = s.Status,
                    FundedAmount = stageFunded,
                    FundedPercentage = (s.BudgetAmount ?? 0) > 0
                        ? Math.Round(stageFunded / (s.BudgetAmount ?? 1) * 100, 2) : 0,
                    RemainingBalance = (s.BudgetAmount ?? 0) - stageFunded,
                    DaysAheadOrBehind = s.ExpectedEndDate.HasValue
                        ? (int)(DateTime.UtcNow - s.ExpectedEndDate.Value).TotalDays : 0,
                    DateTimeCreated = s.DateTimeCreated
                };
            }).ToList();

            var currentStage = project.Stages
                .Where(s => s.Status == StageStatus.InProgress)
                .OrderBy(s => s.DisplayOrder)
                .FirstOrDefault();

            var fundedPct = _healthService.CalculateFundingPercentage(project.TotalBudget, totalFunded);
            var completedPct = _healthService.CalculateCompletionPercentage(stageDtos);

            var dto = new ProjectDto
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                Description = project.Description,
                Location = project.Location,
                TotalBudget = project.TotalBudget,
                ExpectedStartDate = project.ExpectedStartDate,
                ExpectedCompletionDate = project.ExpectedCompletionDate,
                RevisedCompletionDate = project.RevisedCompletionDate,
                InvestorId = project.InvestorId,
                ProjectManagerId = project.ProjectManagerId,
                CoverImageUrl = project.CoverImageUrl,
                Status = project.Status,
                IsFirstFreeProject = project.IsFirstFreeProject,
                IsSubscriptionActive = project.IsSubscriptionActive,
                Currency = project.Currency,
                InvestorName = project.Investor != null
                    ? $"{project.Investor.FirstName} {project.Investor.LastName}" : null,
                ProjectManagerName = project.ProjectManager != null
                    ? $"{project.ProjectManager.FirstName} {project.ProjectManager.LastName}" : null,
                FundedPercentage = fundedPct,
                CompletedPercentage = completedPct,
                TotalFunded = totalFunded,
                TotalRemaining = (project.TotalBudget ?? 0) - totalFunded,
                CurrentStageName = currentStage?.StageName ?? "Not Started",
                RiskLevel = _healthService.CalculateRiskLevel(fundedPct, completedPct,
                    project.RevisedCompletionDate ?? project.ExpectedCompletionDate, null),
                Stages = stageDtos,
                DateTimeCreated = project.DateTimeCreated,
                ParentProjectId = project.ParentProjectId,
                ParentProjectName = project.ParentProject?.ProjectName,
                SubProjects = project.SubProjects
                    .Where(sp => sp.IsDeleted != true)
                    .OrderBy(sp => sp.ProjectName)
                    .Select(sp => new ProjectCardDto
                    {
                        Id = sp.Id,
                        ProjectName = sp.ProjectName ?? "",
                        Location = sp.Location,
                        LatestImageUrl = sp.CoverImageUrl,
                        Status = sp.Status,
                        Currency = sp.Currency,
                        TotalBudget = sp.TotalBudget ?? 0,
                        ParentProjectId = sp.ParentProjectId,
                        RiskLevel = RiskLevel.Green
                    })
                    .ToList()
            };

            return ServiceResult<ProjectDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", projectId);
            return ServiceResult<ProjectDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Update Project ───────────────────────────────────────

    public async Task<ServiceResult<ProjectDto>> UpdateProject(ProjectDto dto, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (project == null)
                return ServiceResult<ProjectDto>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != userId)
                return ServiceResult<ProjectDto>.Failure(new ForbiddenException("Only the investor can update project details"));

            project.ProjectName = dto.ProjectName;
            project.Description = dto.Description;
            project.Location = dto.Location;
            project.TotalBudget = dto.TotalBudget;
            project.ExpectedStartDate = dto.ExpectedStartDate;
            project.ExpectedCompletionDate = dto.ExpectedCompletionDate;
            project.RevisedCompletionDate = dto.RevisedCompletionDate;
            project.CoverImageUrl = dto.CoverImageUrl;
            project.Currency = dto.Currency;

            await _context.SaveChangesAsync();
            return await GetProjectById(project.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", dto.Id);
            return ServiceResult<ProjectDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Delete Project ───────────────────────────────────────

    public async Task<ServiceResult<bool>> DeleteProject(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ServiceResult<bool>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != userId)
                return ServiceResult<bool>.Failure(new ForbiddenException("Only the investor can delete a project"));

            project.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", projectId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Search Projects ──────────────────────────────────────

    public async Task<ServiceResult<PaginationDetails<ProjectCardDto>>> SearchProjects(
        string userId, string? keywords, int offset, int limit,
        string? sortByColumn, bool sortAscending, CancellationToken ct)
    {
        try
        {
            // Same visibility rule as the dashboard: owner, manager, parent's
            // owner/manager, or an active member. Search must not show less
            // than the dashboard already shows.
            var query = _context.tbl_Projects_RS
                .Include(p => p.Stages)
                .Include(p => p.FundingEntries.Where(f => f.Status == FundingStatus.Confirmed))
                .Where(p =>
                    p.InvestorId == userId
                    || p.ProjectManagerId == userId
                    || (p.ParentProject != null
                        && (p.ParentProject.InvestorId == userId || p.ParentProject.ProjectManagerId == userId))
                    || _context.tbl_ProjectMembers.Any(m =>
                        m.ProjectId == p.Id && m.UserId == userId && m.IsActive))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                var kw = keywords.ToLower();
                query = query.Where(p =>
                    (p.ProjectName != null && p.ProjectName.ToLower().Contains(kw)) ||
                    (p.Location != null && p.Location.ToLower().Contains(kw)));
            }

            var total = await query.CountAsync(ct);

            var projects = await query
                .OrderByDescending(p => p.DateTimeCreated)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var cards = projects.Select(project =>
            {
                var totalFunded = project.FundingEntries.Sum(f => f.Amount);
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
                    TimelineStatus = "On Track",
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

            return ServiceResult<PaginationDetails<ProjectCardDto>>.Success(
                new PaginationDetails<ProjectCardDto>
                {
                    Data = cards,
                    TotalSize = total,
                    Limit = limit,
                    OffSet = offset,
                    IsNext = offset + limit < total
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching projects");
            return ServiceResult<PaginationDetails<ProjectCardDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Assign PM ────────────────────────────────────────────

    public async Task<ServiceResult<ProjectDto>> AssignProjectManager(string projectId, string managerId, string investorId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ServiceResult<ProjectDto>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != investorId)
                return ServiceResult<ProjectDto>.Failure(new ForbiddenException("Only the investor can assign a project manager"));

            project.ProjectManagerId = managerId;
            await _context.SaveChangesAsync();

            return await GetProjectById(projectId, investorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning PM");
            return ServiceResult<ProjectDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Timeline ─────────────────────────────────────────────

    public async Task<ServiceResult<List<TimelineEntryDto>>> GetProjectTimeline(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ServiceResult<List<TimelineEntryDto>>.Failure(new NotFoundException("Project not found"));

            if (!await _access.CanReadAsync(project, userId))
                return ServiceResult<List<TimelineEntryDto>>.Failure(new ForbiddenException("Access denied"));

            var stages = await _context.tbl_Stages
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            var timeline = stages.Select(s =>
            {
                var daysVariance = s.ExpectedEndDate.HasValue
                    ? (int)(DateTime.UtcNow - s.ExpectedEndDate.Value).TotalDays : 0;

                string badge;
                if (s.Status == StageStatus.Completed) badge = "Completed";
                else if (daysVariance > 14) badge = "At Risk";
                else if (daysVariance > 0) badge = "Behind Schedule";
                else badge = "On Track";

                return new TimelineEntryDto
                {
                    StageId = s.Id,
                    StageName = s.StageName ?? string.Empty,
                    PlannedStart = s.StartDate,
                    PlannedEnd = s.ExpectedEndDate,
                    ActualProgress = s.CompletionPercentage ?? 0,
                    Status = s.Status,
                    DaysAheadOrBehind = daysVariance,
                    StatusBadge = badge
                };
            }).ToList();

            return ServiceResult<List<TimelineEntryDto>>.Success(timeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timeline");
            return ServiceResult<List<TimelineEntryDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Analytics ────────────────────────────────────────────

    public async Task<ServiceResult<ProjectAnalyticsDto>> GetProjectAnalytics(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ServiceResult<ProjectAnalyticsDto>.Failure(new NotFoundException("Project not found"));

            if (!await _access.CanReadAsync(project, userId))
                return ServiceResult<ProjectAnalyticsDto>.Failure(new ForbiddenException("Access denied"));

            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            var updates = await _context.tbl_ProgressUpdates
                .Where(u => u.ProjectId == projectId)
                .AsNoTracking()
                .ToListAsync();

            var dto = new ProjectAnalyticsDto
            {
                LastUpdate = updates.OrderByDescending(u => u.DateTimeCreated).FirstOrDefault()?.DateTimeCreated,
                UpdatesThisWeek = updates.Count(u => u.DateTimeCreated >= oneWeekAgo),
                TotalUpdates = updates.Count
            };

            return ServiceResult<ProjectAnalyticsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics");
            return ServiceResult<ProjectAnalyticsDto>.Failure(new ServerErrorException(ex.Message));
        }
    }
}
