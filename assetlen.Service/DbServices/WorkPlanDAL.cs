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
/// The work plan and knocking items off it (works-report.md §4.5).
/// <para>
/// A tick is a capture: the one photo goes through the Site Diary's own pipeline
/// (artifact store, hash-dedupe, thumbnail, OCR, the mediator's push) against the
/// line, and then crosses to the client side through <see cref="IFrameExposure"/>
/// in the accountable face's name — the same way the cutoff crosses three frames
/// per piece of work. The Done state is already on the client's plan; the proof
/// travels with it, and nothing the ticker wrote travels at all.
/// </para>
/// </summary>
public sealed partial class WorkPlanDAL : IWorkPlanDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<WorkPlanDAL> _logger;
    private readonly IProjectAccessService _access;
    private readonly IProgressDAL _progress;
    private readonly IFrameExposure _exposure;

    public WorkPlanDAL(AssetlenDbContext context, ILogger<WorkPlanDAL> logger,
        IProjectAccessService access, IProgressDAL progress, IFrameExposure exposure)
    {
        _context = context;
        _logger = logger;
        _access = access;
        _progress = progress;
        _exposure = exposure;
    }

    public async Task<ServiceResult<WorkPlanDto>> GetPlan(string projectId, string? stageId, string userId, CancellationToken ct = default)
    {
        try
        {
            var project = await _context.tbl_Projects_RS.Include(p => p.ParentProject).AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null) return Fail<WorkPlanDto>(new NotFoundException("Project not found."));

            // A stage's checklist is what capture aims at, so every reader of the
            // project reads it; the project-wide plan is a seat of its own.
            var access = await _access.ResolveAsync(project, userId, ct);
            var allowed = string.IsNullOrEmpty(stageId) ? access.CanSeePlan : access.CanRead;
            if (!allowed) return Fail<WorkPlanDto>(new NotFoundException("Project not found."));

            var scope = new Dictionary<string, (tbl_Project Project, ProjectAccess Access)> { [project.Id] = (project, access) };

            // One list for the house: the guest wing's lines read beside the main
            // house's, under their own area, for whoever may read the wing.
            if (string.IsNullOrEmpty(stageId) && project.ParentProjectId is null)
            {
                var subs = await _context.tbl_Projects_RS.AsNoTracking()
                    .Where(p => p.ParentProjectId == project.Id && p.ArchivedAt == null)
                    .ToListAsync(ct);
                foreach (var s in subs) s.ParentProject = project;
                var subAccess = await _access.ResolveManyAsync(subs, userId, ct);
                foreach (var s in subs)
                    if (subAccess.TryGetValue(s.Id, out var a) && a.CanSeePlan) scope[s.Id] = (s, a);
            }

            var ids = scope.Keys.ToList();
            var query = _context.tbl_Deliverables.AsNoTracking()
                .Include(d => d.Stage)
                .Include(d => d.CompletedBy)
                .Where(d => d.ProjectId != null && ids.Contains(d.ProjectId));
            if (!string.IsNullOrEmpty(stageId)) query = query.Where(d => d.StageId == stageId);
            var rows = await query.ToListAsync(ct);

            var items = await MapAsync(rows, scope, ct);
            var stageOrder = rows.ToDictionary(r => r.Id, r => r.Stage?.DisplayOrder ?? int.MaxValue);
            var ordered = items
                .OrderBy(d => d.PlannedStart is null ? 1 : 0)
                .ThenBy(d => d.PlannedStart)
                .ThenBy(d => stageOrder.GetValueOrDefault(d.Id, int.MaxValue))
                .ThenBy(d => d.DisplayOrder)
                .ToList();

            return ServiceResult<WorkPlanDto>.Success(new WorkPlanDto
            {
                ProjectId = project.Id,
                CanTick = access.CanTick,
                CanEdit = access.CanSeeRegister && access.CanWrite,
                Items = ordered
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading the work plan for {ProjectId}", projectId);
            return Fail<WorkPlanDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<DeliverableDto>> Tick(string deliverableId, CaptureFile? photo, string userId, CancellationToken ct = default)
    {
        try
        {
            var (row, access, error) = await LoadForTickAsync(deliverableId, userId, ct);
            if (error is not null) return Fail<DeliverableDto>(error);

            if (photo is null || photo.Content.Length == 0)
                return Fail<DeliverableDto>(new BadRequestException("A tick needs one photo of the finished work. No photo, no tick."));
            // The header is the phone's word; the bytes are the file's. A note renamed .jpg is not a photo.
            if (!(photo.ContentType ?? "").StartsWith("image/", StringComparison.OrdinalIgnoreCase) || !await IsImageAsync(photo.Content, ct))
                return Fail<DeliverableDto>(new BadRequestException("A tick is made on a photo — that file is not an image."));

            if (row!.Status == DeliverableStatus.Done) return await OneAsync(row.Id, row.Project!, access, ct);

            // The cycle number makes the capture idempotent: a double-tap or a retry
            // on a bad connection lands on the same Site Diary entry.
            var cycle = await _context.tbl_DeliverableEvents.CountAsync(e => e.DeliverableId == row.Id && e.Kind == DeliverableEventKind.Ticked, ct);
            var captured = await _progress.Capture(new ProgressUpdateCreateDto
            {
                ProjectId = row.ProjectId,
                DeliverableId = row.Id,
                Description = $"Done: {row.Title}",
                Channel = Channel.Crew,
                ClientCaptureId = $"tick:{row.Id}:{cycle}"
            }, new[] { photo }, null, userId, ct);
            if (!captured.IsSuccess) return Fail<DeliverableDto>(captured.Error);

            var entryId = captured.Data!.Id!;
            var frames = await _context.tbl_ProgressImages.Where(i => i.ProgressUpdateId == entryId && i.IsDeleted != true).ToListAsync(ct);
            var artifactId = frames.OrderBy(f => f.DisplayOrder).Select(f => f.ArtifactId).FirstOrDefault();

            // The proof crosses with the state, in the mediator's name (§10.1). With no
            // mediator on the project it crosses only by someone who may expose;
            // otherwise it waits in the Site Diary for the cutoff like any other frame.
            var face = await _exposure.AccountableFaceAsync(row.ProjectId!, ct);
            var exposer = face.UserId ?? (access.CanExposeToClient ? userId : null);
            if (exposer is not null && frames.Count > 0) await _exposure.ExposeAsync(frames, exposer, ct);

            // One conditional write: of two taps racing, only the one that moves the
            // line to Done records a tick, so the history never shows a line done twice.
            var now = DateTime.UtcNow;
            var moved = await _context.tbl_Deliverables
                .Where(d => d.Id == row.Id && d.Status != DeliverableStatus.Done)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.Status, DeliverableStatus.Done)
                    .SetProperty(d => d.CompletedAt, (DateTime?)now)
                    .SetProperty(d => d.CompletedById, userId)
                    .SetProperty(d => d.CompletionArtifactId, artifactId), ct);
            if (moved == 1)
            {
                _context.tbl_DeliverableEvents.Add(new tbl_DeliverableEvent
                {
                    ProjectId = row.ProjectId,
                    TenantId = row.TenantId,
                    DeliverableId = row.Id,
                    Kind = DeliverableEventKind.Ticked,
                    OccurredAt = now,
                    ById = userId,
                    ArtifactId = artifactId,
                    ProgressUpdateId = entryId
                });
                await _context.SaveChangesAsync(ct);
                await RedateAfterTickAsync(row.Project!, ct);
            }

            return await OneAsync(row.Id, row.Project!, access, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ticking deliverable {Id}", deliverableId);
            return Fail<DeliverableDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<DeliverableDto>> Reopen(string deliverableId, string userId, CancellationToken ct = default)
    {
        try
        {
            var (row, access, error) = await LoadForTickAsync(deliverableId, userId, ct);
            if (error is not null) return Fail<DeliverableDto>(error);
            var line = row!;

            // The line loses its tick, not its proof: the entry, the photo's refs and
            // the Ticked event all stay, and the reopening is dated and attributed.
            var moved = await _context.tbl_Deliverables
                .Where(d => d.Id == line.Id && d.Status == DeliverableStatus.Done)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.Status, DeliverableStatus.InProgress)
                    .SetProperty(d => d.CompletedAt, (DateTime?)null)
                    .SetProperty(d => d.CompletedById, (string?)null)
                    .SetProperty(d => d.CompletionArtifactId, (string?)null), ct);
            if (moved == 1)
            {
                _context.tbl_DeliverableEvents.Add(new tbl_DeliverableEvent
                {
                    ProjectId = line.ProjectId,
                    TenantId = line.TenantId,
                    DeliverableId = line.Id,
                    Kind = DeliverableEventKind.Reopened,
                    OccurredAt = DateTime.UtcNow,
                    ById = userId
                });
                await _context.SaveChangesAsync(ct);
                await RedateAfterTickAsync(line.Project!, ct);
            }

            return await OneAsync(line.Id, line.Project!, access, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening deliverable {Id}", deliverableId);
            return Fail<DeliverableDto>(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Ticking and unticking are one act and share one gate: a stranger is told
    /// nothing, the client side and a read-only bench seat are told it is not theirs.
    /// </summary>
    private async Task<(tbl_Deliverable? Row, ProjectAccess Access, Exception? Error)> LoadForTickAsync(
        string deliverableId, string userId, CancellationToken ct)
    {
        var row = await _context.tbl_Deliverables
            .Include(d => d.Project).ThenInclude(p => p!.ParentProject)
            .FirstOrDefaultAsync(d => d.Id == deliverableId, ct);
        if (row?.Project is null) return (null, ProjectAccess.None, new NotFoundException("Deliverable not found."));

        var access = await _access.ResolveAsync(row.Project, userId, ct);
        if (!access.CanRead) return (null, access, new NotFoundException("Deliverable not found."));
        if (!access.CanTick)
            return (null, access, new ForbiddenException("The delivery side knocks lines off the plan; you can read it."));
        return (row, access, null);
    }

    private async Task<ServiceResult<DeliverableDto>> OneAsync(string id, tbl_Project project, ProjectAccess access, CancellationToken ct)
    {
        var row = await _context.tbl_Deliverables.AsNoTracking()
            .Include(d => d.Stage).Include(d => d.CompletedBy)
            .FirstAsync(d => d.Id == id, ct);
        var items = await MapAsync(new List<tbl_Deliverable> { row },
            new Dictionary<string, (tbl_Project, ProjectAccess)> { [project.Id] = (project, access) }, ct);
        return ServiceResult<DeliverableDto>.Success(items[0]);
    }

    /// <summary>
    /// Map lines for one reader. The delivery side reads true names; the client
    /// side reads the accountable face, and sees a photo only once it has crossed.
    /// </summary>
    private async Task<List<DeliverableDto>> MapAsync(IReadOnlyList<tbl_Deliverable> rows,
        IReadOnlyDictionary<string, (tbl_Project Project, ProjectAccess Access)> scope, CancellationToken ct)
    {
        var ids = rows.Select(r => r.Id).ToList();
        var events = await _context.tbl_DeliverableEvents.AsNoTracking()
            .Include(e => e.By)
            .Where(e => e.DeliverableId != null && ids.Contains(e.DeliverableId))
            .OrderBy(e => e.OccurredAt)
            .ToListAsync(ct);

        var projectIds = scope.Keys.ToList();
        var counts = await _context.tbl_Commitments
            .Where(c => c.ProjectId != null && projectIds.Contains(c.ProjectId) && c.DeliverableId != null && c.SupersededAt == null)
            .GroupBy(c => c.DeliverableId!)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var photoIds = rows.Select(r => r.CompletionArtifactId).Concat(events.Select(e => e.ArtifactId))
            .OfType<string>().Distinct().ToList();
        var crossed = photoIds.Count == 0 ? new HashSet<string>() : (await _context.tbl_ArtifactRefs.AsNoTracking()
            .Where(r => r.ArtifactId != null && photoIds.Contains(r.ArtifactId) && r.Channel == Channel.Client)
            .Select(r => r.ArtifactId!).Distinct().ToListAsync(ct)).ToHashSet();

        var faces = new Dictionary<string, string?>();
        foreach (var (pid, (_, a)) in scope)
            if (!a.CanSeeSiteLog) faces[pid] = (await _exposure.AccountableFaceAsync(pid, ct)).Name;

        return rows.Select(r =>
        {
            var (project, access) = scope[r.ProjectId!];
            var diary = access.CanSeeSiteLog;
            string? Name(AppUser? u) => u is null ? null : diary ? $"{u.FirstName} {u.LastName}".Trim() : faces.GetValueOrDefault(project.Id);
            bool Shown(string? artifactId) => artifactId is not null && (diary || crossed.Contains(artifactId));

            var dto = CommitmentDAL.ToDto(r, access.CanSeeRegister ? counts.GetValueOrDefault(r.Id) : 0);
            dto.ProjectName = project.ProjectName;
            dto.CanTick = access.CanTick;
            dto.CompletedByName = Name(r.CompletedBy);
            if (!Shown(r.CompletionArtifactId))
            {
                dto.CompletionArtifactId = null;
                dto.CompletionThumbnailUrl = null;
                dto.CompletionImageUrl = null;
            }
            dto.History = events.Where(e => e.DeliverableId == r.Id).Select(e => new DeliverableEventDto
            {
                Kind = e.Kind,
                OccurredAt = e.OccurredAt,
                ByName = Name(e.By),
                ArtifactId = Shown(e.ArtifactId) ? e.ArtifactId : null,
                ThumbnailUrl = Shown(e.ArtifactId) ? $"/api/Artifacts/{e.ArtifactId}/thumbnail" : null
            }).ToList();
            return dto;
        }).ToList();
    }

    /// <summary>Whether the bytes open as a photo a phone or camera writes: JPEG, PNG, GIF, WebP, HEIC/AVIF, TIFF.</summary>
    private static async Task<bool> IsImageAsync(Stream content, CancellationToken ct)
    {
        if (!content.CanSeek) return false;
        var head = new byte[12];
        var start = content.Position;
        var read = await content.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false, ct);
        content.Position = start;
        if (read < 4) return false;

        bool At(int offset, string ascii) => read >= offset + ascii.Length
            && System.Text.Encoding.ASCII.GetString(head, offset, ascii.Length) == ascii;

        return (head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF)
            || (head[0] == 0x89 && At(1, "PNG"))
            || At(0, "GIF8")
            || (At(0, "RIFF") && At(8, "WEBP"))
            || (At(4, "ftyp") && new[] { "heic", "heix", "hevc", "heim", "heis", "mif1", "msf1", "avif", "avis" }.Any(b => At(8, b)))
            || At(0, "II*\0") || At(0, "MM\0*");
    }

    private static ServiceResult<T> Fail<T>(Exception e) => ServiceResult<T>.Failure(e);
}
