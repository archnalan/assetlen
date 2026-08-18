using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Hubs;
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

    public ProgressDAL(
        AssetlenDbContext context,
        ILogger<ProgressDAL> logger,
        ITenantProvider tenant,
        IHubContext<AssetlenHub> hub,
        IProjectAccessService access)
    {
        _context = context;
        _logger = logger;
        _tenant = tenant;
        _hub = hub;
        _access = access;
    }

    public async Task<ServiceResult<ProgressUpdateDto>> AddProgressUpdate(ProgressUpdateCreateDto dto, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project == null)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Project not found"));

            // Anyone on the project team may capture — the clerk of works is the
            // primary capturer, not the owner. Role gates at the controller
            // decide *who* may capture; membership decides *where*.
            if (!await _access.CanWriteAsync(project, userId))
                return ServiceResult<ProgressUpdateDto>.Failure(new ForbiddenException("Access denied"));

            var stage = await _context.tbl_Stages.FindAsync(dto.StageId);
            if (stage == null || stage.ProjectId != dto.ProjectId)
                return ServiceResult<ProgressUpdateDto>.Failure(new BadRequestException("Invalid stage"));

            if (dto.Images?.Count > 5)
                return ServiceResult<ProgressUpdateDto>.Failure(new BadRequestException("Maximum 5 images per update"));

            var update = new tbl_ProgressUpdate
            {
                ProjectId = dto.ProjectId,
                StageId = dto.StageId,
                Description = dto.Description,
                CompletionPercentage = dto.CompletionPercentage,
                HasIssues = dto.HasIssues,
                CreatedById = userId,
                Channel = dto.Channel,
                // Client-channel entries still flow through approval before
                // they show in the Client view, even though Channel is set.
                ApprovalStatus = ApprovalStatus.Pending
            };

            _context.tbl_ProgressUpdates.Add(update);
            await _context.SaveChangesAsync();

            // Update stage completion %
            stage.CompletionPercentage = dto.CompletionPercentage;
            if (dto.CompletionPercentage >= 100)
            {
                stage.Status = StageStatus.Completed;
                stage.ActualEndDate ??= DateTime.UtcNow;
            }
            else if (dto.CompletionPercentage > 0 && stage.Status == StageStatus.NotStarted)
            {
                stage.Status = StageStatus.InProgress;
            }

            // Save images (in production, upload to Azure Blob here)
            if (dto.Images?.Any() == true)
            {
                int order = 1;
                foreach (var img in dto.Images)
                {
                    // Interim storage: a data URI on the row. P2 replaces this with
                    // the hash-addressed Artifact store (plan.md), at which point
                    // this becomes an ArtifactId pointer.
                    var imageUrl = BuildImageUrl(img);

                    _context.tbl_ProgressImages.Add(new tbl_ProgressImage
                    {
                        ProgressUpdateId = update.Id,
                        ImageUrl = imageUrl,
                        ThumbnailUrl = imageUrl, // Same for MVP; generate thumbnails in prod
                        Caption = img.Caption,
                        DisplayOrder = order++
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetProgressUpdateById(update.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding progress update");
            return ServiceResult<ProgressUpdateDto>.Failure(new ServerErrorException(ex.Message));
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
                .Include(u => u.Images.OrderBy(i => i.DisplayOrder))
                .Include(u => u.Comments.Where(c => c.ParentCommentId == null).OrderBy(c => c.DateTimeCreated))
                    .ThenInclude(c => c.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Entry not found"));

            var access = await _access.ResolveAsync(update.Project, userId);
            if (!access.CanRead)
                return ServiceResult<ProgressUpdateDto>.Failure(new ForbiddenException("Access denied"));

            // Per-project side, not the tenant-global role. The same person can
            // be client-side on one project and mediate another; a global
            // IsExternal() check would give them one answer for both.
            if (!access.CanSeeSiteLog && update.Channel != Channel.Client)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Entry not found"));

            return ServiceResult<ProgressUpdateDto>.Success(MapUpdateToDto(update, access));
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
            if (channel == Channel.Crew)
            {
                var frames = await _context.tbl_ProgressImages
                    .Where(i => i.ProgressUpdateId == update.Id && i.Channel == Channel.Client)
                    .ToListAsync();
                foreach (var frame in frames)
                {
                    frame.Channel = Channel.Crew;
                    frame.ExposedById = null;
                    frame.ExposedAt = null;
                }
            }

            await _context.SaveChangesAsync();

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

            var dtos = updates.Select(u => MapUpdateToDto(u, access)).ToList();

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
            if (!await _access.CanWriteAsync(project, userId))
                return ServiceResult<ProgressCommentDto>.Failure(new ForbiddenException("Access denied"));

            var comment = new tbl_ProgressComment
            {
                ProgressUpdateId = dto.ProgressUpdateId,
                ProgressImageId = dto.ProgressImageId,
                CommentText = dto.CommentText,
                AuthorId = userId,
                ParentCommentId = dto.ParentCommentId
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

    /// <summary>
    /// Normalise an uploaded image into a storable URL.
    /// <para>
    /// Remote URLs pass through. Otherwise the payload is stored as a data URI
    /// with its declared content type.
    /// </para>
    /// <para>
    /// <b>Do not "simplify" this with <c>TrimStart(string.ToCharArray())</c>.</b>
    /// That overload strips <em>any</em> leading character in the set, and '/'
    /// appears in "image/jpeg" — so it ate the leading '/' of every JPEG's
    /// "/9j/..." payload and silently corrupted every photo ever uploaded.
    /// See plan.md finding A2.
    /// </para>
    /// </summary>
    private static string BuildImageUrl(ProgressImageUploadDto img)
    {
        var raw = img.Base64Image ?? string.Empty;

        if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return raw;

        // Strip an existing "data:<mime>;base64," prefix by finding the comma,
        // never by trimming characters.
        if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var comma = raw.IndexOf(',');
            if (comma >= 0) raw = raw[(comma + 1)..];
        }

        var mime = string.IsNullOrWhiteSpace(img.ContentType) ? "image/jpeg" : img.ContentType;
        return $"data:{mime};base64,{raw}";
    }

    private async Task<ServiceResult<ProgressUpdateDto>> GetProgressUpdateById(string updateId, string userId)
    {
        var update = await _context.tbl_ProgressUpdates
            .Include(u => u.Project)
                .ThenInclude(p => p!.ParentProject)
            .Include(u => u.CreatedBy)
            .Include(u => u.Stage)
            .Include(u => u.Images.OrderBy(i => i.DisplayOrder))
            .Include(u => u.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == updateId);

        if (update == null)
            return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Update not found"));

        var access = await _access.ResolveAsync(update.Project, userId);
        return ServiceResult<ProgressUpdateDto>.Success(MapUpdateToDto(update, access));
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

            var exposing = dto.Channel == Channel.Client;
            foreach (var image in images)
            {
                image.Channel = dto.Channel;
                image.ExposedById = exposing ? userId : null;
                image.ExposedAt = exposing ? DateTime.UtcNow : null;
            }

            // A frame is unreachable on an entry the client cannot open, so
            // exposing any frame carries its entry across with it.
            if (exposing && entry is not null && entry.Channel != Channel.Client)
            {
                var tracked = await _context.tbl_ProgressUpdates.FirstAsync(u => u.Id == entry.Id);
                tracked.Channel = Channel.Client;
            }

            await _context.SaveChangesAsync();

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
    private static ProgressUpdateDto MapUpdateToDto(tbl_ProgressUpdate u, ProjectAccess access)
    {
        var visibleImages = access.CanSeeSiteLog
            ? u.Images ?? new List<tbl_ProgressImage>()
            : (u.Images ?? new List<tbl_ProgressImage>())
                .Where(i => i.Channel == Channel.Client)
                .ToList();

        return new ProgressUpdateDto
        {
            Id = u.Id,
            ProjectId = u.ProjectId,
            StageId = u.StageId,
            Description = u.Description,
            CompletionPercentage = u.CompletionPercentage,
            HasIssues = u.HasIssues,
            CreatedById = u.CreatedById,
            ApprovalStatus = u.ApprovalStatus,
            Channel = u.Channel,
            CreatedByName = u.CreatedBy != null
                ? $"{u.CreatedBy.FirstName} {u.CreatedBy.LastName}" : null,
            StageName = u.Stage?.StageName,
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
                ExposedById = i.ExposedById,
                ExposedAt = i.ExposedAt,
                DateTimeCreated = i.DateTimeCreated
            }).ToList(),
            Comments = u.Comments?.Select(c => new ProgressCommentDto
            {
                Id = c.Id,
                ProgressUpdateId = c.ProgressUpdateId,
                ProgressImageId = c.ProgressImageId,
                CommentText = c.CommentText,
                AuthorId = c.AuthorId,
                AuthorName = c.Author != null
                    ? $"{c.Author.FirstName} {c.Author.LastName}" : null,
                AuthorProfilePicUrl = c.Author?.ProfilePicUrl,
                DateTimeCreated = c.DateTimeCreated
            }).ToList() ?? new()
        };
    }
}
