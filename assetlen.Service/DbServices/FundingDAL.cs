using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class FundingDAL : IFundingDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<FundingDAL> _logger;

    public FundingDAL(AssetlenDbContext context, ILogger<FundingDAL> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<FundingEntryDto>> AddFundingEntry(FundingEntryCreateDto dto, string investorId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS.FindAsync(dto.ProjectId);
            if (project == null)
                return ServiceResult<FundingEntryDto>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != investorId)
                return ServiceResult<FundingEntryDto>.Failure(new ForbiddenException("Only the investor can add funding"));

            if (!project.IsSubscriptionActive && !project.IsFirstFreeProject)
                return ServiceResult<FundingEntryDto>.Failure(new BadRequestException("Project subscription is inactive"));

            var stage = await _context.tbl_Stages.FindAsync(dto.StageId);
            if (stage == null || stage.ProjectId != dto.ProjectId)
                return ServiceResult<FundingEntryDto>.Failure(new BadRequestException("Invalid stage"));

            var entry = new tbl_FundingEntry
            {
                ProjectId = dto.ProjectId,
                StageId = dto.StageId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                PaidById = investorId,
                Status = FundingStatus.Pending,
                Notes = dto.Notes
            };

            _context.tbl_FundingEntries.Add(entry);
            await _context.SaveChangesAsync();

            return ServiceResult<FundingEntryDto>.Success(MapToDto(entry, stage.StageName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding funding entry");
            return ServiceResult<FundingEntryDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<FundingEntryDto>> ConfirmFunding(FundingConfirmDto dto, string managerId)
    {
        try
        {
            var entry = await _context.tbl_FundingEntries
                .Include(f => f.Project)
                .Include(f => f.Stage)
                .Include(f => f.PaidBy)
                .FirstOrDefaultAsync(f => f.Id == dto.FundingEntryId);

            if (entry == null)
                return ServiceResult<FundingEntryDto>.Failure(new NotFoundException("Funding entry not found"));

            if (entry.Project?.ProjectManagerId != managerId)
                return ServiceResult<FundingEntryDto>.Failure(new ForbiddenException("Only the assigned PM can confirm funding"));

            if (entry.Status != FundingStatus.Pending)
                return ServiceResult<FundingEntryDto>.Failure(new BadRequestException("Funding entry is not pending"));

            entry.Status = dto.IsConfirmed ? FundingStatus.Confirmed : FundingStatus.Rejected;
            entry.ConfirmedById = managerId;
            entry.ConfirmationDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(dto.Notes))
                entry.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var result = MapToDto(entry, entry.Stage?.StageName);
            result.PaidByName = entry.PaidBy != null
                ? $"{entry.PaidBy.FirstName} {entry.PaidBy.LastName}" : null;

            return ServiceResult<FundingEntryDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming funding");
            return ServiceResult<FundingEntryDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<FundingEntryDto>>> GetFundingByProject(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
                return ServiceResult<List<FundingEntryDto>>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != userId && project.ProjectManagerId != userId)
                return ServiceResult<List<FundingEntryDto>>.Failure(new ForbiddenException("Access denied"));

            var entries = await _context.tbl_FundingEntries
                .Include(f => f.PaidBy)
                .Include(f => f.ConfirmedBy)
                .Include(f => f.Stage)
                .Where(f => f.ProjectId == projectId)
                .OrderByDescending(f => f.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            var dtos = entries.Select(e => MapToDto(e, e.Stage?.StageName,
                e.PaidBy != null ? $"{e.PaidBy.FirstName} {e.PaidBy.LastName}" : null,
                e.ConfirmedBy != null ? $"{e.ConfirmedBy.FirstName} {e.ConfirmedBy.LastName}" : null
            )).ToList();

            return ServiceResult<List<FundingEntryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting funding for project");
            return ServiceResult<List<FundingEntryDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<FundingEntryDto>>> GetFundingByStage(string stageId, string userId)
    {
        try
        {
            var stage = await _context.tbl_Stages
                .Include(s => s.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return ServiceResult<List<FundingEntryDto>>.Failure(new NotFoundException("Stage not found"));

            if (stage.Project?.InvestorId != userId && stage.Project?.ProjectManagerId != userId)
                return ServiceResult<List<FundingEntryDto>>.Failure(new ForbiddenException("Access denied"));

            var entries = await _context.tbl_FundingEntries
                .Include(f => f.PaidBy)
                .Include(f => f.ConfirmedBy)
                .Where(f => f.StageId == stageId)
                .OrderByDescending(f => f.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            var dtos = entries.Select(e => MapToDto(e, stage.StageName,
                e.PaidBy != null ? $"{e.PaidBy.FirstName} {e.PaidBy.LastName}" : null,
                e.ConfirmedBy != null ? $"{e.ConfirmedBy.FirstName} {e.ConfirmedBy.LastName}" : null
            )).ToList();

            return ServiceResult<List<FundingEntryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting funding for stage");
            return ServiceResult<List<FundingEntryDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<FundingEntryDto>>> GetPendingConfirmations(string managerId)
    {
        try
        {
            var entries = await _context.tbl_FundingEntries
                .Include(f => f.Project)
                .Include(f => f.PaidBy)
                .Include(f => f.Stage)
                .Where(f => f.Project!.ProjectManagerId == managerId && f.Status == FundingStatus.Pending)
                .OrderByDescending(f => f.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            var dtos = entries.Select(e => MapToDto(e, e.Stage?.StageName,
                e.PaidBy != null ? $"{e.PaidBy.FirstName} {e.PaidBy.LastName}" : null
            )).ToList();

            return ServiceResult<List<FundingEntryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending confirmations");
            return ServiceResult<List<FundingEntryDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    private static FundingEntryDto MapToDto(tbl_FundingEntry e, string? stageName,
        string? paidByName = null, string? confirmedByName = null)
    {
        return new FundingEntryDto
        {
            Id = e.Id,
            ProjectId = e.ProjectId,
            StageId = e.StageId,
            Amount = e.Amount,
            PaymentDate = e.PaymentDate,
            PaidById = e.PaidById,
            ConfirmedById = e.ConfirmedById,
            ConfirmationDate = e.ConfirmationDate,
            Status = e.Status,
            Notes = e.Notes,
            PaidByName = paidByName,
            ConfirmedByName = confirmedByName,
            StageName = stageName,
            DateTimeCreated = e.DateTimeCreated
        };
    }
}
