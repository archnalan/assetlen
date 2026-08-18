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
            //
            // Archived projects are absent from every branch. A binned project
            // takes its sub-projects with it — a guest wing whose house is in
            // the bin has nowhere to be — so the parent's ArchivedAt is checked
            // as well as the project's own.
            var projects = await _context.tbl_Projects_RS
                .Include(p => p.Stages)
                .Include(p => p.FundingEntries.Where(f => f.Status == FundingStatus.Confirmed))
                .Include(p => p.ProgressUpdates.OrderByDescending(u => u.DateTimeCreated).Take(3))
                    .ThenInclude(u => u.Images.OrderByDescending(i => i.DisplayOrder).Take(3))
                .Where(p => p.ArchivedAt == null
                            && (p.ParentProject == null || p.ParentProject.ArchivedAt == null))
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

            var visibleIds = projects.Select(p => p.Id).ToList();

            // A plain count of what is open on the project — not of what this
            // reader personally owes, which the attention scan reports.
            var openIssues = await _context.tbl_Flags
                .Where(f => f.Status == FlagStatus.Open && f.ProjectId != null && visibleIds.Contains(f.ProjectId))
                .GroupBy(f => f.ProjectId!)
                .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToDictionaryAsync(x => x.ProjectId, x => x.Count);

            // This reader's own arrangement; absent rows mean "never dragged".
            var prefs = await _context.tbl_ProjectPreferences
                .Where(p => p.UserId == userId && p.ProjectId != null && visibleIds.Contains(p.ProjectId))
                .AsNoTracking()
                .ToDictionaryAsync(p => p.ProjectId!, p => p);

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

                prefs.TryGetValue(project.Id, out var pref);

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
                    CoverImageUrl = project.CoverImageUrl,
                    CurrentStageName = currentStage?.StageName ?? "Not Started",
                    RiskLevel = riskLevel,
                    IsSubscriptionActive = project.IsSubscriptionActive,
                    Status = project.Status,
                    Currency = project.Currency,
                    TotalBudget = project.TotalBudget ?? 0,
                    TotalFunded = totalFunded,
                    ParentProjectId = project.ParentProjectId,
                    OpenIssueCount = openIssues.TryGetValue(project.Id, out var open) ? open : 0,
                    IsPinned = pref?.IsPinned ?? false,
                    SortOrder = pref?.SortOrder ?? int.MaxValue
                });
            }

            // Nest sub-projects under their parent (one level only)
            var byId = cards.ToDictionary(c => c.Id);
            var topLevel = new List<ProjectCardDto>();
            foreach (var card in cards)
            {
                if (card.ParentProjectId != null && byId.TryGetValue(card.ParentProjectId, out var parent))
                {
                    InheritCover(parent, card);
                    parent.SubProjects.Add(card);
                    parent.SubProjectCount = parent.SubProjects.Count;
                }
                else
                {
                    topLevel.Add(card);
                }
            }

            ApplyArrangement(topLevel, prefs);

            // Counts are of ENGAGEMENTS, not rows.
            //
            // A sub-project is part of its parent — it enlarges the parent's
            // billable area rather than becoming a second invoice (assetlen.md
            // §10.3 rule 1). Counting it separately told the developer he was
            // running two projects when he was running one house with a guest
            // wing, and that miscount is what the whole home screen was
            // designed around. Money and completion still roll up across
            // everything, because the wing's stages really are funded work.
            var summary = new PortfolioSummaryDto
            {
                TotalCapitalDeployed = cards.Sum(c => c.TotalFunded),
                ActiveProjectsCount = topLevel.Count(c => c.Status == ProjectStatus.Active),
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

    // ─── Arrangement, covers and the bin ──────────────────────
    // Everything below is per reader or per project chrome. None of it changes
    // a commitment, a figure or a date — which is why the gates here are about
    // *reaching* the project rather than about who may rewrite the record.

    /// <summary>
    /// A wing wears the house's face until it is given one of its own — an
    /// anonymous glyph indented under the parent's cover reads as an unrelated
    /// project, which is the misreading sub-projects exist to prevent. Flagged
    /// rather than copied, so clearing the wing's image never touches the house's.
    /// </summary>
    private static void InheritCover(ProjectCardDto parent, ProjectCardDto child)
    {
        if (child.RecentImageUrls.Count > 0 || !string.IsNullOrEmpty(child.LatestImageUrl)) return;

        var borrowed = parent.CoverImageUrl
            ?? parent.RecentImageUrls.FirstOrDefault()
            ?? parent.LatestImageUrl;

        if (string.IsNullOrEmpty(borrowed)) return;

        child.LatestImageUrl = borrowed;
        child.RecentImageUrls = new List<string> { borrowed };
        child.CoverInherited = true;
    }

    /// <summary>
    /// Pins first (oldest pin at the top), then the reader's dragged order.
    /// A project with no preference row has never been touched and sorts behind
    /// everything that has, which is what puts a just-created project at the
    /// bottom of the list where its creator last saw it.
    /// </summary>
    private static void ApplyArrangement(
        List<ProjectCardDto> topLevel,
        IReadOnlyDictionary<string, tbl_ProjectPreference> prefs)
    {
        topLevel.Sort((a, b) =>
        {
            if (a.IsPinned != b.IsPinned) return a.IsPinned ? -1 : 1;

            if (a.IsPinned && b.IsPinned)
            {
                var pa = prefs.TryGetValue(a.Id, out var x) ? x.PinnedAt : null;
                var pb = prefs.TryGetValue(b.Id, out var y) ? y.PinnedAt : null;
                return (pa ?? DateTime.MaxValue).CompareTo(pb ?? DateTime.MaxValue);
            }

            if (a.SortOrder != b.SortOrder) return a.SortOrder.CompareTo(b.SortOrder);

            // Both untouched: newest first, matching the pre-arrangement order.
            return Nullable.Compare(b.LastUpdateDate, a.LastUpdateDate) is var byUpdate && byUpdate != 0
                ? byUpdate
                : string.Compare(a.ProjectName, b.ProjectName, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Record the order a reader dropped their projects into. Only projects the
    /// caller can reach are written, so a crafted payload cannot create a
    /// preference row pointing at someone else's engagement.
    /// </summary>
    public async Task<ServiceResult<bool>> ReorderProjects(ProjectOrderUpdateDto dto, string userId)
    {
        try
        {
            var ids = dto.Items
                .Select(i => i.ProjectId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (ids.Count == 0) return ServiceResult<bool>.Success(true);

            var reachable = await ReachableProjectIds(ids, userId);
            if (reachable.Count == 0)
                return ServiceResult<bool>.Failure(new ForbiddenException("Access denied"));

            var existing = await _context.tbl_ProjectPreferences
                .Where(p => p.UserId == userId && p.ProjectId != null && ids.Contains(p.ProjectId))
                .ToDictionaryAsync(p => p.ProjectId!, p => p);

            foreach (var item in dto.Items)
            {
                if (!reachable.Contains(item.ProjectId)) continue;

                if (existing.TryGetValue(item.ProjectId, out var pref))
                {
                    pref.SortOrder = item.SortOrder;
                }
                else
                {
                    _context.tbl_ProjectPreferences.Add(new tbl_ProjectPreference
                    {
                        UserId = userId,
                        ProjectId = item.ProjectId,
                        SortOrder = item.SortOrder
                    });
                }
            }

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering projects for {UserId}", userId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Pin a project above the draggable list, or unpin it. The
    /// <see cref="tbl_ProjectPreference.MaxPins"/> cap is enforced here — a
    /// client-side limit is a suggestion.
    /// </summary>
    public async Task<ServiceResult<bool>> SetProjectPinned(string projectId, bool pinned, string userId)
    {
        try
        {
            var project = await LoadProjectWithParent(projectId);
            if (project is null)
                return ServiceResult<bool>.Failure(new NotFoundException("Project not found"));
            if (!await _access.CanReadAsync(project, userId))
                return ServiceResult<bool>.Failure(new ForbiddenException("Access denied"));

            // A wing is drawn under its house wherever the house sits.
            if (!string.IsNullOrEmpty(project.ParentProjectId))
                return ServiceResult<bool>.Failure(new BadRequestException(
                    "A sub-project sits with its parent. Pin the parent instead."));

            var pref = await _context.tbl_ProjectPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ProjectId == projectId);

            if (pinned)
            {
                var pinCount = await _context.tbl_ProjectPreferences
                    .CountAsync(p => p.UserId == userId && p.IsPinned && p.ProjectId != projectId);

                if (pinCount >= tbl_ProjectPreference.MaxPins)
                    return ServiceResult<bool>.Failure(new BadRequestException(
                        $"You can pin up to {tbl_ProjectPreference.MaxPins} projects. Unpin one first."));
            }

            if (pref is null)
            {
                pref = new tbl_ProjectPreference
                {
                    UserId = userId,
                    ProjectId = projectId,
                    SortOrder = int.MaxValue
                };
                _context.tbl_ProjectPreferences.Add(pref);
            }

            pref.IsPinned = pinned;
            pref.PinnedAt = pinned ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinning project {ProjectId}", projectId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Give a project a face, or take it away with a null artifact. Stored as
    /// the artifact's address, never a copy of the bytes (CLAUDE.md §1), so a
    /// withdrawn image stops rendering everywhere at once.
    /// </summary>
    public async Task<ServiceResult<ProjectDto>> SetProjectCover(ProjectCoverUpdateDto dto, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project is null)
                return ServiceResult<ProjectDto>.Failure(new NotFoundException("Project not found"));
            if (!await _access.CanWriteAsync(project, userId))
                return ServiceResult<ProjectDto>.Failure(new ForbiddenException("Access denied"));

            if (string.IsNullOrEmpty(dto.ArtifactId))
            {
                project.CoverImageUrl = null;
            }
            else
            {
                // The artifact must belong to this engagement, or the cover
                // addresses a file the stream endpoint will 403 on every paint.
                var artifact = await _context.tbl_Artifacts
                    .Where(a => a.Id == dto.ArtifactId)
                    .Select(a => new { a.Id, a.ProjectId })
                    .FirstOrDefaultAsync();

                if (artifact is null)
                    return ServiceResult<ProjectDto>.Failure(new NotFoundException("That image is no longer stored."));

                var rootId = project.ParentProjectId ?? project.Id;
                if (artifact.ProjectId != project.Id && artifact.ProjectId != rootId)
                    return ServiceResult<ProjectDto>.Failure(new BadRequestException(
                        "That image belongs to another project."));

                project.CoverImageUrl = $"/api/Artifacts/{artifact.Id}/thumbnail";
            }

            await _context.SaveChangesAsync();
            return await GetProjectById(project.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cover on {ProjectId}", dto.ProjectId);
            return ServiceResult<ProjectDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Send a project to the bin. Nothing underneath is touched for
    /// <see cref="tbl_Project.ArchiveRetentionDays"/> days — this is what the
    /// delete gesture does, because a product whose proposition is that the
    /// record survives cannot destroy one on a single confirmation.
    /// </summary>
    public async Task<ServiceResult<bool>> ArchiveProject(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
                return ServiceResult<bool>.Failure(new NotFoundException("Project not found"));

            // Binning an engagement is the owner's call. A mediator curates what
            // crosses; they do not get to make the record disappear.
            if (project.InvestorId != userId)
                return ServiceResult<bool>.Failure(new ForbiddenException(
                    "Only the account that owns this project can archive it."));

            if (project.ArchivedAt is not null)
                return ServiceResult<bool>.Success(true);

            var now = DateTime.UtcNow;
            project.ArchivedAt = now;
            project.ArchivedById = userId;

            var children = await _context.tbl_Projects_RS
                .Where(p => p.ParentProjectId == projectId && p.ArchivedAt == null)
                .ToListAsync();

            foreach (var child in children)
            {
                child.ArchivedAt = now;
                child.ArchivedById = userId;
            }

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving project {ProjectId}", projectId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>Take a project back out of the bin, with everything it went in with.</summary>
    public async Task<ServiceResult<bool>> RestoreProject(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
                return ServiceResult<bool>.Failure(new NotFoundException("Project not found"));
            if (project.InvestorId != userId)
                return ServiceResult<bool>.Failure(new ForbiddenException(
                    "Only the account that owns this project can restore it."));

            // Restoring a wing whose house is still binned would put a row in
            // the rail with nothing above it.
            if (!string.IsNullOrEmpty(project.ParentProjectId))
            {
                var parentArchived = await _context.tbl_Projects_RS
                    .AnyAsync(p => p.Id == project.ParentProjectId && p.ArchivedAt != null);

                if (parentArchived)
                    return ServiceResult<bool>.Failure(new BadRequestException(
                        "Restore the parent project first — this sub-project went into the bin with it."));
            }

            project.ArchivedAt = null;
            project.ArchivedById = null;

            var children = await _context.tbl_Projects_RS
                .Where(p => p.ParentProjectId == projectId && p.ArchivedAt != null)
                .ToListAsync();

            foreach (var child in children)
            {
                child.ArchivedAt = null;
                child.ArchivedById = null;
            }

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring project {ProjectId}", projectId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>What is in this reader's bin, and how long each item has left.</summary>
    public async Task<ServiceResult<List<ProjectCardDto>>> GetArchivedProjects(string userId)
    {
        try
        {
            var projects = await _context.tbl_Projects_RS
                .Include(p => p.Stages)
                .Where(p => p.ArchivedAt != null)
                .Where(p =>
                    p.InvestorId == userId
                    || p.ProjectManagerId == userId
                    || (p.ParentProject != null
                        && (p.ParentProject.InvestorId == userId || p.ParentProject.ProjectManagerId == userId))
                    || _context.tbl_ProjectMembers.Any(m =>
                        m.ProjectId == p.Id && m.UserId == userId && m.IsActive))
                .OrderByDescending(p => p.ArchivedAt)
                .AsNoTracking()
                .ToListAsync();

            var archiverIds = projects
                .Select(p => p.ArchivedById)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var archivers = await _context.Users
                .Where(u => archiverIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
                .AsNoTracking()
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } n ? n : u.Email);

            var now = DateTime.UtcNow;

            var cards = projects.Select(project =>
            {
                var stageDtos = project.Stages.Select(s => new StageDto
                {
                    CompletionPercentage = s.CompletionPercentage,
                    BudgetAmount = s.BudgetAmount
                }).ToList();

                var purgeDue = project.PurgeDueAt;

                return new ProjectCardDto
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName ?? string.Empty,
                    Location = project.Location,
                    CompletedPercentage = _healthService.CalculateCompletionPercentage(stageDtos),
                    LatestImageUrl = project.CoverImageUrl,
                    CoverImageUrl = project.CoverImageUrl,
                    Status = project.Status,
                    Currency = project.Currency,
                    TotalBudget = project.TotalBudget ?? 0,
                    ParentProjectId = project.ParentProjectId,
                    RiskLevel = RiskLevel.Green,
                    ArchivedAt = project.ArchivedAt,
                    ArchivedByName = project.ArchivedById is { } id && archivers.TryGetValue(id, out var name)
                        ? name : null,
                    PurgeDueAt = purgeDue,
                    DaysUntilPurge = purgeDue is null
                        ? null
                        : Math.Max(0, (int)Math.Ceiling((purgeDue.Value - now).TotalDays))
                };
            }).ToList();

            // Nest, so a house and its wing are one entry in the bin.
            var byId = cards.ToDictionary(c => c.Id);
            var roots = new List<ProjectCardDto>();

            foreach (var card in cards)
            {
                if (card.ParentProjectId != null && byId.TryGetValue(card.ParentProjectId, out var parent))
                {
                    InheritCover(parent, card);
                    parent.SubProjects.Add(card);
                    parent.SubProjectCount = parent.SubProjects.Count;
                }
                else
                {
                    roots.Add(card);
                }
            }

            return ServiceResult<List<ProjectCardDto>>.Success(roots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading the archive for {UserId}", userId);
            return ServiceResult<List<ProjectCardDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Ids from <paramref name="candidates"/> the caller may actually reach.
    /// Same rule as the dashboard, asked once for a whole batch.
    /// </summary>
    private async Task<HashSet<string>> ReachableProjectIds(List<string> candidates, string userId)
    {
        var ids = await _context.tbl_Projects_RS
            .Where(p => candidates.Contains(p.Id))
            .Where(p =>
                p.InvestorId == userId
                || p.ProjectManagerId == userId
                || (p.ParentProject != null
                    && (p.ParentProject.InvestorId == userId || p.ParentProject.ProjectManagerId == userId))
                || _context.tbl_ProjectMembers.Any(m =>
                    m.ProjectId == p.Id && m.UserId == userId && m.IsActive))
            .Select(p => p.Id)
            .AsNoTracking()
            .ToListAsync();

        return new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Load with the parent attached, which is what <see cref="IProjectAccessService"/> needs to resolve a sub-project.</summary>
    private Task<tbl_Project?> LoadProjectWithParent(string projectId)
        => _context.tbl_Projects_RS
            .Include(p => p.ParentProject)
            .FirstOrDefaultAsync(p => p.Id == projectId);

    // ─── Create Project ───────────────────────────────────────

    public async Task<ServiceResult<ProjectDto>> CreateProject(ProjectCreateDto dto, string investorId)
    {
        try
        {
            // One-level nesting: a Sub-project's parent must itself be a root project.
            string? ownerTenantId = null;
            if (!string.IsNullOrEmpty(dto.ParentProjectId))
            {
                var parent = await _context.tbl_Projects_RS
                    .Where(p => p.Id == dto.ParentProjectId)
                    .Select(p => new { p.Id, p.ParentProjectId, p.OwnerTenantId })
                    .FirstOrDefaultAsync();
                if (parent is null)
                    return ServiceResult<ProjectDto>.Failure(new NotFoundException("Parent project not found."));
                if (!string.IsNullOrEmpty(parent.ParentProjectId))
                    return ServiceResult<ProjectDto>.Failure(new BadRequestException(
                        "Sub-projects cannot be nested. The selected parent is already a Sub-project."));

                // A guest wing belongs to whoever owns the house, not to the
                // contractor who happened to add it. It also bills through the
                // parent, so a differing owner here would split one engagement
                // across two accounts.
                ownerTenantId = parent.OwnerTenantId;
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
                // Null on a root project: the DbContext stamps the creating
                // account. A sub-project inherits its parent's owner.
                OwnerTenantId = ownerTenantId,
                Currency = dto.Currency,
                IsFirstFreeProject = isFirstFree,
                IsSubscriptionActive = isFirstFree || isSubProject,
                Status = ProjectStatus.Active
            };

            _context.tbl_Projects_RS.Add(project);
            await _context.SaveChangesAsync();

            // Seat the creator. A root project must never exist without a
            // mediator: the mediator is the only person who can expose Site Diary
            // material to the client side, so a project with none would leave
            // the client silently dark. The developer holds the seat until they
            // delegate it — assetlen.md D5, "Peter appoints the mediator".
            //
            // Sub-projects are deliberately not seated: ProjectAccessService
            // resolves membership parent-aware, so a second row would be a
            // duplicate that can drift out of step with the parent's roster.
            if (!isSubProject)
            {
                _context.tbl_ProjectMembers.Add(new tbl_ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = investorId,
                    Specialization = ProjectMemberSpecialization.ClientOwner,
                    Side = ProjectSide.Client,
                    IsMediator = true,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    AssignedById = investorId
                });

                // A project manager named at creation joins the delivery side
                // but does NOT get the mediator seat. Appointing a mediator is
                // a deliberate act with one accountable name on it (§10.1); it
                // should not happen as a side effect of filling in a field.
                if (!string.IsNullOrEmpty(dto.ProjectManagerId) && dto.ProjectManagerId != investorId)
                {
                    _context.tbl_ProjectMembers.Add(new tbl_ProjectMember
                    {
                        ProjectId = project.Id,
                        UserId = dto.ProjectManagerId,
                        Specialization = ProjectMemberSpecialization.Lead,
                        Side = ProjectSide.Contractor,
                        IsMediator = false,
                        IsActive = true,
                        JoinedAt = DateTime.UtcNow,
                        AssignedById = investorId
                    });
                }

                await _context.SaveChangesAsync();
            }

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
                    .Where(sp => sp.IsDeleted != true && sp.ArchivedAt == null)
                    .OrderBy(sp => sp.ProjectName)
                    .Select(sp => new ProjectCardDto
                    {
                        Id = sp.Id,
                        ProjectName = sp.ProjectName ?? "",
                        Location = sp.Location,
                        LatestImageUrl = sp.CoverImageUrl,
                        CoverImageUrl = sp.CoverImageUrl,
                        RecentImageUrls = string.IsNullOrEmpty(sp.CoverImageUrl)
                            ? new List<string>()
                            : new List<string> { sp.CoverImageUrl },
                        Status = sp.Status,
                        Currency = sp.Currency,
                        TotalBudget = sp.TotalBudget ?? 0,
                        ParentProjectId = sp.ParentProjectId,
                        RiskLevel = RiskLevel.Green
                    })
                    .ToList(),
                ArchivedAt = project.ArchivedAt,
                PurgeDueAt = project.PurgeDueAt
            };

            // A wing with no face of its own wears the house's, here as well as
            // on the dashboard — the project page and the rail must not disagree
            // about what a sub-project looks like.
            var parentFace = new ProjectCardDto
            {
                CoverImageUrl = project.CoverImageUrl,
                LatestImageUrl = project.CoverImageUrl
            };
            foreach (var sub in dto.SubProjects) InheritCover(parentFace, sub);

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

    /// <summary>
    /// Empty one project out of the bin ahead of its thirty days. <b>It must
    /// already be archived</b> — this was once the delete button's whole
    /// implementation, one call from an engagement's record being gone, and the
    /// precondition is what keeps that path unreachable.
    /// </summary>
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

            if (project.ArchivedAt is null)
                return ServiceResult<bool>.Failure(new BadRequestException(
                    "Archive this project first. Deleting goes through the bin so it can be undone."));

            await PurgeArchivedTree(project);
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", projectId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Soft-delete an archived project and everything hanging off it. Rows keep
    /// their bytes and gain <c>IsDeleted</c>, so an operator can still answer
    /// "what was on this engagement". Artifacts are left alone — they are shared
    /// by hash, and removing one would blank a photograph a live project points at.
    /// Callers must save; the whole tree in one <c>SaveChanges</c> keeps it atomic.
    /// </summary>
    /// <remarks>
    /// Filters are bypassed because the sweep runs on a timer with no request
    /// behind it: the ambient tenant is empty, so the tenancy filter would match
    /// nothing and the bin would fill and never empty, with no error anywhere.
    /// </remarks>
    private async Task PurgeArchivedTree(tbl_Project root)
    {
        var ids = new List<string> { root.Id };

        var children = await _context.tbl_Projects_RS
            .IgnoreQueryFilters()
            .Where(p => p.ParentProjectId == root.Id)
            .ToListAsync();

        ids.AddRange(children.Select(c => c.Id));

        var stages = await _context.tbl_Stages.IgnoreQueryFilters().Where(s => ids.Contains(s.ProjectId!)).ToListAsync();
        var funding = await _context.tbl_FundingEntries.IgnoreQueryFilters().Where(f => ids.Contains(f.ProjectId!)).ToListAsync();
        var updates = await _context.tbl_ProgressUpdates.IgnoreQueryFilters().Where(u => ids.Contains(u.ProjectId!)).ToListAsync();
        var flags = await _context.tbl_Flags.IgnoreQueryFilters().Where(f => ids.Contains(f.ProjectId!)).ToListAsync();
        var members = await _context.tbl_ProjectMembers.IgnoreQueryFilters().Where(m => ids.Contains(m.ProjectId!)).ToListAsync();
        var budgets = await _context.tbl_BudgetLineItems.IgnoreQueryFilters().Where(b => ids.Contains(b.ProjectId!)).ToListAsync();
        var subs = await _context.tbl_ProjectSubscriptions.IgnoreQueryFilters().Where(s => ids.Contains(s.ProjectId!)).ToListAsync();
        var prefs = await _context.tbl_ProjectPreferences.IgnoreQueryFilters().Where(p => ids.Contains(p.ProjectId!)).ToListAsync();

        var updateIds = updates.Select(u => u.Id).ToList();
        var images = await _context.tbl_ProgressImages
            .IgnoreQueryFilters()
            .Where(i => updateIds.Contains(i.ProgressUpdateId!))
            .ToListAsync();

        foreach (var e in stages.Cast<IBaseEntity>()
            .Concat(funding).Concat(updates).Concat(images).Concat(flags)
            .Concat(members).Concat(budgets).Concat(subs).Concat(prefs))
        {
            e.IsDeleted = true;
        }

        foreach (var child in children) child.IsDeleted = true;
        root.IsDeleted = true;
    }

    /// <summary>
    /// Sweep the bin: soft-delete every archived project whose thirty days are
    /// up. On a timer only — a dashboard load must not become a write because a
    /// clock ticked.
    /// </summary>
    public async Task<ServiceResult<int>> PurgeExpiredArchives(CancellationToken ct = default)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-tbl_Project.ArchiveRetentionDays);

            var due = await _context.tbl_Projects_RS
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted != true)
                .Where(p => p.ArchivedAt != null && p.ArchivedAt <= cutoff && p.ParentProjectId == null)
                .ToListAsync(ct);

            foreach (var project in due) await PurgeArchivedTree(project);

            if (due.Count > 0) await _context.SaveChangesAsync(ct);
            return ServiceResult<int>.Success(due.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Archive sweep failed");
            return ServiceResult<int>.Failure(new ServerErrorException(ex.Message));
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
                .Where(p => p.ArchivedAt == null
                            && (p.ParentProject == null || p.ParentProject.ArchivedAt == null))
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
