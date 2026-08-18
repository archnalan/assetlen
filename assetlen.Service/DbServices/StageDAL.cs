using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class StageDAL : IStageDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<StageDAL> _logger;
    private readonly IProjectAccessService _access;

    public StageDAL(AssetlenDbContext context, ILogger<StageDAL> logger, IProjectAccessService access)
    {
        _context = context;
        _logger = logger;
        _access = access;
    }

    public async Task<ServiceResult<StageDto>> CreateStage(string projectId, StageCreateDto dto, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS.FindAsync(projectId);
            if (project == null)
                return ServiceResult<StageDto>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != userId && project.ProjectManagerId != userId)
                return ServiceResult<StageDto>.Failure(new ForbiddenException("Access denied"));

            var maxOrder = await _context.tbl_Stages
                .Where(s => s.ProjectId == projectId)
                .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;

            // A catalogue stage brings its own name, detail and phase, so the
            // reader picks from a list rather than typing "Roofing" a fourth
            // slightly different way. Anything they did type still wins.
            var catalogued = StageCatalogue.Find(dto.CatalogueKey);

            if (!string.IsNullOrEmpty(dto.CatalogueKey) && catalogued is null)
                return ServiceResult<StageDto>.Failure(new BadRequestException("That is not a stage in the catalogue."));

            if (catalogued is { } item)
            {
                // The dedup the reader actually notices: the catalogue greys out
                // what is already here, and the server holds the same line for
                // anything that arrives another way.
                var already = await _context.tbl_Stages
                    .AnyAsync(s => s.ProjectId == projectId && s.CatalogueKey == dto.CatalogueKey);

                if (already)
                    return ServiceResult<StageDto>.Failure(
                        new ConflictException($"{item.Name} is already a stage on this project."));
            }

            // One level, as with sub-projects. A sub-stage cannot take a
            // sub-stage — the point is a few major phases with detail folded
            // under them, not a tree.
            if (!string.IsNullOrEmpty(dto.ParentStageId))
            {
                var parent = await _context.tbl_Stages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.ParentStageId);

                if (parent is null || parent.ProjectId != projectId)
                    return ServiceResult<StageDto>.Failure(new BadRequestException("That parent stage is not on this project."));

                if (!string.IsNullOrEmpty(parent.ParentStageId))
                    return ServiceResult<StageDto>.Failure(
                        new BadRequestException("Stages nest one level. Put this under the major stage instead."));
            }

            var stage = new tbl_Stage
            {
                ProjectId = projectId,
                StageName = string.IsNullOrWhiteSpace(dto.StageName) ? catalogued?.Name : dto.StageName,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? catalogued?.Detail : dto.Description,
                BudgetAmount = dto.BudgetAmount,
                StartDate = dto.StartDate,
                ExpectedEndDate = dto.ExpectedEndDate,
                DisplayOrder = dto.DisplayOrder > 0 ? dto.DisplayOrder : maxOrder + 1,
                Status = StageStatus.NotStarted,
                ParentStageId = string.IsNullOrEmpty(dto.ParentStageId) ? null : dto.ParentStageId,
                CatalogueKey = dto.CatalogueKey,
                Phase = catalogued?.Group ?? dto.Phase ?? StageGroup.Custom
            };

            if (string.IsNullOrWhiteSpace(stage.StageName))
                return ServiceResult<StageDto>.Failure(new BadRequestException("A stage needs a name."));

            _context.tbl_Stages.Add(stage);
            await _context.SaveChangesAsync();

            return await GetStageById(stage.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stage");
            return ServiceResult<StageDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<StageDto>> UpdateStage(StageDto dto, string userId)
    {
        try
        {
            var stage = await _context.tbl_Stages
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (stage == null)
                return ServiceResult<StageDto>.Failure(new NotFoundException("Stage not found"));

            if (stage.Project?.InvestorId != userId && stage.Project?.ProjectManagerId != userId)
                return ServiceResult<StageDto>.Failure(new ForbiddenException("Access denied"));

            stage.StageName = dto.StageName;
            stage.Description = dto.Description;
            stage.BudgetAmount = dto.BudgetAmount;
            stage.StartDate = dto.StartDate;
            stage.ExpectedEndDate = dto.ExpectedEndDate;
            stage.ActualEndDate = dto.ActualEndDate;
            stage.CompletionPercentage = dto.CompletionPercentage;
            stage.DisplayOrder = dto.DisplayOrder;
            stage.Status = dto.Status;

            if (dto.CompletionPercentage >= 100)
            {
                stage.Status = StageStatus.Completed;
                stage.ActualEndDate ??= DateTime.UtcNow;
            }
            else if (dto.CompletionPercentage > 0)
            {
                stage.Status = StageStatus.InProgress;
            }

            await _context.SaveChangesAsync();
            return await GetStageById(stage.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stage {StageId}", dto.Id);
            return ServiceResult<StageDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<bool>> DeleteStage(string stageId, string userId)
    {
        try
        {
            var stage = await _context.tbl_Stages
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return ServiceResult<bool>.Failure(new NotFoundException("Stage not found"));

            if (stage.Project?.InvestorId != userId)
                return ServiceResult<bool>.Failure(new ForbiddenException("Only the investor can delete stages"));

            stage.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stage {StageId}", stageId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<StageDto>>> GetStagesByProjectId(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ServiceResult<List<StageDto>>.Failure(new NotFoundException("Project not found"));

            // The clerk needs the stage list to have something to capture against.
            if (!await _access.CanReadAsync(project, userId))
                return ServiceResult<List<StageDto>>.Failure(new ForbiddenException("Access denied"));

            var stages = await _context.tbl_Stages
                .Include(s => s.FundingEntries.Where(f => (f.Status == FundingStatus.Confirmed || f.Status == FundingStatus.Settled)))
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            var dtos = stages.Select(s =>
            {
                var funded = s.FundingEntries.Sum(f => f.ReceivedAmount ?? f.Amount);
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
                    ParentStageId = s.ParentStageId,
                    CatalogueKey = s.CatalogueKey,
                    Phase = s.Phase,
                    FundedAmount = funded,
                    FundedPercentage = (s.BudgetAmount ?? 0) > 0
                        ? Math.Round(funded / (s.BudgetAmount ?? 1) * 100, 2) : 0,
                    RemainingBalance = (s.BudgetAmount ?? 0) - funded,
                    DaysAheadOrBehind = s.ExpectedEndDate.HasValue
                        ? (int)(DateTime.UtcNow - s.ExpectedEndDate.Value).TotalDays : 0,
                    DateTimeCreated = s.DateTimeCreated
                };
            }).ToList();

            // Returned flat AND nested: callers that just need a list of stages
            // to capture against read the flat one, and the screens that show
            // the phases folded read SubStages. Two shapes of one read beats a
            // second endpoint that can disagree with this one.
            var byId = dtos.ToDictionary(d => d.Id!);
            foreach (var dto in dtos)
            {
                if (dto.ParentStageId is { } pid && byId.TryGetValue(pid, out var parent))
                    parent.SubStages.Add(dto);
            }

            return ServiceResult<List<StageDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stages for project {ProjectId}", projectId);
            return ServiceResult<List<StageDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<StageDto>> GetStageById(string stageId, string userId)
    {
        try
        {
            var stage = await _context.tbl_Stages
                .Include(s => s.Project)
                    .ThenInclude(p => p!.ParentProject)
                .Include(s => s.FundingEntries.Where(f => (f.Status == FundingStatus.Confirmed || f.Status == FundingStatus.Settled)))
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return ServiceResult<StageDto>.Failure(new NotFoundException("Stage not found"));

            if (!await _access.CanReadAsync(stage.Project, userId))
                return ServiceResult<StageDto>.Failure(new ForbiddenException("Access denied"));

            var funded = stage.FundingEntries.Sum(f => f.ReceivedAmount ?? f.Amount);
            var dto = new StageDto
            {
                Id = stage.Id,
                ProjectId = stage.ProjectId,
                StageName = stage.StageName,
                Description = stage.Description,
                BudgetAmount = stage.BudgetAmount,
                StartDate = stage.StartDate,
                ExpectedEndDate = stage.ExpectedEndDate,
                ParentStageId = stage.ParentStageId,
                CatalogueKey = stage.CatalogueKey,
                Phase = stage.Phase,
                ActualEndDate = stage.ActualEndDate,
                CompletionPercentage = stage.CompletionPercentage,
                DisplayOrder = stage.DisplayOrder,
                Status = stage.Status,
                FundedAmount = funded,
                FundedPercentage = (stage.BudgetAmount ?? 0) > 0
                    ? Math.Round(funded / (stage.BudgetAmount ?? 1) * 100, 2) : 0,
                RemainingBalance = (stage.BudgetAmount ?? 0) - funded,
                DaysAheadOrBehind = stage.ExpectedEndDate.HasValue
                    ? (int)(DateTime.UtcNow - stage.ExpectedEndDate.Value).TotalDays : 0,
                DateTimeCreated = stage.DateTimeCreated
            };

            return ServiceResult<StageDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stage {StageId}", stageId);
            return ServiceResult<StageDto>.Failure(new ServerErrorException(ex.Message));
        }
    }
}
