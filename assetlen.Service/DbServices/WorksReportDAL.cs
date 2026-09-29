using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.FileProcessingServices.Brief;
using assetlen.Service.FileProcessingServices.Extraction;
using assetlen.Service.FileProcessingServices.Report;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IWorksReportDAL"/>
public class WorksReportDAL : IWorksReportDAL
{
    private const int PaceDays = 21;
    private const int AheadDays = 28;
    private const int QuietReadingDays = 14;
    private const int FramesPerGroup = 12;
    private const int VantageLookbackDays = 45;

    /// <summary>Titles that make a date commitment the project's completion date rather than a short-horizon promise.</summary>
    private static readonly Regex CompletionTitle = new(
        @"\b(complet\w*|hand\s?over|handing over|practical completion|move[- ]in|finish(?:ed)? (?:by|on)|end of (?:the )?project)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex CameraName = new(@"^(?:IMG|VID|PTT|AUD|DOC|STK|DSC|PXL|WhatsApp)[\s_\-]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Fixed, so the same record always serialises to the same bytes and the same hash.</summary>
    internal static readonly JsonSerializerOptions SnapshotJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

    private readonly AssetlenDbContext _context;
    private readonly IProjectAccessService _access;
    private readonly IVantageIndex _vantage;
    private readonly IReadOnlyList<IReportNarrator> _narrators;
    private readonly IVideoPosterQueue _posters;
    private readonly ILogger<WorksReportDAL> _logger;
    private readonly int _stallDays;

    public WorksReportDAL(AssetlenDbContext context, IProjectAccessService access, IVantageIndex vantage,
        IEnumerable<IReportNarrator> narrators, IVideoPosterQueue posters, IConfiguration config, ILogger<WorksReportDAL> logger)
    {
        _context = context;
        _access = access;
        _vantage = vantage;
        _narrators = narrators.ToList();
        _posters = posters;
        _logger = logger;
        _stallDays = int.TryParse(config["Report:StallDays"], out var d) && d is >= 3 and <= 60 ? d : 10;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Who reads, who issues
    // ═════════════════════════════════════════════════════════════════════

    private static bool CanReadReport(ProjectAccess a) => a.CanSeeReport;

    private static bool CanIssueReport(ProjectAccess a) => a.CanIssueReport;

    private static bool CanReadAudience(ProjectAccess a, ProjectSide audience) => audience == ProjectSide.Client || a.CanSeeSiteLog;

    /// <summary>
    /// Project-scoped reads in this service run in requests and in background
    /// jobs alike, so they name the project explicitly instead of relying on the
    /// request's tenant. Access has already been resolved before any of them run.
    /// </summary>
    private IQueryable<T> Q<T>(DbSet<T> set) where T : BaseEntity =>
        set.IgnoreQueryFilters().AsNoTracking().Where(x => x.IsDeleted != true);

    private async Task<(tbl_Project? Project, ProjectAccess Access)> StandingAsync(string projectId, string userId, CancellationToken ct)
    {
        var project = await _context.tbl_Projects_RS.IgnoreQueryFilters()
            .Include(p => p.ParentProject)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.IsDeleted != true && p.ArchivedAt == null, ct);
        if (project is null) return (null, ProjectAccess.None);
        return (project, await _access.ResolveAsync(project, userId, ct));
    }

    // ═════════════════════════════════════════════════════════════════════
    // Live
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<WorksReportDto>> GetLiveAsync(string projectId, string userId, DateTime? asAt = null, CancellationToken ct = default)
    {
        try
        {
            var (project, a) = await StandingAsync(projectId, userId, ct);
            if (project is null || !CanReadReport(a)) return NotFound<WorksReportDto>();

            var at = AsAtOf(asAt);
            var audience = ProjectSide.Client;
            var previous = await PreviousAsync(projectId, audience, at, ct);
            var windowFrom = previous?.AsAt ?? at.AddDays(-7);

            var (dto, requests) = await AssembleAsync(project, at, windowFrom, audience, previous, ct);

            // Live is read far more often than it is issued: templates only, so
            // looking at the page never sends anything anywhere (§6.4).
            dto.Narrative = await NarrateAsync(requests, useModel: false, project.ReportDraftingEnabled, ct);
            dto.IsLive = true;
            Redact(dto, a);
            return ServiceResult<WorksReportDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assembling the live works report for {ProjectId}", projectId);
            return ServiceResult<WorksReportDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>"As at 28 Sep" means the record at the end of that day; nothing is issued as at a future moment.</summary>
    private static DateTime AsAtOf(DateTime? asked)
    {
        // Wall-clock, kind unspecified: the report speaks in the project's days, and
        // a browser in another timezone must not shift "28 Sep" to the 29th.
        var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        if (asked is not { } t) return now;
        var end = DateTime.SpecifyKind(t.TimeOfDay == TimeSpan.Zero ? t.Date.AddDays(1).AddSeconds(-1) : t, DateTimeKind.Unspecified);
        return end > now ? now : end;
    }

    private async Task<tbl_WorksReport?> PreviousAsync(string projectId, ProjectSide audience, DateTime before, CancellationToken ct) =>
        await Q(_context.tbl_WorksReports)
            .Where(r => r.ProjectId == projectId && r.Audience == audience && r.AsAt < before)
            .OrderByDescending(r => r.AsAt).ThenByDescending(r => r.IssuedAt)
            .FirstOrDefaultAsync(ct);

    // ═════════════════════════════════════════════════════════════════════
    // Issue, read back, history
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<WorksReportDto>> IssueAsync(IssueReportDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var (project, a) = await StandingAsync(dto.ProjectId ?? "", userId, ct);
            if (project is null || !CanReadReport(a)) return NotFound<WorksReportDto>();
            if (!CanIssueReport(a))
                return ServiceResult<WorksReportDto>.Failure(new ForbiddenException("Only the mediator or a client principal issues the report."));

            var audience = dto.Audience == ProjectSide.Contractor && a.CanSeeSiteLog ? ProjectSide.Contractor : ProjectSide.Client;
            var issuer = await _context.Users.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
            var row = await IssueCoreAsync(project, AsAtOf(dto.AsAt), audience, ReportIssueKind.Manual, null, null,
                userId, FullName(issuer), Trim(dto.CoveringNote, 1000), ct);
            return await GetIssuedAsync(row.Id, userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing a works report for {ProjectId}", dto.ProjectId);
            return ServiceResult<WorksReportDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    private async Task<tbl_WorksReport> IssueCoreAsync(tbl_Project project, DateTime at, ProjectSide audience, ReportIssueKind kind,
        string? reason, string? triggerKey, string? issuedById, string? issuedByName, string? note, CancellationToken ct)
    {
        var previous = await PreviousAsync(project.Id, audience, at, ct);
        var windowFrom = previous?.AsAt ?? at.AddDays(-7);
        var (dto, requests) = await AssembleAsync(project, at, windowFrom, audience, previous, ct);

        var narrative = await NarrateAsync(requests, useModel: true, project.ReportDraftingEnabled, ct);

        dto.IssueKind = kind;
        dto.IssueReason = reason;
        dto.IssuedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        dto.IssuedByName = issuedByName ?? "Issued on schedule";
        dto.CoveringNote = note;
        dto.PreviousReportId = previous?.Id;
        dto.PreviousAsAt = previous?.AsAt;

        var members = await Q(_context.tbl_ProjectMembers).Include(m => m.User)
            .Where(m => (m.ProjectId == project.Id || m.ProjectId == project.ParentProjectId) && m.IsActive && m.UserId != null)
            .ToListAsync(ct);
        var recipients = members
            .Where(m => m.IsMediator || (m.Side == audience && ProjectSeatDefaults.For(m.Specialization) == ProjectSeat.Principal))
            .Select(MemberName).Distinct().ToList();
        var delivery = recipients.Count == 0 ? "No principal on the roster to receive it yet" : $"In the app for {string.Join(", ", recipients)}";

        var snapshot = JsonSerializer.Serialize(dto, SnapshotJson);
        var narrativeJson = JsonSerializer.Serialize(narrative, SnapshotJson);

        var row = new tbl_WorksReport
        {
            ProjectId = project.Id,
            AsAt = at,
            WindowFrom = windowFrom,
            Audience = audience,
            IssuedById = issuedById,
            IssuedAt = dto.IssuedAt.Value,
            IssueKind = kind,
            IssueReason = reason,
            TriggerKey = triggerKey,
            CoveringNote = note,
            SnapshotJson = snapshot,
            NarrativeJson = narrativeJson,
            ContentSha256 = Hash(snapshot, narrativeJson),
            PreviousReportId = previous?.Id,
            DeliveryNote = delivery,
            DeliveredAt = DateTime.Now
        };
        _context.tbl_WorksReports.Add(row);
        await _context.SaveChangesAsync(ct);
        return row;
    }

    private static string Hash(string snapshot, string narrative) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot + "\n" + narrative))).ToLowerInvariant();

    public async Task<ServiceResult<WorksReportDto>> GetIssuedAsync(string reportId, string userId, CancellationToken ct = default)
    {
        try
        {
            var row = await Q(_context.tbl_WorksReports).FirstOrDefaultAsync(r => r.Id == reportId, ct);
            if (row?.ProjectId is null) return NotFound<WorksReportDto>();
            var (project, a) = await StandingAsync(row.ProjectId, userId, ct);
            if (project is null || !CanReadReport(a) || !CanReadAudience(a, row.Audience)) return NotFound<WorksReportDto>();

            var dto = JsonSerializer.Deserialize<WorksReportDto>(row.SnapshotJson ?? "{}", SnapshotJson) ?? new WorksReportDto();
            dto.Narrative = JsonSerializer.Deserialize<NarrativeDto>(row.NarrativeJson ?? "{}", SnapshotJson) ?? new NarrativeDto();
            dto.Id = row.Id;
            dto.IsLive = false;
            dto.ContentSha256 = row.ContentSha256;
            dto.HashVerified = Hash(row.SnapshotJson ?? "", row.NarrativeJson ?? "") == row.ContentSha256;
            dto.DeliveryNote = row.DeliveryNote;
            Redact(dto, a);
            return ServiceResult<WorksReportDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading works report {ReportId}", reportId);
            return ServiceResult<WorksReportDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<WorksReportSummaryDto>>> GetHistoryAsync(string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var (project, a) = await StandingAsync(projectId, userId, ct);
            if (project is null || !CanReadReport(a)) return NotFound<List<WorksReportSummaryDto>>();

            var rows = await Q(_context.tbl_WorksReports).Include(r => r.IssuedBy)
                .Where(r => r.ProjectId == projectId)
                .OrderByDescending(r => r.AsAt).ThenByDescending(r => r.IssuedAt)
                .ToListAsync(ct);

            var list = new List<WorksReportSummaryDto>();
            foreach (var r in rows.Where(r => CanReadAudience(a, r.Audience)))
            {
                decimal overall = 0;
                string? state = null;
                try
                {
                    using var doc = JsonDocument.Parse(r.SnapshotJson ?? "{}");
                    if (doc.RootElement.TryGetProperty("progress", out var p) && p.TryGetProperty("overallPercent", out var o)) overall = o.GetDecimal();
                    if (doc.RootElement.TryGetProperty("deadline", out var d) && d.TryGetProperty("stateWord", out var s)) state = s.GetString();
                }
                catch (JsonException) { /* an unreadable snapshot still lists */ }

                list.Add(new WorksReportSummaryDto
                {
                    Id = r.Id,
                    ProjectId = projectId,
                    AsAt = r.AsAt,
                    WindowFrom = r.WindowFrom,
                    Audience = r.Audience,
                    IssueKind = r.IssueKind,
                    IssueReason = r.IssueReason,
                    IssuedAt = r.IssuedAt,
                    IssuedByName = r.IssuedBy is { } u ? FullName(u) : "Issued on schedule",
                    ContentSha256 = r.ContentSha256,
                    OverallPercent = overall,
                    DeadlineState = state,
                    DeliveryNote = r.DeliveryNote
                });
            }
            return ServiceResult<List<WorksReportSummaryDto>>.Success(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing works reports for {ProjectId}", projectId);
            return ServiceResult<List<WorksReportSummaryDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Settings and posters
    // ═════════════════════════════════════════════════════════════════════

    private const string DraftingStatement =
        "Drafting sends this project's report facts — figures, dates and short quotes from the record, already " +
        "filtered to the reader's side — to Anthropic's Claude, which writes the few sentences on the page. " +
        "Numbers never come from the model, and every sentence it writes is checked against the facts. " +
        "Off, the report is written from templates and nothing leaves this server.";

    private bool DraftingAvailable => _narrators.Any(n => n is not TemplateReportNarrator && n.IsAvailable);

    /// <summary>Sending the record out is the owner's call — the funder's side, not the mediator's.</summary>
    private static bool CanChangeDrafting(tbl_Project p, ProjectAccess a, string userId) =>
        p.InvestorId == userId || (a.CanManage && a.Side == ProjectSide.Client);

    public async Task<ServiceResult<ReportSettingsDto>> GetSettingsAsync(string projectId, string userId, CancellationToken ct = default)
    {
        var (project, a) = await StandingAsync(projectId, userId, ct);
        if (project is null || !CanReadReport(a)) return NotFound<ReportSettingsDto>();
        return ServiceResult<ReportSettingsDto>.Success(new ReportSettingsDto
        {
            ProjectId = projectId,
            DraftingEnabled = project.ReportDraftingEnabled,
            DraftingAvailable = DraftingAvailable,
            CanChange = CanChangeDrafting(project, a, userId),
            Statement = DraftingStatement
        });
    }

    public async Task<ServiceResult<ReportSettingsDto>> UpdateSettingsAsync(ReportSettingsUpdateDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var (project, a) = await StandingAsync(dto.ProjectId ?? "", userId, ct);
            if (project is null || !CanReadReport(a)) return NotFound<ReportSettingsDto>();
            if (!CanChangeDrafting(project, a, userId))
                return ServiceResult<ReportSettingsDto>.Failure(new ForbiddenException("Only the project's owner decides whether its record is sent for drafting."));

            var tracked = await _context.tbl_Projects_RS.IgnoreQueryFilters().FirstAsync(p => p.Id == project.Id, ct);
            tracked.ReportDraftingEnabled = dto.DraftingEnabled;
            await _context.SaveChangesAsync(ct);
            return await GetSettingsAsync(project.Id, userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing report settings for {ProjectId}", dto.ProjectId);
            return ServiceResult<ReportSettingsDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<NarrativeCheckResultDto>> CheckNarrativeAsync(NarrativeCheckDto dto, string userId, CancellationToken ct = default)
    {
        var (project, a) = await StandingAsync(dto.ProjectId ?? "", userId, ct);
        if (project is null || !CanReadReport(a)) return NotFound<NarrativeCheckResultDto>();

        var at = AsAtOf(null);
        var previous = await PreviousAsync(project.Id, ProjectSide.Client, at, ct);
        var (_, requests) = await AssembleAsync(project, at, previous?.AsAt ?? at.AddDays(-7), ProjectSide.Client, previous, ct);
        var target = requests.FirstOrDefault(r => r.TargetId == dto.Draft.TargetId);
        var result = new NarrativeCheckResultDto { TargetFound = target is not null };
        if (target is not null) result.Accepted = NarrativeValidator.Accept(target, dto.Draft, result.Reasons).Count;
        return ServiceResult<NarrativeCheckResultDto>.Success(result);
    }

    public async Task<ServiceResult<int>> QueuePostersAsync(string projectId, string userId, CancellationToken ct = default)
    {
        var (project, a) = await StandingAsync(projectId, userId, ct);
        if (project is null || !CanReadReport(a)) return NotFound<int>();

        var done = Q(_context.tbl_ArtifactPosters).Where(p => p.Status == PosterStatus.Done).Select(p => p.ArtifactId);
        var videos = await Q(_context.tbl_Artifacts)
            .Where(x => x.ProjectId == projectId && x.MimeType != null && x.MimeType.StartsWith("video/") && !done.Contains(x.Id))
            .Select(x => x.Id).ToListAsync(ct);
        foreach (var id in videos) _posters.Enqueue(id);
        return ServiceResult<int>.Success(videos.Count);
    }

    // ═════════════════════════════════════════════════════════════════════
    // The system's own issuing — weekly and on milestones (§7)
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ReportScheduleRunDto> RunScheduleAsync(DateTime? asAt, bool weekly, bool milestones, string? projectId = null, CancellationToken ct = default)
    {
        var at = AsAtOf(asAt);
        var run = new ReportScheduleRunDto { AsAt = at };

        var q = _context.tbl_Projects_RS.IgnoreQueryFilters().Include(p => p.ParentProject)
            .Where(p => p.IsDeleted != true && p.ArchivedAt == null && p.Status == ProjectStatus.Active);
        if (!string.IsNullOrEmpty(projectId)) q = q.Where(p => p.Id == projectId);
        var projects = await q.ToListAsync(ct);

        foreach (var project in projects)
        {
            try
            {
                var reports = await Q(_context.tbl_WorksReports)
                    .Where(r => r.ProjectId == project.Id && r.Audience == ProjectSide.Client)
                    .Select(r => new { r.Id, r.AsAt, r.IssueKind, r.TriggerKey })
                    .ToListAsync(ct);

                if (milestones)
                {
                    var since = reports.Where(r => r.AsAt <= at).Select(r => (DateTime?)r.AsAt).Max() ?? at.AddDays(-7);
                    var triggers = (await MilestonesAsync(project, since, at, ct))
                        .Where(t => !reports.Any(r => r.TriggerKey != null && r.TriggerKey.Split('|').Contains(t.Key)))
                        .ToList();
                    if (triggers.Count > 0)
                    {
                        var row = await IssueCoreAsync(project, at, ProjectSide.Client, ReportIssueKind.Milestone,
                            Trim(string.Join("; ", triggers.Select(t => t.Reason)), 300),
                            Trim(string.Join("|", triggers.Select(t => t.Key)), 120), null, null, null, ct);
                        run.Milestones++;
                        run.ReportIds.Add(row.Id);
                        reports.Add(new { row.Id, row.AsAt, row.IssueKind, row.TriggerKey });
                    }
                }

                // Weekly whether or not anybody logged in (Law 0) — but never twice in one week.
                if (weekly && !reports.Any(r => r.IssueKind == ReportIssueKind.Scheduled && r.AsAt > at.AddDays(-6) && r.AsAt <= at))
                {
                    var row = await IssueCoreAsync(project, at, ProjectSide.Client, ReportIssueKind.Scheduled,
                        "Weekly report", null, null, null, null, ct);
                    run.Weekly++;
                    run.ReportIds.Add(row.Id);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Scheduled works report failed for {ProjectId}", project.Id);
                run.Notes.Add($"{project.ProjectName}: {ex.Message}");
                _context.ChangeTracker.Clear();
            }
        }
        return run;
    }

    /// <summary>A stage that finished, or a completion date that passed unmet, since the last report.</summary>
    private async Task<List<(string Key, string Reason)>> MilestonesAsync(tbl_Project project, DateTime since, DateTime at, CancellationToken ct)
    {
        var found = new List<(string, string)>();
        var stages = await Q(_context.tbl_Stages).Where(s => s.ProjectId == project.Id).ToListAsync(ct);
        var readings = await Q(_context.tbl_ProgressReadings).Where(r => r.ProjectId == project.Id).ToListAsync(ct);
        var refs = stages.Select(s => new StageRef(s.Id, s.StageName ?? "", s.CatalogueKey)).ToList();
        var byStage = ReadingsByStage(readings, refs, ProjectSide.Client);

        foreach (var s in Leaves(stages))
        {
            var then = StateAt(s, byStage.GetValueOrDefault(s.Id) ?? new(), since);
            var now = StateAt(s, byStage.GetValueOrDefault(s.Id) ?? new(), at);
            if (then.Status != StageStatus.Completed && now.Status == StageStatus.Completed)
                found.Add(($"stage-complete:{s.Id}", $"{s.StageName} complete"));
        }

        var dates = await Q(_context.tbl_Commitments)
            .Where(c => c.ProjectId == project.Id && c.Kind == CommitmentKind.Date && c.DeliverableId == null && c.DueDate != null)
            .ToListAsync(ct);
        var chain = dates.Where(c => IsProjectDeadline(c, refs) && AgreedOf(c) <= at).OrderBy(AgreedOf).ToList();
        DateTime? promised = chain.FirstOrDefault()?.DueDate ?? project.ExpectedCompletionDate;
        var allDone = Leaves(stages).Count > 0 && Leaves(stages).All(s => StateAt(s, byStage.GetValueOrDefault(s.Id) ?? new(), at).Status == StageStatus.Completed);
        if (promised is { } p && p.Date > since.Date && p.Date < at.Date && !allDone)
            found.Add(($"deadline-lapsed:{p:yyyyMMdd}", $"The completion date of {p:d MMM} passed with work still open"));
        return found;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Assembly — every figure final before a word is written (§6.1)
    // ═════════════════════════════════════════════════════════════════════

    private sealed record Frame(string ArtifactId, string? Sha, string? Mime, string? ThumbPath, string? StoragePath,
        DateTime At, string? StageId, string? Caption, string SourceId);

    private sealed record StageState(decimal? Percent, DateTime? PercentAt, StageStatus Status);

    private sealed class Sources
    {
        private readonly Dictionary<string, ReportSourceDto> _all = new(StringComparer.Ordinal);

        public string Add(string id, ReportSourceKind kind, DateTime? at, string label, string? excerpt = null, string? href = null, string? artifactId = null)
        {
            if (!_all.ContainsKey(id))
                _all[id] = new ReportSourceDto { Id = id, Kind = kind, At = at, Label = label, Excerpt = excerpt, Href = href, ArtifactId = artifactId };
            return id;
        }

        public bool Has(string id) => _all.ContainsKey(id);
        public ReportSourceDto? Get(string id) => _all.GetValueOrDefault(id);

        public List<ReportSourceDto> Numbered()
        {
            var list = _all.Values.OrderBy(s => s.At ?? DateTime.MaxValue).ThenBy(s => s.Kind).ThenBy(s => s.Id, StringComparer.Ordinal).ToList();
            for (var i = 0; i < list.Count; i++) list[i].Ref = $"S{i + 1}";
            return list;
        }
    }

    private async Task<(WorksReportDto Dto, List<NarrativeRequest> Requests)> AssembleAsync(
        tbl_Project project, DateTime at, DateTime windowFrom, ProjectSide audience, tbl_WorksReport? previous, CancellationToken ct)
    {
        var pid = project.Id;
        var today = DateTime.Now.Date;
        var live = at.Date >= today;
        var src = new Sources();
        var requests = new List<NarrativeRequest>();
        var currency = project.Currency ?? "UGX";

        // ── The record ─────────────────────────────────────────────────
        var stages = await Q(_context.tbl_Stages).Where(s => s.ProjectId == pid).ToListAsync(ct);
        var stageById = stages.ToDictionary(s => s.Id);
        var refs = stages.Select(s => new StageRef(s.Id, s.StageName ?? "", s.CatalogueKey)).ToList();
        var deliverables = await Q(_context.tbl_Deliverables).Where(d => d.ProjectId == pid).ToListAsync(ct);

        var members = await Q(_context.tbl_ProjectMembers).Include(m => m.User)
            .Where(m => (m.ProjectId == pid || m.ProjectId == project.ParentProjectId) && m.IsActive)
            .ToListAsync(ct);
        var mediator = members.Where(m => m.IsMediator).OrderBy(m => m.ProjectId == pid ? 0 : 1).FirstOrDefault();
        var mediatorName = mediator is null ? null : MemberName(mediator);
        var memberById = members.ToDictionary(m => m.Id);

        var readings = await Q(_context.tbl_ProgressReadings).Where(r => r.ProjectId == pid).ToListAsync(ct);
        var readingsByStage = ReadingsByStage(readings, refs, audience);
        foreach (var list in readingsByStage.Values) list.RemoveAll(r => r.At > at);

        // The thread this audience may read: a client-side report reads the client's own imports only.
        var batches = await Q(_context.tbl_IngestBatches).Where(b => b.ProjectId == pid)
            .Select(b => new { b.Id, b.ImportedSide, b.ArchiveArtifactId }).ToListAsync(ct);
        var readableBatches = batches.Where(b => audience == ProjectSide.Contractor || b.ImportedSide == ProjectSide.Client).Select(b => b.Id).ToList();
        var threadFrom = new[] { windowFrom.AddDays(-VantageLookbackDays), at.AddDays(-60) }.Min();
        var messages = readableBatches.Count == 0 ? new List<tbl_IngestedMessage>() : await Q(_context.tbl_IngestedMessages)
            .Where(m => m.BatchId != null && readableBatches.Contains(m.BatchId) && !m.IsSystemMessage && m.SentAt >= threadFrom && m.SentAt <= at)
            .OrderBy(m => m.SentAt).ThenBy(m => m.SequenceNo)
            .ToListAsync(ct);
        var messageById = messages.ToDictionary(m => m.Id);
        var readingMessageIds = readingsByStage.Values.SelectMany(l => l).Where(r => r.Row.SourceKind == ProgressReadingSource.Ingested && r.Row.SourceId != null)
            .Select(r => r.Row.SourceId!).Where(id => !messageById.ContainsKey(id)).Distinct().ToList();
        if (readingMessageIds.Count > 0)
            foreach (var m in await Q(_context.tbl_IngestedMessages).Where(m => readingMessageIds.Contains(m.Id)).ToListAsync(ct))
                messageById[m.Id] = m;

        string MessageSource(tbl_IngestedMessage m) => src.Add($"msg:{m.Id}", ReportSourceKind.Message, m.SentAt,
            $"From the thread, {m.SentAt:d MMM HH:mm}", Trim(m.Body, 280), $"/project/{pid}/history?day={m.SentAt:yyyy-MM-dd}");

        string ReadingSource(Reading r)
        {
            if (r.Row.SourceKind == ProgressReadingSource.Ingested && r.Row.SourceId is { } mid && messageById.TryGetValue(mid, out var m))
                MessageSource(m);
            var label = r.Row.SourceKind switch
            {
                ProgressReadingSource.Ingested => $"Said in the thread: {r.Row.Percent:0.#}%",
                ProgressReadingSource.Capture => $"Captured on site: {r.Row.Percent:0.#}%",
                _ => $"Recorded on the stage: {r.Row.Percent:0.#}%"
            };
            var excerpt = r.Row.SourceKind == ProgressReadingSource.Ingested && r.Row.SourceId is { } id2 && messageById.TryGetValue(id2, out var m2) ? Trim(m2.Body, 280) : null;
            return src.Add($"reading:{r.Row.Id}", ReportSourceKind.Reading, r.At, label, excerpt,
                r.Row.SourceKind == ProgressReadingSource.Ingested ? $"/project/{pid}/history?day={r.At:yyyy-MM-dd}" : null);
        }

        string StageSource(tbl_Stage s) => src.Add($"stage:{s.Id}", ReportSourceKind.Stage, null, $"Stage record: {s.StageName}",
            null, $"/project/{pid}/stage/{s.Id}");

        // ── Media: the thread's files, re-joined files, and captures this audience may see ──
        var archives = batches.Select(b => b.ArchiveArtifactId).OfType<string>().ToHashSet();
        var windowMessageIds = messages.Where(m => m.SentAt >= windowFrom.AddDays(-VantageLookbackDays)).Select(m => m.Id).ToList();
        var bindings = await Q(_context.tbl_MediaBindings)
            .Where(b => b.ProjectId == pid && b.IngestedMessageId != null && windowMessageIds.Contains(b.IngestedMessageId) && b.ArtifactId != null)
            .Select(b => new { b.IngestedMessageId, b.ArtifactId }).ToListAsync(ct);
        var mediaOf = messages.Where(m => m.ArtifactId != null && !archives.Contains(m.ArtifactId))
            .Select(m => (MessageId: m.Id, ArtifactId: m.ArtifactId!))
            .Concat(bindings.Select(b => (MessageId: b.IngestedMessageId!, ArtifactId: b.ArtifactId!)))
            .Distinct().ToList();

        var captureQuery = Q(_context.tbl_ProgressUpdates).Include(u => u.Images).ThenInclude(i => i.Artifact)
            .Where(u => u.ProjectId == pid);
        if (audience == ProjectSide.Client) captureQuery = captureQuery.Where(u => u.Channel == Channel.Client);
        var captures = (await captureQuery.ToListAsync(ct)).Where(u => Local(u.DateTimeCreated) <= at).ToList();

        var artifactIds = mediaOf.Select(x => x.ArtifactId).Distinct().ToList();
        var artifacts = await Q(_context.tbl_Artifacts).Where(x => artifactIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var captions = await Q(_context.tbl_ArtifactRefs)
            .Where(r => r.ArtifactId != null && artifactIds.Contains(r.ArtifactId) && r.Caption != null)
            .Select(r => new { r.ArtifactId, r.Caption }).ToListAsync(ct);
        var humanCaption = captions.Where(c => !CameraName.IsMatch(c.Caption!))
            .GroupBy(c => c.ArtifactId!).ToDictionary(g => g.Key, g => Path.GetFileNameWithoutExtension(g.First().Caption!));

        // What each message is about, and what its sender's burst of photos is about.
        var stageOfMessage = new Dictionary<string, string?>();
        foreach (var m in messages)
            stageOfMessage[m.Id] = string.IsNullOrWhiteSpace(m.Body) || RuleMessageExtractor.IsAcknowledgement(m.Body) ? null : StageMatcher.Match(m.Body, refs);

        var frames = new List<Frame>();
        foreach (var (messageId, artifactId) in mediaOf)
        {
            if (!messageById.TryGetValue(messageId, out var m) || !artifacts.TryGetValue(artifactId, out var art)) continue;
            if (!IsPicture(art.MimeType)) continue;
            var caption = humanCaption.GetValueOrDefault(artifactId);
            var stageId = (caption is null ? null : StageMatcher.Match(caption, refs))
                          ?? stageOfMessage.GetValueOrDefault(m.Id)
                          ?? messages.Where(o => o.ExternalAuthor == m.ExternalAuthor && Math.Abs((o.SentAt - m.SentAt).TotalMinutes) <= 45 && stageOfMessage.GetValueOrDefault(o.Id) is not null)
                                     .OrderBy(o => Math.Abs((o.SentAt - m.SentAt).TotalMinutes)).Select(o => stageOfMessage[o.Id]).FirstOrDefault();
            MessageSource(m);
            frames.Add(new Frame(art.Id, art.Sha256, art.MimeType, art.ThumbnailPath, art.StoragePath, m.SentAt, stageId, caption,
                src.Add($"{(IsVideo(art.MimeType) ? "video" : "photo")}:{art.Id}", IsVideo(art.MimeType) ? ReportSourceKind.Video : ReportSourceKind.Photo,
                    m.SentAt, $"{(IsVideo(art.MimeType) ? "Video" : "Photo")} from the thread, {m.SentAt:d MMM}", caption,
                    $"/project/{pid}/history?day={m.SentAt:yyyy-MM-dd}", art.Id)));
        }
        foreach (var u in captures)
        {
            var when = Local(u.DateTimeCreated);
            var capId = src.Add($"capture:{u.Id}", ReportSourceKind.Capture, when, $"Captured on site, {when:d MMM}", Trim(u.Description, 280), $"/project/{pid}/entry/{u.Id}");
            foreach (var img in u.Images.Where(i => audience == ProjectSide.Contractor || i.Channel == Channel.Client))
            {
                if (img.Artifact is not { } art || !IsPicture(art.MimeType)) continue;
                frames.Add(new Frame(art.Id, art.Sha256, art.MimeType, art.ThumbnailPath, art.StoragePath, when,
                    u.StageId ?? StageMatcher.Match(u.Description, refs), img.Caption, capId));
            }
        }
        frames = frames.GroupBy(f => f.ArtifactId).Select(g => g.OrderBy(f => f.At).First()).OrderBy(f => f.At).ToList();

        var videoIds = frames.Where(f => IsVideo(f.Mime)).Select(f => f.ArtifactId).ToList();
        var posters = videoIds.Count == 0 ? new Dictionary<string, tbl_ArtifactPoster>() : await Q(_context.tbl_ArtifactPosters)
            .Where(p => p.ArtifactId != null && videoIds.Contains(p.ArtifactId)).ToDictionaryAsync(p => p.ArtifactId!, ct);
        foreach (var missing in videoIds.Where(id => !posters.ContainsKey(id))) _posters.Enqueue(missing);

        ReportFrameDto FrameDto(Frame f)
        {
            var poster = posters.GetValueOrDefault(f.ArtifactId);
            return new ReportFrameDto
            {
                ArtifactId = f.ArtifactId,
                At = f.At,
                Caption = f.Caption,
                IsVideo = IsVideo(f.Mime),
                PosterArtifactId = poster?.PosterArtifactId,
                DurationSeconds = poster?.DurationSeconds,
                PosterStatus = IsVideo(f.Mime) ? poster?.Status ?? PosterStatus.Pending : null,
                SourceId = f.SourceId
            };
        }

        var windowFrames = frames.Where(f => f.At >= windowFrom && f.At <= at).ToList();

        // ── Stages at this moment ──────────────────────────────────────
        var leaves = Leaves(stages);
        var stageStates = leaves.ToDictionary(s => s.Id, s => StateAt(s, readingsByStage.GetValueOrDefault(s.Id) ?? new(), at, live));
        var stageReports = new List<StageReportDto>();

        foreach (var s in leaves)
        {
            var state = stageStates[s.Id];
            var rs = readingsByStage.GetValueOrDefault(s.Id) ?? new();
            var stageSrc = StageSource(s);
            var sr = new StageReportDto
            {
                StageId = s.Id,
                Name = s.StageName ?? "Stage",
                ParentName = s.ParentStageId is { } parent && stageById.TryGetValue(parent, out var ps) ? ps.StageName : null,
                Phase = s.Phase,
                Status = state.Status,
                Percent = state.Percent,
                PercentAt = state.PercentAt,
                PlannedEnd = s.ExpectedEndDate,
                BaselineEnd = s.BaselineEndDate,
                SlipDays = s.ExpectedEndDate is { } pe && s.BaselineEndDate is { } be && pe.Date != be.Date ? (int)(pe.Date - be.Date).TotalDays : null,
                DeliverablesTotal = deliverables.Count(d => d.StageId == s.Id),
                DeliverablesDone = deliverables.Count(d => d.StageId == s.Id && DeliverableDone(d, at, live))
            };
            sr.SourceIds.Add(stageSrc);
            foreach (var r in rs) sr.Readings.Add(new ReadingPointDto { At = r.At, Percent = r.Row.Percent, SourceId = ReadingSource(r) });

            // Stall: in progress, and the latest reading unchanged for the threshold or longer (§4.3).
            if (state.Status == StageStatus.InProgress && rs.Count > 0 && rs[^1].Row.Percent < 100)
            {
                var current = rs[^1].Row.Percent;
                var since = rs[^1].At;
                for (var i = rs.Count - 1; i >= 0 && rs[i].Row.Percent == current; i--) since = rs[i].At;
                var days = (int)(at.Date - since.Date).TotalDays;
                if (days >= _stallDays)
                {
                    sr.IsStalled = true;
                    sr.StalledDays = days;
                    sr.StalledSince = since;
                    var still = messages.LastOrDefault(m => m.SentAt > since && stageOfMessage.GetValueOrDefault(m.Id) == s.Id);
                    if (still is not null) { sr.StillReportedAt = still.SentAt; sr.SourceIds.Add(MessageSource(still)); }
                }
            }

            // Pace over the last three weeks; zero pace gives no forecast, never a guess.
            if (state.Status == StageStatus.Completed)
            {
                sr.Forecast = s.ActualEndDate ?? state.PercentAt;
            }
            else if (rs.Count > 0)
            {
                var latest = rs[^1];
                var windowStart = at.AddDays(-PaceDays);
                var baseline = rs.LastOrDefault(r => r.At < windowStart) ?? rs.FirstOrDefault(r => r.At >= windowStart && r != latest);
                if (baseline is not null && latest.At > baseline.At && latest.Row.Percent > baseline.Row.Percent)
                {
                    var perDay = (double)(latest.Row.Percent - baseline.Row.Percent) / Math.Max(1, (latest.At - baseline.At).TotalDays);
                    var remaining = (double)(100 - latest.Row.Percent);
                    sr.Forecast = at.Date.AddDays(Math.Ceiling(remaining / perDay));
                    sr.ForecastBasis = $"the pace since {baseline.At:d MMM}";
                }
            }
            if (sr.Forecast is { } f && s.ExpectedEndDate is { } planned) sr.ForecastVsPlanDays = (int)(f.Date - planned.Date).TotalDays;

            // A stage in progress with no reading says so in its own sentence; no second marker for the same gap.
            if (rs.Count > 0 && state.Status == StageStatus.InProgress && (at.Date - rs[^1].At.Date).TotalDays >= QuietReadingDays)
                sr.Gaps.Add($"No reading since {rs[^1].At:d MMM}");
            if (s.ExpectedEndDate is null && state.Status != StageStatus.Completed) sr.Gaps.Add("No planned end date");

            // Two frames at most: the newest in the window and the earliest (works-report.md §5, v1).
            var stageFrames = windowFrames.Where(fr => fr.StageId == s.Id).ToList();
            if (stageFrames.Count > 0)
            {
                sr.Frames.Add(FrameDto(stageFrames[0]));
                if (stageFrames.Count > 1) sr.Frames.Add(FrameDto(stageFrames[^1]));
                foreach (var fr in sr.Frames) sr.SourceIds.Add(fr.SourceId);
            }

            stageReports.Add(sr);
        }

        // ── Overall progress ───────────────────────────────────────────
        var progress = new ProgressSummaryDto
        {
            StageCount = leaves.Count,
            Completed = stageReports.Count(s => s.Status == StageStatus.Completed),
            InProgress = stageReports.Count(s => s.Status == StageStatus.InProgress),
            NotStarted = stageReports.Count(s => s.Status == StageStatus.NotStarted),
            Stalled = stageReports.Count(s => s.IsStalled),
            StallThresholdDays = _stallDays
        };
        var budgeted = leaves.All(s => s.BudgetAmount is > 0) && leaves.Count > 0;
        if (leaves.Count > 0)
        {
            decimal Pct(tbl_Stage s) => stageStates[s.Id].Status == StageStatus.Completed ? 100 : Math.Clamp(stageStates[s.Id].Percent ?? 0, 0, 100);
            progress.OverallPercent = budgeted
                ? Math.Round(leaves.Sum(s => Pct(s) * s.BudgetAmount!.Value) / leaves.Sum(s => s.BudgetAmount!.Value), 1)
                : Math.Round(leaves.Average(Pct), 1);
        }
        progress.Weighting = budgeted ? "budget" : "stage count";
        progress.WeightingNote = budgeted
            ? "Each stage counts in proportion to its budget."
            : leaves.Count == 0 ? "No stages on this project yet." : $"Each stage counts equally — {leaves.Count(s => s.BudgetAmount is not > 0)} of {leaves.Count} have no budget.";

        // ── Dates: the completion commitment, its history, and the short promises ──
        var commitments = (await Q(_context.tbl_Commitments).Include(c => c.AgreedBy).Include(c => c.AgreedWithMember).ThenInclude(m => m!.User)
                .Where(c => c.ProjectId == pid).ToListAsync(ct))
            .Where(c => AgreedOf(c) <= at).ToList();
        var successorOf = commitments.Where(c => c.SupersedesId != null).GroupBy(c => c.SupersedesId!)
            .ToDictionary(g => g.Key, g => g.OrderBy(AgreedOf).First());
        bool IsHead(tbl_Commitment c) => !successorOf.ContainsKey(c.Id);
        string CommitmentSrc(tbl_Commitment c)
        {
            if (c.IngestedMessageId is { } mid && messageById.TryGetValue(mid, out var m)) MessageSource(m);
            return src.Add($"commitment:{c.Id}", ReportSourceKind.Commitment, AgreedOf(c), $"In the register: {c.Title}",
                Trim(c.Body, 280), $"/project/{pid}/register?commitment={c.Id}");
        }
        List<string> CommitmentSources(tbl_Commitment c)
        {
            var ids = new List<string> { CommitmentSrc(c) };
            if (c.IngestedMessageId is { } mid && messageById.ContainsKey(mid)) ids.Add($"msg:{mid}");
            return ids;
        }
        bool Delivered(tbl_Commitment c)
        {
            if (c.DeliveredAt is { } d && Local(d) <= at) return true;
            if (c.DeliverableId is { } did && deliverables.FirstOrDefault(x => x.Id == did) is { } del && DeliverableDone(del, at, live)) return true;
            if (c.StageId is { } sid && stageStates.TryGetValue(sid, out var st) && st.Status == StageStatus.Completed && CompletionTitle.IsMatch(c.Title ?? "")) return true;
            return live && c.Maturity >= CommitmentMaturity.Delivered;
        }

        var dateRows = commitments.Where(c => c.Kind == CommitmentKind.Date && c.DueDate != null).ToList();
        var chain = dateRows.Where(c => IsProjectDeadline(c, refs)).OrderBy(AgreedOf).ToList();
        var chainIds = chain.Select(c => c.Id).ToHashSet();
        var strip = new DeadlineStripDto();

        if (chain.Count > 0)
        {
            strip.Basis = "date commitments";
            var first = chain[0];
            strip.Promised = first.DueDate!.Value.Date;
            strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.Set, At = AgreedOf(first), Label = $"Set {AgreedOf(first):d MMM}: {strip.Promised:d MMM}", SourceIds = CommitmentSources(first) });
            foreach (var c in chain.Skip(1))
            {
                var same = c.DueDate!.Value.Date == strip.Promised;
                if (same)
                    strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.Restated, At = AgreedOf(c), Label = $"Restated {AgreedOf(c):d MMM}", SourceIds = CommitmentSources(c) });
            }
            var revised = chain.LastOrDefault(c => c.DueDate!.Value.Date != strip.Promised);
            if (revised is not null)
            {
                strip.ContractorDate = revised.DueDate!.Value.Date;
                strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.ContractorDate, At = strip.ContractorDate.Value, Label = $"Revised date given {AgreedOf(revised):d MMM}: {strip.ContractorDate:d MMM}", SourceIds = CommitmentSources(revised) });
            }
            strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.Promised, At = strip.Promised.Value, Label = $"Promised: {strip.Promised:d MMM}", SourceIds = CommitmentSources(first) });
        }
        else if (project.ExpectedCompletionDate is { } exp)
        {
            strip.Basis = "project dates";
            strip.Promised = exp.Date;
            var projSrc = src.Add($"project:{pid}", ReportSourceKind.Stage, null, "Project record: completion date", null, $"/project/{pid}");
            strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.Promised, At = exp.Date, Label = $"Planned completion: {exp:d MMM}", SourceIds = { projSrc } });
            if (project.RevisedCompletionDate is { } rev && rev.Date != exp.Date)
            {
                strip.ContractorDate = rev.Date;
                strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.ContractorDate, At = rev.Date, Label = $"Revised completion: {rev:d MMM}", SourceIds = { projSrc } });
            }
        }
        else strip.Basis = "none";

