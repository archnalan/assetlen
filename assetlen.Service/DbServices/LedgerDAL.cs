using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <summary>
/// The per-stage cost report and the variation register (assetlen.md §6).
/// <para>
/// Exists because of one hour in the evidence thread: <i>"Too many stages
/// combined. I want to know if they were cleared or not"</i> — settled only by
/// meeting in person (whatsapp-evidence.md F3). Every figure here is computed
/// from rows; nothing is stored as a running total that could drift.
/// </para>
/// </summary>
public class LedgerDAL : ILedgerDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<LedgerDAL> _logger;
    private readonly IProjectAccessService _access;
    private readonly IActiveStageService _activeStage;

    public LedgerDAL(AssetlenDbContext context, ILogger<LedgerDAL> logger,
        IProjectAccessService access, IActiveStageService activeStage)
    {
        _context = context;
        _logger = logger;
        _access = access;
        _activeStage = activeStage;
    }

    // ═════════════════════════════════════════════════════════════════════
    // The ledger
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<StageLedgerDto>> GetStageLedger(string projectId, string userId)
    {
        try
        {
            var (project, access, error) = await MoneySeat(projectId, userId);
            if (error is not null) return Fail<StageLedgerDto>(error);

            var stages = await _context.tbl_Stages.AsNoTracking()
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();
            var funding = await _context.tbl_FundingEntries.AsNoTracking()
                .Where(f => f.ProjectId == projectId)
                .ToListAsync();
            var claims = await _context.tbl_StageClaims.AsNoTracking()
                .Where(c => c.ProjectId == projectId)
                .ToListAsync();
            var variations = await _context.tbl_Variations.AsNoTracking()
                .Where(v => v.ProjectId == projectId)
                .ToListAsync();

            // Majors in order, each followed by its own sub-stages: the order a
            // reader walks the build in, and the order a balance is carried.
            var byParent = stages.Where(s => s.ParentStageId != null)
                .GroupBy(s => s.ParentStageId!)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.DisplayOrder).ToList());
            var ordered = new List<tbl_Stage>();
            foreach (var major in stages.Where(s => s.ParentStageId == null || stages.All(p => p.Id != s.ParentStageId))
                                        .OrderBy(s => s.DisplayOrder))
            {
                ordered.Add(major);
                if (byParent.TryGetValue(major.Id, out var subs)) ordered.AddRange(subs);
            }

            var rows = new List<StageLedgerRowDto>();
            decimal carry = 0m;
            foreach (var s in ordered)
            {
                var forStage = funding.Where(f => f.StageId == s.Id).ToList();

                // A settled release counts at what landed, not what was sent.
                var funded = forStage
                    .Where(f => f.Status is FundingStatus.Confirmed or FundingStatus.Settled)
                    .Sum(f => f.ReceivedAmount ?? f.Amount);
                var pending = forStage
                    .Where(f => f.Status is FundingStatus.Pending or FundingStatus.AmountQueried)
                    .Sum(f => f.ReceivedAmount ?? f.Amount);

                var stageClaims = claims.Where(c => c.StageId == s.Id && c.Status != ClaimStatus.Withdrawn).ToList();
                var claimed = stageClaims.Sum(c => c.Amount);
                var cleared = stageClaims.Where(c => c.Status == ClaimStatus.Cleared).Sum(c => c.ClearedAmount ?? c.Amount);
                var awaiting = stageClaims.Where(c => c.Status == ClaimStatus.Claimed).Sum(c => c.Amount);

                var stageVariations = variations.Where(v => v.StageId == s.Id).ToList();

                var carriedIn = carry;
                var inHand = carriedIn + funded - cleared;
                var closed = s.Status == StageStatus.Completed;

                // "Issue a receipt and carry the balance towards the next stage"
                // (evidence F3): only a closed stage passes its balance on. An open
                // one keeps it in hand, because the work it pays for is not done.
                carry = closed ? inHand : 0m;

                rows.Add(new StageLedgerRowDto
                {
                    StageId = s.Id,
                    StageName = s.StageName,
                    ParentStageId = s.ParentStageId,
                    DisplayOrder = s.DisplayOrder,
                    Phase = s.Phase,
                    Status = s.Status,
                    Budget = s.BudgetAmount ?? 0m,
                    Funded = funded,
                    PendingFunding = pending,
                    CarriedIn = carriedIn,
                    Claimed = claimed,
                    Cleared = cleared,
                    AwaitingClearance = awaiting,
                    InHand = inHand,
                    CarriedForward = closed ? inHand : null,
                    VariationsApproved = stageVariations.Where(v => v.Status == VariationStatus.Approved).Sum(v => v.CostDelta ?? 0m),
                    VariationsProposed = stageVariations.Count(v => v.Status == VariationStatus.Proposed),
                    VariationsUncosted = stageVariations.Count(v => v.CostDelta is null
                                                                    && v.Status is VariationStatus.Proposed or VariationStatus.Approved)
                });
            }

            var dto = new StageLedgerDto
            {
                ProjectId = projectId,
                Currency = project!.Currency ?? "UGX",
                Rows = rows,
                TotalBudget = rows.Sum(r => r.Budget),
                TotalFunded = rows.Sum(r => r.Funded),
                TotalPending = rows.Sum(r => r.PendingFunding),
                TotalClaimed = rows.Sum(r => r.Claimed),
                TotalCleared = rows.Sum(r => r.Cleared),

                // Conservation: whatever was funded and not cleared is still held
                // somewhere, carried or not. The total never depends on ordering.
                TotalInHand = rows.Sum(r => r.Funded) - rows.Sum(r => r.Cleared),
                TotalVariationsApproved = rows.Sum(r => r.VariationsApproved)
            };

            return ServiceResult<StageLedgerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building stage ledger for {ProjectId}", projectId);
            return Fail<StageLedgerDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Claims
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<List<StageClaimDto>>> GetClaims(string projectId, string? stageId, string userId)
    {
        try
        {
            var (project, access, error) = await MoneySeat(projectId, userId);
            if (error is not null) return Fail<List<StageClaimDto>>(error);

            var query = _context.tbl_StageClaims.AsNoTracking()
                .Include(c => c.Stage)
                .Include(c => c.ClaimedBy)
                .Include(c => c.ClearedBy)
                .Where(c => c.ProjectId == projectId);
            if (!string.IsNullOrEmpty(stageId)) query = query.Where(c => c.StageId == stageId);

            var rows = await query.OrderByDescending(c => c.ClaimedAt).ToListAsync();
            return ServiceResult<List<StageClaimDto>>.Success(rows.Select(c => ToDto(c, access, userId)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing claims for {ProjectId}", projectId);
            return Fail<List<StageClaimDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<StageClaimDto>> AddClaim(StageClaimCreateDto dto, string userId)
    {
        try
        {
            if (dto.Amount <= 0)
                return Fail<StageClaimDto>(new BadRequestException("A claim needs an amount above zero."));

            var (project, access, error) = await MoneySeat(dto.ProjectId, userId);
            if (error is not null) return Fail<StageClaimDto>(error);
            if (!access.CanWrite)
                return Fail<StageClaimDto>(new ForbiddenException("You can follow the money here but not record a claim."));

            var stage = await _context.tbl_Stages.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.StageId);
            if (stage is null || stage.ProjectId != project!.Id)
                return Fail<StageClaimDto>(new BadRequestException("That stage is not on this project."));

            if (!string.IsNullOrEmpty(dto.EvidenceArtifactId)
                && !await _context.tbl_Artifacts.AnyAsync(a => a.Id == dto.EvidenceArtifactId && a.ProjectId == project.Id))
                return Fail<StageClaimDto>(new BadRequestException("That file is not stored on this project."));

            var claim = new tbl_StageClaim
            {
                ProjectId = project.Id,
                StageId = stage.Id,
                Amount = dto.Amount,
                ClaimedAt = dto.ClaimedAt ?? DateTime.UtcNow,
                ClaimedById = userId,
                Note = dto.Note,
                EvidenceArtifactId = string.IsNullOrEmpty(dto.EvidenceArtifactId) ? null : dto.EvidenceArtifactId,
                Status = ClaimStatus.Claimed
            };
            _context.tbl_StageClaims.Add(claim);
            await _context.SaveChangesAsync();

            return await OneClaim(claim.Id, access, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding claim");
            return Fail<StageClaimDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<StageClaimDto>> DecideClaim(StageClaimDecisionDto dto, string userId)
    {
        try
        {
            var claim = await _context.tbl_StageClaims.FirstOrDefaultAsync(c => c.Id == dto.ClaimId);
            if (claim is null) return Fail<StageClaimDto>(new NotFoundException("Claim not found."));

            var (project, access, error) = await MoneySeat(claim.ProjectId, userId);
            if (error is not null) return Fail<StageClaimDto>(error);
            if (!CanDecide(access))
                return Fail<StageClaimDto>(new ForbiddenException("The funder clears a claim. The side claiming never clears its own."));
            if (claim.Status is ClaimStatus.Cleared or ClaimStatus.Withdrawn)
                return Fail<StageClaimDto>(new BadRequestException("This claim is already closed."));

            if (dto.Clear)
            {
                if (dto.ClearedAmount is < 0)
                    return Fail<StageClaimDto>(new BadRequestException("A cleared figure cannot be negative."));
                claim.Status = ClaimStatus.Cleared;
                claim.ClearedAmount = dto.ClearedAmount is { } amt && amt != claim.Amount ? amt : null;
                claim.ClearedAt = DateTime.UtcNow;
                claim.ClearedById = userId;
                if (!string.IsNullOrWhiteSpace(dto.Note)) claim.QueryNote = dto.Note;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Note))
                    return Fail<StageClaimDto>(new BadRequestException("Say what is being queried."));
                claim.Status = ClaimStatus.Queried;
                claim.QueryNote = dto.Note;
            }

            await _context.SaveChangesAsync();
            return await OneClaim(claim.Id, access, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deciding claim {Id}", dto.ClaimId);
            return Fail<StageClaimDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<StageClaimDto>> WithdrawClaim(string claimId, string userId)
    {
        try
        {
            var claim = await _context.tbl_StageClaims.FirstOrDefaultAsync(c => c.Id == claimId);
            if (claim is null) return Fail<StageClaimDto>(new NotFoundException("Claim not found."));

            var (project, access, error) = await MoneySeat(claim.ProjectId, userId);
            if (error is not null) return Fail<StageClaimDto>(error);
            if (claim.ClaimedById != userId && !access.CanManage)
                return Fail<StageClaimDto>(new ForbiddenException("Only whoever recorded a claim can withdraw it."));
            if (claim.Status == ClaimStatus.Cleared)
                return Fail<StageClaimDto>(new BadRequestException("A cleared claim is part of the record. Raise a variation instead."));

            claim.Status = ClaimStatus.Withdrawn;
            await _context.SaveChangesAsync();
            return await OneClaim(claim.Id, access, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing claim {Id}", claimId);
            return Fail<StageClaimDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Variations — every extra is also a commitment
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<List<VariationDto>>> GetVariations(string projectId, string userId)
    {
        try
        {
            var (project, access, error) = await MoneySeat(projectId, userId);
            if (error is not null) return Fail<List<VariationDto>>(error);

            var rows = await _context.tbl_Variations.AsNoTracking()
                .Include(v => v.Stage)
                .Include(v => v.RaisedBy)
                .Include(v => v.ApprovedBy)
                .Where(v => v.ProjectId == projectId)
                .ToListAsync();

            var dtos = rows
                .OrderBy(v => v.Stage?.DisplayOrder ?? int.MaxValue)
                .ThenBy(v => v.RaisedAt)
                .Select(v => ToDto(v, access))
                .ToList();
            return ServiceResult<List<VariationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing variations for {ProjectId}", projectId);
            return Fail<List<VariationDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<VariationDto>> AddVariation(VariationCreateDto dto, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return Fail<VariationDto>(new BadRequestException("Say what changed."));

            var (project, access, error) = await MoneySeat(dto.ProjectId, userId);
            if (error is not null) return Fail<VariationDto>(error);
            if (!access.CanWrite || !access.CanSeeRegister)
                return Fail<VariationDto>(new ForbiddenException("You can follow the money here but not raise an extra."));

            tbl_Commitment? commitment = null;
            if (!string.IsNullOrEmpty(dto.CommitmentId))
            {
                commitment = await _context.tbl_Commitments.FirstOrDefaultAsync(c => c.Id == dto.CommitmentId);
                if (commitment is null || commitment.ProjectId != project!.Id)
                    return Fail<VariationDto>(new BadRequestException("That commitment is not on this project."));
            }

            var stageId = commitment?.StageId ?? await _activeStage.ResolveAsync(project!.Id, dto.StageId);
            var raisedAt = dto.RaisedAt ?? DateTime.UtcNow;
            var currency = dto.Currency ?? project!.Currency ?? "UGX";

            // An extra with no commitment behind it is exactly the unlogged
            // variation the evidence thread lost track of. It goes on the
            // register as an item in discussion until the funder decides.
            if (commitment is null)
            {
                var mediatorId = await _context.tbl_ProjectMembers
                    .Where(m => (m.ProjectId == project!.Id || m.ProjectId == project.ParentProjectId) && m.IsActive && m.IsMediator)
                    .OrderBy(m => m.ProjectId == project!.Id ? 0 : 1)
                    .Select(m => m.Id)
                    .FirstOrDefaultAsync();

                commitment = new tbl_Commitment
                {
                    Id = Guid.NewGuid().ToString(),
                    ProjectId = project!.Id,
                    StageId = stageId,
                    Kind = dto.CostDelta is null ? CommitmentKind.Spec : CommitmentKind.Price,
                    Title = dto.Title.Trim(),
                    Body = dto.Reason,
                    Maturity = CommitmentMaturity.InDiscussion,
                    SourceChannel = CommitmentSource.App,
                    AccountableMemberId = mediatorId,
                    AgreedById = userId,
                    RecordedById = userId,
                    RecordedBySide = access.Side,
                    Amount = dto.CostDelta,
                    Currency = dto.CostDelta is null ? null : currency,
                    OwedBySide = ProjectSide.Client
                };
                _context.tbl_Commitments.Add(commitment);
            }

            var variation = new tbl_Variation
            {
                ProjectId = project!.Id,
                StageId = stageId,
                CommitmentId = commitment.Id,
                Title = dto.Title.Trim(),
                Reason = dto.Reason,
                CostDelta = dto.CostDelta,
                Currency = currency,
                TimeDeltaDays = dto.TimeDeltaDays,
                Status = VariationStatus.Proposed,
                RaisedById = userId,
                RaisedAt = raisedAt
            };
            _context.tbl_Variations.Add(variation);
            await _context.SaveChangesAsync();

            return await OneVariation(variation.Id, access);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding variation");
            return Fail<VariationDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<VariationDto>> DecideVariation(VariationDecisionDto dto, string userId)
    {
        try
        {
            var variation = await _context.tbl_Variations.FirstOrDefaultAsync(v => v.Id == dto.VariationId);
            if (variation is null) return Fail<VariationDto>(new NotFoundException("Variation not found."));

            var (project, access, error) = await MoneySeat(variation.ProjectId, userId);
            if (error is not null) return Fail<VariationDto>(error);
            if (!CanDecide(access))
                return Fail<VariationDto>(new ForbiddenException("The funder approves an extra. The side proposing it never does."));
            if (variation.Status != VariationStatus.Proposed)
                return Fail<VariationDto>(new BadRequestException("This variation has already been decided."));

            var now = DateTime.UtcNow;
            variation.Status = dto.Approve ? VariationStatus.Approved : VariationStatus.Rejected;
            variation.ApprovedById = userId;
            variation.ApprovedAt = now;
            variation.DecisionNote = dto.Note;
            if (dto.Approve && dto.CostDelta is not null) variation.CostDelta = dto.CostDelta;

            // The decision lands on the commitment too, so the register and the
            // money screen never tell the reader two different stories.
            if (variation.CommitmentId is not null)
            {
                var c = await _context.tbl_Commitments.FirstOrDefaultAsync(x => x.Id == variation.CommitmentId);
                if (c is not null && c.SupersededAt is null)
                {
                    if (dto.Approve)
                    {
                        if (c.Maturity < CommitmentMaturity.Agreed) c.Maturity = CommitmentMaturity.Agreed;
                        c.AgreedAt ??= now;
                        if (variation.CostDelta is not null)
                        {
                            c.Amount = variation.CostDelta;
                            c.Currency ??= variation.Currency;
                            c.Kind = CommitmentKind.Price;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(dto.Note))
                    {
                        c.ResolutionNote = $"Variation declined: {dto.Note}";
                    }
                }
            }

            await _context.SaveChangesAsync();
            return await OneVariation(variation.Id, access);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deciding variation {Id}", dto.VariationId);
            return Fail<VariationDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Rules and helpers
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The funder decides: a client-side money seat. Under Law 0 that is the
    /// developer alone, and it still works — he records the claim he was
    /// forwarded and clears it himself.
    /// </summary>
    private static bool CanDecide(ProjectAccess access) =>
        access.CanWrite && access.CanSeeMoney && access.Side == ProjectSide.Client;

    /// <summary>Money is per project and per seat — a support seat is answered as if the ledger did not exist.</summary>
    private async Task<(tbl_Project? Project, ProjectAccess Access, Exception? Error)> MoneySeat(string? projectId, string userId)
    {
        var project = await _context.tbl_Projects_RS
            .Include(p => p.ParentProject)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null) return (null, ProjectAccess.None, new NotFoundException("Project not found."));

        var access = await _access.ResolveAsync(project, userId);
        if (!access.CanSeeMoney) return (null, access, new NotFoundException("Project not found."));
        return (project, access, null);
    }

    private static ServiceResult<T> Fail<T>(Exception e) => ServiceResult<T>.Failure(e);

    private static string? FullName(AppUser? u) =>
        u is null ? null : $"{u.FirstName} {u.LastName}".Trim();

    private async Task<ServiceResult<StageClaimDto>> OneClaim(string id, ProjectAccess access, string userId)
    {
        var c = await _context.tbl_StageClaims.AsNoTracking()
            .Include(x => x.Stage)
            .Include(x => x.ClaimedBy)
            .Include(x => x.ClearedBy)
            .FirstAsync(x => x.Id == id);
        return ServiceResult<StageClaimDto>.Success(ToDto(c, access, userId));
    }

    private async Task<ServiceResult<VariationDto>> OneVariation(string id, ProjectAccess access)
    {
        var v = await _context.tbl_Variations.AsNoTracking()
            .Include(x => x.Stage)
            .Include(x => x.RaisedBy)
            .Include(x => x.ApprovedBy)
            .FirstAsync(x => x.Id == id);
        return ServiceResult<VariationDto>.Success(ToDto(v, access));
    }

    private static StageClaimDto ToDto(tbl_StageClaim c, ProjectAccess access, string userId)
    {
        var open = c.Status is ClaimStatus.Claimed or ClaimStatus.Queried;
        return new StageClaimDto
        {
            Id = c.Id,
            ProjectId = c.ProjectId,
            StageId = c.StageId,
            StageName = c.Stage?.StageName,
            Amount = c.Amount,
            ClaimedAt = c.ClaimedAt,
            ClaimedByName = FullName(c.ClaimedBy),
            Note = c.Note,
            EvidenceArtifactId = c.EvidenceArtifactId,
            Status = c.Status,
            ClearedAmount = c.ClearedAmount,
            ClearedAt = c.ClearedAt,
            ClearedByName = FullName(c.ClearedBy),
            QueryNote = c.QueryNote,
            CanClear = open && CanDecide(access),
            CanQuery = c.Status == ClaimStatus.Claimed && CanDecide(access),
            CanWithdraw = open && (c.ClaimedById == userId || access.CanManage),
            DateTimeCreated = c.DateTimeCreated
        };
    }

    private static VariationDto ToDto(tbl_Variation v, ProjectAccess access) => new()
    {
        Id = v.Id,
        ProjectId = v.ProjectId,
        StageId = v.StageId,
        StageName = v.Stage?.StageName,
        CommitmentId = v.CommitmentId,
        Title = v.Title,
        Reason = v.Reason,
        CostDelta = v.CostDelta,
        Currency = v.Currency,
        TimeDeltaDays = v.TimeDeltaDays,
        Status = v.Status,
        RaisedByName = FullName(v.RaisedBy),
        RaisedAt = v.RaisedAt,
        ApprovedByName = FullName(v.ApprovedBy),
        ApprovedAt = v.ApprovedAt,
        DecisionNote = v.DecisionNote,
        CanDecide = v.Status == VariationStatus.Proposed && CanDecide(access),
        DateTimeCreated = v.DateTimeCreated
    };
}
