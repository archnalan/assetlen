using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Hubs;
using assetlen.Service.FileProcessingServices.Push;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class ProgressDAL : IProgressDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<ProgressDAL> _logger;
    private readonly ITenantProvider _tenant;
    private readonly IHubContext<AssetlenHub> _hub;
    private readonly IProjectAccessService _access;
    private readonly IActiveStageService _activeStage;
    private readonly IArtifactDAL _artifacts;
    private readonly IFrameExposure _exposure;
    private readonly INotifier _notifier;

    public ProgressDAL(
        AssetlenDbContext context,
        ILogger<ProgressDAL> logger,
        ITenantProvider tenant,
        IHubContext<AssetlenHub> hub,
        IProjectAccessService access,
        IActiveStageService activeStage,
        IArtifactDAL artifacts,
        IFrameExposure exposure,
        INotifier notifier)
    {
        _artifacts = artifacts;
        _exposure = exposure;
        _notifier = notifier;
        _context = context;
        _logger = logger;
        _tenant = tenant;
        _hub = hub;
        _access = access;
        _activeStage = activeStage;
    }

    /// <summary>
    /// The JSON path older clients post. Its base64 frames go into the artifact
    /// store like any other capture — a data URI on the row had no address, so a
    /// captured frame could never reach the brief, the report or a claim.
    /// </summary>
    public async Task<ServiceResult<ProgressUpdateDto>> AddProgressUpdate(ProgressUpdateCreateDto dto, string userId)
    {
        var frames = new List<CaptureFile>();
        try
        {
            foreach (var img in dto.Images ?? new())
            {
                var raw = img.Base64Image ?? string.Empty;
                if (raw.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;
                // Strip a "data:<mime>;base64," prefix by finding the comma, never by
                // trimming characters: TrimStart ate the '/' of every JPEG (plan.md A2).
                if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    var comma = raw.IndexOf(',');
                    if (comma >= 0) raw = raw[(comma + 1)..];
                }
                frames.Add(new CaptureFile(new MemoryStream(Convert.FromBase64String(raw)), img.FileName,
                    string.IsNullOrWhiteSpace(img.ContentType) ? "image/jpeg" : img.ContentType, img.Caption));
            }
        }
        catch (FormatException)
        {
            return ServiceResult<ProgressUpdateDto>.Failure(new BadRequestException("An image was not valid base64."));
        }

        try
        {
            return await Capture(dto, frames, null, userId);
        }
        finally
        {
            foreach (var f in frames) await f.Content.DisposeAsync();
        }
    }

    /// <summary>Real capture is thirteen to eighteen frames at 22:00 (Nalan.md); room for a long day.</summary>
    public const int MaxFramesPerCapture = 24;

    public async Task<ServiceResult<ProgressUpdateDto>> Capture(ProgressUpdateCreateDto dto, IReadOnlyList<CaptureFile> frames,
        CaptureFile? voice, string userId, CancellationToken ct = default)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, ct);
            if (project == null)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Project not found"));

            // The Site Diary is the delivery side's; whoever posts to it must be able to read it.
            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanCapture)
                return ServiceResult<ProgressUpdateDto>.Failure(access.CanRead
                    ? new ForbiddenException("Access denied")
                    : new NotFoundException("Project not found"));

            // The queue retries until it hears back; a second arrival is the first one.
            if (!string.IsNullOrWhiteSpace(dto.ClientCaptureId))
            {
                var existing = await _context.tbl_ProgressUpdates.AsNoTracking()
                    .Where(u => u.ProjectId == project.Id && u.ClientCaptureId == dto.ClientCaptureId)
                    .Select(u => u.Id).FirstOrDefaultAsync(ct);
                if (existing is not null) return await GetProgressUpdateById(existing, userId);
            }

            if (frames.Count > MaxFramesPerCapture)
                return ServiceResult<ProgressUpdateDto>.Failure(
                    new BadRequestException($"At most {MaxFramesPerCapture} frames in one capture."));

            tbl_Deliverable? deliverable = null;
            if (!string.IsNullOrEmpty(dto.DeliverableId))
            {
                deliverable = await _context.tbl_Deliverables.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == dto.DeliverableId && d.ProjectId == project.Id, ct);
                if (deliverable is null)
                    return ServiceResult<ProgressUpdateDto>.Failure(new BadRequestException("That deliverable is not on this project."));
            }

            // Nothing floats (CLAUDE.md §1), but making the clerk pick a stage on
            // every batch is the tax that sends people back to the chat. Aiming at a
            // deliverable names its stage; naming nothing files it against the live one.
            var stageId = await _activeStage.ResolveAsync(dto.ProjectId, deliverable?.StageId ?? dto.StageId, ct);
            var stage = stageId is null ? null : await _context.tbl_Stages.FindAsync(new object[] { stageId }, ct);
            if (stage == null || stage.ProjectId != dto.ProjectId)
                return ServiceResult<ProgressUpdateDto>.Failure(
                    new BadRequestException("This project has no stage to capture against."));

            if (string.IsNullOrWhiteSpace(dto.Description) && frames.Count == 0 && voice is null)
                return ServiceResult<ProgressUpdateDto>.Failure(
                    new BadRequestException("A capture needs a photo, a voice note or a line of text."));

            // Store every file first: an entry that points at frames that failed to
            // store would show gaps the reader cannot explain.
            var stored = new List<(string ArtifactId, string? Caption)>();
            foreach (var f in frames)
            {
                var r = await _artifacts.IngestAsync(f.Content, f.FileName, f.ContentType, project.Id!, userId, dto.CapturedAt, ct);
                if (!r.IsSuccess) return ServiceResult<ProgressUpdateDto>.Failure(r.Error);
                stored.Add((r.Data!.Id!, f.Caption));
            }

            string? voiceId = null;
            if (voice is not null)
            {
                var r = await _artifacts.IngestAsync(voice.Content, voice.FileName, voice.ContentType, project.Id!, userId, dto.CapturedAt, ct);
                if (!r.IsSuccess) return ServiceResult<ProgressUpdateDto>.Failure(r.Error);
                voiceId = r.Data!.Id;
            }

            var update = new tbl_ProgressUpdate
            {
                ProjectId = dto.ProjectId,
                StageId = stageId,
                DeliverableId = deliverable?.Id,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                CompletionPercentage = dto.CompletionPercentage,
                HasIssues = dto.HasIssues,
                CreatedById = userId,
                // Uploading is not exposing: only the mediator may start on the client side.
                Channel = dto.Channel == Channel.Client && access.CanExposeToClient ? Channel.Client : Channel.Crew,
                ApprovalStatus = ApprovalStatus.Pending,
                ClientCaptureId = string.IsNullOrWhiteSpace(dto.ClientCaptureId) ? null : dto.ClientCaptureId.Trim(),
                CapturedAt = ClampCapturedAt(dto.CapturedAt),
                VoiceArtifactId = voiceId
            };

            _context.tbl_ProgressUpdates.Add(update);
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException) when (update.ClientCaptureId is not null)
            {
                // Two retries raced past the lookup above; the winner is the capture.
                _context.Entry(update).State = EntityState.Detached;
                var winner = await _context.tbl_ProgressUpdates.AsNoTracking()
                    .Where(u => u.ProjectId == project.Id && u.ClientCaptureId == update.ClientCaptureId)
                    .Select(u => u.Id).FirstOrDefaultAsync(ct);
                if (winner is null) throw;
                return await GetProgressUpdateById(winner, userId);
            }

            // Captured offline at 22:00 belongs to that evening's brief, not to the
            // morning the signal came back; every reader files by DateTimeCreated.
            if (update.CapturedAt is { } shot && Math.Abs((shot - update.DateTimeCreated!.Value).TotalMinutes) > 2)
                await _context.tbl_ProgressUpdates.Where(u => u.Id == update.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.DateTimeCreated, shot), ct);

            if (dto.CompletionPercentage is { } pct)
            {
                if (pct != stage.CompletionPercentage)
                    _context.tbl_ProgressReadings.Add(new tbl_ProgressReading
                    {
                        ProjectId = stage.ProjectId,
                        TenantId = stage.TenantId,
                        StageId = stage.Id,
                        Subject = stage.StageName,
                        Percent = Math.Clamp(pct, 0, 100),
                        ObservedAt = update.CapturedAt ?? DateTime.UtcNow,
                        SourceKind = ProgressReadingSource.Capture,
                        SourceId = update.Id
                    });

                stage.CompletionPercentage = pct;
                if (pct >= 100)
                {
                    stage.Status = StageStatus.Completed;
                    stage.ActualEndDate ??= DateTime.UtcNow;
                }
                else if (pct > 0 && stage.Status == StageStatus.NotStarted)
                {
                    stage.Status = StageStatus.InProgress;
                }
            }

            if (deliverable is not null && deliverable.Status == DeliverableStatus.NotStarted)
                await _context.tbl_Deliverables.Where(d => d.Id == deliverable.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, DeliverableStatus.InProgress), ct);

            var order = 1;
            var images = stored.Select(s => new tbl_ProgressImage
            {
                ProgressUpdateId = update.Id,
                ArtifactId = s.ArtifactId,
                Caption = s.Caption,
                DisplayOrder = order++,
                Channel = Channel.Crew
            }).ToList();
            _context.tbl_ProgressImages.AddRange(images);
            await _context.SaveChangesAsync(ct);

            await _exposure.AddRefsAsync(update, images, ct);
            if (voiceId is not null)
                await _exposure.AddRefsAsync(update, new[] { new tbl_ProgressImage { ProgressUpdateId = update.Id, ArtifactId = voiceId, Channel = Channel.Crew, Caption = "Voice note" } }, ct);

            // An entry posted straight to the client side still exposes frame by frame.
            if (update.Channel == Channel.Client && images.Count > 0)
                await _exposure.ExposeAsync(images, userId, ct);

            // Whoever mediates hears about it at WhatsApp speed; the bench's traffic
            // never wakes the client side (assetlen.md D5).
            var author = await _context.Users.AsNoTracking().Where(u => u.Id == userId)
                .Select(u => (u.FirstName + " " + u.LastName).Trim()).FirstOrDefaultAsync(ct);
            var what = deliverable?.Title ?? stage.StageName ?? "site";
            await _notifier.NotifyAsync(project.Id!, a => a.CanSeeSiteLog && a.CanExposeToClient, userId, PushKind.Capture,
                $"{author}: {(images.Count == 0 ? "a note" : images.Count == 1 ? "1 photo" : $"{images.Count} photos")} on {what}",
                update.Description ?? (voiceId is null ? "Captured on site" : "Voice note"),
                $"/project/{project.Id}/entry/{update.Id}", ct);

            return await GetProgressUpdateById(update.Id!, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting a capture");
            return ServiceResult<ProgressUpdateDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// A shot time from the phone is trusted within reason: not in the future, and
    /// not older than a fortnight — an older one is a clock problem, not a capture.
    /// </summary>
    private static DateTime? ClampCapturedAt(DateTime? capturedAt)
    {
        if (capturedAt is not { } at) return null;
        var utc = at.Kind == DateTimeKind.Local ? at.ToUniversalTime() : DateTime.SpecifyKind(at, DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        return utc > now ? now : utc < now.AddDays(-14) ? null : utc;
    }

    public async Task<ServiceResult<CaptureTodayDto>> GetCaptureToday(string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var project = await _context.tbl_Projects_RS.Include(p => p.ParentProject).AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null) return ServiceResult<CaptureTodayDto>.Failure(new NotFoundException("Project not found"));
            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanCapture)
                return ServiceResult<CaptureTodayDto>.Failure(new NotFoundException("Project not found"));

            var activeId = await _activeStage.ResolveAsync(projectId, null, ct);
            var stages = await _context.tbl_Stages.AsNoTracking().Where(s => s.ProjectId == projectId).ToListAsync(ct);
            var deliverables = await _context.tbl_Deliverables.AsNoTracking().Where(d => d.ProjectId == projectId).ToListAsync(ct);

            var todayUtc = DateTime.Today.ToUniversalTime();
            var since = todayUtc.AddDays(-30);
            var recent = await _context.tbl_ProgressUpdates.AsNoTracking()
                .Where(u => u.ProjectId == projectId && u.DateTimeCreated >= since)
                .Select(u => new { u.DeliverableId, u.DateTimeCreated })
                .ToListAsync(ct);

            var stageById = stages.ToDictionary(s => s.Id!);
            // Today's work first: deliverables on stages under way, then on the stage
            // capture would fall to, then what is next. Finished ones go last, not away —
            // a snag photographed on a signed-off item still belongs to it.
            int Rank(tbl_Deliverable d)
            {
                var s = d.StageId is null ? null : stageById.GetValueOrDefault(d.StageId);
                if (d.Status == DeliverableStatus.Done) return 4;
                if (s?.Status == StageStatus.InProgress) return d.Status == DeliverableStatus.InProgress ? 0 : 1;
                if (s?.Id == activeId) return 2;
                return 3;
            }

            var list = deliverables
                .OrderBy(Rank)
                .ThenBy(d => d.StageId is null ? int.MaxValue : stageById.GetValueOrDefault(d.StageId)?.DisplayOrder ?? int.MaxValue)
                .ThenBy(d => d.DisplayOrder)
                .Select(d =>
                {
                    var s = d.StageId is null ? null : stageById.GetValueOrDefault(d.StageId);
                    var mine = recent.Where(r => r.DeliverableId == d.Id).ToList();
                    return new CaptureDeliverableDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        StageId = d.StageId,
                        StageName = s?.StageName,
                        Phase = s?.Phase,
                        Status = d.Status,
                        CapturesToday = mine.Count(r => r.DateTimeCreated >= todayUtc),
                        LastCapturedAt = mine.Count == 0 ? null : mine.Max(r => r.DateTimeCreated)
                    };
                }).ToList();

            return ServiceResult<CaptureTodayDto>.Success(new CaptureTodayDto
            {
                ProjectId = projectId,
                ActiveStageId = activeId,
                ActiveStageName = activeId is null ? null : stageById.GetValueOrDefault(activeId)?.StageName,
                Deliverables = list,
                CapturesToday = recent.Count(r => r.DateTimeCreated >= todayUtc)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading today's deliverables for {ProjectId}", projectId);
            return ServiceResult<CaptureTodayDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProgressUpdateDto>> GetProgressUpdate(string updateId, string userId)
    {
        try
        {
            var update = await _context.tbl_ProgressUpdates
                .Include(u => u.Project)
                    .ThenInclude(p => p!.ParentProject)
                .Include(u => u.CreatedBy)
                .Include(u => u.Stage)
                .Include(u => u.Deliverable)
                .Include(u => u.Images.OrderBy(i => i.DisplayOrder))
                .Include(u => u.Comments.Where(c => c.ParentCommentId == null).OrderBy(c => c.DateTimeCreated))
                    .ThenInclude(c => c.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Entry not found"));

            var access = await _access.ResolveAsync(update.Project, userId);
            if (!access.CanRead)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Entry not found"));

            // Per-project side, not the tenant-global role. The same person can
            // be client-side on one project and mediate another; a global
            // IsExternal() check would give them one answer for both.
            if (!access.CanSeeSiteLog && update.Channel != Channel.Client)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Entry not found"));

            return ServiceResult<ProgressUpdateDto>.Success((await MapAsync(new[] { update }, access)).Single());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress update {UpdateId}", updateId);
            return ServiceResult<ProgressUpdateDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProgressUpdateDto>> SetChannel(string updateId, Channel channel, string userId)
    {
        try
        {
            var update = await _context.tbl_ProgressUpdates
                .Include(u => u.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Entry not found"));

            // Exposing to the client side is the mediator's decision. Owner and
            // manager authority also qualifies; nobody else does — the clerk of
            // works must not be able to put anything in front of the client.
            if (!await _access.CanExposeToClientAsync(update.Project, userId))
                return ServiceResult<ProgressUpdateDto>.Failure(new ForbiddenException(
                    "Only a project mediator, owner or manager can change what the client sees"));

            update.Channel = channel;

            // Promoting the entry does NOT promote its frames. That was the old
            // behaviour and it forwarded whole batches. Withdrawing it does pull
            // the frames back, because a frame cannot be visible on an entry the
            // reader can no longer open.
            await _context.SaveChangesAsync();
            if (channel == Channel.Crew)
            {
                var frames = await _context.tbl_ProgressImages
                    .Where(i => i.ProgressUpdateId == update.Id && i.Channel == Channel.Client)
                    .ToListAsync();
                await _exposure.WithdrawAsync(frames);
            }

            return await GetProgressUpdateById(update.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting channel for {UpdateId}", updateId);
            return ServiceResult<ProgressUpdateDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProgressUpdateDto>> SetApprovalStatus(ProgressApprovalDto dto, string investorId)
    {
        try
        {
            var update = await _context.tbl_ProgressUpdates
                .Include(u => u.Project)
                .FirstOrDefaultAsync(u => u.Id == dto.ProgressUpdateId);

            if (update == null)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Progress update not found"));

            if (update.Project?.InvestorId != investorId)
                return ServiceResult<ProgressUpdateDto>.Failure(new ForbiddenException("Only the investor can approve updates"));

            update.ApprovalStatus = dto.Status;
            await _context.SaveChangesAsync();

            return await GetProgressUpdateById(update.Id, investorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting approval status");
            return ServiceResult<ProgressUpdateDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<PaginationDetails<ProgressUpdateDto>>> GetProgressUpdates(
        string projectId, string? stageId, int offset, int limit, string userId, CancellationToken ct)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project == null)
                return ServiceResult<PaginationDetails<ProgressUpdateDto>>.Failure(new NotFoundException("Project not found"));

            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanRead)
                return ServiceResult<PaginationDetails<ProgressUpdateDto>>.Failure(new ForbiddenException("Access denied"));

            var query = _context.tbl_ProgressUpdates
                .Include(u => u.CreatedBy)
                .Include(u => u.Stage)
                .Include(u => u.Deliverable)
                .Include(u => u.Images.OrderBy(i => i.DisplayOrder))
                .Include(u => u.Comments.Where(c => c.ParentCommentId == null))
                    .ThenInclude(c => c.Author)
                .Where(u => u.ProjectId == projectId)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(stageId))
                query = query.Where(u => u.StageId == stageId);

            // Per-project side. A mediator reads the whole Site Diary even though
            // they may sit on the client side of this project.
            if (!access.CanSeeSiteLog)
                query = query.Where(u => u.Channel == Channel.Client);

            var total = await query.CountAsync(ct);

            var updates = await query
                .OrderByDescending(u => u.DateTimeCreated)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var dtos = await MapAsync(updates, access);

            return ServiceResult<PaginationDetails<ProgressUpdateDto>>.Success(
                new PaginationDetails<ProgressUpdateDto>
                {
                    Data = dtos,
                    TotalSize = total,
                    Limit = limit,
                    OffSet = offset,
                    IsNext = offset + limit < total
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress updates");
            return ServiceResult<PaginationDetails<ProgressUpdateDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProgressCommentDto>> AddComment(ProgressCommentCreateDto dto, string userId)
    {
        try
        {
            // Validate the user has access to the project + resolve the
            // parent entry so its Channel governs the broadcast.
            tbl_Project? project = null;
            tbl_ProgressUpdate? parentEntry = null;

            if (!string.IsNullOrEmpty(dto.ProgressUpdateId))
            {
                parentEntry = await _context.tbl_ProgressUpdates
                    .Include(u => u.Project)
                    .FirstOrDefaultAsync(u => u.Id == dto.ProgressUpdateId);
                project = parentEntry?.Project;
            }
            else if (!string.IsNullOrEmpty(dto.ProgressImageId))
            {
                var image = await _context.tbl_ProgressImages
                    .Include(i => i.ProgressUpdate)
                        .ThenInclude(u => u!.Project)
                    .FirstOrDefaultAsync(i => i.Id == dto.ProgressImageId);
                parentEntry = image?.ProgressUpdate;
                project = parentEntry?.Project;
            }

            if (project == null)
                return ServiceResult<ProgressCommentDto>.Failure(new NotFoundException("Target not found"));

            // Commenting is how Peter asks a question — any active member may.
            var access = await _access.ResolveAsync(project, userId);
            if (!access.CanSeeSiteLog && parentEntry?.Channel != Channel.Client)
                return ServiceResult<ProgressCommentDto>.Failure(new NotFoundException("Target not found"));
            if (!access.CanWrite)
                return ServiceResult<ProgressCommentDto>.Failure(new ForbiddenException("Access denied"));

            var comment = new tbl_ProgressComment
            {
                ProgressUpdateId = dto.ProgressUpdateId,
                ProgressImageId = dto.ProgressImageId,
                CommentText = dto.CommentText,
                AuthorId = userId,
                ParentCommentId = dto.ParentCommentId,
                // Said where the entry already stood: a remark on a Diary entry stays in the Diary if its frames later cross.
                Channel = parentEntry?.Channel ?? Channel.Crew
            };

            _context.tbl_ProgressComments.Add(comment);
            await _context.SaveChangesAsync();

            var author = await _context.Users.FindAsync(userId);

            var dtoOut = new ProgressCommentDto
            {
                Id = comment.Id,
                ProgressUpdateId = comment.ProgressUpdateId,
                ProgressImageId = comment.ProgressImageId,
                CommentText = comment.CommentText,
                AuthorId = comment.AuthorId,
                ParentCommentId = comment.ParentCommentId,
                AuthorName = author != null ? $"{author.FirstName} {author.LastName}" : null,
                AuthorProfilePicUrl = author?.ProfilePicUrl,
                DateTimeCreated = comment.DateTimeCreated
            };

            // Broadcast over the Stream. Channel inherits the parent entry's
            // Channel — a comment on a Crew entry stays Crew, so external
            // principals never see it via the live transport.
            var streamChannel = parentEntry?.Channel ?? Channel.Crew;
            await BroadcastComment(dtoOut, streamChannel);

            return ServiceResult<ProgressCommentDto>.Success(dtoOut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return ServiceResult<ProgressCommentDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<ProgressCommentDto>>> GetRecentComments(string managerId, int count)
    {
        try
        {
            var comments = await _context.tbl_ProgressComments
                .Include(c => c.Author)
                .Include(c => c.ProgressUpdate)
                    .ThenInclude(u => u!.Project)
                .Where(c => c.ProgressUpdate != null
                    && c.ProgressUpdate.Project != null
                    && c.ProgressUpdate.Project.ProjectManagerId == managerId
                    && c.AuthorId != managerId) // Comments from others
                .OrderByDescending(c => c.DateTimeCreated)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();

            var dtos = comments.Select(c => new ProgressCommentDto
            {
                Id = c.Id,
                ProgressUpdateId = c.ProgressUpdateId,
                ProgressImageId = c.ProgressImageId,
                CommentText = c.CommentText,
                AuthorId = c.AuthorId,
                ParentCommentId = c.ParentCommentId,
                AuthorName = c.Author != null ? $"{c.Author.FirstName} {c.Author.LastName}" : null,
                AuthorProfilePicUrl = c.Author?.ProfilePicUrl,
                DateTimeCreated = c.DateTimeCreated
            }).ToList();

            return ServiceResult<List<ProgressCommentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent comments");
            return ServiceResult<List<ProgressCommentDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Private helpers ──────────────────────────────────────

    private async Task BroadcastComment(ProgressCommentDto comment, Channel channel)
    {
        var streamId = comment.ProgressUpdateId;
        if (string.IsNullOrEmpty(streamId)) return;
        try
        {
            var envelope = new StreamCommentEvent
            {
                StreamId = streamId,
                Channel = channel,
                Comment = comment
            };
            var target = channel == Channel.Crew
                ? AssetlenHub.CrewStreamGroup(streamId)
                : AssetlenHub.StreamGroup(streamId);
            await _hub.Clients.Group(target).SendAsync("ReceiveStreamComment", envelope);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Hub broadcast failed for stream {StreamId}", streamId);
        }
    }


    private async Task<ServiceResult<ProgressUpdateDto>> GetProgressUpdateById(string updateId, string userId)
    {
        var update = await _context.tbl_ProgressUpdates
            .Include(u => u.Project)
                .ThenInclude(p => p!.ParentProject)
            .Include(u => u.CreatedBy)
            .Include(u => u.Stage)
            .Include(u => u.Deliverable)
            .Include(u => u.Images.OrderBy(i => i.DisplayOrder))
            .Include(u => u.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == updateId);

        if (update == null)
            return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Update not found"));

        var access = await _access.ResolveAsync(update.Project, userId);
        return ServiceResult<ProgressUpdateDto>.Success((await MapAsync(new[] { update }, access)).Single());
    }

    /// <summary>
    /// Expose or withdraw individual frames on an entry. This is the operation
    /// that replaces forwarding: the mediator picks three of eighteen rather
    /// than flipping the whole batch across.
    /// </summary>
    public async Task<ServiceResult<ProgressUpdateDto>> SetImageChannel(
        ProgressImageExposureDto dto, string userId)
    {
        try
        {
            var ids = dto.ImageIds.Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();
            if (ids.Count == 0)
                return ServiceResult<ProgressUpdateDto>.Failure(new BadRequestException("No images supplied"));

            var images = await _context.tbl_ProgressImages
                .Include(i => i.ProgressUpdate)
                    .ThenInclude(u => u!.Project)
                        .ThenInclude(p => p!.ParentProject)
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (images.Count == 0)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Images not found"));

            var entries = images.Select(i => i.ProgressUpdateId).Distinct().ToList();
            if (entries.Count > 1)
                return ServiceResult<ProgressUpdateDto>.Failure(
                    new BadRequestException("All images must belong to the same entry"));

            var entry = images[0].ProgressUpdate;
            if (!await _access.CanExposeToClientAsync(entry?.Project, userId))
                return ServiceResult<ProgressUpdateDto>.Failure(new ForbiddenException(
                    "Only a project mediator, owner or manager can change what the client sees"));

            // Frame and pointer move together (IFrameExposure), and exposing a frame
            // carries its entry across — a frame is unreachable on an entry the client cannot open.
            if (dto.Channel == Channel.Client) await _exposure.ExposeAsync(images, userId);
            else await _exposure.WithdrawAsync(images);

            return await GetProgressUpdateById(entry!.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting image channel");
            return ServiceResult<ProgressUpdateDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Map an entry for one specific reader.
    /// <para>
    /// <b>The image filter is the point of P2.</b> Before it, promoting an
    /// entry pushed every frame it held to the client — and a real capture is
    /// thirteen to eighteen frames posted in one batch, so that switch was a
    /// forward, not a curation. Now each frame carries its own channel and a
    /// client-side reader sees only the ones a mediator exposed.
    /// </para>
    /// <para>
    /// <paramref name="access"/> is per project, never the tenant-global role:
    /// a mediator sitting on the client side still reads the whole Site Diary.
    /// </para>
    /// </summary>
    private async Task<List<ProgressUpdateDto>> MapAsync(IReadOnlyList<tbl_ProgressUpdate> updates, ProjectAccess access)
    {
        // Everything that crosses reads in the accountable face's name (§10.1); true
        // authorship stays on the delivery side, where it is the mediator's own record.
        string? face = null;
        if (!access.CanSeeSiteLog && updates.FirstOrDefault()?.ProjectId is { } pid)
            face = (await _exposure.AccountableFaceAsync(pid)).Name;

        // A comment crosses only on the client channel or from the client side's own
        // hand; the bench's remarks on an entry stay in the Site Diary when its frames cross.
        var clientSide = new HashSet<string>();
        if (!access.CanSeeSiteLog && updates.FirstOrDefault()?.ProjectId is { } projectId)
        {
            var project = await _context.tbl_Projects_RS.IgnoreQueryFilters().AsNoTracking()
                .Where(p => p.Id == projectId).Select(p => new { p.ParentProjectId, p.InvestorId }).FirstOrDefaultAsync();
            var parentId = project?.ParentProjectId;
            var ids = await _context.tbl_ProjectMembers.IgnoreQueryFilters().AsNoTracking()
                .Where(m => (m.ProjectId == projectId || (parentId != null && m.ProjectId == parentId))
                            && m.Side == ProjectSide.Client && m.UserId != null && m.IsDeleted != true)
                .Select(m => m.UserId!).ToListAsync();
            clientSide.UnionWith(ids);
            if (project?.InvestorId is { } investor) clientSide.Add(investor);
        }

        var voiceIds = access.CanSeeSiteLog
            ? updates.Select(u => u.VoiceArtifactId).OfType<string>().ToList()
            : new List<string>();
        var texts = voiceIds.Count == 0
            ? new Dictionary<string, tbl_ArtifactText>()
            : await _context.tbl_ArtifactTexts.AsNoTracking()
                .Where(t => t.ArtifactId != null && voiceIds.Contains(t.ArtifactId))
                .ToDictionaryAsync(t => t.ArtifactId!);

        return updates.Select(u => MapUpdateToDto(u, access, face, texts, clientSide)).ToList();
    }

    /// <summary>
    /// Map an entry for one specific reader.
    /// <para>
    /// <b>The image filter is the point of P2.</b> Each frame carries its own
    /// channel and a client-side reader sees only the ones a mediator exposed —
    /// a real capture is thirteen to eighteen frames, and the batch is not the unit.
    /// </para>
    /// </summary>
    private static ProgressUpdateDto MapUpdateToDto(tbl_ProgressUpdate u, ProjectAccess access,
        string? accountableName, IReadOnlyDictionary<string, tbl_ArtifactText> texts, IReadOnlySet<string> clientSide)
    {
        var diary = access.CanSeeSiteLog;
        var visibleImages = diary
            ? u.Images ?? new List<tbl_ProgressImage>()
            : (u.Images ?? new List<tbl_ProgressImage>())
                .Where(i => i.Channel == Channel.Client)
                .ToList();
        var voiceText = u.VoiceArtifactId is not null ? texts.GetValueOrDefault(u.VoiceArtifactId) : null;

        return new ProgressUpdateDto
        {
            Id = u.Id,
            ProjectId = u.ProjectId,
            StageId = u.StageId,
            Description = u.Description,
            CompletionPercentage = u.CompletionPercentage,
            HasIssues = u.HasIssues,
            CreatedById = diary ? u.CreatedById : null,
            ApprovalStatus = u.ApprovalStatus,
            Channel = u.Channel,
            CreatedByName = diary
                ? (u.CreatedBy != null ? $"{u.CreatedBy.FirstName} {u.CreatedBy.LastName}" : null)
                : accountableName,
            StageName = u.Stage?.StageName,
            DeliverableId = u.DeliverableId,
            DeliverableTitle = u.Deliverable?.Title,
            CapturedAt = u.CapturedAt,
            ClientCaptureId = diary ? u.ClientCaptureId : null,
            // A voice note is the bench talking; it stays in the Site Diary.
            VoiceArtifactId = diary ? u.VoiceArtifactId : null,
            VoiceUrl = diary && u.VoiceArtifactId is not null ? $"/api/Artifacts/{u.VoiceArtifactId}/content" : null,
            VoiceTranscript = diary ? voiceText?.Text : null,
            VoiceTranscriptStatus = diary && u.VoiceArtifactId is not null ? voiceText?.Status ?? ArtifactTextStatus.Pending : null,
            DateTimeCreated = u.DateTimeCreated,
            ImageCount = u.Images?.Count ?? 0,
            Images = visibleImages.Select(i => new ProgressImageDto
            {
                Id = i.Id,
                ProgressUpdateId = i.ProgressUpdateId,
                ArtifactId = i.ArtifactId,
                // An artifact-backed frame streams from its permanent address;
                // a pre-P2 row falls back to the inline URL it still carries.
                ImageUrl = i.ArtifactId is not null
                    ? $"/api/Artifacts/{i.ArtifactId}/content" : i.ImageUrl,
                ThumbnailUrl = i.ArtifactId is not null
                    ? $"/api/Artifacts/{i.ArtifactId}/thumbnail" : i.ThumbnailUrl,
                Caption = i.Caption,
                DisplayOrder = i.DisplayOrder,
                Channel = i.Channel,
                ExposedById = diary ? i.ExposedById : null,
                ExposedAt = i.ExposedAt,
                Curation = diary ? i.Curation : FrameCuration.Auto,
                DateTimeCreated = i.DateTimeCreated
            }).ToList(),
            Comments = u.Comments?
                .Where(c => diary || c.Channel == Channel.Client || (c.AuthorId is not null && clientSide.Contains(c.AuthorId)))
                .Select(c =>
                {
                    var trueName = diary || (c.AuthorId is not null && clientSide.Contains(c.AuthorId));
                    return new ProgressCommentDto
                    {
                        Id = c.Id,
                        ProgressUpdateId = c.ProgressUpdateId,
                        ProgressImageId = c.ProgressImageId,
                        CommentText = c.CommentText,
                        AuthorId = trueName ? c.AuthorId : null,
                        AuthorName = !trueName ? accountableName
                            : c.Author != null ? $"{c.Author.FirstName} {c.Author.LastName}" : null,
                        AuthorProfilePicUrl = trueName ? c.Author?.ProfilePicUrl : null,
                        DateTimeCreated = c.DateTimeCreated
                    };
                }).ToList() ?? new()
        };
    }
}
