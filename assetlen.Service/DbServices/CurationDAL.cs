using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.FileProcessingServices.Push;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <summary>
/// Curation by exception (assetlen.md §5, tier 3). The mediator keeps or drops
/// individual frames; at the cutoff the day's selection crosses whether or not
/// he touched it. Nothing here gates the brief itself — it assembles from what
/// already arrives with the contractor silent (Law 0). This only decides which
/// captured frames join it.
/// </summary>
public interface ICurationDAL
{
    Task<ServiceResult<CurationDraftDto>> GetDraft(string projectId, DateTime? day, string userId, CancellationToken ct = default);
    Task<ServiceResult<CurationDraftDto>> Mark(CurationMarkDto dto, string userId, CancellationToken ct = default);
    Task<ServiceResult<CurationPublishResultDto>> Publish(string projectId, DateTime? day, string userId, CancellationToken ct = default);

    /// <summary>Publish every project whose cutoff has passed and whose day has not crossed. Nobody signed in.</summary>
    Task<List<CurationPublishResultDto>> RunCutoffAsync(DateTime? now = null, string? projectId = null, CancellationToken ct = default);
}

/// <summary>
/// Which captured frames cross when nobody chooses: up to three per piece of work
/// — the first of the day, the latest, and one between — counting any already
/// shown. Seventeen frames in a row read as "nothing much changed"
/// (whatsapp-evidence.md F1); three of eighteen do not. A kept frame always goes;
/// a dropped one never does.
/// </summary>
public static class CurationRule
{
    public const int PerBlock = 3;

    public sealed record Candidate(string ImageId, string BlockKey, DateTime At, int Order, FrameCuration Curation, Channel Channel);

    public static Dictionary<string, string> Select(IEnumerable<Candidate> frames)
    {
        var chosen = new Dictionary<string, string>();
        foreach (var block in frames.GroupBy(f => f.BlockKey))
        {
            var ordered = block.OrderBy(f => f.At).ThenBy(f => f.Order).ToList();
            foreach (var f in ordered.Where(f => f.Curation == FrameCuration.Promoted && f.Channel != Channel.Client))
                chosen[f.ImageId] = "Kept by the mediator";

            var room = PerBlock - ordered.Count(f => f.Channel == Channel.Client) - ordered.Count(f => chosen.ContainsKey(f.ImageId));
            var open = ordered.Where(f => f.Curation == FrameCuration.Auto && f.Channel != Channel.Client).ToList();
            if (room <= 0 || open.Count == 0) continue;

            var picks = new List<(Candidate F, string Why)>
            {
                (open[^1], "The latest on this work"),
                (open[0], "The first of the day on this work"),
                (open[open.Count / 2], "Between the two")
            };
            foreach (var (f, why) in picks)
            {
                if (room == 0) break;
                if (chosen.ContainsKey(f.ImageId)) continue;
                chosen[f.ImageId] = why;
                room--;
            }
        }
        return chosen;
    }
}

public sealed class CurationDAL : ICurationDAL
{
    private readonly AssetlenDbContext _context;
    private readonly IProjectAccessService _access;
    private readonly IFrameExposure _exposure;
    private readonly INotifier _notifier;
    private readonly ILogger<CurationDAL> _logger;
    private readonly int _cutoffHour;
    private const int SettleMinutes = 10;

    public CurationDAL(AssetlenDbContext context, IProjectAccessService access, IFrameExposure exposure,
        INotifier notifier, IConfiguration config, ILogger<CurationDAL> logger)
    {
        _context = context;
        _access = access;
        _exposure = exposure;
        _notifier = notifier;
        _logger = logger;
        _cutoffHour = int.TryParse(config["Brief:CutoffHour"], out var h) && h is >= 0 and <= 23 ? h : 20;
    }

    private IQueryable<T> Q<T>(DbSet<T> set) where T : BaseEntity =>
        set.IgnoreQueryFilters().AsNoTracking().Where(x => x.IsDeleted != true);

    // ─── Draft ───────────────────────────────────────────────────────────

