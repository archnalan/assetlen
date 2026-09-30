using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.FileProcessingServices.Extraction;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.Scheduling;

namespace assetlen.Service.DbServices;

/// <summary>
/// The scheduler (works-report.md §4.6): the plan's dates are computed from
/// durations, waits and order on the site calendar, stored as the last result,
/// and every proposed change is shown with its effect on the handover before
/// anyone saves it. The house and its guest wing are one plan.
/// </summary>
public sealed partial class WorkPlanDAL
{
    private static readonly PlanEngine Engine = new();

    /// <summary>Everything the engine reads for one house, loaded once.</summary>
    private sealed class Family
    {
        public required tbl_Project Root { get; init; }
        public required List<tbl_Deliverable> AllLines { get; init; }
        public required List<tbl_PlanWait> Waits { get; init; }
        public tbl_WorkSchedule? Schedule { get; set; }
        public required List<tbl_Commitment> Dates { get; init; }
        public tbl_Commitment? HandoverHead { get; init; }
        public DateOnly? HandoverOverride { get; set; }

        /// <summary>The lines on the schedule: those with days to them.</summary>
        public List<tbl_Deliverable> Lines => AllLines
            .Where(l => l.WorkDays is not null || l.MakeDays is not null)
            .OrderBy(l => l.DisplayOrder).ThenBy(l => l.Id, StringComparer.Ordinal)
            .ToList();
    }

