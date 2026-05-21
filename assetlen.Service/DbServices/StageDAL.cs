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

    public StageDAL(AssetlenDbContext context, ILogger<StageDAL> logger)
    {
        _context = context;
        _logger = logger;
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

            var stage = new tbl_Stage
            {
                ProjectId = projectId,
                StageName = dto.StageName,
                Description = dto.Description,
                BudgetAmount = dto.BudgetAmount,
                StartDate = dto.StartDate,
                ExpectedEndDate = dto.ExpectedEndDate,
                DisplayOrder = dto.DisplayOrder > 0 ? dto.DisplayOrder : maxOrder + 1,
                Status = StageStatus.NotStarted
            };

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
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ServiceResult<List<StageDto>>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != userId && project.ProjectManagerId != userId)
                return ServiceResult<List<StageDto>>.Failure(new ForbiddenException("Access denied"));

            var stages = await _context.tbl_Stages
                .Include(s => s.FundingEntries.Where(f => f.Status == FundingStatus.Confirmed))
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            var dtos = stages.Select(s =>
            {
                var funded = s.FundingEntries.Sum(f => f.Amount);
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
                    FundedAmount = funded,
                    FundedPercentage = (s.BudgetAmount ?? 0) > 0
                        ? Math.Round(funded / (s.BudgetAmount ?? 1) * 100, 2) : 0,
                    RemainingBalance = (s.BudgetAmount ?? 0) - funded,
                    DaysAheadOrBehind = s.ExpectedEndDate.HasValue
                        ? (int)(DateTime.UtcNow - s.ExpectedEndDate.Value).TotalDays : 0,
                    DateTimeCreated = s.DateTimeCreated
                };
            }).ToList();

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
                .Include(s => s.FundingEntries.Where(f => f.Status == FundingStatus.Confirmed))
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return ServiceResult<StageDto>.Failure(new NotFoundException("Stage not found"));

            if (stage.Project?.InvestorId != userId && stage.Project?.ProjectManagerId != userId)
                return ServiceResult<StageDto>.Failure(new ForbiddenException("Access denied"));

            var funded = stage.FundingEntries.Sum(f => f.Amount);
            var dto = new StageDto
            {
                Id = stage.Id,
                ProjectId = stage.ProjectId,
                StageName = stage.StageName,
                Description = stage.Description,
                BudgetAmount = stage.BudgetAmount,
                StartDate = stage.StartDate,
                ExpectedEndDate = stage.ExpectedEndDate,
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