    public async Task<ServiceResult<CurationDraftDto>> GetDraft(string projectId, DateTime? day, string userId, CancellationToken ct = default)
    {
        try
        {
            var access = await _access.ResolveAsync(projectId, userId, ct);
            if (!(access.CanSeeSiteLog && access.CanExposeToClient))
                return ServiceResult<CurationDraftDto>.Failure(new NotFoundException("Project not found."));
            return ServiceResult<CurationDraftDto>.Success(await BuildDraftAsync(projectId, (day ?? DateTime.Now).Date, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building curation draft for {ProjectId}", projectId);
            return ServiceResult<CurationDraftDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    private sealed record Loaded(List<(tbl_ProgressImage Image, tbl_ProgressUpdate Entry)> Frames, Dictionary<string, string> Chosen,
        Dictionary<string, (string Title, string? Stage, StageGroup? Phase)> Blocks);

    private async Task<Loaded> LoadDayAsync(string projectId, DateTime day, CancellationToken ct)
    {
        var fromUtc = day.ToUniversalTime();
        var toUtc = day.AddDays(1).ToUniversalTime();
        var entries = await Q(_context.tbl_ProgressUpdates)
            .Include(u => u.Images)
            .Include(u => u.Stage)
            .Include(u => u.Deliverable)
            .Include(u => u.CreatedBy)
            .Where(u => u.ProjectId == projectId && u.DateTimeCreated >= fromUtc && u.DateTimeCreated < toUtc)
            .ToListAsync(ct);

        var frames = entries
            .SelectMany(e => e.Images.Where(i => i.IsDeleted != true && i.ArtifactId != null).Select(i => (Image: i, Entry: e)))
            .ToList();

        var blocks = new Dictionary<string, (string, string?, StageGroup?)>();
        string KeyOf(tbl_ProgressUpdate e)
        {
            var key = e.DeliverableId ?? e.StageId ?? "site";
            if (!blocks.ContainsKey(key))
                blocks[key] = (e.Deliverable?.Title ?? e.Stage?.StageName ?? "On site", e.Deliverable is null ? null : e.Stage?.StageName, e.Stage?.Phase);
            return key;
        }

        var chosen = CurationRule.Select(frames.Select(x => new CurationRule.Candidate(
            x.Image.Id!, KeyOf(x.Entry), x.Entry.DateTimeCreated ?? DateTime.UtcNow, x.Image.DisplayOrder, x.Image.Curation, x.Image.Channel)));
        return new Loaded(frames, chosen, blocks);
    }

    private async Task<CurationDraftDto> BuildDraftAsync(string projectId, DateTime day, CancellationToken ct)
    {
        var loaded = await LoadDayAsync(projectId, day, ct);
        var publication = await Q(_context.tbl_BriefPublications).Include(p => p.PublishedBy)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Day == day, ct);
        var face = await _exposure.AccountableFaceAsync(projectId, ct);

        var dto = new CurationDraftDto
        {
            ProjectId = projectId,
            Day = day,
            CutoffHour = _cutoffHour,
            PublishedAt = publication?.PublishedAt,
            PublishedBy = publication?.Trigger,
            PublishedByName = publication?.PublishedBy is { } u ? $"{u.FirstName} {u.LastName}".Trim() : null,
            AccountableName = face.Name,
            FrameTotal = loaded.Frames.Count,
            AlreadyShown = loaded.Frames.Count(f => f.Image.Channel == Channel.Client),
            Dropped = loaded.Frames.Count(f => f.Image.Curation == FrameCuration.Dropped),
            WillPublish = loaded.Chosen.Count
        };

        foreach (var group in loaded.Frames.GroupBy(f => f.Entry.DeliverableId ?? f.Entry.StageId ?? "site"))
        {
            var (title, stage, phase) = loaded.Blocks[group.Key];
            dto.Blocks.Add(new CurationBlockDto
            {
                Key = group.Key,
                Title = title,
                StageName = stage,
                Phase = phase,
                Frames = group.OrderBy(f => f.Entry.DateTimeCreated).ThenBy(f => f.Image.DisplayOrder).Select(f => new CurationFrameDto
                {
                    ImageId = f.Image.Id,
                    ProgressUpdateId = f.Entry.Id,
                    ArtifactId = f.Image.ArtifactId,
                    ThumbnailUrl = $"/api/Artifacts/{f.Image.ArtifactId}/thumbnail",
                    Caption = f.Image.Caption,
                    EntryNote = f.Entry.Description,
                    CapturedByName = f.Entry.CreatedBy is { } a ? $"{a.FirstName} {a.LastName}".Trim() : null,
                    At = DateTime.SpecifyKind(f.Entry.DateTimeCreated ?? DateTime.UtcNow, DateTimeKind.Utc).ToLocalTime(),
                    Curation = f.Image.Curation,
                    Channel = f.Image.Channel,
                    WillPublish = loaded.Chosen.ContainsKey(f.Image.Id!),
                    Reason = loaded.Chosen.GetValueOrDefault(f.Image.Id!)
                }).ToList()
            });
        }
        dto.Blocks = dto.Blocks.OrderByDescending(b => b.Frames.Max(f => f.At)).ToList();
        return dto;
    }

    // ─── Keep / drop ─────────────────────────────────────────────────────

    public async Task<ServiceResult<CurationDraftDto>> Mark(CurationMarkDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var ids = dto.ImageIds.Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();
            if (ids.Count == 0) return ServiceResult<CurationDraftDto>.Failure(new BadRequestException("No frames named."));

            var frames = await _context.tbl_ProgressImages.Include(i => i.ProgressUpdate)
                .Where(i => ids.Contains(i.Id)).ToListAsync(ct);
            var projects = frames.Select(f => f.ProgressUpdate?.ProjectId).Distinct().ToList();
            if (frames.Count != ids.Count || projects.Count != 1 || projects[0] is null)
                return ServiceResult<CurationDraftDto>.Failure(new NotFoundException("Frames not found."));

            var projectId = projects[0]!;
            var access = await _access.ResolveAsync(projectId, userId, ct);
            if (!access.CanSeeSiteLog) return ServiceResult<CurationDraftDto>.Failure(new NotFoundException("Frames not found."));
            if (!access.CanExposeToClient)
                return ServiceResult<CurationDraftDto>.Failure(new ForbiddenException("Only the mediator decides what crosses to the client."));

            var now = DateTime.UtcNow;
            foreach (var f in frames)
            {
                f.Curation = dto.Curation;
                f.CuratedById = userId;
                f.CuratedAt = now;
            }
            await _context.SaveChangesAsync(ct);

            // Dropping a frame that already crossed pulls it back: "I'd have dropped that one" means it goes.
            if (dto.Curation == FrameCuration.Dropped)
            {
                var shown = frames.Where(f => f.Channel == Channel.Client).ToList();
                if (shown.Count > 0) await _exposure.WithdrawAsync(shown, ct);
            }

            var day = (frames[0].ProgressUpdate!.DateTimeCreated ?? DateTime.UtcNow);
            return ServiceResult<CurationDraftDto>.Success(await BuildDraftAsync(projectId,
                DateTime.SpecifyKind(day, DateTimeKind.Utc).ToLocalTime().Date, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking frames");
            return ServiceResult<CurationDraftDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Publish ─────────────────────────────────────────────────────────

    public async Task<ServiceResult<CurationPublishResultDto>> Publish(string projectId, DateTime? day, string userId, CancellationToken ct = default)
    {
        try
        {
            var access = await _access.ResolveAsync(projectId, userId, ct);
            if (!access.CanSeeSiteLog) return ServiceResult<CurationPublishResultDto>.Failure(new NotFoundException("Project not found."));
            if (!access.CanExposeToClient)
                return ServiceResult<CurationPublishResultDto>.Failure(new ForbiddenException("Only the mediator publishes to the client."));

            var result = await PublishCoreAsync(projectId, (day ?? DateTime.Now).Date, userId, BriefPublishTrigger.Mediator, ct);
            return ServiceResult<CurationPublishResultDto>.Success(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing {ProjectId}", projectId);
            return ServiceResult<CurationPublishResultDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Sends the day's selection across. Every frame goes in the accountable face's
    /// name (§10.1); on a cutoff with no mediator appointed nothing crosses at all —
    /// there is no one for it to cross as, so the Site Diary stays closed (fail-closed).
    /// </summary>
    private async Task<CurationPublishResultDto?> PublishCoreAsync(string projectId, DateTime day, string? userId,
        BriefPublishTrigger trigger, CancellationToken ct)
    {
        var face = await _exposure.AccountableFaceAsync(projectId, ct);
        var exposedBy = trigger == BriefPublishTrigger.Mediator ? userId : face.UserId;
        if (exposedBy is null) return null;

        var loaded = await LoadDayAsync(projectId, day, ct);
        var ids = loaded.Chosen.Keys.ToList();
        var frames = await _context.tbl_ProgressImages.IgnoreQueryFilters().Where(i => ids.Contains(i.Id)).ToListAsync(ct);
        var exposed = frames.Count == 0 ? 0 : await _exposure.ExposeAsync(frames, exposedBy, ct);

        var tenantId = await Q(_context.tbl_Projects_RS).Where(p => p.Id == projectId).Select(p => p.TenantId).FirstOrDefaultAsync(ct);
        var row = await _context.tbl_BriefPublications.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Day == day, ct);
        var now = DateTime.UtcNow;
        var dropped = loaded.Frames.Count(f => f.Image.Curation == FrameCuration.Dropped);
        if (row is null)
        {
            row = new tbl_BriefPublication { ProjectId = projectId, TenantId = tenantId, Day = day, Trigger = trigger };
            _context.tbl_BriefPublications.Add(row);
        }
        else if (trigger == BriefPublishTrigger.Mediator)
        {
            row.Trigger = trigger;
        }
        row.PublishedAt = now;
        row.PublishedById = exposedBy;
        row.FramesExposed += exposed;
        row.FramesDropped = dropped;
        await _context.SaveChangesAsync(ct);

        if (exposed > 0)
        {
            var name = await Q(_context.tbl_Projects_RS).Where(p => p.Id == projectId).Select(p => p.ProjectName).FirstOrDefaultAsync(ct);
            await _notifier.NotifyAsync(projectId, a => a.IsClientSide && a.CanSeeBrief, null, PushKind.BriefPublished,
                $"{name}: the day's brief", $"{exposed} new {(exposed == 1 ? "photo" : "photos")} from {face.Name ?? "site"}",
                $"/project/{projectId}/brief?day={day:yyyy-MM-dd}", ct);
        }

        return new CurationPublishResultDto
        {
            ProjectId = projectId,
            Day = day,
            Trigger = row.Trigger,
            FramesExposed = exposed,
            FramesDropped = dropped,
            PublishedAt = now,
            Changed = exposed > 0
        };
    }

    public async Task<List<CurationPublishResultDto>> RunCutoffAsync(DateTime? now = null, string? projectId = null, CancellationToken ct = default)
    {
        var at = now ?? DateTime.Now;
        var today = at.Date;
        var results = new List<CurationPublishResultDto>();

        // Yesterday too: a server that was down at 20:00 still owes that evening's brief.
        foreach (var day in new[] { today.AddDays(-1), today })
        {
            if (day == today && at.Hour < _cutoffHour) continue;
            var fromUtc = day.ToUniversalTime();
            var toUtc = day.AddDays(1).ToUniversalTime();

            var projectIds = await Q(_context.tbl_ProgressImages)
                .Where(i => i.ArtifactId != null && i.Channel == Channel.Crew && i.Curation != FrameCuration.Dropped
                            && i.ProgressUpdate != null && i.ProgressUpdate.IsDeleted != true
                            && i.ProgressUpdate.DateTimeCreated >= fromUtc && i.ProgressUpdate.DateTimeCreated < toUtc
                            && (projectId == null || i.ProgressUpdate.ProjectId == projectId))
                .Select(i => i.ProgressUpdate!.ProjectId!)
                .Distinct().ToListAsync(ct);

            // A batch still arriving over one bar of signal is not split by the cutoff:
            // a project whose last capture of the day is under ten minutes old waits for the next run.
            var settledBefore = at.ToUniversalTime().AddMinutes(-SettleMinutes);
            foreach (var pid in projectIds)
            {
                // Once per day: after the cutoff has run, further frames are the mediator's to send.
                if (await Q(_context.tbl_BriefPublications).AnyAsync(p => p.ProjectId == pid && p.Day == day, ct)) continue;
                var last = await Q(_context.tbl_ProgressUpdates)
                    .Where(u => u.ProjectId == pid && u.DateTimeCreated >= fromUtc && u.DateTimeCreated < toUtc)
                    .MaxAsync(u => u.DateTimeCreated, ct);
                if (last > settledBefore) continue;
                try
                {
                    var r = await PublishCoreAsync(pid, day, null, BriefPublishTrigger.Cutoff, ct);
                    if (r is not null) results.Add(r);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cutoff publish failed for {ProjectId} on {Day}", pid, day);
                }
            }
        }
        return results;
    }
}

/// <summary>Runs the cutoff with nobody logged in. Hourly; the cutoff hour decides which run acts.</summary>
public sealed class CutoffPublishJob
{
    private readonly ICurationDAL _curation;
    public CutoffPublishJob(ICurationDAL curation) => _curation = curation;

    [AutomaticRetry(Attempts = 1)]
    [DisableConcurrentExecution(600)]
    public Task RunAsync() => _curation.RunCutoffAsync();
}
