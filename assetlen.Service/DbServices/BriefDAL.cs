using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.FileProcessingServices.Brief;
using assetlen.Service.FileProcessingServices.Extraction;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IBriefDAL"/>
public class BriefDAL : IBriefDAL
{
    /// <summary>How far back the brief looks for the "before" half of a same-view pair.</summary>
    private const int VantageLookbackDays = 45;

    /// <summary>A photo with no words of its own belongs to what its sender said within this many minutes.</summary>
    private const int BurstMinutes = 45;

    private const int NotesPerBlock = 6;
    private const int FramesPerBlock = 6;
    private const int PairsPerBlock = 2;

    private static readonly Regex CameraName = new(@"^(?:IMG|VID|PTT|AUD|DOC|STK|DSC|PXL|WhatsApp)[\s_\-]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly AssetlenDbContext _context;
    private readonly IProjectAccessService _access;
    private readonly IVantageIndex _vantage;
    private readonly ILogger<BriefDAL> _logger;
    private readonly int _cutoffHour;

    public BriefDAL(AssetlenDbContext context, IProjectAccessService access, IVantageIndex vantage,
        IConfiguration config, ILogger<BriefDAL> logger)
    {
        _context = context;
        _access = access;
        _vantage = vantage;
        _logger = logger;
        _cutoffHour = int.TryParse(config["Brief:CutoffHour"], out var h) && h is >= 0 and <= 23 ? h : 20;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Who stands where
    // ═════════════════════════════════════════════════════════════════════

    private sealed record Standing(tbl_Project Project, ProjectAccess Access);

    /// <summary>The dashboard's candidate rule, then one membership query for all of them.</summary>
    private async Task<Dictionary<string, Standing>> LoadProjectsAsync(string userId, string? projectId, CancellationToken ct)
    {
        var q = _context.tbl_Projects_RS.AsNoTracking()
            .Include(p => p.ParentProject)
            .Where(p => p.ArchivedAt == null && (p.ParentProject == null || p.ParentProject.ArchivedAt == null))
            .Where(p => p.InvestorId == userId
                        || p.ProjectManagerId == userId
                        || (p.ParentProject != null && (p.ParentProject.InvestorId == userId || p.ParentProject.ProjectManagerId == userId))
                        || _context.tbl_ProjectMembers.Any(m => m.ProjectId == p.Id && m.UserId == userId && m.IsActive)
                        || (p.ParentProjectId != null && _context.tbl_ProjectMembers.Any(m => m.ProjectId == p.ParentProjectId && m.UserId == userId && m.IsActive)));

        if (!string.IsNullOrEmpty(projectId)) q = q.Where(p => p.Id == projectId);

        var projects = await q.ToListAsync(ct);
        var standings = await _access.ResolveManyAsync(projects, userId, ct);

        return projects
            .Where(p => standings.TryGetValue(p.Id, out var a) && a.CanRead)
            .ToDictionary(p => p.Id, p => new Standing(p, standings[p.Id]));
    }

    /// <summary>
    /// The funder reads for progress, money and dates; the representative for
    /// specs, finishes and the choices she owes (Dinah.md). A client-side
    /// principal kept off the money reads as the representative whatever her title.
    /// </summary>
    private static ReaderEmphasis EmphasisOf(ProjectAccess a) =>
        a.Side != ProjectSide.Client ? ReaderEmphasis.Delivery
        : a.Specialization == ProjectMemberSpecialization.ClientRepresentative || !a.CanSeeMoney ? ReaderEmphasis.Representative
        : ReaderEmphasis.Funder;

    /// <summary>The thread's read rule (SearchDAL, IngestDAL): the principals, of the side that forwarded it, or its importer.</summary>
    private static bool CanReadThread(ProjectSide side, string? importedById, ProjectAccess a, string userId) =>
        (!string.IsNullOrEmpty(importedById) && importedById == userId)
        || (a.CanRead && a.Seat == ProjectSeat.Principal && (a.CanSeeSiteLog || a.Side == side));

    /// <summary>What was read out of the thread is exactly as readable as the thread (ExtractionDAL).</summary>
    private static bool CanReadExtracted(ProjectSide side, string? importedById, ProjectAccess a, string userId) =>
        a.CanSeeSiteLog || a.Side == side || (!string.IsNullOrEmpty(importedById) && importedById == userId);

    private static bool CanDecideMoney(ProjectAccess a) => a.CanWrite && a.CanSeeMoney && a.Side == ProjectSide.Client;

    // ═════════════════════════════════════════════════════════════════════
    // Decisions owed
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<List<OwedItemDto>>> GetOwedAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var scope = await LoadProjectsAsync(userId, null, ct);
            return ServiceResult<List<OwedItemDto>>.Success(await OwedAsync(scope, userId, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing what {UserId} owes", userId);
            return ServiceResult<List<OwedItemDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    private async Task<List<OwedItemDto>> OwedAsync(Dictionary<string, Standing> scope, string userId, CancellationToken ct)
    {
        var today = DateTime.Now.Date;
        var owed = new List<OwedItemDto>();
        var pids = scope.Keys.ToList();
        if (pids.Count == 0) return owed;

        string NameOf(string? pid) => pid is not null && scope.TryGetValue(pid, out var s) ? s.Project.ProjectName ?? "—" : "—";
        ProjectAccess AccessOn(string? pid) => pid is not null && scope.TryGetValue(pid, out var s) ? s.Access : ProjectAccess.None;

        // ── Register: choices this side owes, spoken agreements waiting on this reader ──
        var registerPids = scope.Where(s => s.Value.Access.CanSeeRegister).Select(s => s.Key).ToList();
        if (registerPids.Count > 0)
        {
            var open = await _context.tbl_Commitments.AsNoTracking()
                .Include(c => c.Stage)
                .Include(c => c.Deliverable)
                .Where(c => c.ProjectId != null && registerPids.Contains(c.ProjectId) && c.SupersededAt == null
                            && ((c.Kind == CommitmentKind.Choice && c.Maturity < CommitmentMaturity.Agreed)
                                || ((c.SourceChannel == CommitmentSource.Verbal || c.SourceChannel == CommitmentSource.Meeting)
                                    && c.CounterpartyConfirmedAt == null && c.DisputedAt == null)))
                .ToListAsync(ct);

            foreach (var c in open)
            {
                var a = AccessOn(c.ProjectId);
                var choice = c.Kind == CommitmentKind.Choice && c.Maturity < CommitmentMaturity.Agreed
                             && a.Side is not null && c.OwedBySide == a.Side;
                var confirm = CommitmentDAL.CanAnswerSpoken(c, a, userId);
                if (!choice && !confirm) continue;

                var due = c.DueDate ?? c.Deliverable?.DueDate;
                owed.Add(new OwedItemDto
                {
                    Key = $"commitment:{c.Id}",
                    Kind = choice ? OwedKind.Choice : OwedKind.Confirmation,
                    ProjectId = c.ProjectId!,
                    ProjectName = NameOf(c.ProjectId),
                    StageName = c.Stage?.StageName,
                    StagePhase = c.Stage?.Phase,
                    DeliverableTitle = c.Deliverable?.Title,
                    Title = choice ? c.Title ?? "A choice" : $"Confirm what was agreed: {c.Title}",
                    Consequence = choice ? ConsequenceOf(c, today) : "Until you confirm it, it stands as their word alone",
                    DueBy = due,
                    IsOverdue = due is { } d && d.Date < today,
                    Amount = a.CanSeeMoney ? c.Amount : null,
                    Currency = a.CanSeeMoney ? c.Currency : null,
                    AmountHidden = !a.CanSeeMoney && c.Amount is not null,
                    Href = $"/project/{c.ProjectId}/register?commitment={c.Id}"
                });
            }
        }

        // ── Questions and blockers assigned to this reader — whichever channel they were raised on ──
        var flags = await _context.tbl_Flags.AsNoTracking()
            .Include(f => f.Stage)
            .Where(f => f.ProjectId != null && pids.Contains(f.ProjectId) && f.AssignedToId == userId
                        && (f.Status == FlagStatus.Open || f.Status == FlagStatus.InProgress))
            .ToListAsync(ct);
        foreach (var f in flags)
        {
            owed.Add(new OwedItemDto
            {
                Key = $"flag:{f.Id}",
                Kind = OwedKind.Question,
                ProjectId = f.ProjectId!,
                ProjectName = NameOf(f.ProjectId),
                StageName = f.Stage?.StageName,
                StagePhase = f.Stage?.Phase,
                Title = f.Title ?? "Open question",
                Consequence = Trim(f.Description, 120) ?? (f.CommitmentId is null ? "Work waits on this" : "A query on something agreed"),
                DueBy = f.DueDate,
                IsOverdue = f.DueDate is { } d && d.Date < today,
                Href = $"/project/{f.ProjectId}/register?view=questions"
            });
        }

        // ── Money the funder decides ──
        var decidePids = scope.Where(s => CanDecideMoney(s.Value.Access)).Select(s => s.Key).ToList();
        if (decidePids.Count > 0)
        {
            var claims = await _context.tbl_StageClaims.AsNoTracking()
                .Include(c => c.Stage)
                .Where(c => c.ProjectId != null && decidePids.Contains(c.ProjectId) && c.Status == ClaimStatus.Claimed)
                .ToListAsync(ct);
            foreach (var c in claims)
            {
                owed.Add(new OwedItemDto
                {
                    Key = $"claim:{c.Id}",
                    Kind = OwedKind.Claim,
                    ProjectId = c.ProjectId!,
                    ProjectName = NameOf(c.ProjectId),
                    StageName = c.Stage?.StageName,
                    StagePhase = c.Stage?.Phase,
                    Title = $"Clear the claim on {c.Stage?.StageName ?? "this project"}",
                    Consequence = Trim(c.Note, 120) ?? "The delivery side is waiting to be paid for work it says is done",
                    Amount = c.Amount,
                    Currency = scope[c.ProjectId!].Project.Currency ?? "UGX",
                    Href = $"/project/{c.ProjectId}/money"
                });
            }

            var variations = await _context.tbl_Variations.AsNoTracking()
                .Include(v => v.Stage)
                .Where(v => v.ProjectId != null && decidePids.Contains(v.ProjectId) && v.Status == VariationStatus.Proposed)
                .ToListAsync(ct);
            foreach (var v in variations)
            {
                owed.Add(new OwedItemDto
                {
                    Key = $"variation:{v.Id}",
                    Kind = OwedKind.Variation,
                    ProjectId = v.ProjectId!,
                    ProjectName = NameOf(v.ProjectId),
                    StageName = v.Stage?.StageName,
                    StagePhase = v.Stage?.Phase,
                    Title = $"An extra waits on your yes or no: {v.Title}",
                    Consequence = v.CostDelta is null
                        ? "Not costed yet — agreeing to it now is agreeing to an unknown figure"
                        : v.TimeDeltaDays is > 0 ? $"Adds {v.TimeDeltaDays} days" : Trim(v.Reason, 120),
                    Amount = v.CostDelta,
                    Currency = v.Currency ?? scope[v.ProjectId!].Project.Currency ?? "UGX",
                    Href = $"/project/{v.ProjectId}/money"
                });
            }
        }

        // ── Releases stalled at either end (FundingDAL.GetFundingNeedingMe's rule, on these projects) ──
        var funding = await _context.tbl_FundingEntries.AsNoTracking()
            .Include(f => f.Project)
            .Include(f => f.Stage)
            .Where(f => f.ProjectId != null && pids.Contains(f.ProjectId)
                        && ((f.Status == FundingStatus.Pending && f.Project!.ProjectManagerId == userId)
                            || (f.Status == FundingStatus.AmountQueried && (f.PaidById == userId || f.Project!.InvestorId == userId))))
            .ToListAsync(ct);
        foreach (var f in funding)
        {
            var gap = f.Status == FundingStatus.AmountQueried;
            owed.Add(new OwedItemDto
            {
                Key = $"funding:{f.Id}",
                Kind = OwedKind.Funding,
                ProjectId = f.ProjectId!,
                ProjectName = NameOf(f.ProjectId),
                StageName = f.Stage?.StageName,
                StagePhase = f.Stage?.Phase,
                Title = gap
                    ? $"Less arrived than was sent on {f.Stage?.StageName ?? "this project"}"
                    : $"Confirm the release on {f.Stage?.StageName ?? "this project"}",
                Consequence = gap
                    ? "Accept what landed or take it up — until then the stage's money is in question"
                    : "Until it is acknowledged, the funder cannot count it as landed",
                Amount = gap ? (f.Amount - (f.ReceivedAmount ?? f.Amount)) : f.Amount,
                Currency = f.Project?.Currency ?? "UGX",
                Href = $"/project/{f.ProjectId}/money"
            });
        }

        // ── What was read from the thread and waits for one tap ──
        var proposalPids = scope.Where(s => s.Value.Access.CanSeeRegister && s.Value.Access.CanWrite).Select(s => s.Key).ToList();
        if (proposalPids.Count > 0)
        {
            var pending = await _context.tbl_ExtractionProposals.AsNoTracking()
                .Where(p => p.ProjectId != null && proposalPids.Contains(p.ProjectId) && p.Status == ProposalStatus.Pending)
                .Select(p => new { p.ProjectId, p.SourceSide, p.SourceImportedById, p.SourceSentAt })
                .ToListAsync(ct);

            foreach (var g in pending.Where(p => CanReadExtracted(p.SourceSide, p.SourceImportedById, AccessOn(p.ProjectId), userId))
                                     .GroupBy(p => p.ProjectId!))
            {
                var n = g.Count();
                owed.Add(new OwedItemDto
                {
                    Key = $"proposals:{g.Key}",
                    Kind = OwedKind.Proposals,
                    ProjectId = g.Key,
                    ProjectName = NameOf(g.Key),
                    Title = n == 1 ? "One item read from the thread waits for you" : $"{n} items read from the thread wait for you",
                    Consequence = "Until you accept them they are not in your register — nothing extracted becomes truth on its own",
                    Href = $"/project/{g.Key}/register?view=thread"
                });
            }
        }

        // Soonest by-when first, undated last — an item with a real deadline
        // outranks one somebody can sit on (Law 4).
        return owed
            .OrderBy(o => o.DueBy ?? DateTime.MaxValue)
            .ThenBy(o => o.Kind)
            .ThenBy(o => o.ProjectName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>What waiting on a choice costs, in words — what it holds up and when that work is due.</summary>
    private static string? ConsequenceOf(tbl_Commitment c, DateTime today)
    {
        var parts = new List<string>();
        if (c.Deliverable?.Title is { } d) parts.Add($"Holds up {d}");
        else if (c.Stage?.StageName is { } s) parts.Add($"Holds up {s}");

        if (c.Stage?.StartDate is { } start && start.Date >= today)
            parts.Add($"{c.Stage.StageName} is due to start {start:d MMM}");
        if (c.LeadTimeDays is > 0)
            parts.Add($"{c.LeadTimeDays} days' lead time once decided");

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Home
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<HomeDto>> GetHomeAsync(string userId, int movedDays, CancellationToken ct = default)
    {
        try
        {
            movedDays = Math.Clamp(movedDays, 1, 60);
            var since = DateTime.Now.Date.AddDays(-(movedDays - 1));
            var scope = await LoadProjectsAsync(userId, null, ct);
            var pids = scope.Keys.ToList();

            var owed = await OwedAsync(scope, userId, ct);
            var money = await MoneyAsync(scope, ct);
            var moved = await MovedAsync(scope, userId, since, ct);

            var stages = await _context.tbl_Stages.AsNoTracking()
                .Where(s => s.ProjectId != null && pids.Contains(s.ProjectId))
                .ToListAsync(ct);
            var stageIds = stages.Select(s => s.Id).ToList();
            var readings = await _context.tbl_ProgressReadings.AsNoTracking()
                .Where(r => r.StageId != null && stageIds.Contains(r.StageId))
                .Select(r => new { r.ProjectId, r.StageId, r.Percent, r.ObservedAt, r.SourceSide, r.SourceImportedById })
                .ToListAsync(ct);
            var blockers = await _context.tbl_Flags.AsNoTracking()
                .Where(f => f.ProjectId != null && pids.Contains(f.ProjectId) && f.CommitmentId == null
                            && (f.Status == FlagStatus.Open || f.Status == FlagStatus.InProgress))
                .GroupBy(f => f.ProjectId!)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

            var dto = new HomeDto { Since = since, Owed = owed };

            foreach (var (pid, s) in scope.OrderBy(x => x.Value.Project.ParentProjectId is null ? 0 : 1)
                                          .ThenByDescending(x => x.Value.Project.DateTimeCreated))
            {
                var a = s.Access;
                var mine = owed.Where(o => o.ProjectId == pid).ToList();
                var current = stages.Where(st => st.ProjectId == pid && st.Status == StageStatus.InProgress)
                    .OrderBy(st => st.DisplayOrder).FirstOrDefault()
                    ?? stages.Where(st => st.ProjectId == pid && st.Status == StageStatus.NotStarted)
                        .OrderBy(st => st.DisplayOrder).FirstOrDefault();
                var reading = current is null ? null : readings
                    .Where(r => r.StageId == current.Id && (a.CanSeeSiteLog || r.SourceSide == null || r.SourceSide == a.Side || r.SourceImportedById == userId))
                    .OrderByDescending(r => r.ObservedAt).FirstOrDefault();

                var lines = moved.TryGetValue(pid, out var m) ? m : new List<MovedLineDto>();
                dto.Projects.Add(new HomeProjectDto
                {
                    Id = pid,
                    Name = s.Project.ProjectName ?? "—",
                    ParentProjectId = s.Project.ParentProjectId,
                    ParentName = s.Project.ParentProject?.ProjectName,
                    Standing = ProjectAccessDto.From(a, pid),
                    Emphasis = EmphasisOf(a),
                    Money = a.CanSeeMoney ? money.GetValueOrDefault(pid) : null,
                    OwedCount = mine.Count,
                    OverdueCount = mine.Count(o => o.IsOverdue),
                    NextOwed = mine.FirstOrDefault(),
                    CurrentStageName = current?.StageName,
                    CurrentStagePhase = current?.Phase,
                    CurrentStagePercent = reading?.Percent ?? current?.CompletionPercentage,
                    Moved = lines.OrderByDescending(l => l.At).Take(5).ToList(),
                    LastMovedAt = lines.Count == 0 ? null : lines.Max(l => l.At),
                    OpenBlockers = a.CanSeeRegister || a.Side == ProjectSide.Client ? blockers.GetValueOrDefault(pid) : 0
                });
            }

            var withMoney = dto.Projects.Where(p => p.Money is not null).Select(p => p.Money!).ToList();
            var currencies = withMoney.Select(m => m.Currency).Distinct().ToList();
            if (withMoney.Count > 0 && currencies.Count == 1)
            {
                dto.Currency = currencies[0];
                dto.Portfolio = new HomeMoneyDto
                {
                    Currency = currencies[0],
                    Budget = withMoney.Sum(m => m.Budget),
                    Funded = withMoney.Sum(m => m.Funded),
                    PendingFunding = withMoney.Sum(m => m.PendingFunding),
                    Claimed = withMoney.Sum(m => m.Claimed),
                    Cleared = withMoney.Sum(m => m.Cleared),
                    AwaitingClearance = withMoney.Sum(m => m.AwaitingClearance),
                    InHand = withMoney.Sum(m => m.InHand),
                    VariationsApproved = withMoney.Sum(m => m.VariationsApproved),
                    VariationsUncosted = withMoney.Sum(m => m.VariationsUncosted)
                };
            }

            return ServiceResult<HomeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assembling home for {UserId}", userId);
            return ServiceResult<HomeDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// The stage ledger's totals, per project, from the same rows and the same
    /// rules (LedgerDAL) — so the figure on the home and on the money tab can
    /// never disagree. Conservation makes the totals independent of ordering.
    /// </summary>
    private async Task<Dictionary<string, HomeMoneyDto>> MoneyAsync(Dictionary<string, Standing> scope, CancellationToken ct)
    {
        var pids = scope.Where(s => s.Value.Access.CanSeeMoney).Select(s => s.Key).ToList();
        var result = new Dictionary<string, HomeMoneyDto>();
        if (pids.Count == 0) return result;

        var stages = await _context.tbl_Stages.AsNoTracking()
            .Where(s => s.ProjectId != null && pids.Contains(s.ProjectId))
            .Select(s => new { s.Id, s.ProjectId, s.BudgetAmount })
            .ToListAsync(ct);
        var funding = await _context.tbl_FundingEntries.AsNoTracking()
            .Where(f => f.ProjectId != null && pids.Contains(f.ProjectId))
            .Select(f => new { f.ProjectId, f.StageId, f.Status, f.Amount, f.ReceivedAmount })
            .ToListAsync(ct);
        var claims = await _context.tbl_StageClaims.AsNoTracking()
            .Where(c => c.ProjectId != null && pids.Contains(c.ProjectId) && c.Status != ClaimStatus.Withdrawn)
            .Select(c => new { c.ProjectId, c.StageId, c.Status, c.Amount, c.ClearedAmount })
            .ToListAsync(ct);
        var variations = await _context.tbl_Variations.AsNoTracking()
            .Where(v => v.ProjectId != null && pids.Contains(v.ProjectId))
            .Select(v => new { v.ProjectId, v.StageId, v.Status, v.CostDelta })
            .ToListAsync(ct);

        foreach (var pid in pids)
        {
            var ids = stages.Where(s => s.ProjectId == pid).Select(s => s.Id).ToHashSet();
            var f = funding.Where(x => x.StageId != null && ids.Contains(x.StageId)).ToList();
            var c = claims.Where(x => x.StageId != null && ids.Contains(x.StageId)).ToList();
            var v = variations.Where(x => x.StageId != null && ids.Contains(x.StageId)).ToList();

            var funded = f.Where(x => x.Status is FundingStatus.Confirmed or FundingStatus.Settled).Sum(x => x.ReceivedAmount ?? x.Amount);
            var cleared = c.Where(x => x.Status == ClaimStatus.Cleared).Sum(x => x.ClearedAmount ?? x.Amount);

            result[pid] = new HomeMoneyDto
            {
                Currency = scope[pid].Project.Currency ?? "UGX",
                Budget = stages.Where(s => s.ProjectId == pid).Sum(s => s.BudgetAmount ?? 0m),
                Funded = funded,
                PendingFunding = f.Where(x => x.Status is FundingStatus.Pending or FundingStatus.AmountQueried).Sum(x => x.ReceivedAmount ?? x.Amount),
                Claimed = c.Sum(x => x.Amount),
                Cleared = cleared,
                AwaitingClearance = c.Where(x => x.Status == ClaimStatus.Claimed).Sum(x => x.Amount),
                InHand = funded - cleared,
                VariationsApproved = v.Where(x => x.Status == VariationStatus.Approved).Sum(x => x.CostDelta ?? 0m),
                VariationsUncosted = v.Count(x => x.CostDelta is null && x.Status is VariationStatus.Proposed or VariationStatus.Approved)
            };
        }

        return result;
    }

    /// <summary>What moved since the reader last had reason to look — each line one fact, each through its own surface's rule.</summary>
    private async Task<Dictionary<string, List<MovedLineDto>>> MovedAsync(Dictionary<string, Standing> scope, string userId, DateTime since, CancellationToken ct)
    {
        var result = scope.Keys.ToDictionary(k => k, _ => new List<MovedLineDto>());
        var pids = scope.Keys.ToList();
        if (pids.Count == 0) return result;
        var sinceUtc = since.ToUniversalTime();

        // Commitments, for the seats that have a register.
        var registerPids = scope.Where(s => s.Value.Access.CanSeeRegister).Select(s => s.Key).ToList();
        var commitments = await _context.tbl_Commitments.AsNoTracking()
            .Include(c => c.Stage)
            .Where(c => c.ProjectId != null && registerPids.Contains(c.ProjectId) && c.DateTimeCreated >= sinceUtc)
            .ToListAsync(ct);
        foreach (var c in commitments)
        {
            result[c.ProjectId!].Add(new MovedLineDto
            {
                Kind = MovedKind.Commitment,
                Text = c.SupersedesId is null ? $"Recorded: {c.Title}" : $"Restated: {c.Title}",
                At = Local(c.DateTimeCreated),
                Href = $"/project/{c.ProjectId}/register?commitment={c.Id}",
                StageName = c.Stage?.StageName,
                StagePhase = c.Stage?.Phase
            });
        }

        // The thread: one line per project, counted, never a message list.
        var batches = await _context.tbl_IngestBatches.AsNoTracking()
            .Where(b => b.ProjectId != null && pids.Contains(b.ProjectId))
            .Select(b => new { b.Id, b.ProjectId, b.ImportedSide, b.ImportedById })
            .ToListAsync(ct);
        var readable = batches.Where(b => CanReadThread(b.ImportedSide, b.ImportedById, scope[b.ProjectId!].Access, userId))
            .Select(b => b.Id).ToList();
        if (readable.Count > 0)
        {
            var thread = await _context.tbl_IngestedMessages.AsNoTracking()
                .Where(m => m.BatchId != null && readable.Contains(m.BatchId) && !m.IsSystemMessage && m.SentAt >= since)
                .GroupBy(m => m.ProjectId!)
                .Select(g => new { g.Key, Messages = g.Count(m => m.ArtifactId == null), Media = g.Count(m => m.ArtifactId != null), Last = g.Max(m => m.SentAt) })
                .ToListAsync(ct);
            foreach (var t in thread)
            {
                var text = t.Media > 0 ? $"{t.Messages} messages and {t.Media} files in the thread" : $"{t.Messages} messages in the thread";
                result[t.Key].Add(new MovedLineDto
                {
                    Kind = MovedKind.Thread,
                    Text = text,
                    At = t.Last,
                    Href = $"/project/{t.Key}/brief?day={t.Last:yyyy-MM-dd}"
                });
            }
        }

        // Captures, as the reader's side may see them.
        var captures = await _context.tbl_ProgressUpdates.AsNoTracking()
            .Include(u => u.Stage)
            .Where(u => u.ProjectId != null && pids.Contains(u.ProjectId) && u.DateTimeCreated >= sinceUtc)
            .Select(u => new { u.Id, u.ProjectId, u.Channel, StageName = u.Stage != null ? u.Stage.StageName : null, Phase = u.Stage != null ? (StageGroup?)u.Stage.Phase : null, u.DateTimeCreated })
            .ToListAsync(ct);
        foreach (var g in captures.Where(u => scope[u.ProjectId!].Access.CanSeeSiteLog || u.Channel == Channel.Client)
                                  .GroupBy(u => (u.ProjectId!, u.StageName)))
        {
            var last = g.OrderByDescending(u => u.DateTimeCreated).First();
            result[g.Key.Item1].Add(new MovedLineDto
            {
                Kind = MovedKind.Capture,
                Text = g.Count() == 1 ? $"Captured on {g.Key.StageName ?? "site"}" : $"{g.Count()} captures on {g.Key.StageName ?? "site"}",
                At = Local(last.DateTimeCreated),
                Href = $"/project/{g.Key.Item1}/entry/{last.Id}",
                StageName = g.Key.StageName,
                StagePhase = last.Phase
            });
        }

        // Progress readings — where the work says it stands.
        var readings = await _context.tbl_ProgressReadings.AsNoTracking()
            .Include(r => r.Stage)
            .Where(r => r.ProjectId != null && pids.Contains(r.ProjectId) && r.ObservedAt >= since && r.SourceKind != ProgressReadingSource.Stage)
            .ToListAsync(ct);
        foreach (var r in readings)
        {
            var a = scope[r.ProjectId!].Access;
            if (!(a.CanSeeSiteLog || r.SourceSide == null || r.SourceSide == a.Side || r.SourceImportedById == userId)) continue;
            result[r.ProjectId!].Add(new MovedLineDto
            {
                Kind = MovedKind.Reading,
                Text = $"{r.Stage?.StageName ?? r.Subject} at {r.Percent:0.#}%",
                At = r.ObservedAt,
                Href = $"/project/{r.ProjectId}/brief?day={r.ObservedAt:yyyy-MM-dd}",
                StageName = r.Stage?.StageName,
                StagePhase = r.Stage?.Phase
            });
        }

        // Money, for the money seats.
        var moneyPids = scope.Where(s => s.Value.Access.CanSeeMoney).Select(s => s.Key).ToList();
        if (moneyPids.Count > 0)
        {
            var releases = await _context.tbl_FundingEntries.AsNoTracking()
                .Include(f => f.Stage)
                .Where(f => f.ProjectId != null && moneyPids.Contains(f.ProjectId) && f.DateTimeCreated >= sinceUtc)
                .ToListAsync(ct);
            foreach (var f in releases)
                result[f.ProjectId!].Add(new MovedLineDto
                {
                    Kind = MovedKind.Money,
                    Text = $"Release recorded on {f.Stage?.StageName ?? "the project"}",
                    At = Local(f.DateTimeCreated),
                    Href = $"/project/{f.ProjectId}/money",
                    StageName = f.Stage?.StageName,
                    StagePhase = f.Stage?.Phase
                });

            var claims = await _context.tbl_StageClaims.AsNoTracking()
                .Include(c => c.Stage)
                .Where(c => c.ProjectId != null && moneyPids.Contains(c.ProjectId) && c.DateTimeCreated >= sinceUtc)
                .ToListAsync(ct);
            foreach (var c in claims)
                result[c.ProjectId!].Add(new MovedLineDto
                {
                    Kind = MovedKind.Money,
                    Text = c.Status == ClaimStatus.Cleared ? $"Claim cleared on {c.Stage?.StageName}" : $"Claim raised on {c.Stage?.StageName}",
                    At = Local(c.DateTimeCreated),
                    Href = $"/project/{c.ProjectId}/money",
                    StageName = c.Stage?.StageName,
                    StagePhase = c.Stage?.Phase
                });
        }

        // Blockers reach the client side whichever channel raised them — the truth floor.
        var blockers = await _context.tbl_Flags.AsNoTracking()
            .Include(f => f.Stage)
            .Where(f => f.ProjectId != null && pids.Contains(f.ProjectId) && f.CommitmentId == null && f.DateTimeCreated >= sinceUtc)
            .ToListAsync(ct);
        foreach (var f in blockers)
        {
            var a = scope[f.ProjectId!].Access;
            if (!(a.CanSeeSiteLog || f.Channel == Channel.Client || a.Side == ProjectSide.Client)) continue;
            result[f.ProjectId!].Add(new MovedLineDto
            {
                Kind = MovedKind.Blocker,
                Text = $"Blocker: {f.Title}",
                At = Local(f.DateTimeCreated),
                Href = $"/project/{f.ProjectId}/register?view=questions",
                StageName = f.Stage?.StageName,
                StagePhase = f.Stage?.Phase
            });
        }

        return result;
    }

    // ═════════════════════════════════════════════════════════════════════
    // The daily brief
    // ═════════════════════════════════════════════════════════════════════

    private sealed record Frame(
        string ArtifactId, string Sha, string? Mime, string? ThumbPath, string? StoragePath, bool HasThumb,
        DateTime At, Filing Filing, string Origin, string? Href, string? Caption);

    private sealed class Block
    {
        public required string Key { get; init; }
        public tbl_Stage? Stage { get; init; }
        public tbl_Deliverable? Deliverable { get; init; }
        public List<BriefNoteDto> Notes { get; } = new();
        public List<Frame> Frames { get; } = new();
        public List<TruthItemDto> Commitments { get; } = new();
        public decimal? Percent { get; set; }
        public decimal? PercentBefore { get; set; }
        public string? PercentSource { get; set; }
    }

    public async Task<ServiceResult<DailyBriefDto>> GetBriefAsync(string projectId, DateTime? day, int days, string userId, CancellationToken ct = default)
    {
        try
        {
            var scope = await LoadProjectsAsync(userId, projectId, ct);

            // The brief is written for the client side and whoever publishes to
            // it. The bench is answered as if it did not exist — absent, not refused.
            if (!scope.TryGetValue(projectId, out var standing) || !standing.Access.CanSeeBrief)
                return ServiceResult<DailyBriefDto>.Failure(new NotFoundException("Project not found."));

            var project = standing.Project;
            var a = standing.Access;
            days = Math.Clamp(days, 1, 31);
            var last = (day ?? DateTime.Now).Date;
            var from = last.AddDays(-(days - 1));
            var to = last.AddDays(1);
            var lookFrom = from.AddDays(-VantageLookbackDays);
            var emphasis = EmphasisOf(a);

            var stages = await _context.tbl_Stages.AsNoTracking().Where(s => s.ProjectId == projectId).ToListAsync(ct);
            var deliverables = await _context.tbl_Deliverables.AsNoTracking().Where(d => d.ProjectId == projectId).ToListAsync(ct);
            var stageById = stages.ToDictionary(s => s.Id);
            var deliverableById = deliverables.ToDictionary(d => d.Id);
            var filer = new BriefFiler(
                stages.Select(s => new StageRef(s.Id, s.StageName ?? "", s.CatalogueKey)).ToList(),
                deliverables.Where(d => d.StageId != null).Select(d => new DeliverableRef(d.Id, d.StageId!, d.Title ?? "")).ToList());

            var members = await _context.tbl_ProjectMembers.AsNoTracking()
                .Include(m => m.User)
                .Where(m => (m.ProjectId == projectId || m.ProjectId == project.ParentProjectId) && m.IsActive)
                .ToListAsync(ct);
            var mediator = members.Where(m => m.IsMediator).OrderBy(m => m.ProjectId == projectId ? 0 : 1).FirstOrDefault();
            var mediatorName = mediator is null ? null : MemberName(mediator);
            var contractorSide = members.Where(m => m.Side == ProjectSide.Contractor && m.UserId != null).Select(m => m.UserId!).ToHashSet();

            var blocks = new Dictionary<string, Block>();
            Block BlockFor(Filing f)
            {
                var key = f.DeliverableId ?? f.StageId ?? "unfiled";
                if (blocks.TryGetValue(key, out var b)) return b;
                b = new Block
                {
                    Key = key,
                    Stage = f.StageId is not null ? stageById.GetValueOrDefault(f.StageId) : null,
                    Deliverable = f.DeliverableId is not null ? deliverableById.GetValueOrDefault(f.DeliverableId) : null
                };
                blocks[key] = b;
                return b;
            }

            var counts = new BriefCountsDto();
            var frames = new List<Frame>();

            // ── The thread ─────────────────────────────────────────────────
            var batches = await _context.tbl_IngestBatches.AsNoTracking()
                .Where(b => b.ProjectId == projectId)
                .Select(b => new { b.Id, b.ImportedSide, b.ImportedById, b.ArchiveArtifactId })
                .ToListAsync(ct);
            var readable = batches.Where(b => CanReadThread(b.ImportedSide, b.ImportedById, a, userId)).Select(b => b.Id).ToList();

            var messages = readable.Count == 0 ? new List<tbl_IngestedMessage>() : await _context.tbl_IngestedMessages.AsNoTracking()
                .Include(m => m.AuthorMember).ThenInclude(x => x!.User)
                .Where(m => m.BatchId != null && readable.Contains(m.BatchId) && !m.IsSystemMessage
                            && m.SentAt >= lookFrom && m.SentAt < to)
                .OrderBy(m => m.SentAt).ThenBy(m => m.SequenceNo)
                .ToListAsync(ct);
            var messageIds = messages.Select(m => m.Id).ToList();

            // A human decided which deliverable a commitment is on; that beats any guess from its wording.
            var decidedFiling = await _context.tbl_Commitments.AsNoTracking()
                .Where(c => c.ProjectId == projectId && c.IngestedMessageId != null && messageIds.Contains(c.IngestedMessageId) && c.DeliverableId != null)
                .Select(c => new { c.IngestedMessageId, c.StageId, c.DeliverableId })
                .ToListAsync(ct);
            var decided = decidedFiling.GroupBy(c => c.IngestedMessageId!)
                .ToDictionary(g => g.Key, g => new Filing(g.First().StageId ?? deliverableById.GetValueOrDefault(g.First().DeliverableId!)?.StageId, g.First().DeliverableId));

            var bindings = await _context.tbl_MediaBindings.AsNoTracking()
                .Where(b => b.IngestedMessageId != null && messageIds.Contains(b.IngestedMessageId) && b.ArtifactId != null)
                .Select(b => new { b.IngestedMessageId, b.ArtifactId })
                .ToListAsync(ct);
            var archives = batches.Select(b => b.ArchiveArtifactId).OfType<string>().ToHashSet();
            var mediaOf = messages.Where(m => m.ArtifactId != null && !archives.Contains(m.ArtifactId))
                .Select(m => (MessageId: m.Id, ArtifactId: m.ArtifactId!))
                .Concat(bindings.Select(b => (MessageId: b.IngestedMessageId!, ArtifactId: b.ArtifactId!)))
                .Distinct()
                .ToList();

            var artifactIds = mediaOf.Select(x => x.ArtifactId).Distinct().ToList();
            var artifacts = await _context.tbl_Artifacts.AsNoTracking()
                .Where(x => artifactIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);
            var captions = await _context.tbl_ArtifactRefs.AsNoTracking()
                .Where(r => r.ArtifactId != null && artifactIds.Contains(r.ArtifactId) && r.Caption != null)
                .Select(r => new { r.ArtifactId, r.Caption })
                .ToListAsync(ct);
            var humanCaption = captions
                .Where(c => !CameraName.IsMatch(c.Caption!))
                .GroupBy(c => c.ArtifactId!)
                .ToDictionary(g => g.Key, g => Path.GetFileNameWithoutExtension(g.First().Caption!));

            // Text first: what each message is about.
            var filingOf = new Dictionary<string, Filing>();
            foreach (var m in messages)
            {
                if (decided.TryGetValue(m.Id, out var f)) { filingOf[m.Id] = f; continue; }
                filingOf[m.Id] = string.IsNullOrWhiteSpace(m.Body) ? Filing.None : filer.File(m.Body);
            }

            // Then each file: its own human name, else what its sender said around it.
            // Real capture is thirteen to eighteen frames sent together with one line of words.
            var mediaByMessage = mediaOf.GroupBy(x => x.MessageId).ToDictionary(g => g.Key, g => g.Select(x => x.ArtifactId).ToList());
            foreach (var m in messages)
            {
                if (!mediaByMessage.TryGetValue(m.Id, out var ids)) continue;
                foreach (var id in ids)
                {
                    if (!artifacts.TryGetValue(id, out var art) || art.Sha256 is null) continue;
                    var caption = humanCaption.GetValueOrDefault(id);
                    var filing = caption is not null ? filer.File(caption) : Filing.None;
                    if (!filing.IsFiled) filing = filingOf[m.Id];
                    if (!filing.IsFiled) filing = BurstFiling(messages, filingOf, m);

                    frames.Add(new Frame(art.Id, art.Sha256, art.MimeType, art.ThumbnailPath, art.StoragePath,
                        art.ThumbnailPath is not null, m.SentAt, filing, "From the thread",
                        $"/project/{projectId}/history?day={m.SentAt:yyyy-MM-dd}", caption));
                }
            }

            foreach (var m in messages.Where(m => m.SentAt >= from && m.SentAt < to))
            {
                if (string.IsNullOrWhiteSpace(m.Body)) continue;
                counts.MessagesRead++;

                // "Okay", "Noted", "Good progress" are read and given no space. A
                // brief that reprints the chat is the chat.
                if (RuleMessageExtractor.IsAcknowledgement(m.Body)) { counts.AcknowledgementsSetAside++; continue; }

                var f = filingOf[m.Id];
                if (!f.IsFiled) counts.Unfiled++;
                BlockFor(f).Notes.Add(new BriefNoteDto
                {
                    Id = m.Id,
                    Text = Trim(m.Body, 320)!,
                    At = m.SentAt,
                    Who = m.AuthorMember is { } am ? MemberName(am) : m.ExternalAuthor,
                    Origin = "From the thread",
                    Href = $"/project/{projectId}/history?day={m.SentAt:yyyy-MM-dd}"
                });
            }

            // ── Captures, as this side may see them ────────────────────────
            var lookFromUtc = lookFrom.ToUniversalTime();
            var toUtc = to.ToUniversalTime();
            var captureQuery = _context.tbl_ProgressUpdates.AsNoTracking()
                .Include(u => u.Images).ThenInclude(i => i.Artifact)
                .Where(u => u.ProjectId == projectId && u.DateTimeCreated >= lookFromUtc && u.DateTimeCreated < toUtc);
            if (!a.CanSeeSiteLog) captureQuery = captureQuery.Where(u => u.Channel == Channel.Client);
            var captures = await captureQuery.ToListAsync(ct);

            var curated = false;
            foreach (var u in captures)
            {
                var at = Local(u.DateTimeCreated);
                var guess = filer.File(u.Description);
                var f = u.StageId is null ? guess
                    : guess.StageId == u.StageId ? guess
                    : new Filing(u.StageId, null);

                foreach (var img in u.Images.Where(i => a.CanSeeSiteLog || i.Channel == Channel.Client))
                {
                    if (img.Artifact is not { Sha256: not null } art) continue;
                    frames.Add(new Frame(art.Id, art.Sha256, art.MimeType, art.ThumbnailPath, art.StoragePath,
                        art.ThumbnailPath is not null, at, f, "Captured on site", $"/project/{projectId}/entry/{u.Id}", img.Caption));
                }

                if (at < from) continue;
                counts.Captures++;
                if (u.CreatedById is not null && contractorSide.Contains(u.CreatedById)) curated = true;
                if (string.IsNullOrWhiteSpace(u.Description)) { BlockFor(f); continue; }

                BlockFor(f).Notes.Add(new BriefNoteDto
                {
                    Id = u.Id,
                    Text = Trim(u.Description, 320)!,
                    At = at,
                    // One accountable face (§10.1): the client side reads the mediator's name, never the bench's.
                    Who = a.CanSeeSiteLog ? null : mediatorName,
                    Origin = "Captured on site",
                    Href = $"/project/{projectId}/entry/{u.Id}"
                });
            }

            // ── Frames in the window, and same-view pairs ──────────────────
            var windowFrames = frames.Where(x => x.At >= from && x.At < to)
                .GroupBy(x => x.ArtifactId).Select(g => g.First()).ToList();
            counts.Frames = windowFrames.Count;
            foreach (var fr in windowFrames) BlockFor(fr.Filing).Frames.Add(fr);

            // ── Progress readings ──────────────────────────────────────────
            var readingQuery = _context.tbl_ProgressReadings.AsNoTracking()
                .Where(r => r.ProjectId == projectId && r.ObservedAt < to);
            if (!a.CanSeeSiteLog)
                readingQuery = readingQuery.Where(r => r.SourceSide == null || r.SourceSide == a.Side || r.SourceImportedById == userId);
            var readings = await readingQuery.ToListAsync(ct);
            foreach (var g in readings
                         .Select(r => (R: r, F: r.StageId is not null ? new Filing(r.StageId, null) : filer.File(r.Subject)))
                         .Where(x => x.F.StageId is not null)
                         .GroupBy(x => x.F.StageId!))
            {
                var inWindow = g.Where(x => x.R.ObservedAt >= from).OrderByDescending(x => x.R.ObservedAt).FirstOrDefault();
                if (inWindow.R is null) continue;
                var before = g.Where(x => x.R.ObservedAt < from).OrderByDescending(x => x.R.ObservedAt).FirstOrDefault();

                var ofStage = blocks.Values.Where(b => b.Stage?.Id == g.Key).ToList();
                if (ofStage.Count == 0) ofStage.Add(BlockFor(new Filing(g.Key, null)));
                foreach (var b in ofStage)
                {
                    b.Percent = inWindow.R.Percent;
                    b.PercentBefore = before.R?.Percent;
                    b.PercentSource = inWindow.R.SourceKind switch
                    {
                        ProgressReadingSource.Ingested => $"Said in the thread, {inWindow.R.ObservedAt:d MMM}",
                        ProgressReadingSource.Capture => $"Captured on site, {inWindow.R.ObservedAt:d MMM}",
                        _ => $"Recorded {inWindow.R.ObservedAt:d MMM}"
                    };
                }
            }

            // ── The truth floor ────────────────────────────────────────────
            var owed = (await OwedAsync(scope, userId, ct)).Where(o => o.ProjectId == projectId).ToList();
            var floor = await TruthFloorAsync(project, a, userId, from, to, mediatorName, owed, deliverableById, ct);

            // Commitments that moved in the window sit in their deliverable's block too.
            foreach (var item in floor.SelectMany(s => s.Items).Where(i => i.Key.StartsWith("commitment:")))
            {
                if (item.DeliverableTitle is null && item.StageName is null) continue;
                var target = blocks.Values.FirstOrDefault(b => b.Deliverable?.Title == item.DeliverableTitle && item.DeliverableTitle != null)
                             ?? blocks.Values.FirstOrDefault(b => b.Deliverable is null && b.Stage?.StageName == item.StageName);
                if (target is not null && target.Commitments.All(c => c.Key != item.Key)) target.Commitments.Add(item);
            }

            // ── Assemble, in build order, then weighted for this reader ─────
            var dtoBlocks = new List<BriefBlockDto>();
            foreach (var b in blocks.Values)
            {
                if (b.Notes.Count == 0 && b.Frames.Count == 0 && b.Percent is null && b.Commitments.Count == 0) continue;
                dtoBlocks.Add(await ToBlockDtoAsync(b, frames, ct));
            }
            counts.Paired = dtoBlocks.Sum(b => b.Pairs.Count);

            var ordered = OrderBlocks(dtoBlocks, stages, deliverables, emphasis, owed, out var reasons);
            foreach (var b in ordered) b.EmphasisReason = reasons.GetValueOrDefault(b.Key);

            var publishesAt = last.AddHours(_cutoffHour);
            return ServiceResult<DailyBriefDto>.Success(new DailyBriefDto
            {
                ProjectId = projectId,
                ProjectName = project.ProjectName,
                Day = last,
                From = from,
                Days = days,
                Emphasis = emphasis,
                ReaderSide = a.Side,
                MediatorName = mediatorName,
                Rule = RuleFor(a, mediatorName),
                PublishesAt = publishesAt,
                IsPublished = DateTime.Now >= publishesAt,
                AssembledWithoutCurator = !curated,
                TruthFloor = floor,
                Blocks = ordered,
                Counts = counts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assembling the brief for {ProjectId}", projectId);
            return ServiceResult<DailyBriefDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Stated to both parties once, plainly, and never quietly widened (assetlen.md §5).
    /// </summary>
    private static string RuleFor(ProjectAccess a, string? mediator) => a.Side == ProjectSide.Client && !a.IsMediator
        ? $"Anything that moves money, moves a date, changes an agreed spec, or is a blocker or a decision you owe reaches you on this page whether or not anyone curates it. {mediator ?? "The mediator"} controls emphasis, not those facts."
        : "Anything that moves money, moves a date, changes an agreed spec, or is a blocker or a decision the client owes reaches the client on this page whether or not it is curated. Curation controls emphasis, not those facts.";

    /// <summary>A photo with no words of its own is about what its sender said nearest to it.</summary>
    private static Filing BurstFiling(List<tbl_IngestedMessage> messages, Dictionary<string, Filing> filingOf, tbl_IngestedMessage media)
    {
        var author = media.AuthorMemberId ?? media.ExternalAuthor;
        var window = TimeSpan.FromMinutes(BurstMinutes);
        return messages
            .Where(m => m.Id != media.Id && !string.IsNullOrWhiteSpace(m.Body)
                        && (m.AuthorMemberId ?? m.ExternalAuthor) == author
                        && (m.SentAt - media.SentAt).Duration() <= window
                        && filingOf.TryGetValue(m.Id, out var f) && f.IsFiled)
            .OrderBy(m => (m.SentAt - media.SentAt).Duration())
            .ThenBy(m => m.SentAt <= media.SentAt ? 0 : 1)
            .Select(m => filingOf[m.Id])
            .FirstOrDefault();
    }

    private async Task<BriefBlockDto> ToBlockDtoAsync(Block b, List<Frame> allFrames, CancellationToken ct)
    {
        var dto = new BriefBlockDto
        {
            Key = b.Key,
            StageId = b.Stage?.Id,
            StageName = b.Stage?.StageName,
            StagePhase = b.Stage?.Phase,
            StageStatus = b.Stage?.Status,
            DeliverableId = b.Deliverable?.Id,
            DeliverableTitle = b.Deliverable?.Title,
            DeliverableStatus = b.Deliverable?.Status,
            Percent = b.Percent,
            PercentBefore = b.PercentBefore,
            PercentSource = b.PercentSource,
            Notes = b.Notes.OrderBy(n => n.At).Take(NotesPerBlock).ToList(),
            Commitments = b.Commitments,
            FrameTotal = b.Frames.Count
        };

        var after = b.Frames.OrderByDescending(f => f.At).ToList();
        var used = new HashSet<string>();

        if (after.Count > 0 && b.Stage is not null)
        {
            // The "before" comes from the same deliverable where it can, else from the same stage.
            var sameBlock = allFrames.Where(f => (f.Filing.DeliverableId ?? f.Filing.StageId ?? "unfiled") == b.Key).ToList();
            var sameStage = allFrames.Where(f => f.Filing.StageId == b.Stage.Id).ToList();

            var candidates = new List<(Frame After, Frame Before, double D)>();
            foreach (var af in after.Where(f => f.HasThumb))
            {
                var fa = await _vantage.FingerprintAsync(af.Sha, af.Mime, af.ThumbPath, af.StoragePath, ct);
                if (fa is null) continue;

                var earlier = sameBlock.Where(f => f.At.Date < af.At.Date && f.HasThumb).ToList();
                if (earlier.Count == 0) earlier = sameStage.Where(f => f.At.Date < af.At.Date && f.HasThumb).ToList();

                foreach (var bf in earlier.GroupBy(f => f.ArtifactId).Select(g => g.First()))
                {
                    if (bf.Sha == af.Sha) continue;
                    var fb = await _vantage.FingerprintAsync(bf.Sha, bf.Mime, bf.ThumbPath, bf.StoragePath, ct);
                    if (fb is null) continue;
                    var d = VantageIndex.Distance(fa.Value, fb.Value);
                    if (d <= VantageIndex.SameViewThreshold) candidates.Add((af, bf, d));
                }
            }

            // Closest framing first, then the longest interval: "same view, three days on" says more than "same view, an hour on".
            foreach (var (af, bf, d) in candidates.OrderBy(c => c.D).ThenByDescending(c => (c.After.At - c.Before.At).TotalDays))
            {
                if (dto.Pairs.Count >= PairsPerBlock) break;
                if (used.Contains(af.ArtifactId) || used.Contains(bf.ArtifactId)) continue;
                used.Add(af.ArtifactId);
                used.Add(bf.ArtifactId);
                dto.Pairs.Add(new VantagePairDto
                {
                    Before = ToFrameDto(bf),
                    After = ToFrameDto(af),
                    DaysApart = (int)Math.Round((af.At.Date - bf.At.Date).TotalDays),
                    Distance = Math.Round(d, 3)
                });
            }
        }

        dto.Frames = after.Where(f => !used.Contains(f.ArtifactId)).Take(FramesPerBlock).Select(ToFrameDto).ToList();
        return dto;
    }

    private static BriefFrameDto ToFrameDto(Frame f) => new()
    {
        ArtifactId = f.ArtifactId,
        MimeType = f.Mime,
        HasThumbnail = f.HasThumb,
        At = f.At,
        Caption = f.Caption,
        Origin = f.Origin,
        Href = f.Href
    };

    /// <summary>
    /// Build order, then the reader's emphasis lifted to the top. Emphasis
    /// reorders; nothing is removed, so the funder and the representative always
    /// hold the same set of blocks (assetlen.md §5 — emphasis, not truth).
    /// </summary>
    private static List<BriefBlockDto> OrderBlocks(List<BriefBlockDto> blocks, List<tbl_Stage> stages, List<tbl_Deliverable> deliverables,
        ReaderEmphasis emphasis, List<OwedItemDto> owed, out Dictionary<string, string> reasons)
    {
        var stageOrder = stages.ToDictionary(s => s.Id, s =>
        {
            var parent = s.ParentStageId is not null ? stages.FirstOrDefault(p => p.Id == s.ParentStageId) : null;
            return parent is null ? (s.DisplayOrder, -1) : (parent.DisplayOrder, s.DisplayOrder);
        });
        var deliverableOrder = deliverables.ToDictionary(d => d.Id, d => d.DisplayOrder);

        var built = blocks
            .OrderBy(b => b.StageId is null ? 1 : 0)
            .ThenBy(b => b.StageId is not null && stageOrder.TryGetValue(b.StageId, out var o) ? o.Item1 : int.MaxValue)
            .ThenBy(b => b.StageId is not null && stageOrder.TryGetValue(b.StageId, out var o) ? o.Item2 : int.MaxValue)
            .ThenBy(b => b.DeliverableId is null ? -1 : deliverableOrder.GetValueOrDefault(b.DeliverableId))
            .ToList();

        reasons = new Dictionary<string, string>();
        foreach (var b in built)
        {
            string? reason = emphasis switch
            {
                ReaderEmphasis.Funder when b.Percent is not null && b.Percent != b.PercentBefore => "Progress moved",
                ReaderEmphasis.Funder when b.Commitments.Any(c => c.Kind is TruthFloorKind.Money or TruthFloorKind.Date) => "Money or a date moved here",
                ReaderEmphasis.Representative when owed.Any(o => o.DeliverableTitle != null && o.DeliverableTitle == b.DeliverableTitle
                                                                || (o.DeliverableTitle == null && o.StageName != null && o.StageName == b.StageName && b.DeliverableId == null))
                    => "A choice you owe is here",
                ReaderEmphasis.Representative when b.Commitments.Any(c => c.Kind is TruthFloorKind.Spec or TruthFloorKind.DecisionOwed) => "A spec or finish moved here",
                ReaderEmphasis.Representative when b.StagePhase is StageGroup.Finishes => "Finishes",
                _ => null
            };
            if (reason is not null) reasons[b.Key] = reason;
        }

        var lead = reasons;
        return built.Where(b => lead.ContainsKey(b.Key)).Concat(built.Where(b => !lead.ContainsKey(b.Key))).ToList();
    }

    // ── The floor itself ────────────────────────────────────────────────────

    private async Task<List<TruthFloorSectionDto>> TruthFloorAsync(
        tbl_Project project, ProjectAccess a, string userId, DateTime from, DateTime to, string? mediatorName,
        List<OwedItemDto> owed, Dictionary<string, tbl_Deliverable> deliverables, CancellationToken ct)
    {
        var pid = project.Id;
        var currency = project.Currency ?? "UGX";
        var items = new List<TruthItemDto>();
        var fromUtc = from.ToUniversalTime();
        var toUtc = to.ToUniversalTime();
        bool InWindow(DateTime? d) => d is { } x && x >= from && x < to;

        // ── The register: agreed, restated, disputed in the window ──
        if (a.CanSeeRegister || a.Side == ProjectSide.Client)
        {
            var all = await _context.tbl_Commitments.AsNoTracking()
                .Include(c => c.Stage)
                .Include(c => c.Deliverable)
                .Where(c => c.ProjectId == pid)
                .ToListAsync(ct);
            var byId = all.ToDictionary(c => c.Id);

            foreach (var c in all)
            {
                var recorded = c.AgreedAt ?? Local(c.DateTimeCreated);
                var prev = c.SupersedesId is not null ? byId.GetValueOrDefault(c.SupersedesId) : null;
                var disputed = c.DisputedAt is not null && InWindow(Local(c.DisputedAt));
                if (!InWindow(recorded) && !disputed) continue;

                TruthItemDto Item(TruthFloorKind kind, string title) => new()
                {
                    Key = $"commitment:{c.Id}",
                    Kind = kind,
                    Title = title,
                    Detail = Trim(c.Body, 200),
                    At = recorded,
                    StageName = c.Stage?.StageName,
                    StagePhase = c.Stage?.Phase,
                    DeliverableTitle = c.Deliverable?.Title,
                    Amount = a.CanSeeMoney ? c.Amount : null,
                    Currency = a.CanSeeMoney ? c.Currency ?? currency : null,
                    AmountHidden = !a.CanSeeMoney && c.Amount is not null,
                    Source = c.SourceChannel switch
                    {
                        CommitmentSource.Verbal => "Agreed on a call, in your register",
                        CommitmentSource.Meeting => "Agreed in a meeting, in your register",
                        CommitmentSource.Ingested => "From the thread, in your register",
                        _ => "In your register"
                    },
                    Who = c.RecordedBySide == ProjectSide.Contractor && !a.CanSeeSiteLog ? mediatorName : null,
                    DueBy = c.DueDate,
                    Href = a.CanSeeRegister ? $"/project/{pid}/register?commitment={c.Id}" : null
                };

                if (disputed)
                {
                    var d = Item(TruthFloorKind.Spec, $"Queried: {c.Title}");
                    d.Key = $"dispute:{c.Id}";
                    d.Detail = Trim(c.DisputeNote, 200) ?? d.Detail;
                    items.Add(d);
                }
                if (!InWindow(recorded)) continue;

                if (c.Amount is not null && (prev is null || prev.Amount != c.Amount))
                {
                    var m = Item(TruthFloorKind.Money, prev is null ? c.Title ?? "A figure was agreed" : $"Figure restated: {c.Title}");
                    if (prev?.Amount is { } was && a.CanSeeMoney) m.Detail = $"Was {currency} {was:N0}";
                    items.Add(m);
                }
                if (c.DueDate is not null && (prev is null || prev.DueDate?.Date != c.DueDate?.Date))
                {
                    var d = Item(TruthFloorKind.Date, prev is null ? c.Title ?? "A date was agreed" : $"Date moved: {c.Title}");
                    d.Key = $"date:{c.Id}";
                    d.PreviousDate = prev?.DueDate;
                    d.NewDate = c.DueDate;
                    items.Add(d);
                }
                if (c.Kind is CommitmentKind.Spec or CommitmentKind.Material or CommitmentKind.Choice
                    && c.Maturity >= CommitmentMaturity.Agreed
                    && (prev is null || prev.Title != c.Title || prev.Body != c.Body))
                {
                    var s = Item(TruthFloorKind.Spec, prev is null ? c.Title ?? "A spec was agreed" : $"Spec changed: {c.Title}");
                    s.Key = $"spec:{c.Id}";
                    if (prev is not null) s.Detail = $"Was: {Trim(prev.Title, 160)}";
                    items.Add(s);
                }
            }
        }

        // ── The ledger, for the money seat ──
        if (a.CanSeeMoney)
        {
            var releases = await _context.tbl_FundingEntries.AsNoTracking()
                .Include(f => f.Stage)
                .Where(f => f.ProjectId == pid
                            && ((f.PaymentDate >= from && f.PaymentDate < to)
                                || (f.ConfirmationDate >= from && f.ConfirmationDate < to)
                                || (f.DateTimeCreated >= fromUtc && f.DateTimeCreated < toUtc)))
                .ToListAsync(ct);
            foreach (var f in releases)
                items.Add(new TruthItemDto
                {
                    Key = $"funding:{f.Id}",
                    Kind = TruthFloorKind.Money,
                    Title = f.Status switch
                    {
                        FundingStatus.Pending => $"Release sent for {f.Stage?.StageName ?? "the project"} — not yet acknowledged",
                        FundingStatus.AmountQueried => $"Release on {f.Stage?.StageName ?? "the project"} — less arrived than was sent",
                        _ => $"Release acknowledged on {f.Stage?.StageName ?? "the project"}"
                    },
                    At = f.PaymentDate ?? Local(f.DateTimeCreated),
                    StageName = f.Stage?.StageName,
                    StagePhase = f.Stage?.Phase,
                    Amount = f.ReceivedAmount ?? f.Amount,
                    Currency = currency,
                    Source = "Recorded in the ledger",
                    Href = $"/project/{pid}/money"
                });

            var claims = await _context.tbl_StageClaims.AsNoTracking()
                .Include(c => c.Stage)
                .Where(c => c.ProjectId == pid
                            && ((c.ClaimedAt >= from && c.ClaimedAt < to) || (c.ClearedAt >= from && c.ClearedAt < to)
                                || (c.DateTimeCreated >= fromUtc && c.DateTimeCreated < toUtc)))
                .ToListAsync(ct);
            foreach (var c in claims)
                items.Add(new TruthItemDto
                {
                    Key = $"claim:{c.Id}",
                    Kind = TruthFloorKind.Money,
                    Title = c.Status == ClaimStatus.Cleared ? $"Claim cleared on {c.Stage?.StageName}" : $"Claim raised on {c.Stage?.StageName}",
                    Detail = Trim(c.Note, 200),
                    At = c.ClearedAt ?? c.ClaimedAt ?? Local(c.DateTimeCreated),
                    StageName = c.Stage?.StageName,
                    StagePhase = c.Stage?.Phase,
                    Amount = c.ClearedAmount ?? c.Amount,
                    Currency = currency,
                    Source = "Recorded in the ledger",
                    Who = !a.CanSeeSiteLog ? mediatorName : null,
                    Href = $"/project/{pid}/money"
                });

            var variations = await _context.tbl_Variations.AsNoTracking()
                .Include(v => v.Stage)
                .Where(v => v.ProjectId == pid
                            && ((v.RaisedAt >= from && v.RaisedAt < to) || (v.ApprovedAt >= from && v.ApprovedAt < to)
                                || (v.DateTimeCreated >= fromUtc && v.DateTimeCreated < toUtc)))
                .ToListAsync(ct);
            foreach (var v in variations)
                items.Add(new TruthItemDto
                {
                    Key = $"variation:{v.Id}",
                    Kind = TruthFloorKind.Money,
                    Title = v.Status == VariationStatus.Approved ? $"Extra approved: {v.Title}" : $"Extra proposed: {v.Title}",
                    Detail = v.CostDelta is null ? "Not costed yet" : Trim(v.Reason, 200),
                    At = v.ApprovedAt ?? v.RaisedAt ?? Local(v.DateTimeCreated),
                    StageName = v.Stage?.StageName,
                    StagePhase = v.Stage?.Phase,
                    Amount = v.CostDelta,
                    Currency = v.Currency ?? currency,
                    Source = "Variation register",
                    Href = $"/project/{pid}/money"
                });
        }

        // ── Stage dates that no longer match the plan as first agreed ──
        var slipped = await _context.tbl_Stages.AsNoTracking()
            .Where(s => s.ProjectId == pid && s.BaselineEndDate != null && s.ExpectedEndDate != null
                        && s.ExpectedEndDate != s.BaselineEndDate
                        && s.DateTimeModified >= fromUtc && s.DateTimeModified < toUtc)
            .ToListAsync(ct);
        foreach (var s in slipped)
            items.Add(new TruthItemDto
            {
                Key = $"stage-date:{s.Id}",
                Kind = TruthFloorKind.Date,
                Title = $"{s.StageName} now due {s.ExpectedEndDate:d MMM}",
                Detail = $"First planned for {s.BaselineEndDate:d MMM yyyy}",
                At = s.DateTimeModified is null ? null : Local(s.DateTimeModified),
                PreviousDate = s.BaselineEndDate,
                NewDate = s.ExpectedEndDate,
                StageName = s.StageName,
                StagePhase = s.Phase,
                Source = "The stage plan"
            });

        // ── Read from the thread, not yet confirmed — the silent contractor's facts ──
        if (a.CanSeeRegister || a.Side == ProjectSide.Client)
        {
            var proposals = await _context.tbl_ExtractionProposals.AsNoTracking()
                .Include(p => p.Stage)
                .Where(p => p.ProjectId == pid && p.Status == ProposalStatus.Pending
                            && p.SourceSentAt >= from && p.SourceSentAt < to)
                .ToListAsync(ct);
            foreach (var p in proposals.Where(p => CanReadExtracted(p.SourceSide, p.SourceImportedById, a, userId)))
            {
                var kind = p.Kind switch
                {
                    ProposalKind.Blocker => TruthFloorKind.Blocker,
                    ProposalKind.Date => TruthFloorKind.Date,
                    ProposalKind.Price => TruthFloorKind.Money,
                    _ when p.Amount is not null => TruthFloorKind.Money,
                    _ when p.DueDate is not null => TruthFloorKind.Date,
                    _ => TruthFloorKind.Spec
                };
                items.Add(new TruthItemDto
                {
                    Key = $"proposal:{p.Id}",
                    Kind = kind,
                    Title = p.Title ?? "Read from the thread",
                    Detail = Trim(p.Detail, 200),
                    At = p.SourceSentAt,
                    NewDate = p.DueDate,
                    DueBy = p.DueDate,
                    StageName = p.Stage?.StageName,
                    StagePhase = p.Stage?.Phase,
                    Amount = a.CanSeeMoney ? p.Amount : null,
                    Currency = a.CanSeeMoney ? p.Currency ?? currency : null,
                    AmountHidden = !a.CanSeeMoney && p.Amount is not null,
                    Source = "Read from the thread — not yet in your register",
                    Unconfirmed = true,
                    Href = a.CanSeeRegister ? $"/project/{pid}/register?view=thread" : null
                });
            }
        }

        // ── Blockers: every open one, whichever channel raised it ──
        var blockers = await _context.tbl_Flags.AsNoTracking()
            .Include(f => f.Stage)
            .Include(f => f.OwnerMember).ThenInclude(m => m!.User)
            .Where(f => f.ProjectId == pid && f.CommitmentId == null && (f.Status == FlagStatus.Open || f.Status == FlagStatus.InProgress))
            .ToListAsync(ct);
        var today = DateTime.Now.Date;
        foreach (var f in blockers)
        {
            // The floor carries it across; the delivery side's own wording of the
            // detail stays on its side, and the accountable face answers for it.
            var crossed = f.Channel == Channel.Crew && !a.CanSeeSiteLog;
            items.Add(new TruthItemDto
            {
                Key = $"flag:{f.Id}",
                Kind = TruthFloorKind.Blocker,
                Title = f.Title ?? "A blocker",
                Detail = crossed ? null : Trim(f.Description, 200),
                At = Local(f.DateTimeCreated),
                StageName = f.Stage?.StageName,
                StagePhase = f.Stage?.Phase,
                Source = f.OwnerMember is { } om ? $"Waiting on {MemberName(om)}" : f.OwnerPartyName is { } party ? $"Waiting on {party}" : "Open blocker",
                Who = crossed ? mediatorName : null,
                CrossedByFloor = crossed,
                DueBy = f.DueDate,
                IsOverdue = f.DueDate is { } d && d.Date < today,
                Href = a.CanSeeRegister ? $"/project/{pid}/register?view=questions" : null
            });
        }

        // ── Decisions this reader owes: standing, not windowed ──
        foreach (var o in owed)
            items.Add(new TruthItemDto
            {
                Key = $"owed:{o.Key}",
                Kind = TruthFloorKind.DecisionOwed,
                Title = o.Title,
                Detail = o.Consequence,
                StageName = o.StageName,
                StagePhase = o.StagePhase,
                DeliverableTitle = o.DeliverableTitle,
                Amount = o.Amount,
                Currency = o.Currency,
                AmountHidden = o.AmountHidden,
                DueBy = o.DueBy,
                IsOverdue = o.IsOverdue,
                Source = "You owe this",
                Href = o.Href
            });

        var order = FloorOrder(EmphasisOf(a));
        return order.Select(k => new TruthFloorSectionDto
        {
            Kind = k,
            Label = k switch
            {
                TruthFloorKind.Money => "Money that moved",
                TruthFloorKind.Date => "Dates that moved",
                TruthFloorKind.Spec => "Specs agreed or changed",
                TruthFloorKind.Blocker => "Blockers",
                _ => "Decisions you owe"
            },
            Items = items.Where(i => i.Kind == k)
                .GroupBy(i => i.Key).Select(g => g.First())
                .OrderByDescending(i => i.IsOverdue)
                .ThenBy(i => k == TruthFloorKind.DecisionOwed ? i.DueBy ?? DateTime.MaxValue : DateTime.MaxValue)
                .ThenByDescending(i => i.At)
                .ToList()
        }).ToList();
    }

    /// <summary>Every section, for every reader; only the order is theirs.</summary>
    public static TruthFloorKind[] FloorOrder(ReaderEmphasis e) => e switch
    {
        ReaderEmphasis.Funder => [TruthFloorKind.Money, TruthFloorKind.Date, TruthFloorKind.Blocker, TruthFloorKind.DecisionOwed, TruthFloorKind.Spec],
        ReaderEmphasis.Representative => [TruthFloorKind.DecisionOwed, TruthFloorKind.Spec, TruthFloorKind.Blocker, TruthFloorKind.Date, TruthFloorKind.Money],
        _ => [TruthFloorKind.DecisionOwed, TruthFloorKind.Blocker, TruthFloorKind.Money, TruthFloorKind.Date, TruthFloorKind.Spec]
    };

    // ═════════════════════════════════════════════════════════════════════
    // Helpers
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>Row stamps are written in UTC; the brief's days are the reader's days.</summary>
    private static DateTime Local(DateTime? utc) => utc is { } u ? DateTime.SpecifyKind(u, DateTimeKind.Utc).ToLocalTime() : DateTime.MinValue;

    private static string MemberName(tbl_ProjectMember m) =>
        (m.User is { } u ? $"{u.FirstName} {u.LastName}".Trim() : null) is { Length: > 0 } n ? n : m.PartyName ?? m.Title ?? "Unnamed";

    private static string? Trim(string? text, int max)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var clean = Regex.Replace(text.Trim(), @"\s+", " ");
        return clean.Length <= max ? clean : clean[..(max - 1)].TrimEnd() + "…";
    }
}
