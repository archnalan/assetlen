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
/// The commitments register (assetlen.md §3). Every read and write resolves the
/// caller's standing once through <see cref="IProjectAccessService"/>; what the
/// reader may do to each item is stamped here, never inferred on the client.
/// </summary>
public class CommitmentDAL : ICommitmentDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<CommitmentDAL> _logger;
    private readonly IProjectAccessService _access;
    private readonly IActiveStageService _activeStage;

    public CommitmentDAL(AssetlenDbContext context, ILogger<CommitmentDAL> logger,
        IProjectAccessService access, IActiveStageService activeStage)
    {
        _context = context;
        _logger = logger;
        _access = access;
        _activeStage = activeStage;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Deliverables
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<List<DeliverableDto>>> GetDeliverables(string projectId, string? stageId, string userId)
    {
        try
        {
            var project = await LoadProject(projectId);
            if (project is null) return Fail<List<DeliverableDto>>(new NotFoundException("Project not found."));

            // The checklist is what capture is aimed at, so every seat that can
            // read the project reads it — the bench included.
            var access = await _access.ResolveAsync(project, userId);
            if (!access.CanRead) return Fail<List<DeliverableDto>>(new NotFoundException("Project not found."));

            var query = _context.tbl_Deliverables
                .Include(d => d.Stage)
                .Include(d => d.CompletedBy)
                .Where(d => d.ProjectId == projectId);
            if (!string.IsNullOrEmpty(stageId)) query = query.Where(d => d.StageId == stageId);

            var rows = await query.AsNoTracking().ToListAsync();

            var counts = await _context.tbl_Commitments
                .Where(c => c.ProjectId == projectId && c.DeliverableId != null && c.SupersededAt == null)
                .GroupBy(c => c.DeliverableId!)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var dtos = rows
                .OrderBy(d => d.Stage?.DisplayOrder ?? int.MaxValue)
                .ThenBy(d => d.DisplayOrder)
                .Select(d => ToDto(d, counts.GetValueOrDefault(d.Id)))
                .ToList();

            return ServiceResult<List<DeliverableDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing deliverables for {ProjectId}", projectId);
            return Fail<List<DeliverableDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<DeliverableDto>> AddDeliverable(DeliverableCreateDto dto, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return Fail<DeliverableDto>(new BadRequestException("A deliverable needs a title."));

            var project = await LoadProject(dto.ProjectId);
            if (project is null) return Fail<DeliverableDto>(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, userId);
            if (!access.CanSeeRegister) return Fail<DeliverableDto>(new NotFoundException("Project not found."));
            if (!access.CanWrite) return Fail<DeliverableDto>(new ForbiddenException("You can read this checklist but not change it."));

            var stage = await _context.tbl_Stages.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.StageId);
            if (stage is null || stage.ProjectId != project.Id)
                return Fail<DeliverableDto>(new BadRequestException("That stage is not on this project."));

            var maxOrder = await _context.tbl_Deliverables
                .Where(d => d.StageId == stage.Id)
                .MaxAsync(d => (int?)d.DisplayOrder) ?? 0;

            var row = new tbl_Deliverable
            {
                ProjectId = project.Id,
                StageId = stage.Id,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                DueDate = dto.DueDate,
                DisplayOrder = dto.DisplayOrder > 0 ? dto.DisplayOrder : maxOrder + 1,
                Status = DeliverableStatus.NotStarted
            };
            _context.tbl_Deliverables.Add(row);
            await _context.SaveChangesAsync();

            row.Stage = stage;
            return ServiceResult<DeliverableDto>.Success(ToDto(row, 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding deliverable");
            return Fail<DeliverableDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<DeliverableDto>> UpdateDeliverable(DeliverableUpdateDto dto, string userId)
    {
        try
        {
            var row = await _context.tbl_Deliverables
                .Include(d => d.Stage)
                .Include(d => d.Project).ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(d => d.Id == dto.Id);
            if (row is null) return Fail<DeliverableDto>(new NotFoundException("Deliverable not found."));

            var access = await _access.ResolveAsync(row.Project, userId);
            if (!access.CanSeeRegister) return Fail<DeliverableDto>(new NotFoundException("Deliverable not found."));
            if (!access.CanWrite) return Fail<DeliverableDto>(new ForbiddenException("You can read this checklist but not change it."));

            if (!string.IsNullOrWhiteSpace(dto.Title)) row.Title = dto.Title.Trim();
            if (dto.Description is not null) row.Description = dto.Description;
            if (dto.DueDate.HasValue) row.DueDate = dto.DueDate;
            if (dto.DisplayOrder.HasValue) row.DisplayOrder = dto.DisplayOrder.Value;
            if (dto.Status.HasValue && dto.Status.Value != row.Status)
            {
                row.Status = dto.Status.Value;
                row.CompletedAt = row.Status == DeliverableStatus.Done ? DateTime.UtcNow : null;
                row.CompletedById = row.Status == DeliverableStatus.Done ? userId : null;
            }

            await _context.SaveChangesAsync();
            var count = await _context.tbl_Commitments.CountAsync(c => c.DeliverableId == row.Id && c.SupersededAt == null);
            await _context.Entry(row).Reference(r => r.CompletedBy).LoadAsync();
            return ServiceResult<DeliverableDto>.Success(ToDto(row, count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deliverable {Id}", dto.Id);
            return Fail<DeliverableDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<bool>> DeleteDeliverable(string deliverableId, string userId)
    {
        try
        {
            var row = await _context.tbl_Deliverables
                .Include(d => d.Project).ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(d => d.Id == deliverableId);
            if (row is null) return Fail<bool>(new NotFoundException("Deliverable not found."));

            var access = await _access.ResolveAsync(row.Project, userId);
            if (!access.CanSeeRegister) return Fail<bool>(new NotFoundException("Deliverable not found."));
            if (!access.CanManage) return Fail<bool>(new ForbiddenException("Only the project's owner or manager can remove a checklist line."));

            // A line with commitments on it is part of the record. Removing it
            // would orphan what was agreed against it.
            if (await _context.tbl_Commitments.AnyAsync(c => c.DeliverableId == row.Id))
                return Fail<bool>(new ConflictException("Commitments are filed against this deliverable. Move them first."));

            row.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting deliverable {Id}", deliverableId);
            return Fail<bool>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Commitments — reads
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<List<CommitmentDto>>> GetCommitments(string projectId, string? stageId, bool includeSuperseded, string userId)
    {
        try
        {
            var project = await LoadProject(projectId);
            if (project is null) return Fail<List<CommitmentDto>>(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, userId);

            // A commitment is addressed to a decision-maker. The bench is
            // answered as if the register did not exist — absent, not refused.
            if (!access.CanSeeRegister) return Fail<List<CommitmentDto>>(new NotFoundException("Project not found."));

            var all = await LoadCommitments(c => c.ProjectId == projectId);
            var shown = all
                .Where(c => includeSuperseded || c.SupersededAt == null)
                .Where(c => string.IsNullOrEmpty(stageId) || c.StageId == stageId)
                .ToList();

            var dtos = await ToDtosAsync(shown, all, project, access, userId);
            return ServiceResult<List<CommitmentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing commitments for {ProjectId}", projectId);
            return Fail<List<CommitmentDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> GetCommitment(string commitmentId, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(commitmentId, userId);
            if (error is not null) return Fail<CommitmentDto>(error);
            return ServiceResult<CommitmentDto>.Success(await OneDtoAsync(c!, project!, access, userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting commitment {Id}", commitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<CommitmentDto>>> GetChain(string commitmentId, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(commitmentId, userId);
            if (error is not null) return Fail<List<CommitmentDto>>(error);

            var all = await LoadCommitments(x => x.ProjectId == c!.ProjectId);
            var byId = all.ToDictionary(x => x.Id);

            // Walk back to the first statement, then forward to the current one.
            var first = byId[c!.Id];
            var guard = 0;
            while (first.SupersedesId is { } prev && byId.TryGetValue(prev, out var older) && guard++ < 500)
                first = older;

            var chain = new List<tbl_Commitment> { first };
            guard = 0;
            while (chain[^1].SupersededById is { } next && byId.TryGetValue(next, out var newer) && guard++ < 500)
                chain.Add(newer);

            var dtos = await ToDtosAsync(chain, all, project!, access, userId);
            return ServiceResult<List<CommitmentDto>>.Success(dtos.OrderBy(d => chain.FindIndex(x => x.Id == d.Id)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading chain for {Id}", commitmentId);
            return Fail<List<CommitmentDto>>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Commitments — writes
    // ═════════════════════════════════════════════════════════════════════

    public Task<ServiceResult<CommitmentDto>> AddCommitment(CommitmentCreateDto dto, string userId)
        => CreateAsync(dto, userId, verbal: false);

    public Task<ServiceResult<CommitmentDto>> LogDecision(CommitmentCreateDto dto, string userId)
        => CreateAsync(dto, userId, verbal: true);

    private async Task<ServiceResult<CommitmentDto>> CreateAsync(CommitmentCreateDto dto, string userId, bool verbal)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return Fail<CommitmentDto>(new BadRequestException("Say what was agreed."));
            if (!Enum.IsDefined(dto.Kind))
                return Fail<CommitmentDto>(new BadRequestException("Unknown kind of commitment."));

            var project = await LoadProject(dto.ProjectId);
            if (project is null) return Fail<CommitmentDto>(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, userId);
            if (!access.CanSeeRegister) return Fail<CommitmentDto>(new NotFoundException("Project not found."));
            if (!access.CanWrite) return Fail<CommitmentDto>(new ForbiddenException("You can read the register but not add to it."));

            // Nothing floats (CLAUDE.md §1): a deliverable brings its stage, and
            // an item recorded with neither lands on the stage that is live.
            tbl_Deliverable? deliverable = null;
            if (!string.IsNullOrEmpty(dto.DeliverableId))
            {
                deliverable = await _context.tbl_Deliverables.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dto.DeliverableId);
                if (deliverable is null || deliverable.ProjectId != project.Id)
                    return Fail<CommitmentDto>(new BadRequestException("That deliverable is not on this project."));
            }
            var stageId = deliverable?.StageId ?? await _activeStage.ResolveAsync(project.Id, dto.StageId);

            var memberError = await CheckMemberAsync(project, dto.AgreedWithMemberId);
            if (memberError is not null) return Fail<CommitmentDto>(memberError);

            if (!string.IsNullOrEmpty(dto.DependsOnStageId)
                && !await _context.tbl_Stages.AnyAsync(s => s.Id == dto.DependsOnStageId && s.ProjectId == project.Id))
                return Fail<CommitmentDto>(new BadRequestException("That stage is not on this project."));

            var mediator = await AccountableMemberAsync(project);
            var source = dto.SourceChannel;
            var maturity = dto.Maturity ?? CommitmentMaturity.Agreed;
            var agreedWithMemberId = string.IsNullOrEmpty(dto.AgreedWithMemberId) ? null : dto.AgreedWithMemberId;

            if (verbal)
            {
                // A spoken agreement is attributed to both parties and lands at
                // Agreed — and it stays visibly unconfirmed until the other side
                // says so (assetlen.md §3).
                if (source is not (CommitmentSource.Verbal or CommitmentSource.Meeting))
                    source = CommitmentSource.Verbal;
                maturity = CommitmentMaturity.Agreed;

                if (agreedWithMemberId is null && string.IsNullOrWhiteSpace(dto.AgreedWithPartyName))
                    agreedWithMemberId = await DefaultCounterpartyAsync(project, access, mediator);
            }

            var now = DateTime.UtcNow;
            var row = new tbl_Commitment
            {
                ProjectId = project.Id,
                StageId = stageId,
                DeliverableId = deliverable?.Id,
                Kind = dto.Kind,
                Title = dto.Title.Trim(),
                Body = dto.Body,
                Maturity = maturity,
                QueryState = CommitmentQueryState.None,
                SourceChannel = source,
                AccountableMemberId = mediator?.Id,
                AgreedById = userId,
                AgreedWithMemberId = agreedWithMemberId,
                AgreedWithPartyName = string.IsNullOrWhiteSpace(dto.AgreedWithPartyName) ? null : dto.AgreedWithPartyName.Trim(),
                AgreedAt = dto.AgreedAt ?? (maturity >= CommitmentMaturity.Agreed ? now : null),
                RecordedById = userId,
                RecordedBySide = access.Side,
                Amount = dto.Amount,
                Currency = dto.Amount is null ? null : (dto.Currency ?? project.Currency ?? "UGX"),
                DueDate = dto.DueDate,
                LeadTimeDays = dto.LeadTimeDays,
                DependsOnStageId = string.IsNullOrEmpty(dto.DependsOnStageId) ? null : dto.DependsOnStageId,
                OwedBySide = dto.OwedBySide,
                IngestedMessageId = dto.IngestedMessageId,
                DeliveredAt = maturity >= CommitmentMaturity.Delivered ? now : null,
                VerifiedAt = maturity >= CommitmentMaturity.Verified ? now : null
            };

            _context.tbl_Commitments.Add(row);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(dto.IngestedMessageId))
            {
                _context.tbl_CommitmentLinks.Add(new tbl_CommitmentLink
                {
                    ProjectId = project.Id,
                    CommitmentId = row.Id,
                    TargetType = CommitmentLinkTarget.IngestedMessage,
                    TargetId = dto.IngestedMessageId,
                    Relation = CommitmentLinkRelation.Source,
                    CreatedById = userId
                });
                await _context.SaveChangesAsync();
            }

            return await GetCommitment(row.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding commitment");
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> Confirm(string commitmentId, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(commitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);

            if (!CanAnswerSpoken(c!, access, userId))
                return Fail<CommitmentDto>(new ForbiddenException(
                    "Only the other side confirms a spoken agreement — never the person who wrote it down."));

            c!.CounterpartyConfirmedAt = DateTime.UtcNow;
            c.CounterpartyConfirmedById = userId;
            await _context.SaveChangesAsync();
            return await GetCommitment(c.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming commitment {Id}", commitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> Dispute(CommitmentNoteDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);

            if (!CanAnswerSpoken(c!, access, userId))
                return Fail<CommitmentDto>(new ForbiddenException(
                    "Only the other side can dispute a spoken agreement."));

            var now = DateTime.UtcNow;
            c!.DisputedAt = now;
            c.DisputedById = userId;
            c.DisputeNote = dto.Note;

            // The dispute is a query on the item, owed back to whoever wrote it
            // down — so it lands on their list rather than in a message.
            OpenQuery(c, project!, userId, $"That's not what we said: {c.Title}", dto.Note, c.RecordedById);
            await _context.SaveChangesAsync();
            return await GetCommitment(c.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disputing commitment {Id}", dto.CommitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> RaiseQuery(CommitmentNoteDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);
            if (!access.CanWrite) return Fail<CommitmentDto>(new ForbiddenException("You can read the register but not query it."));
            if (c!.SupersededAt is not null)
                return Fail<CommitmentDto>(new BadRequestException("This has been restated. Query the current statement."));
            if (c.QueryState == CommitmentQueryState.QueryRaised)
                return Fail<CommitmentDto>(new ConflictException("There is already an open query on this."));
            if (string.IsNullOrWhiteSpace(dto.Note))
                return Fail<CommitmentDto>(new BadRequestException("Say what the question is."));

            // Asked of the other side: a client-side question goes to the
            // accountable face, a delivery-side one to the developer.
            string? owedBy;
            if (access.Side == ProjectSide.Client)
            {
                owedBy = await _context.tbl_ProjectMembers
                    .Where(m => m.Id == c.AccountableMemberId)
                    .Select(m => m.UserId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                owedBy = project!.InvestorId ?? project.ParentProject?.InvestorId;
            }

            OpenQuery(c, project!, userId, c.Title ?? "Query", dto.Note, owedBy);
            await _context.SaveChangesAsync();
            return await GetCommitment(c.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising query on {Id}", dto.CommitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> ResolveQuery(CommitmentResolveDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);
            if (!access.CanWrite) return Fail<CommitmentDto>(new ForbiddenException("You can read the register but not resolve a query on it."));
            if (c!.QueryState != CommitmentQueryState.QueryRaised)
                return Fail<CommitmentDto>(new BadRequestException("There is no open query on this."));
            if (string.IsNullOrWhiteSpace(dto.Note))
                return Fail<CommitmentDto>(new BadRequestException("Say how it was resolved — that sentence is what the next reader needs."));
            if (dto.Amount is not null && !access.CanSeeMoney)
                return Fail<CommitmentDto>(new ForbiddenException("Money is not part of your seat on this project."));

            var now = DateTime.UtcNow;
            var openFlags = await _context.tbl_Flags
                .Where(f => f.CommitmentId == c.Id && (f.Status == FlagStatus.Open || f.Status == FlagStatus.InProgress))
                .ToListAsync();
            foreach (var f in openFlags)
            {
                f.Status = FlagStatus.Resolved;
                f.ResolvedById = userId;
                f.ResolvedDate = now;
            }

            c.QueryState = CommitmentQueryState.Resolved;
            c.ResolutionNote = dto.Note.Trim();
            c.ResolvedAt = now;
            c.ResolvedById = userId;

            var changes = (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title.Trim() != c.Title)
                          || (dto.Amount is not null && dto.Amount != c.Amount)
                          || (dto.DueDate is not null && dto.DueDate != c.DueDate);

            if (!changes)
            {
                await _context.SaveChangesAsync();
                return await GetCommitment(c.Id, userId);
            }

            // The answer changes the item. It becomes a new statement so the
            // figure that was questioned is still on record beside the one
            // that replaced it — resolution never rewrites history.
            var successor = Successor(c, userId, access, new CommitmentRestateDto
            {
                CommitmentId = c.Id,
                Title = dto.Title,
                Amount = dto.Amount,
                DueDate = dto.DueDate
            });
            successor.QueryState = CommitmentQueryState.Resolved;
            successor.ResolutionNote = c.ResolutionNote;
            successor.ResolvedAt = now;
            successor.ResolvedById = userId;

            _context.tbl_Commitments.Add(successor);
            await _context.SaveChangesAsync();

            return await GetCommitment(successor.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving query on {Id}", dto.CommitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> SetMaturity(CommitmentMaturityDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);
            if (!access.CanWrite) return Fail<CommitmentDto>(new ForbiddenException("You can read the register but not change it."));
            if (!Enum.IsDefined(dto.Maturity)) return Fail<CommitmentDto>(new BadRequestException("Unknown state."));
            if (c!.SupersededAt is not null)
                return Fail<CommitmentDto>(new BadRequestException("This has been restated. Move the current statement."));

            // Verification is the check against present reality, so it belongs
            // to whoever holds the other end: the client side, or the mediator.
            if (dto.Maturity == CommitmentMaturity.Verified && !CanVerify(access))
                return Fail<CommitmentDto>(new ForbiddenException("The client side or the mediator verifies delivered work."));

            // Backwards moves are how a register gets quietly rewritten. A
            // changed mind is a restatement or a query, not an undo.
            if (dto.Maturity < c.Maturity)
                return Fail<CommitmentDto>(new BadRequestException("A commitment does not move backwards. Raise a query or restate it."));

            var now = DateTime.UtcNow;
            c.Maturity = dto.Maturity;
            if (dto.Maturity >= CommitmentMaturity.Agreed) c.AgreedAt ??= now;
            if (dto.Maturity >= CommitmentMaturity.Delivered) c.DeliveredAt ??= now;
            if (dto.Maturity >= CommitmentMaturity.Verified) c.VerifiedAt ??= now;

            await _context.SaveChangesAsync();
            return await GetCommitment(c.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting maturity on {Id}", dto.CommitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> Clear(string commitmentId, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(commitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);
            if (!CanClear(c!, access))
                return Fail<CommitmentDto>(new ForbiddenException("The funder clears a priced item, once it is agreed and not under query."));

            c!.QueryState = CommitmentQueryState.Cleared;
            c.ClearedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetCommitment(c.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing {Id}", commitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentDto>> Restate(CommitmentRestateDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);
            if (!access.CanWrite) return Fail<CommitmentDto>(new ForbiddenException("You can read the register but not change it."));
            if (c!.SupersededAt is not null)
                return Fail<CommitmentDto>(new ConflictException("This has already been restated. Restate the current statement."));
            if (dto.Amount is not null && !access.CanSeeMoney)
                return Fail<CommitmentDto>(new ForbiddenException("Money is not part of your seat on this project."));

            var successor = Successor(c, userId, access, dto);
            _context.tbl_Commitments.Add(successor);
            await _context.SaveChangesAsync();

            return await GetCommitment(successor.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restating {Id}", dto.CommitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Parked ideas — references and estimates accumulate silently (§3, Law 4)
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<CommitmentDto>> Park(CommitmentParkDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId, tracked: true);
            if (error is not null) return Fail<CommitmentDto>(error);
            if (!access.CanWrite) return Fail<CommitmentDto>(new ForbiddenException("You can read the register but not change it."));
            if (c!.SupersededAt is not null || c.Maturity >= CommitmentMaturity.Agreed)
                return Fail<CommitmentDto>(new BadRequestException("Only something not yet agreed can be re-filed. Restate an agreed item instead."));
            if (dto.LeadTimeDays is < 0 or > 730)
                return Fail<CommitmentDto>(new BadRequestException("A lead time is between 0 and 730 days."));

            foreach (var sid in new[] { dto.StageId, dto.DependsOnStageId })
            {
                if (!string.IsNullOrEmpty(sid) && !await _context.tbl_Stages.AnyAsync(s => s.Id == sid && s.ProjectId == project!.Id))
                    return Fail<CommitmentDto>(new BadRequestException("That stage is not on this project."));
            }

            if (!string.IsNullOrEmpty(dto.StageId) && dto.StageId != c.StageId)
            {
                c.StageId = dto.StageId;
                c.DeliverableId = null;
            }
            if (dto.ClearDependency) c.DependsOnStageId = null;
            else if (!string.IsNullOrEmpty(dto.DependsOnStageId)) c.DependsOnStageId = dto.DependsOnStageId;
            if (dto.LeadTimeDays is not null) c.LeadTimeDays = dto.LeadTimeDays == 0 ? null : dto.LeadTimeDays;

            await _context.SaveChangesAsync();
            return await GetCommitment(c.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parking {Id}", dto.CommitmentId);
            return Fail<CommitmentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<CommitmentEstimateDto>> AddEstimate(CommitmentEstimateCreateDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId);
            if (error is not null) return Fail<CommitmentEstimateDto>(error);
            if (!access.CanWrite) return Fail<CommitmentEstimateDto>(new ForbiddenException("You can read the register but not add to it."));
            if (c!.SupersededAt is not null || c.Maturity >= CommitmentMaturity.Agreed)
                return Fail<CommitmentEstimateDto>(new BadRequestException("An estimate goes on something not yet agreed. An agreed figure is restated."));
            if (dto.Amount is null && string.IsNullOrWhiteSpace(dto.Note))
                return Fail<CommitmentEstimateDto>(new BadRequestException("Give a figure or say where one came from."));
            if (dto.Amount is < 0)
                return Fail<CommitmentEstimateDto>(new BadRequestException("An estimate cannot be negative."));
            if (dto.Amount is not null && !access.CanSeeMoney)
                return Fail<CommitmentEstimateDto>(new ForbiddenException("Money is not part of your seat on this project."));
            if (!string.IsNullOrEmpty(dto.IngestedMessageId)
                && await DescribeTargetAsync(CommitmentLinkTarget.IngestedMessage, dto.IngestedMessageId, project!, access, userId) is null)
                return Fail<CommitmentEstimateDto>(new NotFoundException("That message is not on this project."));
            if (!string.IsNullOrEmpty(dto.ArtifactId)
                && await DescribeTargetAsync(CommitmentLinkTarget.Artifact, dto.ArtifactId, project!, access, userId) is null)
                return Fail<CommitmentEstimateDto>(new NotFoundException("That file is not on this project."));

            var row = new tbl_CommitmentEstimate
            {
                ProjectId = project!.Id,
                CommitmentId = c.Id,
                Amount = dto.Amount,
                Currency = dto.Amount is null ? null : (dto.Currency ?? c.Currency ?? project.Currency ?? "UGX"),
                Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
                IngestedMessageId = string.IsNullOrEmpty(dto.IngestedMessageId) ? null : dto.IngestedMessageId,
                ArtifactId = string.IsNullOrEmpty(dto.ArtifactId) ? null : dto.ArtifactId,
                RecordedById = userId
            };
            _context.tbl_CommitmentEstimates.Add(row);
            await _context.SaveChangesAsync();

            // Where the figure came from is a reference too — the idea gathers
            // its sources without anyone being asked anything.
            if (row.IngestedMessageId is not null)
                await AddLink(new CommitmentLinkCreateDto { CommitmentId = c.Id, TargetType = CommitmentLinkTarget.IngestedMessage, TargetId = row.IngestedMessageId, Relation = CommitmentLinkRelation.Relates, Note = "Estimate" }, userId);
            if (row.ArtifactId is not null)
                await AddLink(new CommitmentLinkCreateDto { CommitmentId = c.Id, TargetType = CommitmentLinkTarget.Artifact, TargetId = row.ArtifactId, Relation = CommitmentLinkRelation.Relates, Note = "Estimate" }, userId);

            var list = await GetEstimates(c.Id, userId);
            return list.IsSuccess
                ? ServiceResult<CommitmentEstimateDto>.Success(list.Data.First(e => e.Id == row.Id))
                : Fail<CommitmentEstimateDto>(list.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding an estimate to {Id}", dto.CommitmentId);
            return Fail<CommitmentEstimateDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<CommitmentEstimateDto>>> GetEstimates(string commitmentId, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(commitmentId, userId);
            if (error is not null) return Fail<List<CommitmentEstimateDto>>(error);

            var rows = await _context.tbl_CommitmentEstimates.AsNoTracking()
                .Include(e => e.RecordedBy)
                .Where(e => e.CommitmentId == c!.Id)
                .OrderByDescending(e => e.DateTimeCreated)
                .ToListAsync();

            var money = access.CanSeeMoney;
            return ServiceResult<List<CommitmentEstimateDto>>.Success(rows.Select(e => new CommitmentEstimateDto
            {
                Id = e.Id,
                CommitmentId = e.CommitmentId,
                Amount = money ? e.Amount : null,
                Currency = money ? e.Currency : null,
                AmountHidden = !money && e.Amount is not null,
                Note = e.Note,
                RecordedByName = FullName(e.RecordedBy),
                RecordedAt = e.DateTimeCreated,
                IngestedMessageId = e.IngestedMessageId,
                ArtifactId = e.ArtifactId,
                DateTimeCreated = e.DateTimeCreated
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading estimates for {Id}", commitmentId);
            return Fail<List<CommitmentEstimateDto>>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Links — both directions off one row
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<CommitmentLinkDto>> AddLink(CommitmentLinkCreateDto dto, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(dto.CommitmentId, userId);
            if (error is not null) return Fail<CommitmentLinkDto>(error);
            if (!access.CanWrite) return Fail<CommitmentLinkDto>(new ForbiddenException("You can read the register but not change it."));
            if (!Enum.IsDefined(dto.TargetType) || !Enum.IsDefined(dto.Relation))
                return Fail<CommitmentLinkDto>(new BadRequestException("Unknown link."));
            if (dto.TargetType == CommitmentLinkTarget.Commitment && dto.TargetId == c!.Id)
                return Fail<CommitmentLinkDto>(new BadRequestException("A commitment cannot be its own evidence."));

            // The target must be on the same project and something this reader
            // can see — a link must never become a way to learn what is behind
            // the channel boundary.
            var target = await DescribeTargetAsync(dto.TargetType, dto.TargetId!, project!, access, userId);
            if (target is null)
                return Fail<CommitmentLinkDto>(new NotFoundException("That item is not on this project."));

            var existing = await _context.tbl_CommitmentLinks.AsNoTracking()
                .FirstOrDefaultAsync(l => l.CommitmentId == c!.Id && l.TargetType == dto.TargetType
                                          && l.TargetId == dto.TargetId && l.Relation == dto.Relation);
            if (existing is not null)
                return ServiceResult<CommitmentLinkDto>.Success(ToLinkDto(existing, c!, target.Value));

            var link = new tbl_CommitmentLink
            {
                ProjectId = project!.Id,
                CommitmentId = c!.Id,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                Relation = dto.Relation,
                Note = dto.Note,
                CreatedById = userId
            };
            _context.tbl_CommitmentLinks.Add(link);
            await _context.SaveChangesAsync();

            return ServiceResult<CommitmentLinkDto>.Success(ToLinkDto(link, c, target.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking {Id}", dto.CommitmentId);
            return Fail<CommitmentLinkDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<bool>> RemoveLink(string linkId, string userId)
    {
        try
        {
            var link = await _context.tbl_CommitmentLinks.FirstOrDefaultAsync(l => l.Id == linkId);
            if (link is null) return Fail<bool>(new NotFoundException("Link not found."));
            var project = await LoadProject(link.ProjectId);
            var access = await _access.ResolveAsync(project, userId);
            if (!access.CanSeeRegister) return Fail<bool>(new NotFoundException("Link not found."));
            if (link.CreatedById != userId && !access.CanManage)
                return Fail<bool>(new ForbiddenException("Only whoever made the link, or the project's owner, can remove it."));

            link.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing link {Id}", linkId);
            return Fail<bool>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<CommitmentLinkDto>>> GetLinks(string commitmentId, string userId)
    {
        try
        {
            var (c, project, access, error) = await LoadForCaller(commitmentId, userId);
            if (error is not null) return Fail<List<CommitmentLinkDto>>(error);

            var outgoing = await _context.tbl_CommitmentLinks.AsNoTracking()
                .Where(l => l.CommitmentId == c!.Id)
                .ToListAsync();

            // The other direction: commitments that cite this one as evidence —
            // "materials on site" pointing back at the cement order.
            var incoming = await _context.tbl_CommitmentLinks.AsNoTracking()
                .Include(l => l.Commitment)
                .Where(l => l.TargetType == CommitmentLinkTarget.Commitment && l.TargetId == c!.Id)
                .ToListAsync();

            var result = new List<CommitmentLinkDto>();
            foreach (var l in outgoing)
            {
                var t = await DescribeTargetAsync(l.TargetType, l.TargetId!, project!, access, userId);
                if (t is not null) result.Add(ToLinkDto(l, c!, t.Value));
            }
            foreach (var l in incoming.Where(l => l.Commitment is not null))
            {
                // Seen from this side, the target is the citing commitment.
                result.Add(new CommitmentLinkDto
                {
                    Id = l.Id,
                    CommitmentId = c!.Id,
                    CommitmentTitle = c.Title,
                    CommitmentKind = c.Kind,
                    TargetType = CommitmentLinkTarget.Commitment,
                    TargetId = l.CommitmentId,
                    Relation = l.Relation,
                    Note = l.Note,
                    TargetLabel = l.Commitment!.Title,
                    TargetDate = l.Commitment.AgreedAt,
                    DateTimeCreated = l.DateTimeCreated
                });
            }

            return ServiceResult<List<CommitmentLinkDto>>.Success(result.OrderBy(r => r.TargetDate ?? r.DateTimeCreated).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing links for {Id}", commitmentId);
            return Fail<List<CommitmentLinkDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<CommitmentLinkDto>>> GetBacklinks(string projectId, CommitmentLinkTarget targetType, string targetId, string userId)
    {
        try
        {
            var project = await LoadProject(projectId);
            if (project is null) return Fail<List<CommitmentLinkDto>>(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, userId);
            if (!access.CanSeeRegister) return Fail<List<CommitmentLinkDto>>(new NotFoundException("Project not found."));

            var target = await DescribeTargetAsync(targetType, targetId, project, access, userId);
            if (target is null) return Fail<List<CommitmentLinkDto>>(new NotFoundException("That item is not on this project."));

            var links = await _context.tbl_CommitmentLinks.AsNoTracking()
                .Include(l => l.Commitment)
                .Where(l => l.ProjectId == projectId && l.TargetType == targetType && l.TargetId == targetId)
                .ToListAsync();

            var result = links.Where(l => l.Commitment is not null)
                .Select(l => ToLinkDto(l, l.Commitment!, target.Value))
                .ToList();
            return ServiceResult<List<CommitmentLinkDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading backlinks for {Type} {Id}", targetType, targetId);
            return Fail<List<CommitmentLinkDto>>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Accountability — a group-by, not a feature
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<AccountabilityDto>> GetAccountability(string projectId, string userId)
    {
        try
        {
            var project = await LoadProject(projectId);
            if (project is null) return Fail<AccountabilityDto>(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, userId);
            if (!access.CanSeeRegister) return Fail<AccountabilityDto>(new NotFoundException("Project not found."));

            var today = DateTime.UtcNow.Date;
            var heads = await _context.tbl_Commitments.AsNoTracking()
                .Where(c => c.ProjectId == projectId && c.SupersededAt == null)
                .ToListAsync();

            var memberIds = heads.Select(h => h.AccountableMemberId).OfType<string>().Distinct().ToList();
            var members = await _context.tbl_ProjectMembers.AsNoTracking()
                .Include(m => m.User)
                .Where(m => memberIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            var rows = heads
                .GroupBy(h => h.AccountableMemberId ?? "")
                .Select(g =>
                {
                    members.TryGetValue(g.Key, out var m);
                    return new AccountabilityRowDto
                    {
                        MemberId = g.Key == "" ? null : g.Key,
                        Name = m is null ? "No mediator appointed" : MemberName(m),
                        IsMediator = m?.IsMediator == true,
                        IsActive = m?.IsActive == true,
                        Side = m?.Side,
                        Total = g.Count(),
                        Idea = g.Count(c => c.Maturity == CommitmentMaturity.Idea),
                        InDiscussion = g.Count(c => c.Maturity == CommitmentMaturity.InDiscussion),
                        Agreed = g.Count(c => c.Maturity == CommitmentMaturity.Agreed),
                        Delivered = g.Count(c => c.Maturity == CommitmentMaturity.Delivered),
                        Verified = g.Count(c => c.Maturity == CommitmentMaturity.Verified),
                        OpenQueries = g.Count(c => c.QueryState == CommitmentQueryState.QueryRaised),
                        AwaitingConfirmation = g.Count(IsAwaitingCounterparty),
                        Overdue = g.Count(c => IsOverdue(c, today))
                    };
                })
                .OrderByDescending(r => r.IsActive)
                .ThenByDescending(r => r.Total)
                .ToList();

            // Blockers by whoever has to move. Channel rules still hold: the
            // client side sees the blockers that crossed, never the crew's own.
            var blockerQuery = _context.tbl_Flags.AsNoTracking()
                .Include(f => f.OwnerMember).ThenInclude(m => m!.User)
                .Include(f => f.Stage)
                .Where(f => f.ProjectId == projectId && f.CommitmentId == null
                            && (f.Status == FlagStatus.Open || f.Status == FlagStatus.InProgress));
            if (!access.CanSeeSiteLog) blockerQuery = blockerQuery.Where(f => f.Channel == Channel.Client);
            var blockers = await blockerQuery.ToListAsync();

            var owners = blockers
                .GroupBy(f => f.OwnerPartyName ?? (f.OwnerMember is null ? "" : MemberName(f.OwnerMember)))
                .Select(g => new BlockerOwnerRowDto
                {
                    OwnerName = g.Key == "" ? null : g.Key,
                    OwnerMemberId = g.Select(f => f.OwnerMemberId).FirstOrDefault(id => id != null),
                    Open = g.Count(),
                    OldestDaysOpen = g.Max(f => (int)(today - (f.DateTimeCreated ?? today).Date).TotalDays),
                    Items = g.OrderBy(f => f.DateTimeCreated).Select(f => new FlagDto
                    {
                        Id = f.Id,
                        ProjectId = f.ProjectId,
                        StageId = f.StageId,
                        StageName = f.Stage?.StageName,
                        Title = f.Title,
                        Description = f.Description,
                        Status = f.Status,
                        Severity = f.Severity,
                        Channel = f.Channel,
                        DueDate = f.DueDate,
                        DateTimeCreated = f.DateTimeCreated,
                        OwnerMemberId = f.OwnerMemberId,
                        OwnerName = g.Key == "" ? null : g.Key
                    }).ToList()
                })
                .OrderBy(r => r.OwnerName is null)
                .ThenByDescending(r => r.OldestDaysOpen)
                .ToList();

            return ServiceResult<AccountabilityDto>.Success(new AccountabilityDto
            {
                ProjectId = projectId,
                Commitments = rows,
                Blockers = owners
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing accountability for {ProjectId}", projectId);
            return Fail<AccountabilityDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Rules — one place each
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A spoken agreement is answered by the other side — a principal who sits
    /// opposite whoever wrote it down, and never that person.
    /// </summary>
    internal static bool CanAnswerSpoken(tbl_Commitment c, ProjectAccess access, string userId) =>
        c.SourceChannel is CommitmentSource.Verbal or CommitmentSource.Meeting
        && c.SupersededAt is null
        && c.CounterpartyConfirmedAt is null
        && c.DisputedAt is null
        && access.CanWrite && access.CanSeeRegister
        && access.Side is not null
        && c.RecordedBySide is not null
        && access.Side != c.RecordedBySide
        && c.RecordedById != userId;

    private static bool CanVerify(ProjectAccess access) =>
        access.CanWrite && access.CanSeeRegister && (access.Side == ProjectSide.Client || access.IsMediator);

    /// <summary>The funder clears a priced item. Clearing is the money moving, so it is the money seat's call.</summary>
    private static bool CanClear(tbl_Commitment c, ProjectAccess access) =>
        access.CanWrite && access.CanSeeMoney && access.Side == ProjectSide.Client
        && c.SupersededAt is null
        && c.Amount is not null
        && c.Maturity >= CommitmentMaturity.Agreed
        && c.QueryState is CommitmentQueryState.None or CommitmentQueryState.Resolved;

    private static bool IsAwaitingCounterparty(tbl_Commitment c) =>
        c.SourceChannel is CommitmentSource.Verbal or CommitmentSource.Meeting
        && c.SupersededAt is null
        && c.CounterpartyConfirmedAt is null
        && c.DisputedAt is null;

    private static bool IsOverdue(tbl_Commitment c, DateTime today) =>
        c.SupersededAt is null
        && c.DueDate is { } due && due.Date < today
        && c.Maturity < CommitmentMaturity.Delivered;

    // ═════════════════════════════════════════════════════════════════════
    // Helpers
    // ═════════════════════════════════════════════════════════════════════

    private static ServiceResult<T> Fail<T>(Exception e) => ServiceResult<T>.Failure(e);

    private Task<tbl_Project?> LoadProject(string? projectId) =>
        _context.tbl_Projects_RS
            .Include(p => p.ParentProject)
            .FirstOrDefaultAsync(p => p.Id == projectId);

    private Task<List<tbl_Commitment>> LoadCommitments(System.Linq.Expressions.Expression<Func<tbl_Commitment, bool>> where) =>
        _context.tbl_Commitments.AsNoTracking()
            .Include(c => c.Stage)
            .Include(c => c.DependsOnStage)
            .Include(c => c.Deliverable)
            .Include(c => c.AccountableMember).ThenInclude(m => m!.User)
            .Include(c => c.AgreedWithMember).ThenInclude(m => m!.User)
            .Include(c => c.AgreedBy)
            .Include(c => c.RecordedBy)
            .Where(where)
            .ToListAsync();

    private async Task<(tbl_Commitment? C, tbl_Project? Project, ProjectAccess Access, Exception? Error)> LoadForCaller(
        string? commitmentId, string userId, bool tracked = false)
    {
        if (string.IsNullOrEmpty(commitmentId))
            return (null, null, ProjectAccess.None, new BadRequestException("CommitmentId is required."));

        var query = _context.tbl_Commitments.AsQueryable();
        if (!tracked) query = query.AsNoTracking();
        var c = await query.FirstOrDefaultAsync(x => x.Id == commitmentId);
        if (c is null) return (null, null, ProjectAccess.None, new NotFoundException("Commitment not found."));

        var project = await LoadProject(c.ProjectId);
        var access = await _access.ResolveAsync(project, userId);
        if (project is null || !access.CanSeeRegister)
            return (null, null, access, new NotFoundException("Commitment not found."));

        return (c, project, access, null);
    }

    private async Task<Exception?> CheckMemberAsync(tbl_Project project, string? memberId)
    {
        if (string.IsNullOrEmpty(memberId)) return null;
        var ok = await _context.tbl_ProjectMembers.AnyAsync(m => m.Id == memberId
            && (m.ProjectId == project.Id || m.ProjectId == project.ParentProjectId));
        return ok ? null : new BadRequestException("That person is not on this project.");
    }

    /// <summary>The mediator is the accountable face on everything (§10.1). A sub-project falls back to its parent's.</summary>
    private async Task<tbl_ProjectMember?> AccountableMemberAsync(tbl_Project project)
    {
        async Task<tbl_ProjectMember?> On(string? pid) => pid is null ? null :
            await _context.tbl_ProjectMembers
                .Where(m => m.ProjectId == pid && m.IsActive && m.IsMediator)
                .OrderBy(m => m.JoinedAt ?? m.DateTimeCreated)
                .FirstOrDefaultAsync();

        return await On(project.Id) ?? await On(project.ParentProjectId);
    }

    /// <summary>
    /// When a spoken decision names nobody, it was with the other side's
    /// principal: the mediator for the client side, the developer for the delivery side.
    /// </summary>
    private async Task<string?> DefaultCounterpartyAsync(tbl_Project project, ProjectAccess access, tbl_ProjectMember? mediator)
    {
        if (access.Side == ProjectSide.Client)
            return mediator?.Id;

        var investorId = project.InvestorId ?? project.ParentProject?.InvestorId;
        return await _context.tbl_ProjectMembers
            .Where(m => m.ProjectId == project.Id && m.IsActive && m.UserId == investorId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync();
    }

    private void OpenQuery(tbl_Commitment c, tbl_Project project, string userId, string title, string? note, string? owedBy)
    {
        c.QueryState = CommitmentQueryState.QueryRaised;
        _context.tbl_Flags.Add(new tbl_Flag
        {
            ProjectId = project.Id,
            StageId = c.StageId,
            CommitmentId = c.Id,
            Title = title.Length > 200 ? title[..200] : title,
            Description = note,
            Severity = FlagSeverity.High,

            // A query on a commitment is between the two principals, so it
            // crosses by definition — the truth floor (§5) covers agreed specs.
            Channel = Channel.Client,
            Status = FlagStatus.Open,
            CreatedById = userId,
            AssignedToId = owedBy
        });
    }

    private static tbl_Commitment Successor(tbl_Commitment c, string userId, ProjectAccess access, CommitmentRestateDto dto)
    {
        var now = DateTime.UtcNow;
        var nextId = Guid.NewGuid().ToString();
        c.SupersededAt = now;
        c.SupersededById = nextId;
        return new tbl_Commitment
        {
            Id = nextId,
            ProjectId = c.ProjectId,
            StageId = c.StageId,
            DeliverableId = c.DeliverableId,
            Kind = c.Kind,
            Title = string.IsNullOrWhiteSpace(dto.Title) ? c.Title : dto.Title.Trim(),
            Body = dto.Body ?? c.Body,
            Maturity = c.Maturity >= CommitmentMaturity.Agreed ? CommitmentMaturity.Agreed : c.Maturity,
            QueryState = CommitmentQueryState.None,
            SourceChannel = dto.SourceChannel ?? c.SourceChannel,
            AccountableMemberId = c.AccountableMemberId,
            AgreedById = userId,
            AgreedWithMemberId = c.AgreedWithMemberId,
            AgreedWithPartyName = c.AgreedWithPartyName,
            AgreedAt = dto.AgreedAt ?? now,
            RecordedById = userId,
            RecordedBySide = access.Side,
            Amount = dto.Amount ?? c.Amount,
            Currency = c.Currency,
            DueDate = dto.DueDate ?? c.DueDate,
            LeadTimeDays = c.LeadTimeDays,
            DependsOnStageId = c.DependsOnStageId,
            OwedBySide = c.OwedBySide,
            SupersedesId = c.Id
        };
    }

    private static string? FullName(AppUser? u) =>
        u is null ? null : $"{u.FirstName} {u.LastName}".Trim();

    private static string MemberName(tbl_ProjectMember m) =>
        FullName(m.User) ?? m.PartyName ?? m.Title ?? "Unnamed";

    private static DeliverableDto ToDto(tbl_Deliverable d, int commitments) => new()
    {
        Id = d.Id,
        ProjectId = d.ProjectId,
        StageId = d.StageId,
        StageName = d.Stage?.StageName,
        Title = d.Title,
        Description = d.Description,
        DisplayOrder = d.DisplayOrder,
        Status = d.Status,
        DueDate = d.DueDate,
        CompletedAt = d.CompletedAt,
        CompletedByName = FullName(d.CompletedBy),
        CommitmentCount = commitments,
        DateTimeCreated = d.DateTimeCreated
    };

    private async Task<CommitmentDto> OneDtoAsync(tbl_Commitment c, tbl_Project project, ProjectAccess access, string userId)
    {
        var all = await LoadCommitments(x => x.ProjectId == c.ProjectId);
        var fresh = all.First(x => x.Id == c.Id);
        return (await ToDtosAsync(new List<tbl_Commitment> { fresh }, all, project, access, userId))[0];
    }

    private async Task<List<CommitmentDto>> ToDtosAsync(
        List<tbl_Commitment> shown, List<tbl_Commitment> all, tbl_Project project, ProjectAccess access, string userId)
    {
        var ids = shown.Select(c => c.Id).ToList();
        var today = DateTime.UtcNow.Date;
        var byId = all.ToDictionary(c => c.Id);

        var openFlagQuery = _context.tbl_Flags.AsNoTracking()
            .Where(f => f.CommitmentId != null && ids.Contains(f.CommitmentId)
                        && (f.Status == FlagStatus.Open || f.Status == FlagStatus.InProgress));
        if (!access.CanSeeSiteLog) openFlagQuery = openFlagQuery.Where(f => f.Channel == Channel.Client);
        var openFlags = await openFlagQuery
            .Select(f => new { f.CommitmentId, f.Id })
            .ToListAsync();

        var variations = await _context.tbl_Variations.AsNoTracking()
            .Where(v => v.CommitmentId != null && ids.Contains(v.CommitmentId))
            .Select(v => new { v.CommitmentId, v.Id, v.Status })
            .ToListAsync();

        var linkCounts = await _context.tbl_CommitmentLinks.AsNoTracking()
            .Where(l => (l.CommitmentId != null && ids.Contains(l.CommitmentId))
                        || (l.TargetType == CommitmentLinkTarget.Commitment && l.TargetId != null && ids.Contains(l.TargetId)))
            .Select(l => new { l.CommitmentId, l.TargetType, l.TargetId })
            .ToListAsync();

        var estimates = await _context.tbl_CommitmentEstimates.AsNoTracking()
            .Where(e => e.CommitmentId != null && ids.Contains(e.CommitmentId))
            .Select(e => new { e.CommitmentId, e.Amount, e.Currency, e.DateTimeCreated })
            .ToListAsync();

        // A question asked with a circle on a receipt stays with the item
        // through its restatements — the successor a resolution writes still
        // shows what was circled.
        List<string> Lineage(tbl_Commitment c)
        {
            var chain = new List<string> { c.Id };
            var at = c;
            while (at.SupersedesId is { } prev && byId.TryGetValue(prev, out var older) && chain.Count < 500)
            {
                chain.Add(older.Id);
                at = older;
            }
            return chain;
        }
        var lineage = shown.ToDictionary(c => c.Id, Lineage);
        var lineageIds = lineage.Values.SelectMany(x => x).Distinct().ToList();
        var markupQuery = _context.tbl_Annotations.AsNoTracking()
            .Where(a => a.CommitmentId != null && lineageIds.Contains(a.CommitmentId) && a.SupersededAt == null);
        if (!access.CanSeeSiteLog) markupQuery = markupQuery.Where(a => a.Channel == Channel.Client);
        var markups = await markupQuery
            .Select(a => new { a.CommitmentId, a.ArtifactId, a.LayerId, a.Note, a.DateTimeCreated })
            .ToListAsync();

        var confirmers = shown.Select(c => c.CounterpartyConfirmedById).Concat(shown.Select(c => c.DisputedById))
            .OfType<string>().Distinct().ToList();
        var names = await _context.Users.AsNoTracking()
            .Where(u => confirmers.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());

        return shown
            .OrderBy(c => c.Stage?.DisplayOrder ?? int.MaxValue)
            .ThenBy(c => c.Deliverable?.DisplayOrder ?? int.MaxValue)
            .ThenBy(c => c.AgreedAt ?? c.DateTimeCreated)
            .Select(c =>
            {
                var accountable = c.AccountableMember is null ? null : MemberName(c.AccountableMember);

                // One accountable face (§10.1): the client side reads the
                // mediator's name on anything the bench wrote, never the bench's.
                var hideAuthor = !access.CanSeeSiteLog && c.RecordedBySide == ProjectSide.Contractor;

                var restated = 0;
                var cursor = c;
                while (cursor.SupersedesId is { } prev && byId.TryGetValue(prev, out var older) && restated < 500)
                {
                    restated++;
                    cursor = older;
                }

                var variation = variations.FirstOrDefault(v => v.CommitmentId == c.Id);
                var amountVisible = access.CanSeeMoney;
                var isHead = c.SupersededAt is null;

                var previous = c.SupersedesId is { } pid && byId.TryGetValue(pid, out var p) ? p : null;
                var mine = lineage[c.Id];
                var markup = markups.Where(m => mine.Contains(m.CommitmentId!))
                    .OrderByDescending(m => m.DateTimeCreated).FirstOrDefault();
                var ests = estimates.Where(e => e.CommitmentId == c.Id).ToList();
                var priced = ests.Where(e => e.Amount is not null).ToList();
                var undecided = isHead && c.Maturity < CommitmentMaturity.Agreed;
                var decideBy = isHead
                    ? DecideByRule.Compute(c.Maturity, c.Stage?.StageName, c.Stage?.StartDate,
                        c.DependsOnStage?.StageName, c.DependsOnStage?.StartDate, c.LeadTimeDays, today)
                    : null;

                return new CommitmentDto
                {
                    Id = c.Id,
                    ProjectId = c.ProjectId,
                    StageId = c.StageId,
                    StageName = c.Stage?.StageName,
                    StagePhase = c.Stage?.Phase,
                    DeliverableId = c.DeliverableId,
                    DeliverableTitle = c.Deliverable?.Title,
                    Kind = c.Kind,
                    Title = c.Title,
                    Body = c.Body,
                    Maturity = c.Maturity,
                    QueryState = c.QueryState,
                    SourceChannel = c.SourceChannel,
                    AccountableMemberId = c.AccountableMemberId,
                    AccountableName = accountable,
                    AgreedByName = hideAuthor ? accountable : FullName(c.AgreedBy),
                    AgreedWithMemberId = c.AgreedWithMemberId,
                    AgreedWithName = c.AgreedWithMember is not null ? MemberName(c.AgreedWithMember) : c.AgreedWithPartyName,
                    AgreedAt = c.AgreedAt,
                    RecordedByName = hideAuthor ? null : FullName(c.RecordedBy),
                    RecordedBySide = c.RecordedBySide,
                    Amount = amountVisible ? c.Amount : null,
                    AmountHidden = !amountVisible && c.Amount is not null,
                    Currency = amountVisible ? c.Currency : null,
                    DueDate = c.DueDate,
                    LeadTimeDays = c.LeadTimeDays,
                    OwedBySide = c.OwedBySide,
                    SupersedesId = c.SupersedesId,
                    SupersededById = c.SupersededById,
                    SupersededAt = c.SupersededAt,
                    RestatementCount = restated,
                    CounterpartyConfirmedAt = c.CounterpartyConfirmedAt,
                    CounterpartyConfirmedByName = c.CounterpartyConfirmedById is { } cb ? names.GetValueOrDefault(cb) : null,
                    DisputedAt = c.DisputedAt,
                    DisputedByName = c.DisputedById is { } db ? names.GetValueOrDefault(db) : null,
                    DisputeNote = c.DisputeNote,
                    ResolutionNote = c.ResolutionNote,
                    ResolvedAt = c.ResolvedAt,
                    ClearedAt = c.ClearedAt,
                    DeliveredAt = c.DeliveredAt,
                    VerifiedAt = c.VerifiedAt,
                    IngestedMessageId = c.IngestedMessageId,
                    OpenQueryFlagId = openFlags.FirstOrDefault(f => f.CommitmentId == c.Id)?.Id,
                    VariationId = variation?.Id,
                    VariationStatus = variation?.Status,
                    LinkCount = linkCounts.Count(l => l.CommitmentId == c.Id
                                                      || (l.TargetType == CommitmentLinkTarget.Commitment && l.TargetId == c.Id)),
                    CanConfirm = CanAnswerSpoken(c, access, userId),
                    CanDispute = CanAnswerSpoken(c, access, userId),
                    CanAdvance = isHead && access.CanWrite && c.Maturity < CommitmentMaturity.Delivered,
                    CanVerify = isHead && c.Maturity == CommitmentMaturity.Delivered && CanVerify(access),
                    CanRaiseQuery = isHead && access.CanWrite && c.QueryState != CommitmentQueryState.QueryRaised,
                    CanResolveQuery = isHead && access.CanWrite && c.QueryState == CommitmentQueryState.QueryRaised,
                    CanClear = CanClear(c, access),
                    CanRestate = isHead && access.CanWrite,
                    CanAddEstimate = undecided && access.CanWrite,
                    CanPark = undecided && access.CanWrite,
                    CanPriceEstimate = undecided && access.CanWrite && access.CanSeeMoney,
                    PreviousAmount = amountVisible ? previous?.Amount : null,
                    PreviousDueDate = previous?.DueDate,
                    PreviousClearedAt = previous?.ClearedAt,
                    MarkupArtifactId = markup?.ArtifactId,
                    MarkupLayerId = markup?.LayerId,
                    MarkupNote = markup?.Note,
                    DependsOnStageId = c.DependsOnStageId,
                    DependsOnStageName = c.DependsOnStage?.StageName,
                    DecideBy = decideBy?.Date,
                    DecideByReason = decideBy?.Reason,
                    IsSurfaced = decideBy?.Surfaced ?? false,
                    EstimateCount = ests.Count,
                    EstimateLow = amountVisible && priced.Count > 0 ? priced.Min(e => e.Amount) : null,
                    EstimateHigh = amountVisible && priced.Count > 0 ? priced.Max(e => e.Amount) : null,
                    LatestEstimate = amountVisible ? priced.OrderByDescending(e => e.DateTimeCreated).FirstOrDefault()?.Amount : null,
                    EstimateCurrency = amountVisible ? priced.Select(e => e.Currency).FirstOrDefault(x => x != null) : null,
                    IsAwaitingCounterparty = IsAwaitingCounterparty(c),
                    IsOverdue = IsOverdue(c, today),
                    DateTimeCreated = c.DateTimeCreated
                };
            })
            .ToList();
    }

    private readonly record struct TargetInfo(string? Label, DateTime? Date, string? ArtifactId = null);

    /// <summary>
    /// The target's label, or null when it is not on this project or not this
    /// reader's to see. Links never become a way round the channel boundary.
    /// </summary>
    private async Task<TargetInfo?> DescribeTargetAsync(
        CommitmentLinkTarget type, string targetId, tbl_Project project, ProjectAccess access, string userId)
    {
        var pid = project.Id;
        switch (type)
        {
            case CommitmentLinkTarget.Commitment:
            {
                var t = await _context.tbl_Commitments.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.Title, x.AgreedAt }).FirstOrDefaultAsync();
                return t is null ? null : new TargetInfo(t.Title, t.AgreedAt);
            }
            case CommitmentLinkTarget.Flag:
            {
                var t = await _context.tbl_Flags.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.Title, x.DateTimeCreated, x.Channel }).FirstOrDefaultAsync();
                if (t is null || (!access.CanSeeSiteLog && t.Channel != Channel.Client)) return null;
                return new TargetInfo(t.Title, t.DateTimeCreated);
            }
            case CommitmentLinkTarget.ProgressUpdate:
            {
                var t = await _context.tbl_ProgressUpdates.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.Description, x.DateTimeCreated, x.Channel }).FirstOrDefaultAsync();
                if (t is null || (!access.CanSeeSiteLog && t.Channel != Channel.Client)) return null;
                return new TargetInfo(Excerpt(t.Description), t.DateTimeCreated);
            }
            case CommitmentLinkTarget.IngestedMessage:
            {
                var t = await _context.tbl_IngestedMessages.AsNoTracking()
                    .Include(x => x.Batch)
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .FirstOrDefaultAsync();
                if (t is null) return null;
                var readable = access.CanSeeSiteLog
                               || (t.Batch is not null && (t.Batch.ImportedSide == access.Side || t.Batch.ImportedById == userId));
                if (!readable) return null;
                return new TargetInfo($"{t.ExternalAuthor}: {Excerpt(t.Body)}", t.SentAt);
            }
            case CommitmentLinkTarget.FundingEntry:
            {
                if (!access.CanSeeMoney) return null;
                var t = await _context.tbl_FundingEntries.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.Amount, x.PaymentDate, x.Notes }).FirstOrDefaultAsync();
                return t is null ? null : new TargetInfo(t.Notes ?? $"Release of {t.Amount:N0}", t.PaymentDate);
            }
            case CommitmentLinkTarget.Claim:
            {
                if (!access.CanSeeMoney) return null;
                var t = await _context.tbl_StageClaims.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.Amount, x.ClaimedAt, x.Note }).FirstOrDefaultAsync();
                return t is null ? null : new TargetInfo(t.Note ?? $"Claim of {t.Amount:N0}", t.ClaimedAt);
            }
            case CommitmentLinkTarget.Variation:
            {
                if (!access.CanSeeMoney) return null;
                var t = await _context.tbl_Variations.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.Title, x.RaisedAt }).FirstOrDefaultAsync();
                return t is null ? null : new TargetInfo(t.Title, t.RaisedAt);
            }
            case CommitmentLinkTarget.Document:
            {
                var t = await _context.tbl_Documents.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.Title, x.DateTimeCreated, x.Channel }).FirstOrDefaultAsync();
                if (t is null || (!access.CanSeeSiteLog && t.Channel != Channel.Client)) return null;
                return new TargetInfo(t.Title, t.DateTimeCreated);
            }
            case CommitmentLinkTarget.Artifact:
            {
                var t = await _context.tbl_Artifacts.AsNoTracking()
                    .Where(x => x.Id == targetId && x.ProjectId == pid)
                    .Select(x => new { x.OriginalFileName, x.CapturedAt, x.DateTimeCreated, x.UploadedById }).FirstOrDefaultAsync();
                if (t is null) return null;
                if (!access.CanSeeSiteLog && t.UploadedById != userId)
                {
                    var exposed = await _context.tbl_ArtifactRefs.AnyAsync(r => r.ArtifactId == targetId && r.Channel == Channel.Client)
                                  || await _context.tbl_ArtifactRevisions.AnyAsync(r => r.ArtifactId == targetId
                                        && r.Document != null && r.Document.Channel == Channel.Client);
                    if (!exposed) return null;
                }
                return new TargetInfo(t.OriginalFileName ?? "File", t.CapturedAt ?? t.DateTimeCreated, targetId);
            }
            case CommitmentLinkTarget.Annotation:
            {
                // A layer is addressed by its layer id; the reader sees its
                // current version, and only if they may see the file under it.
                var layer = await _context.tbl_Annotations.AsNoTracking()
                    .Where(x => x.LayerId == targetId && x.ProjectId == pid && x.SupersededAt == null)
                    .Select(x => new { x.ArtifactId, x.Version, x.Channel, x.DateTimeCreated })
                    .FirstOrDefaultAsync();
                if (layer?.ArtifactId is null || (!access.CanSeeSiteLog && layer.Channel != Channel.Client)) return null;
                var file = await DescribeTargetAsync(CommitmentLinkTarget.Artifact, layer.ArtifactId, project, access, userId);
                if (file is null) return null;
                return new TargetInfo($"Marked up: {file.Value.Label} (v{layer.Version})", layer.DateTimeCreated, layer.ArtifactId);
            }
            default:
                return null;
        }
    }

    private static string? Excerpt(string? text) =>
        text is null ? null : text.Length <= 120 ? text : text[..117] + "…";

    private static CommitmentLinkDto ToLinkDto(tbl_CommitmentLink l, tbl_Commitment c, TargetInfo target) => new()
    {
        Id = l.Id,
        CommitmentId = c.Id,
        CommitmentTitle = c.Title,
        CommitmentKind = c.Kind,
        TargetType = l.TargetType,
        TargetId = l.TargetId,
        Relation = l.Relation,
        Note = l.Note,
        TargetLabel = target.Label,
        TargetDate = target.Date,
        ArtifactId = target.ArtifactId,
        DateTimeCreated = l.DateTimeCreated
    };
}