        // A short promise lapses when its day passed with the work not done and no restatement before it.
        var lapsed = dateRows.Where(c => !chainIds.Contains(c.Id) && c.DueDate!.Value.Date < at.Date && !Delivered(c)
                                         && !(successorOf.TryGetValue(c.Id, out var next) && AgreedOf(next).Date <= c.DueDate!.Value.Date))
            .OrderBy(c => c.DueDate).ToList();
        foreach (var c in lapsed)
            strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.Lapsed, At = c.DueDate!.Value.Date, Label = $"Lapsed: {c.Title} (due {c.DueDate:d MMM})", SourceIds = CommitmentSources(c) });
        strip.LapsedCount = lapsed.Count;

        // Pace forecast: the latest open-stage forecast, and the open stages it cannot speak for.
        var open = stageReports.Where(s => s.Status != StageStatus.Completed).ToList();
        var withForecast = open.Where(s => s.Forecast is not null).ToList();
        if (withForecast.Count > 0)
        {
            var latest = withForecast.OrderBy(s => s.Forecast).Last();
            strip.Forecast = latest.Forecast!.Value.Date;
            strip.ForecastBasis = $"{latest.ForecastBasis} on {latest.Name}";
            strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.Forecast, At = strip.Forecast.Value, Label = $"Pace forecast: {strip.Forecast:d MMM}", SourceIds = latest.Readings.TakeLast(2).Select(r => r.SourceId).ToList() });
        }
        strip.ForecastExcludes = open.Where(s => s.Status == StageStatus.InProgress && s.Forecast is null).Select(s => s.Name).ToList();
        var notStarted = open.Count(s => s.Status == StageStatus.NotStarted);
        if (notStarted > 0) strip.ForecastExcludes.Add($"{notStarted} stage{(notStarted == 1 ? "" : "s")} not started");
        strip.Marks.Add(new DeadlineMarkDto { Kind = DeadlineMarkKind.Today, At = at, Label = $"Today: {at:d MMM}" });

        var allDone = leaves.Count > 0 && open.Count == 0;
        (strip.Tone, strip.StateWord) =
            strip.Promised is null ? (ReportTone.Neutral, "No date on record")
            : allDone ? (ReportTone.Good, "Complete")
            : strip.Promised.Value < at.Date ? (ReportTone.Late, "Passed")
            : (strip.Forecast > strip.Promised || strip.ContractorDate > strip.Promised || progress.Stalled > 0 || strip.LapsedCount > 0
               || open.Any(s => s.Status == StageStatus.InProgress && s.Forecast is null)) ? (ReportTone.Watch, "At risk")
            : (ReportTone.Good, "On course");
        var markDates = strip.Marks.Select(m => m.At.Date).Append(windowFrom.Date).ToList();
        strip.AxisFrom = markDates.Min().AddDays(-3);
        strip.AxisTo = markDates.Max().AddDays(5);

        // ── Decisions and agreements ───────────────────────────────────
        var variations = (await Q(_context.tbl_Variations).Include(v => v.Stage).Include(v => v.ApprovedBy)
                .Where(v => v.ProjectId == pid).ToListAsync(ct))
            .Where(v => (v.RaisedAt ?? Local(v.DateTimeCreated)) <= at).ToList();
        var variationCommitments = variations.Select(v => v.CommitmentId).OfType<string>().ToHashSet();
        string? AgreedWith(tbl_Commitment c) => c.AgreedWithPartyName ?? (c.AgreedWithMember is { } am ? MemberName(am) : null);

        var decisions = new List<DecisionReportDto>();
        foreach (var c in commitments.Where(c => IsHead(c) && c.Kind != CommitmentKind.Date && c.Maturity >= CommitmentMaturity.Agreed
                                                 && !variationCommitments.Contains(c.Id))
                                     .OrderByDescending(AgreedOf))
        {
            var d = new DecisionReportDto
            {
                Id = c.Id,
                Kind = c.Kind,
                Title = c.Title ?? "Agreed",
                StageName = c.StageId is { } sid && stageById.TryGetValue(sid, out var st) ? st.StageName : null,
                Phase = c.StageId is { } sid2 && stageById.TryGetValue(sid2, out var st2) ? st2.Phase : null,
                // One accountable face (§10.1): the client side reads the mediator's name for the delivery side's words.
                AgreedByName = audience == ProjectSide.Client && c.RecordedBySide == ProjectSide.Contractor ? mediatorName : FullName(c.AgreedBy) ?? mediatorName,
                AgreedWith = AgreedWith(c),
                AgreedAt = AgreedOf(c),
                Maturity = c.Maturity,
                QueryState = c.QueryState,
                IsNew = AgreedOf(c) >= windowFrom,
                Restatements = Chain(c, commitments).Count - 1,
                Amount = c.Amount,
                Currency = c.Currency ?? (c.Amount is null ? null : currency)
            };
            d.SourceIds.AddRange(CommitmentSources(c));
            if (c.SourceChannel is CommitmentSource.Verbal or CommitmentSource.Meeting && c.CounterpartyConfirmedAt is null) d.Gaps.Add("Not confirmed by the other side");
            if (c.QueryState == CommitmentQueryState.QueryRaised) d.Gaps.Add("Queried, not resolved");
            if (c.IngestedMessageId is null && c.SourceChannel == CommitmentSource.App) d.Gaps.Add("No evidence linked");
            decisions.Add(d);
        }

        // ── Variations — each with its cost and time, or the gap where they should be ──
        var variationReports = new List<VariationReportDto>();
        foreach (var v in variations.OrderByDescending(v => v.RaisedAt ?? Local(v.DateTimeCreated)))
        {
            var raised = v.RaisedAt ?? Local(v.DateTimeCreated);
            var approvedAt = v.ApprovedAt is { } ap ? Local(ap) : (DateTime?)null;
            var status = v.Status == VariationStatus.Approved && approvedAt > at ? VariationStatus.Proposed : v.Status;
            var vr = new VariationReportDto
            {
                Id = v.Id,
                Title = v.Title ?? "A change",
                Reason = Trim(v.Reason, 300),
                StageName = v.Stage?.StageName,
                Phase = v.Stage?.Phase,
                CostDelta = v.CostDelta,
                Currency = v.Currency ?? currency,
                TimeDeltaDays = v.TimeDeltaDays,
                Status = status,
                RaisedAt = raised,
                ApprovedAt = status == VariationStatus.Approved ? approvedAt : null,
                ApprovedByName = status == VariationStatus.Approved ? FullName(v.ApprovedBy) : null,
                IsNew = raised >= windowFrom
            };
            vr.SourceIds.Add(src.Add($"variation:{v.Id}", ReportSourceKind.Variation, raised, $"Variation: {v.Title}", Trim(v.Reason, 280), $"/project/{pid}/money"));
            if (v.CommitmentId is { } cid && commitments.FirstOrDefault(c => c.Id == cid) is { } vc) vr.SourceIds.AddRange(CommitmentSources(vc));
            if (v.CostDelta is null) vr.Gaps.Add("Not costed");
            if (status == VariationStatus.Proposed) vr.Gaps.Add("No approval on record");
            if (v.TimeDeltaDays is null) vr.Gaps.Add("Time impact not stated");
            variationReports.Add(vr);
        }

        // A change of spec in the window with no variation on record is itself a finding (R3's exit).
        var scopeGaps = new List<ReportGapDto>();
        foreach (var c in commitments.Where(c => IsHead(c) && c.SupersedesId != null && c.Kind is CommitmentKind.Spec or CommitmentKind.Material or CommitmentKind.Choice
                                                 && AgreedOf(c) >= windowFrom))
        {
            var ids = Chain(c, commitments).Select(x => x.Id).ToHashSet();
            if (variations.Any(v => v.CommitmentId != null && ids.Contains(v.CommitmentId))) continue;
            var g = new ReportGapDto
            {
                Key = $"scope:{c.Id}",
                Text = $"Changed on {AgreedOf(c):d MMM} with no variation on record: {c.Title}",
                StageName = c.StageId is { } sid && stageById.TryGetValue(sid, out var st) ? st.StageName : null
            };
            g.SourceIds.AddRange(CommitmentSources(c));
            scopeGaps.Add(g);
        }

        // ── Blockers, by who has to move ───────────────────────────────
        var flags = (await Q(_context.tbl_Flags).Include(f => f.Stage).Include(f => f.OwnerMember).ThenInclude(m => m!.User)
                .Where(f => f.ProjectId == pid && f.CommitmentId == null && f.Status != FlagStatus.Archived).ToListAsync(ct))
            .Where(f => (f.RaisedAt ?? Local(f.DateTimeCreated)) <= at
                        && (f.Status is FlagStatus.Open or FlagStatus.InProgress || (f.ResolvedDate is { } rd && Local(rd) > at)))
            .ToList();
        var blockerItems = new List<(string Owner, BlockerReportDto Item)>();
        foreach (var f in flags)
        {
            var since = f.RaisedAt ?? Local(f.DateTimeCreated);
            // The floor carries a crew-channel blocker across; the crew's own wording stays on its side.
            var crossed = f.Channel == Channel.Crew && audience == ProjectSide.Client;
            var owner = f.OwnerMember is { } om ? MemberName(om) : f.OwnerPartyName ?? "Owner not named";
            var b = new BlockerReportDto
            {
                Id = f.Id,
                Title = f.Title ?? "A blocker",
                Detail = crossed ? null : Trim(f.Description, 240),
                Since = since,
                DaysOpen = Math.Max(0, (int)(at.Date - since.Date).TotalDays),
                StageName = f.Stage?.StageName,
                Phase = f.Stage?.Phase,
                LastChase = f.LastNudgeAt is { } n ? Local(n) : null,
                Crossed = crossed
            };
            b.SourceIds.Add(src.Add($"blocker:{f.Id}", ReportSourceKind.Blocker, since, $"Blocker: {f.Title}", crossed ? null : Trim(f.Description, 280),
                $"/project/{pid}/register?view=questions"));
            blockerItems.Add((owner, b));
        }
        var lanes = blockerItems.GroupBy(x => x.Owner)
            .Select(g => new BlockerLaneDto { Owner = g.Key, Items = g.Select(x => x.Item).OrderByDescending(x => x.DaysOpen).ToList(), OldestDays = g.Max(x => x.Item.DaysOpen) })
            .OrderByDescending(l => l.OldestDays).ToList();

        // ── Ahead: the next four weeks ─────────────────────────────────
        var ahead = new AheadDto { HorizonDays = AheadDays };
        var horizon = at.Date.AddDays(AheadDays);
        foreach (var d in deliverables.Where(d => !DeliverableDone(d, at, live) && d.DueDate is { } due && due.Date <= horizon).OrderBy(d => d.DueDate))
        {
            var st = d.StageId is { } sid && stageById.TryGetValue(sid, out var s) ? s : null;
            ahead.Items.Add(new AheadItemDto
            {
                Key = $"deliverable:{d.Id}",
                Kind = AheadKind.Deliverable,
                Title = d.Title ?? "Deliverable",
                DueBy = d.DueDate!.Value.Date,
                DaysAway = (int)(d.DueDate!.Value.Date - at.Date).TotalDays,
                StageName = st?.StageName,
                Phase = st?.Phase,
                SourceIds = { src.Add($"deliverable:{d.Id}", ReportSourceKind.Deliverable, null, $"Deliverable: {d.Title}", null, st is null ? null : $"/project/{pid}/stage/{st.Id}") }
            });
        }
        foreach (var c in dateRows.Where(c => IsHead(c) && !chainIds.Contains(c.Id) && !Delivered(c) && c.DueDate!.Value.Date >= at.Date && c.DueDate!.Value.Date <= horizon))
            ahead.Items.Add(new AheadItemDto
            {
                Key = $"date:{c.Id}",
                Kind = AheadKind.DatePromise,
                Title = c.Title ?? "A promised date",
                DueBy = c.DueDate!.Value.Date,
                DaysAway = (int)(c.DueDate!.Value.Date - at.Date).TotalDays,
                StageName = c.StageId is { } sid && stageById.TryGetValue(sid, out var st) ? st.StageName : null,
                SourceIds = CommitmentSources(c)
            });
        foreach (var c in commitments.Where(c => IsHead(c) && c.Kind == CommitmentKind.Choice && c.Maturity < CommitmentMaturity.Agreed))
        {
            var del = c.DeliverableId is { } did ? deliverables.FirstOrDefault(x => x.Id == did) : null;
            var st = c.StageId is { } sid && stageById.TryGetValue(sid, out var s) ? s : null;
            var due = c.DueDate ?? del?.DueDate;
            ahead.Items.Add(new AheadItemDto
            {
                Key = $"owed:{c.Id}",
                Kind = AheadKind.DecisionOwed,
                Title = c.Title ?? "A decision",
                DueBy = due?.Date,
                DaysAway = due is { } dd ? (int)(dd.Date - at.Date).TotalDays : null,
                OwedBy = c.OwedBySide,
                Consequence = del is not null ? $"Holds up {del.Title}" : st is not null ? $"Holds up {st.StageName}" : null,
                StageName = st?.StageName,
                Phase = st?.Phase,
                SourceIds = CommitmentSources(c)
            });
        }
        if (strip.Tone is ReportTone.Watch or ReportTone.Late && strip.ContractorDate is null && strip.Promised is not null)
            ahead.Items.Add(new AheadItemDto
            {
                Key = "owed:revised-date",
                Kind = AheadKind.DecisionOwed,
                Title = "A revised completion date",
                OwedBy = ProjectSide.Contractor,
                Consequence = $"The {strip.Promised:d MMM} date is {(strip.Tone == ReportTone.Late ? "past" : "at risk")} and no new date is on record",
                SourceIds = strip.Marks.Where(m => m.Kind == DeadlineMarkKind.Promised).SelectMany(m => m.SourceIds).ToList()
            });
        foreach (var s in stageReports.Where(s => s.Status == StageStatus.NotStarted)
                     .OrderBy(s => stageById[s.StageId].StartDate ?? DateTime.MaxValue).ThenBy(s => stageById[s.StageId].DisplayOrder).Take(3))
        {
            var start = stageById[s.StageId].StartDate;
            ahead.Items.Add(new AheadItemDto
            {
                Key = $"next:{s.StageId}",
                Kind = AheadKind.NextStage,
                Title = s.Name,
                DueBy = start?.Date,
                DaysAway = start is { } sd ? (int)(sd.Date - at.Date).TotalDays : null,
                Consequence = start is null ? "Not yet scheduled" : null,
                StageName = s.Name,
                Phase = s.Phase,
                SourceIds = { $"stage:{s.StageId}" }
            });
        }

        // ── Money — the ledger's own arithmetic on the rows as they stood ──
        var funding = (await Q(_context.tbl_FundingEntries).Where(f => f.ProjectId == pid).ToListAsync(ct))
            .Where(f => (f.PaymentDate ?? Local(f.DateTimeCreated)) <= at)
            .Select(f => AsAtFunding(f, at)).ToList();
        var claims = (await Q(_context.tbl_StageClaims).Where(c => c.ProjectId == pid).ToListAsync(ct))
            .Where(c => (c.ClaimedAt is { } ca ? Local(ca) : Local(c.DateTimeCreated)) <= at)
            .Select(c => AsAtClaim(c, at)).ToList();
        var stagesAsAt = stages.Select(s => AsAtStage(s, stageStates)).ToList();
        var ledger = StageLedgerMath.Build(pid, currency, stagesAsAt, funding, claims, variations);
        var money = new MoneyReportDto
        {
            Currency = currency,
            Rows = ledger.Rows.Select(r => new MoneyRowDto
            {
                StageId = r.StageId ?? "",
                StageName = r.StageName ?? "",
                Phase = r.Phase,
                Budget = r.Budget,
                Funded = r.Funded,
                Pending = r.PendingFunding,
                Claimed = r.Claimed,
                Cleared = r.Cleared,
                InHand = r.InHand,
                CarriedForward = r.CarriedForward
            }).ToList(),
            TotalBudget = ledger.TotalBudget,
            TotalFunded = ledger.TotalFunded,
            TotalPending = ledger.TotalPending,
            TotalClaimed = ledger.TotalClaimed,
            TotalCleared = ledger.TotalCleared,
            TotalInHand = ledger.TotalInHand
        };
        foreach (var f in funding.OrderBy(f => f.PaymentDate ?? Local(f.DateTimeCreated)))
        {
            var when = f.PaymentDate ?? Local(f.DateTimeCreated);
            var gap = f.Status switch
            {
                FundingStatus.Pending => "Receipt not confirmed in the record",
                FundingStatus.AmountQueried => "What arrived is in question",
                FundingStatus.Rejected => "The delivery side says nothing arrived",
                _ => null
            };
            money.Releases.Add(new ReleaseReportDto
            {
                Id = f.Id,
                StageName = f.StageId is { } sid && stageById.TryGetValue(sid, out var st) ? st.StageName : null,
                Amount = f.Amount,
                ReceivedAmount = f.ReceivedAmount,
                At = when,
                Status = f.Status,
                Gap = gap,
                SourceId = src.Add($"release:{f.Id}", ReportSourceKind.Release, when, $"Release of {currency} {f.Amount:#,0}", Trim(f.Notes, 280), $"/project/{pid}/money")
            });
        }
        var unconfirmed = funding.Where(f => f.Status == FundingStatus.Pending).ToList();
        if (unconfirmed.Count > 0) money.Gaps.Add($"{currency} {unconfirmed.Sum(f => f.Amount):#,0} sent in {unconfirmed.Count} release{(unconfirmed.Count == 1 ? "" : "s")}, receipt not confirmed in the record");
        foreach (var c in claims.Where(c => c.Status == ClaimStatus.Claimed))
            src.Add($"claim:{c.Id}", ReportSourceKind.Claim, c.ClaimedAt, $"Claim of {currency} {c.Amount:#,0}", Trim(c.Note, 280), $"/project/{pid}/money");
        if (claims.Any(c => c.Status == ClaimStatus.Claimed)) money.Gaps.Add($"{claims.Count(c => c.Status == ClaimStatus.Claimed)} claim(s) waiting to be cleared");

        // ── Footage, and the same view then and now ────────────────────
        var footage = windowFrames.GroupBy(f => f.StageId)
            .Select(g => new FootageGroupDto
            {
                StageName = g.Key is { } sid && stageById.TryGetValue(sid, out var st) ? st.StageName : null,
                Phase = g.Key is { } sid2 && stageById.TryGetValue(sid2, out var st2) ? st2.Phase : null,
                Total = g.Count(),
                Frames = g.OrderByDescending(f => f.At).Take(FramesPerGroup).Select(FrameDto).ToList()
            })
            .OrderBy(g => g.StageName is null ? 1 : 0).ThenBy(g => g.Phase).ToList();

        var changed = new List<ReportPairDto>();
        foreach (var s in leaves)
        {
            var after = windowFrames.Where(f => f.StageId == s.Id && !IsVideo(f.Mime)).OrderByDescending(f => f.At).ToList();
            var candidates = frames.Where(f => f.StageId == s.Id && !IsVideo(f.Mime) && f.At >= windowFrom.AddDays(-VantageLookbackDays)).ToList();
            var used = new HashSet<string>();
            foreach (var a in after)
            {
                if (used.Contains(a.ArtifactId) || a.Sha is null) continue;
                var fa = await _vantage.FingerprintAsync(a.Sha, a.Mime, a.ThumbPath, a.StoragePath, ct);
                if (fa is null) continue;
                (Frame F, double D)? best = null;
                foreach (var b in candidates.Where(b => b.At.Date < a.At.Date && !used.Contains(b.ArtifactId) && b.Sha is not null))
                {
                    var fb = await _vantage.FingerprintAsync(b.Sha!, b.Mime, b.ThumbPath, b.StoragePath, ct);
                    if (fb is null) continue;
                    var dist = VantageIndex.Distance(fa.Value, fb.Value);
                    if (dist <= VantageIndex.SameViewThreshold && (best is null || dist < best.Value.D)) best = (b, dist);
                }
                if (best is null) continue;
                used.Add(a.ArtifactId);
                used.Add(best.Value.F.ArtifactId);
                changed.Add(new ReportPairDto
                {
                    StageName = s.StageName ?? "",
                    Phase = s.Phase,
                    Before = FrameDto(best.Value.F),
                    After = FrameDto(a),
                    DaysApart = (int)(a.At.Date - best.Value.F.At.Date).TotalDays,
                    Basis = "same view"
                });
                if (changed.Count(p => p.StageName == s.StageName) >= 2) break;
            }
        }

        var hero = windowFrames.Where(f => !IsVideo(f.Mime)).OrderByDescending(f => f.At).FirstOrDefault()
                   ?? frames.Where(f => !IsVideo(f.Mime)).OrderByDescending(f => f.At).FirstOrDefault();

        // ── Headlines ──────────────────────────────────────────────────
        var owedCount = ahead.Items.Count(i => i.Kind == AheadKind.DecisionOwed && i.OwedBy == ProjectSide.Client);
        var openBlockers = blockerItems.Count;
        var headlines = new List<HeadlineCardDto>
        {
            new()
            {
                Key = "built",
                Value = $"{progress.OverallPercent:0.#}%",
                StateWord = progress.Stalled > 0 ? $"{progress.Stalled} stalled" : "built",
                Tone = progress.Stalled > 0 ? ReportTone.Watch : ReportTone.Neutral,
                Line = $"weighted by {progress.Weighting} · {progress.Completed} of {progress.StageCount} stages complete"
            },
            new()
            {
                Key = "deadline",
                Value = strip.Promised is { } pr ? pr.ToString("d MMM", CultureInfo.InvariantCulture) : "—",
                StateWord = strip.StateWord,
                Tone = strip.Tone,
                Line = $"{strip.LapsedCount} lapsed promise{(strip.LapsedCount == 1 ? "" : "s")}" +
                       (strip.Forecast is { } fc ? $" · pace {fc:d MMM}" : " · no pace forecast")
            },
            new()
            {
                Key = "money",
                Value = Short(money.TotalFunded),
                Unit = currency,
                StateWord = "acknowledged",
                Tone = money.TotalPending > 0 ? ReportTone.Watch : ReportTone.Neutral,
                Line = money.TotalPending > 0 ? $"{Short(money.TotalPending)} sent, not yet acknowledged" : $"{Short(money.TotalCleared)} cleared against claims"
            },
            new()
            {
                Key = "owed",
                Value = owedCount.ToString(CultureInfo.InvariantCulture),
                StateWord = $"owed · {openBlockers} stuck",
                Tone = openBlockers > 0 ? ReportTone.Watch : ReportTone.Neutral,
                Line = lanes.Count > 0 ? $"oldest blocker {lanes[0].OldestDays} days, {lanes[0].Owner}" : "no open blocker"
            }
        };

        // ── Since the last report ──────────────────────────────────────
        ReportChangesDto? sinceLast = null;
        if (previous?.SnapshotJson is { } prevJson)
        {
            var prev = JsonSerializer.Deserialize<WorksReportDto>(prevJson, SnapshotJson);
            if (prev is not null)
            {
                var prevStages = prev.StageGroups.SelectMany(g => g.Stages).ToDictionary(s => s.StageId);
                var prevBlockers = prev.Blockers.SelectMany(l => l.Items).ToDictionary(b => b.Id);
                sinceLast = new ReportChangesDto
                {
                    PreviousReportId = previous.Id,
                    PreviousAsAt = previous.AsAt,
                    OverallFrom = prev.Progress.OverallPercent,
                    OverallTo = progress.OverallPercent,
                    StageMoves = stageReports.Where(s => !prevStages.TryGetValue(s.StageId, out var p) || p.Percent != s.Percent)
                        .Select(s => new StageMoveDto { StageName = s.Name, From = prevStages.GetValueOrDefault(s.StageId)?.Percent, To = s.Percent }).ToList(),
                    NewDecisions = decisions.Where(d => prev.Decisions.All(p => p.Id != d.Id)).Select(d => d.Title).ToList(),
                    NewVariations = variationReports.Where(v => prev.Variations.All(p => p.Id != v.Id)).Select(v => v.Title).ToList(),
                    BlockersOpened = blockerItems.Where(b => !prevBlockers.ContainsKey(b.Item.Id)).Select(b => b.Item.Title).ToList(),
                    BlockersCleared = prevBlockers.Values.Where(p => blockerItems.All(b => b.Item.Id != p.Id)).Select(p => p.Title).ToList(),
                    LapsedFrom = prev.Deadline.LapsedCount,
                    LapsedTo = strip.LapsedCount,
                    ForecastFrom = prev.Deadline.Forecast,
                    ForecastTo = strip.Forecast,
                    FundedDelta = prev.Money is { } pm ? money.TotalFunded - pm.TotalFunded : null
                };
            }
        }

        // ── Words: the requests, each with its facts and a template ────
        var answer = new NarrativeRequest { TargetId = "answer", Kind = NarrativeTargetKind.Answer };
        var promisedSrc = strip.Marks.Where(m => m.Kind is DeadlineMarkKind.Promised or DeadlineMarkKind.Set).SelectMany(m => m.SourceIds).Distinct().ToArray();
        if (strip.Promised is { } promised)
        {
            answer.Say(strip.Tone switch
            {
                ReportTone.Good when allDone => $"Complete; the date on record was {promised:d MMM}.",
                ReportTone.Good => $"On course for {promised:d MMM}.",
                ReportTone.Late => $"Not complete by {promised:d MMM}; {open.Count} stage{(open.Count == 1 ? " is" : "s are")} still open.",
                _ => $"Not on course for {promised:d MMM}."
            }, promisedSrc);
            var forecastMark = strip.Marks.FirstOrDefault(m => m.Kind == DeadlineMarkKind.Forecast);
            if (!allDone && strip.Forecast is { } fcast && forecastMark is not null)
                answer.Say(fcast > promised
                    ? $"{Cap(strip.ForecastBasis)} points to {fcast:d MMM}, {(int)(fcast - promised).TotalDays} days after it."
                    : $"{Cap(strip.ForecastBasis)} points to {fcast:d MMM}.", forecastMark.SourceIds.ToArray());
            else if (!allDone && stageReports.FirstOrDefault(s => s.IsStalled) is { } stalled)
                answer.Say($"{stalled.Name} has read {stalled.Percent:0.#}% since {stalled.StalledSince:d MMM}, so there is no pace to forecast from.", stalled.Readings.TakeLast(1).Select(r => r.SourceId).ToArray());
            if (!allDone && strip.Tone != ReportTone.Good)
            {
                var cMark = strip.Marks.FirstOrDefault(m => m.Kind == DeadlineMarkKind.ContractorDate);
                if (strip.ContractorDate is { } cd && cMark is not null) answer.Say($"The latest date given is {cd:d MMM}.", cMark.SourceIds.ToArray());
                else answer.Say("The contractor has not yet given a revised date.", promisedSrc);
            }
        }
        else answer.Say("No completion date is on record for this project.", StageSourceOrProject());
        requests.Add(answer);

        string StageSourceOrProject() => leaves.Count > 0 ? $"stage:{leaves[0].Id}" : src.Add($"project:{pid}", ReportSourceKind.Stage, null, "Project record", null, $"/project/{pid}");

        var cover = new NarrativeRequest { TargetId = "cover", Kind = NarrativeTargetKind.Cover };
        var allStageSrc = stageReports.Select(s => $"stage:{s.StageId}").ToArray();
        if (leaves.Count > 0)
        {
            cover.Say($"{progress.OverallPercent:0.#}% built, weighted by {progress.Weighting}: {progress.Completed} of {progress.StageCount} stages complete, {progress.InProgress} in progress.", allStageSrc);
            var stalledStages = stageReports.Where(s => s.IsStalled).ToList();
            if (stalledStages.Count == 1)
                cover.Say($"{stalledStages[0].Name} has read {stalledStages[0].Percent:0.#}% since {stalledStages[0].StalledSince:d MMM}.", stalledStages[0].Readings.TakeLast(1).Select(r => r.SourceId).Append($"stage:{stalledStages[0].StageId}").ToArray());
            else if (stalledStages.Count > 1)
                cover.Say($"{stalledStages[0].Name} and {stalledStages.Count - 1} other stage{(stalledStages.Count == 2 ? "" : "s")} have not moved for {_stallDays} days or more.", stalledStages.Select(s => $"stage:{s.StageId}").ToArray());
            else cover.Say($"No stage has gone {_stallDays} days without moving.", allStageSrc);
        }
        else cover.Say("No stages are set up on this project yet.", StageSourceOrProject());
        if (lanes.Count > 0)
        {
            var oldest = lanes[0].Items[0];
            cover.Say($"{openBlockers} blocker{(openBlockers == 1 ? " is" : "s are")} open; the oldest, {oldest.Title}, has waited {oldest.DaysOpen} days on {lanes[0].Owner}.", oldest.SourceIds.ToArray());
        }
        else if (cover.Template.Count < 3) cover.Say("No blocker is open.", StageSourceOrProject());
        requests.Add(cover);

        foreach (var s in stageReports)
        {
            var r = new NarrativeRequest { TargetId = $"stage:{s.StageId}", Kind = NarrativeTargetKind.Stage };
            var inWindow = s.Readings.Where(x => x.At >= windowFrom).ToList();
            var beforeWindow = s.Readings.LastOrDefault(x => x.At < windowFrom);
            if (s.Status == StageStatus.Completed)
                r.Say(stageById[s.StageId].ActualEndDate is { } ae ? $"Complete on {ae:d MMM}." : "Complete.", s.SourceIds.Take(1).Concat(s.Readings.TakeLast(1).Select(x => x.SourceId)).ToArray());
            else if (inWindow.Count > 0 && beforeWindow is not null && inWindow[^1].Percent != beforeWindow.Percent)
                r.Say($"{inWindow[^1].Percent:0.#}% on {inWindow[^1].At:d MMM}, from {beforeWindow.Percent:0.#}% on {beforeWindow.At:d MMM}.", inWindow[^1].SourceId, beforeWindow.SourceId);
            else if (inWindow.Count > 0)
                r.Say($"{inWindow[^1].Percent:0.#}% on {inWindow[^1].At:d MMM}.", inWindow[^1].SourceId);
            else if (s.Readings.Count > 0)
                r.Say($"{s.Readings[^1].Percent:0.#}% since {s.Readings[^1].At:d MMM}; no new reading in this window.", s.Readings[^1].SourceId);
            else if (s.Status == StageStatus.NotStarted)
                r.Say(stageById[s.StageId].StartDate is { } sd ? $"Not started; planned to start {sd:d MMM}." : "Not started.", $"stage:{s.StageId}");
            else r.Say("No progress reading on record.", $"stage:{s.StageId}");

            if (s.Status != StageStatus.Completed)
            {
                if (s.IsStalled)
                    r.Say(s.StillReportedAt is { } sra
                        ? $"Unchanged for {s.StalledDays} days, though the thread still reports work on it on {sra:d MMM}."
                        : $"Unchanged for {s.StalledDays} days.", s.SourceIds.Where(x => x.StartsWith("msg:")).DefaultIfEmpty($"stage:{s.StageId}").ToArray());
                else if (s.DeliverablesTotal > 0)
                    r.Say($"{s.DeliverablesDone} of {s.DeliverablesTotal} deliverables done.", $"stage:{s.StageId}");
                else if (s.Forecast is { } sf && s.ForecastBasis is not null)
                    r.Say($"{Cap(s.ForecastBasis)} points to 100% on {sf:d MMM}.", s.Readings.TakeLast(2).Select(x => x.SourceId).ToArray());
            }

            // The words of the record the model may draw on, never the whole thread.
            foreach (var id in s.SourceIds.Concat(s.Readings.Select(x => x.SourceId)).Distinct())
            {
                if (src.Get(id)?.Excerpt is { } ex) r.Snippets.Add((id, ex));
                r.SourceIds.Add(id);
            }
            foreach (var m in messages.Where(m => m.SentAt >= windowFrom && stageOfMessage.GetValueOrDefault(m.Id) == s.StageId).TakeLast(3))
            {
                var id = MessageSource(m);
                r.SourceIds.Add(id);
                r.Snippets.Add((id, Trim(m.Body, 280) ?? ""));
            }
            requests.Add(r);
        }

        foreach (var d in decisions)
        {
            var r = new NarrativeRequest { TargetId = $"decision:{d.Id}", Kind = NarrativeTargetKind.Decision };
            r.Say($"{d.Title}, agreed {d.AgreedAt:d MMM}{(d.AgreedWith is null ? "" : $" with {d.AgreedWith}")}.", d.SourceIds.ToArray());
            foreach (var id in d.SourceIds) if (src.Get(id)?.Excerpt is { } ex) r.Snippets.Add((id, ex));
            requests.Add(r);
        }
        foreach (var v in variationReports)
        {
            var r = new NarrativeRequest { TargetId = $"variation:{v.Id}", Kind = NarrativeTargetKind.Variation };
            r.Say($"{v.Title}: {(v.CostDelta is null ? "not costed" : "costed")}, {(v.Status == VariationStatus.Approved && v.ApprovedAt is { } apd ? $"approved {apd:d MMM}" : "no approval on record")}.", v.SourceIds.ToArray());
            foreach (var id in v.SourceIds) if (src.Get(id)?.Excerpt is { } ex) r.Snippets.Add((id, ex));
            requests.Add(r);
        }
        foreach (var (owner, b) in blockerItems)
        {
            var r = new NarrativeRequest { TargetId = $"blocker:{b.Id}", Kind = NarrativeTargetKind.Blocker };
            r.Say($"Open {b.DaysOpen} days since {b.Since:d MMM}, waiting on {owner}.", b.SourceIds.ToArray());
            if (b.Detail is not null) r.Snippets.Add((b.SourceIds[0], b.Detail));
            requests.Add(r);
        }

        // ── The page ───────────────────────────────────────────────────
        var dto = new WorksReportDto
        {
            ProjectId = pid,
            ProjectName = project.ProjectName ?? "Project",
            ParentProjectName = project.ParentProject?.ProjectName,
            Currency = currency,
            AsAt = at,
            WindowFrom = windowFrom,
            WindowLabel = previous is null ? $"the 7 days to {at:d MMM}" : $"since the last report, {previous.AsAt:d MMM}",
            Audience = audience,
            MediatorName = mediatorName,
            Cover = new ReportCoverDto
            {
                HeroArtifactId = hero?.ArtifactId,
                Title = project.ProjectName ?? "Project",
                Subtitle = $"Works report · as at {at:d MMM yyyy}"
            },
            Headlines = headlines,
            Deadline = strip,
            Progress = progress,
            StageGroups = stageReports.GroupBy(s => s.Phase)
                .OrderBy(g => Array.IndexOf(StageCatalogue.Groups, g.Key) is var i && i < 0 ? 99 : i)
                .Select(g => new StageGroupReportDto { Phase = g.Key, Label = StageCatalogue.GroupName(g.Key), Stages = g.OrderBy(s => stageById[s.StageId].DisplayOrder).ToList() })
                .ToList(),
            Changed = changed,
            SinceLast = sinceLast,
            Decisions = decisions,
            Variations = variationReports,
            ScopeGaps = scopeGaps,
            Blockers = lanes,
            Ahead = new AheadDto { HorizonDays = AheadDays, Items = ahead.Items.OrderBy(i => i.DueBy ?? DateTime.MaxValue).ThenBy(i => i.Kind).ToList() },
            Money = money,
            Footage = footage,
            Sources = src.Numbered()
        };
        return (dto, requests);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Words
    // ═════════════════════════════════════════════════════════════════════

    private async Task<NarrativeDto> NarrateAsync(List<NarrativeRequest> requests, bool useModel, bool projectConsents, CancellationToken ct)
    {
        var template = _narrators.OfType<TemplateReportNarrator>().FirstOrDefault() ?? new TemplateReportNarrator();
        var model = useModel && projectConsents ? _narrators.FirstOrDefault(n => n is not TemplateReportNarrator && n.IsAvailable) : null;
        var result = new NarrativeDto { DraftingEnabled = projectConsents, Engine = template.Engine };

        var drafted = model is null ? new List<NarrativeTargetDto>() : await model.DraftAsync(requests, ct);
        var templated = await template.DraftAsync(requests, ct);

        foreach (var req in requests)
        {
            var reasons = new List<string>();
            var accepted = model is null ? new() : NarrativeValidator.Accept(req, drafted.FirstOrDefault(d => d.TargetId == req.TargetId), reasons);
            if (accepted.Count > 0)
            {
                result.Accepted += accepted.Count;
                result.Targets.Add(new NarrativeTargetDto { TargetId = req.TargetId, Templated = false, Sentences = accepted });
                continue;
            }
            if (model is not null)
            {
                result.Rejected++;
                result.RejectionReasons.AddRange(reasons.Take(3));
            }
            result.Targets.Add(templated.First(t => t.TargetId == req.TargetId));
        }
        if (model is not null) result.Engine = result.Accepted > 0 ? $"{model.Engine}, templated where refused" : $"{template.Engine} (model drafts refused or unavailable)";
        return result;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Redaction for the reader — the snapshot is frozen, the reader is not
    // ═════════════════════════════════════════════════════════════════════

    private static void Redact(WorksReportDto dto, ProjectAccess a)
    {
        dto.CanIssue = CanIssueReport(a);
        if (a.CanSeeMoney) return;

        // Absent, not refused (§2.7): no money section, no money card, no release chips.
        dto.Money = null;
        dto.Headlines.RemoveAll(h => h.Key == "money");
        if (dto.SinceLast is not null) dto.SinceLast.FundedDelta = null;
        foreach (var d in dto.Decisions.Where(d => d.Amount is not null)) { d.Amount = null; d.AmountHidden = true; }
        foreach (var v in dto.Variations.Where(v => v.CostDelta is not null)) { v.CostDelta = null; v.AmountHidden = true; }
        var money = dto.Sources.Where(s => s.Kind is ReportSourceKind.Release or ReportSourceKind.Claim).Select(s => s.Id).ToHashSet();
        dto.Sources.RemoveAll(s => money.Contains(s.Id));
        foreach (var t in dto.Narrative.Targets)
            foreach (var s in t.Sentences) s.SourceIds.RemoveAll(money.Contains);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Helpers
    // ═════════════════════════════════════════════════════════════════════

    private sealed record Reading(tbl_ProgressReading Row, DateTime At);

    /// <summary>
    /// Readings per stage, oldest first, in local time. A reading read from the
    /// thread with no stage named is matched by its words; a client-side report
    /// reads only what the client side may (the P7 rule).
    /// </summary>
    private static Dictionary<string, List<Reading>> ReadingsByStage(List<tbl_ProgressReading> readings, List<StageRef> refs, ProjectSide audience)
    {
        var result = new Dictionary<string, List<Reading>>();
        foreach (var r in readings)
        {
            if (audience == ProjectSide.Client && r.SourceSide is not null && r.SourceSide != ProjectSide.Client) continue;
            var sid = r.StageId ?? StageMatcher.Match(r.Subject, refs);
            if (sid is null) continue;
            // Stage and capture readings are stamped by the server in UTC; a thread reading carries the time it was said.
            var at = r.SourceKind == ProgressReadingSource.Ingested ? r.ObservedAt : Local(r.ObservedAt);
            (result.TryGetValue(sid, out var list) ? list : result[sid] = new()).Add(new Reading(r, at));
        }
        foreach (var list in result.Values) list.Sort((x, y) => x.At.CompareTo(y.At));
        return result;
    }

    private static List<tbl_Stage> Leaves(List<tbl_Stage> stages)
    {
        var parents = stages.Where(s => s.ParentStageId != null).Select(s => s.ParentStageId!).ToHashSet();
        return stages.Where(s => !parents.Contains(s.Id)).OrderBy(s => s.DisplayOrder).ToList();
    }

    /// <summary>Where a stage stood at a moment: its latest reading then, and the status that reading implies.</summary>
    private static StageState StateAt(tbl_Stage s, List<Reading> rs, DateTime at, bool live = false)
    {
        var latest = rs.LastOrDefault(r => r.At <= at);
        var pct = latest?.Row.Percent ?? (live ? s.CompletionPercentage : null);
        var derived = (s.ActualEndDate is { } ae && ae <= at) || pct >= 100 ? StageStatus.Completed
            : pct > 0 || (s.StartDate is { } sd && sd <= at) ? StageStatus.InProgress
            : StageStatus.NotStarted;
        var status = live && s.Status > derived ? s.Status : derived;
        return new StageState(status == StageStatus.Completed ? pct ?? 100 : pct, latest?.At, status);
    }

    private static tbl_Stage AsAtStage(tbl_Stage s, Dictionary<string, StageState> states) => new()
    {
        Id = s.Id,
        ProjectId = s.ProjectId,
        StageName = s.StageName,
        ParentStageId = s.ParentStageId,
        DisplayOrder = s.DisplayOrder,
        Phase = s.Phase,
        BudgetAmount = s.BudgetAmount,
        Status = states.TryGetValue(s.Id, out var st) ? st.Status : s.Status
    };

    /// <summary>A release as it stood: acknowledged only if it was acknowledged by then.</summary>
    private static tbl_FundingEntry AsAtFunding(tbl_FundingEntry f, DateTime at)
    {
        if (f.Status == FundingStatus.Pending || f.ConfirmationDate is not { } c || Local(c) <= at) return f;
        return new tbl_FundingEntry
        {
            Id = f.Id, ProjectId = f.ProjectId, StageId = f.StageId, Amount = f.Amount, PaymentDate = f.PaymentDate,
            PaidById = f.PaidById, Notes = f.Notes, DateTimeCreated = f.DateTimeCreated, Status = FundingStatus.Pending
        };
    }

    private static tbl_StageClaim AsAtClaim(tbl_StageClaim c, DateTime at)
    {
        if (c.Status == ClaimStatus.Claimed || c.ClearedAt is not { } cl || Local(cl) <= at) return c;
        return new tbl_StageClaim
        {
            Id = c.Id, ProjectId = c.ProjectId, StageId = c.StageId, Amount = c.Amount, ClaimedAt = c.ClaimedAt,
            Note = c.Note, DateTimeCreated = c.DateTimeCreated, Status = ClaimStatus.Claimed
        };
    }

    private static bool DeliverableDone(tbl_Deliverable d, DateTime at, bool live) =>
        d.Status == DeliverableStatus.Done && (d.CompletedAt is not { } c ? live : Local(c) <= at);

    /// <summary>When it was said: the agreed moment as recorded, else when it was written down.</summary>
    private static DateTime AgreedOf(tbl_Commitment c) => c.AgreedAt ?? Local(c.DateTimeCreated);

    /// <summary>
    /// The project's completion date, as opposed to a short promise about one
    /// piece of work: worded as completion, and naming no stage or deliverable.
    /// "Guest wing plaster complete this week" is a stage's promise, not the deadline.
    /// </summary>
    private static bool IsProjectDeadline(tbl_Commitment c, List<StageRef> refs) =>
        c.Kind == CommitmentKind.Date && c.DeliverableId == null && c.DueDate != null
        && CompletionTitle.IsMatch(c.Title ?? "") && StageMatcher.Match(c.Title, refs) is null;

    private static List<tbl_Commitment> Chain(tbl_Commitment head, List<tbl_Commitment> all)
    {
        var chain = new List<tbl_Commitment> { head };
        var cur = head;
        while (cur.SupersedesId is { } prev && all.FirstOrDefault(c => c.Id == prev) is { } p && chain.Count < 50)
        {
            chain.Add(p);
            cur = p;
        }
        return chain;
    }

    private static bool IsVideo(string? mime) => VideoPosterJob.IsVideo(mime);
    private static bool IsPicture(string? mime) => mime is not null && (mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || IsVideo(mime));

    private static DateTime Local(DateTime? utc) => utc is { } u
        ? DateTime.SpecifyKind(DateTime.SpecifyKind(u, DateTimeKind.Utc).ToLocalTime(), DateTimeKind.Unspecified)
        : DateTime.MinValue;

    private static string MemberName(tbl_ProjectMember m) =>
        (m.User is { } u ? $"{u.FirstName} {u.LastName}".Trim() : null) is { Length: > 0 } n ? n : m.PartyName ?? m.Title ?? "Unnamed";

    private static string? FullName(AppUser? u) => u is null ? null : $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } n ? n : u.Email;

    private static string Short(decimal amount) => amount switch
    {
        >= 1_000_000_000 => $"{amount / 1_000_000_000m:0.##}B",
        >= 1_000_000 => $"{amount / 1_000_000m:0.##}M",
        >= 1_000 => $"{amount / 1_000m:0.##}K",
        _ => amount.ToString("0", CultureInfo.InvariantCulture)
    };

    private static string Cap(string? s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s[1..];

    private static string? Trim(string? text, int max)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var clean = Regex.Replace(text.Trim(), @"\s+", " ");
        return clean.Length <= max ? clean : clean[..(max - 1)].TrimEnd() + "…";
    }

    private static ServiceResult<T> NotFound<T>() => ServiceResult<T>.Failure(new NotFoundException("Project not found."));
}
