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
    private readonly IProjectAccessService _access;
    private readonly IActiveStageService _activeStage;

    public FundingDAL(AssetlenDbContext context, ILogger<FundingDAL> logger,
        IProjectAccessService access, IActiveStageService activeStage)
    {
        _context = context;
        _logger = logger;
        _access = access;
        _activeStage = activeStage;
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

            // A release with no stage named funds whatever the site is working
            // on. "Too many stages combined" is the complaint this whole screen
            // exists to answer (assetlen.md), so a release must never float.
            var stageId = await _activeStage.ResolveAsync(dto.ProjectId, dto.StageId);

            var stage = stageId is null ? null : await _context.tbl_Stages.FindAsync(stageId);
            if (stage == null || stage.ProjectId != dto.ProjectId)
                return ServiceResult<FundingEntryDto>.Failure(
                    new BadRequestException("This project has no stage to fund."));

            // The ledger is kept in one currency so totals mean something, but
            // the figure the funder actually sent stays on the record beside it.
            // Peter funds from abroad; "I sent 4,000" and "UGX 15.1M arrived" are
            // both true and the difference is exactly what the two of them argue
            // about later.
            var declaredCurrency = string.IsNullOrWhiteSpace(dto.Currency)
                ? project.Currency
                : dto.Currency.Trim().ToUpperInvariant();

            var converting = !string.Equals(declaredCurrency, project.Currency, StringComparison.OrdinalIgnoreCase);

            if (converting && dto.ExchangeRate is not > 0)
                return ServiceResult<FundingEntryDto>.Failure(
                    new BadRequestException($"A rate is needed to record {declaredCurrency} against a {project.Currency} project"));

            if (dto.Amount <= 0)
                return ServiceResult<FundingEntryDto>.Failure(new BadRequestException("A release must be more than zero"));

            var rate = converting ? dto.ExchangeRate!.Value : 1m;
            var inProjectCurrency = decimal.Round(dto.Amount * rate, 2, MidpointRounding.AwayFromZero);

            var entry = new tbl_FundingEntry
            {
                ProjectId = dto.ProjectId,
                StageId = stageId,
                Amount = inProjectCurrency,
                DeclaredCurrency = declaredCurrency,
                DeclaredAmount = dto.Amount,
                ExchangeRate = converting ? rate : null,
                PaymentDate = dto.PaymentDate,
                PaidById = investorId,
                Status = FundingStatus.Pending,
                Notes = dto.Notes,
                EvidenceArtifactId = dto.EvidenceArtifactId,
                EvidenceFileName = dto.EvidenceFileName
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

            if (dto.ReceivedAmount is < 0)
                return ServiceResult<FundingEntryDto>.Failure(new BadRequestException("A received amount cannot be negative"));

            entry.ConfirmedById = managerId;
            entry.ConfirmationDate = DateTime.UtcNow;
            entry.ReceiptNote = dto.Notes;

            if (!dto.IsConfirmed)
            {
                entry.Status = FundingStatus.Rejected;
                entry.ReceivedAmount = 0m;
            }
            else
            {
                // Saying "yes, all of it" is one tap and stays one tap — that is
                // the common case and the flow must not tax it. Naming a
                // different figure is the honest answer when charges or a rate
                // ate part of it, and it hands the decision back to the funder
                // rather than quietly writing off the difference.
                entry.ReceivedAmount = dto.ReceivedAmount ?? entry.Amount;

                entry.Status = entry.ReceivedAmount == entry.Amount
                    ? FundingStatus.Confirmed
                    : FundingStatus.AmountQueried;
            }

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

    public async Task<ServiceResult<FundingEntryDto>> SettleFunding(FundingSettleDto dto, string investorId)
    {
        try
        {
            var entry = await _context.tbl_FundingEntries
                .Include(f => f.Project)
                .Include(f => f.Stage)
                .Include(f => f.ConfirmedBy)
                .FirstOrDefaultAsync(f => f.Id == dto.FundingEntryId);

            if (entry == null)
                return ServiceResult<FundingEntryDto>.Failure(new NotFoundException("Funding entry not found"));

            // Only the person whose money it was may write off the difference.
            if (entry.PaidById != investorId && entry.Project?.InvestorId != investorId)
                return ServiceResult<FundingEntryDto>.Failure(new ForbiddenException("Only the funder can accept a shortfall"));

            if (entry.Status != FundingStatus.AmountQueried)
                return ServiceResult<FundingEntryDto>.Failure(new BadRequestException("There is no gap to accept on this release"));

            // Accepting closes it at the figure that actually landed, which is
            // what the stage is really funded by. The declared figure stays on
            // the row so the shortfall is still answerable months later.
            entry.Status = FundingStatus.Settled;
            entry.SettledAt = DateTime.UtcNow;
            entry.SettledById = investorId;
            if (!string.IsNullOrEmpty(dto.Notes)) entry.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var result = MapToDto(entry, entry.Stage?.StageName, null,
                entry.ConfirmedBy != null ? $"{entry.ConfirmedBy.FirstName} {entry.ConfirmedBy.LastName}" : null);

            return ServiceResult<FundingEntryDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error settling funding entry {EntryId}", dto.FundingEntryId);
            return ServiceResult<FundingEntryDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Releases waiting on this reader personally, whichever end of the exchange
    /// they are on: declared-and-unacknowledged for the delivery side, and
    /// reported-short-and-unanswered for the funder. One queue, because a
    /// release stalls just as badly at either end.
    /// </summary>
    public async Task<ServiceResult<List<FundingEntryDto>>> GetFundingNeedingMe(string userId)
    {
        try
        {
            var entries = await _context.tbl_FundingEntries
                .Include(f => f.Project)
                .Include(f => f.PaidBy)
                .Include(f => f.ConfirmedBy)
                .Include(f => f.Stage)
                .Where(f =>
                    (f.Status == FundingStatus.Pending && f.Project!.ProjectManagerId == userId)
                    || (f.Status == FundingStatus.AmountQueried
                        && (f.PaidById == userId || f.Project!.InvestorId == userId)))
                .OrderByDescending(f => f.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            var dtos = entries.Select(e =>
            {
                var dto = MapToDto(e, e.Stage?.StageName,
                    e.PaidBy != null ? $"{e.PaidBy.FirstName} {e.PaidBy.LastName}" : null,
                    e.ConfirmedBy != null ? $"{e.ConfirmedBy.FirstName} {e.ConfirmedBy.LastName}" : null);
                dto.ProjectName = e.Project?.ProjectName;
                return WithActions(dto, userId, e.Project?.ProjectManagerId, e.Project?.InvestorId);
            }).ToList();

            return ServiceResult<List<FundingEntryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting funding awaiting this reader");
            return ServiceResult<List<FundingEntryDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<FundingEntryDto>>> GetFundingByProject(string projectId, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
                return ServiceResult<List<FundingEntryDto>>.Failure(new NotFoundException("Project not found"));

            // Peter's core need: money against progress. Members read; only the
            // investor adds funding and only the manager confirms it.
            if (!await _access.CanReadAsync(project, userId))
                return ServiceResult<List<FundingEntryDto>>.Failure(new ForbiddenException("Access denied"));

            var entries = await _context.tbl_FundingEntries
                .Include(f => f.PaidBy)
                .Include(f => f.ConfirmedBy)
                .Include(f => f.Stage)
                .Where(f => f.ProjectId == projectId)
                .OrderByDescending(f => f.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            var dtos = entries.Select(e => WithActions(
                MapToDto(e, e.Stage?.StageName,
                    e.PaidBy != null ? $"{e.PaidBy.FirstName} {e.PaidBy.LastName}" : null,
                    e.ConfirmedBy != null ? $"{e.ConfirmedBy.FirstName} {e.ConfirmedBy.LastName}" : null),
                userId, project.ProjectManagerId, project.InvestorId
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
                    .ThenInclude(p => p!.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return ServiceResult<List<FundingEntryDto>>.Failure(new NotFoundException("Stage not found"));

            if (!await _access.CanReadAsync(stage.Project, userId))
                return ServiceResult<List<FundingEntryDto>>.Failure(new ForbiddenException("Access denied"));

            var entries = await _context.tbl_FundingEntries
                .Include(f => f.PaidBy)
                .Include(f => f.ConfirmedBy)
                .Where(f => f.StageId == stageId)
                .OrderByDescending(f => f.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            var dtos = entries.Select(e => WithActions(
                MapToDto(e, stage.StageName,
                    e.PaidBy != null ? $"{e.PaidBy.FirstName} {e.PaidBy.LastName}" : null,
                    e.ConfirmedBy != null ? $"{e.ConfirmedBy.FirstName} {e.ConfirmedBy.LastName}" : null),
                userId, stage.Project?.ProjectManagerId, stage.Project?.InvestorId
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

    /// <summary>
    /// Stamp who may act on this release. Being on the money means following it;
    /// moving it belongs to the named party on that side. The endpoints enforce
    /// the same rule — this only stops the UI offering a button that would 403.
    /// </summary>
    private static FundingEntryDto WithActions(
        FundingEntryDto dto, string? viewerId, string? managerId, string? investorId)
    {
        if (string.IsNullOrEmpty(viewerId)) return dto;

        dto.CanConfirm = dto.Status == FundingStatus.Pending
                         && viewerId == managerId
                         && viewerId != dto.PaidById;

        dto.CanSettle = dto.Status == FundingStatus.AmountQueried
                        && (viewerId == dto.PaidById || viewerId == investorId);

        return dto;
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
            DeclaredCurrency = e.DeclaredCurrency,
            DeclaredAmount = e.DeclaredAmount,
            ExchangeRate = e.ExchangeRate,
            ReceivedAmount = e.ReceivedAmount,
            ReceiptNote = e.ReceiptNote,
            EvidenceArtifactId = e.EvidenceArtifactId,
            EvidenceFileName = e.EvidenceFileName,
            SettledAt = e.SettledAt,
            PaidByName = paidByName,
            ConfirmedByName = confirmedByName,
            StageName = stageName,
            DateTimeCreated = e.DateTimeCreated
        };
    }
}