    // ═════════════════════════════════════════════════════════════════════
    // Reading
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<ScheduleDto>> GetSchedule(string projectId, DateOnly? asAt, string userId, CancellationToken ct = default)
    {
        try
        {
            var (root, access, scope, error) = await ResolveScheduleAsync(projectId, userId, ct);
            if (error is not null) return Fail<ScheduleDto>(error);

            var family = await LoadFamilyAsync(root!, track: false, ct);
            var outcome = Compute(family, asAt ?? SiteToday());
            return ServiceResult<ScheduleDto>.Success(await BuildAsync(family, outcome, access, scope, ct));
        }
        catch (PlanShapeException ex)
        {
            return Fail<ScheduleDto>(new ConflictException(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading the schedule for {ProjectId}", projectId);
            return Fail<ScheduleDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<SchedulePreviewDto>> PreviewSchedule(SchedulePreviewRequestDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var (root, access, scope, error) = await ResolveScheduleAsync(dto.ProjectId!, userId, ct);
            if (error is not null) return Fail<SchedulePreviewDto>(error);
            if (!access.CanEditPlan) return Fail<SchedulePreviewDto>(new ForbiddenException(NotYourPlan));
            if (dto.Proposed is null) return Fail<SchedulePreviewDto>(new BadRequestException("Nothing proposed."));

            var today = dto.AsAt ?? SiteToday();

            // Three independent copies, none of them tracked: a preview cannot save.
            var current = await LoadFamilyAsync(root!, track: false, ct);
            var currentPlan = Compute(current, today);

            var proposed = await LoadFamilyAsync(root!, track: false, ct);
            if (Apply(proposed, dto.Proposed, userId, today, persist: false) is { } bad) return Fail<SchedulePreviewDto>(bad);
            var proposedPlan = Compute(proposed, today);

            var c = PlanComparison.Of(currentPlan, proposedPlan);
            var result = new SchedulePreviewDto
            {
                Current = Summary(currentPlan),
                Proposed = Summary(proposedPlan),
                Sentences = c.Sentences.ToList(),
                CriticalPathChanged = c.CriticalPathChanged,
                Moved = c.Moved.Select(m => new ActivityMoveDto
                {
                    Id = m.Key, Title = m.Title,
                    StartBefore = m.StartBefore, StartAfter = m.StartAfter,
                    FinishBefore = m.FinishBefore, FinishAfter = m.FinishAfter
                }).ToList(),
                Plan = await BuildAsync(proposed, proposedPlan, access, scope, ct)
            };

            if (dto.Alternative is not null)
            {
                var alternative = await LoadFamilyAsync(root!, track: false, ct);
                if (Apply(alternative, dto.Alternative, userId, today, persist: false) is { } badAlt) return Fail<SchedulePreviewDto>(badAlt);
                var alternativePlan = Compute(alternative, today);
                result.Alternative = Summary(alternativePlan);
                result.AlternativeSentences = PlanComparison.Of(currentPlan, alternativePlan).Sentences.ToList();
                result.AlternativePlan = await BuildAsync(alternative, alternativePlan, access, scope, ct);
            }

            return ServiceResult<SchedulePreviewDto>.Success(result);
        }
        catch (PlanShapeException ex)
        {
            return Fail<SchedulePreviewDto>(new BadRequestException(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing the schedule for {ProjectId}", dto.ProjectId);
            return Fail<SchedulePreviewDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Saving
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<ScheduleDto>> SaveSchedule(ScheduleChangeDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var (root, access, scope, error) = await ResolveScheduleAsync(dto.ProjectId!, userId, ct);
            if (error is not null) return Fail<ScheduleDto>(error);
            if (!access.CanEditPlan) return Fail<ScheduleDto>(new ForbiddenException(NotYourPlan));

            var today = SiteToday();
            var family = await LoadFamilyAsync(root!, track: true, ct);
            if (Apply(family, dto, userId, today, persist: true) is { } bad) return Fail<ScheduleDto>(bad);

            // Computed before anything is written: an order that loops back is refused whole.
            var outcome = Compute(family, today);

            if (dto.Handover is { } handover && Day(family.HandoverHead?.DueDate) != handover)
                await RestateHandoverAsync(family, handover, access, userId, ct);

            Store(family, outcome, userId);
            await _context.SaveChangesAsync(ct);

            var fresh = await LoadFamilyAsync(root!, track: false, ct);
            return ServiceResult<ScheduleDto>.Success(await BuildAsync(fresh, Compute(fresh, today), access, scope, ct));
        }
        catch (PlanShapeException ex)
        {
            return Fail<ScheduleDto>(new BadRequestException(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving the schedule for {ProjectId}", dto.ProjectId);
            return Fail<ScheduleDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ScheduleDto>> Recompute(string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var (root, access, scope, error) = await ResolveScheduleAsync(projectId, userId, ct);
            if (error is not null) return Fail<ScheduleDto>(error);
            if (!access.CanEditPlan) return Fail<ScheduleDto>(new ForbiddenException(NotYourPlan));

            var today = SiteToday();
            var family = await LoadFamilyAsync(root!, track: true, ct);
            var outcome = Compute(family, today);
            Store(family, outcome, userId);
            await _context.SaveChangesAsync(ct);
            return ServiceResult<ScheduleDto>.Success(await BuildAsync(family, outcome, access, scope, ct));
        }
        catch (PlanShapeException ex)
        {
            return Fail<ScheduleDto>(new ConflictException(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recomputing the schedule for {ProjectId}", projectId);
            return Fail<ScheduleDto>(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Law 0: with nobody editing, every scheduled house re-dates from its ticks and
    /// from elapsed time. Run daily and after every tick.
    /// </summary>
    public async Task<int> RedateAllAsync(CancellationToken ct = default)
    {
        var roots = await _context.tbl_WorkSchedules.IgnoreQueryFilters().AsNoTracking()
            .Where(s => s.IsDeleted != true && s.ProjectId != null)
            .Select(s => s.ProjectId!).ToListAsync(ct);
        var n = 0;
        foreach (var id in roots)
            if (await RedateProjectAsync(id, ct)) n++;
        return n;
    }

    public async Task<bool> RedateProjectAsync(string rootId, CancellationToken ct = default)
    {
        try
        {
            var root = await _context.tbl_Projects_RS.IgnoreQueryFilters().AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == rootId && p.IsDeleted != true && p.ArchivedAt == null, ct);
            if (root is null) return false;
            var family = await LoadFamilyAsync(root, track: true, ct);
            if (family.Schedule is null) return false;
            Store(family, Compute(family, SiteToday()), null);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            // A plan that cannot be re-dated keeps its last dates; a tick must never fail on it.
            _logger.LogWarning(ex, "Could not re-date the schedule of {ProjectId}", rootId);
            return false;
        }
    }

    /// <summary>A tick or a reopening re-dates what follows it (§4.6).</summary>
    private Task RedateAfterTickAsync(tbl_Project project, CancellationToken ct)
    {
        // The tick was written past the tracker; re-read the line as it now stands.
        _context.ChangeTracker.Clear();
        return RedateProjectAsync(project.ParentProjectId ?? project.Id, ct);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Standing
    // ═════════════════════════════════════════════════════════════════════

    private const string NotYourPlan = "The delivery side plans the work; you can read the plan.";

    /// <summary>
    /// The plan belongs to the house. A stranger is told nothing; a seat without the
    /// plan is told nothing either, as on the checklist (§4.5).
    /// </summary>
    private async Task<(tbl_Project? Root, ProjectAccess Access, Dictionary<string, (tbl_Project Project, ProjectAccess Access)> Scope, Exception? Error)>
        ResolveScheduleAsync(string projectId, string userId, CancellationToken ct)
    {
        var scope = new Dictionary<string, (tbl_Project, ProjectAccess)>();
        var project = await _context.tbl_Projects_RS.Include(p => p.ParentProject).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return (null, ProjectAccess.None, scope, new NotFoundException("Project not found."));

        var root = project.ParentProject ?? project;
        root.ParentProject = null;
        var access = await _access.ResolveAsync(root, userId, ct);
        if (!access.CanSeePlan) return (null, access, scope, new NotFoundException("Project not found."));

        scope[root.Id] = (root, access);
        var subs = await _context.tbl_Projects_RS.AsNoTracking()
            .Where(p => p.ParentProjectId == root.Id && p.ArchivedAt == null)
            .ToListAsync(ct);
        foreach (var s in subs) s.ParentProject = root;
        var subAccess = await _access.ResolveManyAsync(subs, userId, ct);
        foreach (var s in subs)
            if (subAccess.TryGetValue(s.Id, out var a) && a.CanSeePlan) scope[s.Id] = (s, a);

        return (root, access, scope, null);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Loading and computing
    // ═════════════════════════════════════════════════════════════════════

    private async Task<Family> LoadFamilyAsync(tbl_Project root, bool track, CancellationToken ct)
    {
        IQueryable<T> Q<T>(IQueryable<T> set) where T : BaseEntity =>
            (track ? set : set.AsNoTracking()).IgnoreQueryFilters()
                .Where(x => x.TenantId == root.TenantId && x.IsDeleted != true);

        var ids = await Q(_context.tbl_Projects_RS)
            .Where(p => (p.Id == root.Id || p.ParentProjectId == root.Id) && p.ArchivedAt == null)
            .Select(p => p.Id).ToListAsync(ct);

        var lines = await Q(_context.tbl_Deliverables).Include(d => d.Stage).Include(d => d.CompletedBy)
            .Where(d => d.ProjectId != null && ids.Contains(d.ProjectId))
            .ToListAsync(ct);
        var waits = await Q(_context.tbl_PlanWaits)
            .Where(w => w.ProjectId != null && ids.Contains(w.ProjectId) && w.RemovedAt == null)
            .OrderBy(w => w.DisplayOrder).ThenBy(w => w.DateTimeCreated)
            .ToListAsync(ct);
        var schedule = await Q(_context.tbl_WorkSchedules).FirstOrDefaultAsync(s => s.ProjectId == root.Id, ct);

        var dates = await Q(_context.tbl_Commitments)
            .Where(c => c.ProjectId == root.Id && c.Kind == CommitmentKind.Date && c.DeliverableId == null && c.DueDate != null)
            .ToListAsync(ct);
        var stages = await Q(_context.tbl_Stages).AsNoTracking().Where(s => s.ProjectId == root.Id).ToListAsync(ct);
        var refs = stages.Select(s => new StageRef(s.Id, s.StageName ?? "", s.CatalogueKey)).ToList();
        var head = dates
            .Where(c => c.SupersededAt == null && WorksReportDAL.IsProjectDeadline(c, refs))
            .OrderBy(WorksReportDAL.AgreedOf).ThenBy(c => c.DateTimeCreated)
            .LastOrDefault();

        return new Family { Root = root, AllLines = lines, Waits = waits, Schedule = schedule, Dates = dates, HandoverHead = head };
    }

    private static PlanOutcome Compute(Family f, DateOnly today)
    {
        var calendar = CalendarOf(f.Schedule);
        var lines = f.Lines;
        var onPlan = lines.Select(l => l.Id).ToHashSet(StringComparer.Ordinal);
        // Two houses, two sets of doors: what the plan says about a line names its area
        // whenever another line shares its title.
        var twins = lines.GroupBy(l => (l.Title ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var activities = lines.Select(l => new PlanActivity
        {
            Key = l.Id,
            Title = twins.Contains((l.Title ?? "").Trim()) && !string.IsNullOrWhiteSpace(l.Area) ? $"{l.Title} · {l.Area}" : l.Title,
            Area = l.Area,
            Trade = l.Trade,
            WorkDays = l.WorkDays ?? 0,
            MakeDays = l.MakeDays ?? 0,
            CureDays = l.CureDays ?? 0,
            EarliestStart = Day(l.EarliestStart),
            TeamKey = l.TeamKey,
            QueueOrder = l.QueueOrder ?? l.DisplayOrder,
            ActualStart = Day(l.ActualStart),
            PinnedFinish = Day(l.PinnedFinish),
            DoneOn = l.Status == DeliverableStatus.Done ? LocalDay(l.CompletedAt) ?? Day(l.PlannedEnd) ?? today : null,
            Waits = WaitsOf(f, l, onPlan).Select(w => new PlanWait
            {
                Kind = w.Kind,
                Title = w.Title,
                ActivityKey = w.PredecessorId,
                Link = w.Link,
                Arrival = w.Arrival,
                Days = w.Days,
                CalendarDays = w.CalendarDays || w.Kind == WaitKind.Drying,
                From = Day(w.FromDate),
                Until = Day(w.UntilDate),
                AfterMaking = w.AfterMaking,
                Cleared = w.ClearedAt is not null,
                ClearedOn = LocalDay(w.ClearedAt)
            }).ToList()
        }).ToList();

        return Engine.Compute(new PlanInput
        {
            Today = today,
            Calendar = calendar,
            Activities = activities,
            Handover = f.HandoverOverride ?? Day(f.HandoverHead?.DueDate),
            Lateness = f.Schedule?.ExtendLateWaits == false ? LateWaitRule.Off : LateWaitRule.Calibrated
        });
    }

    /// <summary>A line's live waits, in order, leaving out any on a line no longer on the plan.</summary>
    private static List<tbl_PlanWait> WaitsOf(Family f, tbl_Deliverable line, HashSet<string> onPlan) => f.Waits
        .Where(w => w.DeliverableId == line.Id)
        .Where(w => w.Kind != WaitKind.Activity || (w.PredecessorId is not null && onPlan.Contains(w.PredecessorId)))
        .OrderBy(w => w.DisplayOrder).ThenBy(w => w.DateTimeCreated)
        .ToList();

    private static SiteCalendar CalendarOf(tbl_WorkSchedule? s) => new(
        s is null ? DayOfWeek.Saturday : (DayOfWeek)s.RestDay,
        ParseDays(s?.Holidays));

    private static List<DateOnly> ParseDays(string? csv) => (csv ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(x => DateOnly.TryParseExact(x, "yyyy-MM-dd", out var d) ? d : (DateOnly?)null)
        .OfType<DateOnly>().ToList();

    // ═════════════════════════════════════════════════════════════════════
    // Applying a change — the same code for a preview and a save
    // ═════════════════════════════════════════════════════════════════════

    private Exception? Apply(Family f, ScheduleChangeDto change, string userId, DateOnly today, bool persist)
    {
        var all = f.AllLines.ToDictionary(l => l.Id, StringComparer.Ordinal);
        var calendar = CalendarOf(f.Schedule);
        var now = DateTime.UtcNow;

        foreach (var ac in change.Activities)
        {
            if (ac.Id is null || !all.TryGetValue(ac.Id, out var l))
                return new BadRequestException("That line is not on this plan.");

            if (ac.WorkDays is { } wd) l.WorkDays = wd;
            if (ac.MakeDays is { } md) l.MakeDays = md;
            if (ac.CureDays is { } cd) l.CureDays = cd;
            if ((l.WorkDays ?? 0) == 0 && (l.MakeDays ?? 0) == 0 && (ac.WorkDays is not null || ac.MakeDays is not null))
                return new BadRequestException($"{l.Title} needs at least one working day, on site or off.");

            if (ac.ClearEarliestStart) l.EarliestStart = null;
            else if (ac.EarliestStart is { } es) l.EarliestStart = Utc(es);

            if (ac.ClearPinnedFinish) l.PinnedFinish = null;
            else if (ac.PinnedFinish is { } pf) l.PinnedFinish = Utc(pf);
            if (ac.NeedsMoreDays is { } more) l.PinnedFinish = Utc(calendar.AddWork(today.AddDays(1), more));

            if (ac.ClearTeam) { l.TeamKey = null; l.QueueOrder = null; }
            else if (!string.IsNullOrWhiteSpace(ac.TeamKey) && !string.Equals(l.TeamKey, ac.TeamKey.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var team = ac.TeamKey.Trim();
                var last = f.AllLines.Where(x => string.Equals(x.TeamKey, team, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.QueueOrder ?? x.DisplayOrder).DefaultIfEmpty(0).Max();
                l.TeamKey = team;
                l.QueueOrder = last + 1;
            }
            if (ac.Trade is not null) l.Trade = string.IsNullOrWhiteSpace(ac.Trade) ? null : ac.Trade.Trim();

            if (ac.Waits is { } waits)
            {
                var existing = f.Waits.Where(w => w.DeliverableId == l.Id).ToList();
                var kept = waits.Where(w => w.Id is not null).Select(w => w.Id!).ToHashSet(StringComparer.Ordinal);
                foreach (var gone in existing.Where(w => !kept.Contains(w.Id)))
                {
                    gone.RemovedAt = now;
                    gone.RemovedById = userId;
                    f.Waits.Remove(gone);
                }

                for (var i = 0; i < waits.Count; i++)
                {
                    var wi = waits[i];
                    if (Check(wi, l, all) is { } bad) return bad;

                    var w = wi.Id is null ? null : existing.FirstOrDefault(x => x.Id == wi.Id);
                    if (wi.Id is not null && w is null) return new BadRequestException("That wait is not on this line.");
                    if (w is null)
                    {
                        w = new tbl_PlanWait
                        {
                            // A preview's new wait is not a row anyone can edit next time.
                            Id = persist ? Guid.NewGuid().ToString() : "draft-" + Guid.NewGuid().ToString("N"),
                            TenantId = l.TenantId,
                            ProjectId = l.ProjectId,
                            DeliverableId = l.Id,
                            CreatedById = userId,
                            DateTimeCreated = now
                        };
                        f.Waits.Add(w);
                        if (persist) _context.tbl_PlanWaits.Add(w);
                    }
                    w.Kind = wi.Kind;
                    w.Arrival = wi.Kind == WaitKind.Arrival ? wi.Arrival ?? ArrivalKind.Delivery : null;
                    w.Title = string.IsNullOrWhiteSpace(wi.Title) ? null : wi.Title.Trim();
                    w.PredecessorId = wi.Kind == WaitKind.Activity ? wi.PredecessorId : null;
                    w.Link = wi.Kind == WaitKind.Activity ? wi.Link : WaitLink.FinishToStart;
                    w.Days = wi.Kind == WaitKind.Activity ? 0 : wi.Days;
                    w.CalendarDays = wi.Kind == WaitKind.Drying || (wi.Kind == WaitKind.Arrival && wi.CalendarDays);
                    w.FromDate = wi.Kind == WaitKind.Activity ? null : Utc(wi.From);
                    w.UntilDate = wi.Kind == WaitKind.Arrival ? Utc(wi.Until) : null;
                    w.AfterMaking = wi.AfterMaking;
                    // A lead time counts from the day it was said, never from a sliding today.
                    if (w.Kind != WaitKind.Activity && w.UntilDate is null && w.FromDate is null && !w.AfterMaking) w.FromDate = Utc(today);
                    w.DisplayOrder = i;
                }
            }

            foreach (var id in ac.ClearWaitIds ?? new())
            {
                var w = f.Waits.FirstOrDefault(x => x.Id == id && x.DeliverableId == l.Id);
                if (w is null) return new BadRequestException("That wait is not on this line.");
                w.ClearedAt ??= now;
                w.ClearedById ??= userId;
            }
        }

        foreach (var q in change.Queues)
        {
            var team = q.TeamKey?.Trim();
            if (string.IsNullOrEmpty(team)) return new BadRequestException("A queue needs its team.");
            for (var i = 0; i < q.OrderedIds.Count; i++)
            {
                if (!all.TryGetValue(q.OrderedIds[i], out var l)) return new BadRequestException("That line is not on this plan.");
                l.TeamKey = team;
                l.QueueOrder = i + 1;
            }
        }

        if (change.Handover is { } h) f.HandoverOverride = h;

        if (change.RestDay is { } restDay && !Enum.IsDefined(restDay))
            return new BadRequestException("The rest day is a day of the week.");

        if (change.RestDay is not null || change.Holidays is not null)
        {
            if (f.Schedule is null)
            {
                f.Schedule = new tbl_WorkSchedule { Id = Guid.NewGuid().ToString(), TenantId = f.Root.TenantId, ProjectId = f.Root.Id, DateTimeCreated = now };
                if (persist) _context.tbl_WorkSchedules.Add(f.Schedule);
            }
            if (change.RestDay is { } rest) f.Schedule.RestDay = (int)rest;
            if (change.Holidays is { } days) f.Schedule.Holidays = string.Join(',', days.Distinct().Order().Select(d => d.ToString("yyyy-MM-dd")));
        }
        return null;
    }

    private static Exception? Check(WaitInputDto w, tbl_Deliverable line, IReadOnlyDictionary<string, tbl_Deliverable> all) => w.Kind switch
    {
        _ when !Enum.IsDefined(w.Kind) || !Enum.IsDefined(w.Link) || (w.Arrival is { } k && !Enum.IsDefined(k))
            => new BadRequestException("A line waits on another activity, something arriving by a date, or drying."),
        WaitKind.Activity when w.PredecessorId is null || !all.ContainsKey(w.PredecessorId)
            => new BadRequestException("It waits on a line that is not on this plan."),
        WaitKind.Activity when w.PredecessorId == line.Id
            => new BadRequestException($"{line.Title} cannot wait on itself."),
        WaitKind.Arrival when w.Until is null && w.Days <= 0
            => new BadRequestException("Say when it arrives — a date, or how many days from now."),
        WaitKind.Drying when w.Days <= 0
            => new BadRequestException("Drying needs its number of days."),
        _ => null
    };

    /// <summary>
    /// The handover is a Date commitment, so a new date restates it and the old
    /// statement is kept (P4). With none on record, the plan's date becomes the first.
    /// </summary>
    private async Task RestateHandoverAsync(Family f, DateOnly handover, ProjectAccess access, string userId, CancellationToken ct)
    {
        var title = $"Handover {PlanComparison.Day(handover)}";
        if (f.HandoverHead is { } head)
        {
            var tracked = await _context.tbl_Commitments.FirstAsync(c => c.Id == head.Id, ct);
            _context.tbl_Commitments.Add(CommitmentDAL.Successor(tracked, userId, access, new CommitmentRestateDto
            {
                CommitmentId = head.Id,
                Title = title,
                Body = "Restated with the works plan.",
                DueDate = Utc(handover),
                SourceChannel = CommitmentSource.App
            }));
            return;
        }

        var mediator = await _context.tbl_ProjectMembers
            .Where(m => m.ProjectId == f.Root.Id && m.IsActive && m.IsMediator)
            .OrderBy(m => m.JoinedAt ?? m.DateTimeCreated)
            .Select(m => m.Id).FirstOrDefaultAsync(ct);
        _context.tbl_Commitments.Add(new tbl_Commitment
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = f.Root.TenantId,
            ProjectId = f.Root.Id,
            Kind = CommitmentKind.Date,
            Title = title,
            Body = "Set with the works plan.",
            Maturity = CommitmentMaturity.Agreed,
            QueryState = CommitmentQueryState.None,
            SourceChannel = CommitmentSource.App,
            AccountableMemberId = mediator,
            AgreedById = userId,
            AgreedAt = DateTime.UtcNow,
            RecordedById = userId,
            RecordedBySide = access.Side,
            DueDate = Utc(handover)
        });
    }

    /// <summary>The computed dates become the plan's dates: never typed, always the last result.</summary>
    private void Store(Family f, PlanOutcome outcome, string? savedBy)
    {
        var byId = f.AllLines.ToDictionary(l => l.Id, StringComparer.Ordinal);
        foreach (var a in outcome.Activities)
        {
            if (!byId.TryGetValue(a.Key, out var l)) continue;
            l.PlannedStart = Utc(a.Start);
            // A late line keeps its planned finish on record, so it reads late rather than quietly on time;
            // what follows it has already moved with today.
            l.PlannedEnd = Utc(a.LateSince ?? a.Finish);
            l.PlannedMakeStart = Utc(a.MakeStart);
            l.PlannedMakeEnd = Utc(a.MakeEnd);
        }

        var now = DateTime.UtcNow;
        if (f.Schedule is null)
        {
            f.Schedule = new tbl_WorkSchedule
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = f.Root.TenantId,
                ProjectId = f.Root.Id,
                DateTimeCreated = now
            };
            _context.tbl_WorkSchedules.Add(f.Schedule);
        }

        var s = f.Schedule;
        var complete = Utc(outcome.WorksComplete);
        if (s.WorksComplete is { } was && complete is { } next && was.Date != next.Date)
        {
            s.PreviousWorksComplete = was;
            s.WorksCompleteMovedAt = now;
        }
        s.WorksComplete = complete;
        s.ReserveDays = outcome.ReserveDays;
        s.CriticalPath = string.Join(',', outcome.CriticalPath);
        s.LastComputedAt = now;
        if (savedBy is not null)
        {
            s.LastSavedAt = now;
            s.LastSavedById = savedBy;
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Shaping for one reader
    // ═════════════════════════════════════════════════════════════════════

    private async Task<ScheduleDto> BuildAsync(Family f, PlanOutcome outcome, ProjectAccess access,
        Dictionary<string, (tbl_Project Project, ProjectAccess Access)> scope, CancellationToken ct)
    {
        var lines = f.Lines;
        var visible = lines.Where(l => l.ProjectId is not null && scope.ContainsKey(l.ProjectId)).ToList();
        var shown = visible.Select(l => l.Id).ToHashSet(StringComparer.Ordinal);
        var onPlan = lines.Select(l => l.Id).ToHashSet(StringComparer.Ordinal);
        var titles = f.AllLines.ToDictionary(l => l.Id, l => l.Title, StringComparer.Ordinal);

        var mapped = await MapAsync(visible, scope, ct);
        var lineDtos = mapped.ToDictionary(d => d.Id!, StringComparer.Ordinal);

        var activities = new List<ScheduleActivityDto>();
        foreach (var l in visible)
        {
            var a = outcome[l.Id];
            if (a is null) continue;
            var rows = WaitsOf(f, l, onPlan);
            activities.Add(new ScheduleActivityDto
            {
                Id = l.Id,
                Title = l.Title,
                Area = l.Area,
                Trade = l.Trade,
                StageName = l.Stage?.StageName,
                StagePhase = l.Stage?.Phase,
                Status = l.Status,
                WorkDays = l.WorkDays ?? 0,
                MakeDays = l.MakeDays ?? 0,
                CureDays = l.CureDays ?? 0,
                EarliestStart = Day(l.EarliestStart),
                TeamKey = l.TeamKey,
                QueueOrder = l.QueueOrder ?? l.DisplayOrder,
                ActualStart = Day(l.ActualStart),
                PinnedFinish = Day(l.PinnedFinish),
                DoneOn = a.Done ? a.Finish : null,
                Waits = a.Waits.Select(span =>
                {
                    var row = span.Index < rows.Count ? rows[span.Index] : null;
                    return new ScheduleWaitDto
                    {
                        Id = row?.Id is { } rid && !rid.StartsWith("draft-", StringComparison.Ordinal) ? rid : null,
                        Kind = span.Kind,
                        Arrival = span.Arrival,
                        Title = span.Title,
                        PredecessorId = span.ActivityKey,
                        PredecessorTitle = span.ActivityKey is { } k ? titles.GetValueOrDefault(k) : null,
                        Link = span.Link,
                        Days = row?.Days ?? 0,
                        CalendarDays = row?.CalendarDays ?? false,
                        From = Day(row?.FromDate),
                        Until = Day(row?.UntilDate),
                        AfterMaking = span.AfterMaking,
                        Cleared = span.Cleared,
                        ClearedOn = LocalDay(row?.ClearedAt),
                        Start = span.Start,
                        End = span.End,
                        Overdue = span.Overdue,
                        DueWas = span.DueWas,
                        Binding = span.Binding
                    };
                }).ToList(),
                Start = a.Start,
                Finish = a.Finish,
                MakeStart = a.MakeStart,
                MakeEnd = a.MakeEnd,
                CureEnd = a.CureEnd,
                Critical = a.Critical,
                FloatDays = a.FloatDays,
                SlipDays = a.SlipDays,
                Late = a.Late,
                LateSince = a.LateSince,
                Done = a.Done,
                Started = a.Started,
                Line = lineDtos.GetValueOrDefault(l.Id)
            });
        }

        var history = new List<HandoverStatementDto>();
        for (var cur = f.HandoverHead; cur is not null && history.Count < 50; cur = f.Dates.FirstOrDefault(c => c.Id == cur.SupersedesId))
            history.Add(new HandoverStatementDto
            {
                CommitmentId = cur.Id,
                Title = cur.Title,
                Date = Day(cur.DueDate),
                AgreedAt = cur.AgreedAt,
                IsCurrent = cur.Id == f.HandoverHead!.Id
            });
        history.Reverse();

        return new ScheduleDto
        {
            ProjectId = f.Root.Id,
            Today = outcome.Today,
            WorksComplete = outcome.WorksComplete,
            Handover = outcome.Handover,
            HandoverCommitmentId = f.HandoverHead?.Id,
            HandoverHistory = history,
            ReserveDays = outcome.ReserveDays,
            DaysOver = outcome.DaysOver,
            PreviousWorksComplete = Day(f.Schedule?.PreviousWorksComplete),
            WorksCompleteMovedAt = f.Schedule?.WorksCompleteMovedAt,
            LastSavedAt = f.Schedule?.LastSavedAt,
            CriticalPath = outcome.CriticalPath.Where(shown.Contains).ToList(),
            Activities = activities,
            Actions = outcome.Actions.Where(x => shown.Contains(x.ActivityKey)).Select(x => new ScheduleActionDto
            {
                By = x.By,
                What = x.What,
                Kind = x.Kind,
                ActivityId = x.ActivityKey,
                ActivityTitle = x.ActivityTitle,
                SetsTheDate = x.SetsTheDate,
                Overdue = x.Overdue
            }).ToList(),
            RestDay = f.Schedule is null ? DayOfWeek.Saturday : (DayOfWeek)f.Schedule.RestDay,
            Holidays = ParseDays(f.Schedule?.Holidays),
            ExtendLateWaits = f.Schedule?.ExtendLateWaits != false,
            Teams = visible.Where(l => !string.IsNullOrWhiteSpace(l.TeamKey))
                .GroupBy(l => l.TeamKey!, StringComparer.OrdinalIgnoreCase)
                .Select(g => new TeamQueueDto
                {
                    TeamKey = g.Key,
                    OrderedIds = g.OrderBy(l => l.QueueOrder ?? l.DisplayOrder).ThenBy(l => l.DisplayOrder).Select(l => l.Id).ToList()
                }).ToList(),
            CanEdit = access.CanEditPlan,
            CanTick = access.CanTick,
            IsScheduled = f.Schedule is not null
        };
    }

    private static ScheduleSummaryDto Summary(PlanOutcome p) => new()
    {
        WorksComplete = p.WorksComplete,
        Handover = p.Handover,
        ReserveDays = p.ReserveDays,
        DaysOver = p.DaysOver,
        CriticalPath = p.CriticalPath.ToList(),
        CriticalTitles = p.CriticalPath.Select(k => p[k]?.Title ?? k).ToList()
    };

    // ═════════════════════════════════════════════════════════════════════
    // Days — the site's calendar days, never shifted
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>The server's clock is the site's clock (CLAUDE.md §5.1.1).</summary>
    private static DateOnly SiteToday() => DateOnly.FromDateTime(DateTime.Now);

    /// <summary>A stored calendar day (UTC midnight, handed back unlabelled) as the day it names.</summary>
    private static DateOnly? Day(DateTime? v) => v is { } d ? DateOnly.FromDateTime(d) : null;

    /// <summary>An instant, as the day it fell on at the site.</summary>
    private static DateOnly? LocalDay(DateTime? utc) => utc is { } u
        ? DateOnly.FromDateTime(DateTime.SpecifyKind(u, DateTimeKind.Utc).ToLocalTime())
        : null;

    private static DateTime Utc(DateOnly d) => new(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
    private static DateTime? Utc(DateOnly? d) => d is { } v ? Utc(v) : null;
}
