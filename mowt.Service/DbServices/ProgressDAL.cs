using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models.RemoteSite;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace mowt.Service.DbServices;

public class ProgressDAL : IProgressDAL
{
    private readonly mowtDbContext _context;
    private readonly ILogger<ProgressDAL> _logger;

    public ProgressDAL(mowtDbContext context, ILogger<ProgressDAL> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<ProgressUpdateDto>> AddProgressUpdate(ProgressUpdateCreateDto dto, string userId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS.FindAsync(dto.ProjectId);
            if (project == null)
                return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Project not found"));

            // PM or investor can add updates
            if (project.InvestorId != userId && project.ProjectManagerId != userId)
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
                    // For MVP: store base64 as URL placeholder.
                    // In production: convert to blob URL via Azure Blob Storage service.
                    var imageUrl = img.Base64Image.StartsWith("http")
                        ? img.Base64Image
                        : $"data:image/jpeg;base64,{img.Base64Image.TrimStart("data:image/jpeg;base64,".ToCharArray())}";

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
            var project = await _context.tbl_Projects_RS.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project == null)
                return ServiceResult<PaginationDetails<ProgressUpdateDto>>.Failure(new NotFoundException("Project not found"));

            if (project.InvestorId != userId && project.ProjectManagerId != userId)
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

            var total = await query.CountAsync(ct);

            var updates = await query
                .OrderByDescending(u => u.DateTimeCreated)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var dtos = updates.Select(u => MapUpdateToDto(u)).ToList();

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
            // Validate the user has access to the project
            tbl_Project? project = null;

            if (!string.IsNullOrEmpty(dto.ProgressUpdateId))
            {
                var update = await _context.tbl_ProgressUpdates
                    .Include(u => u.Project)
                    .FirstOrDefaultAsync(u => u.Id == dto.ProgressUpdateId);
                project = update?.Project;
            }
            else if (!string.IsNullOrEmpty(dto.ProgressImageId))
            {
                var image = await _context.tbl_ProgressImages
                    .Include(i => i.ProgressUpdate)
                        .ThenInclude(u => u!.Project)
                    .FirstOrDefaultAsync(i => i.Id == dto.ProgressImageId);
                project = image?.ProgressUpdate?.Project;
            }

            if (project == null)
                return ServiceResult<ProgressCommentDto>.Failure(new NotFoundException("Target not found"));

            if (project.InvestorId != userId && project.ProjectManagerId != userId)
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

            return ServiceResult<ProgressCommentDto>.Success(new ProgressCommentDto
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
            });
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

    private async Task<ServiceResult<ProgressUpdateDto>> GetProgressUpdateById(string updateId, string userId)
    {
        var update = await _context.tbl_ProgressUpdates
            .Include(u => u.CreatedBy)
            .Include(u => u.Stage)
            .Include(u => u.Images.OrderBy(i => i.DisplayOrder))
            .Include(u => u.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == updateId);

        if (update == null)
            return ServiceResult<ProgressUpdateDto>.Failure(new NotFoundException("Update not found"));

        return ServiceResult<ProgressUpdateDto>.Success(MapUpdateToDto(update));
    }

    private static ProgressUpdateDto MapUpdateToDto(tbl_ProgressUpdate u)
    {
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
            CreatedByName = u.CreatedBy != null
                ? $"{u.CreatedBy.FirstName} {u.CreatedBy.LastName}" : null,
            StageName = u.Stage?.StageName,
            DateTimeCreated = u.DateTimeCreated,
            Images = u.Images?.Select(i => new ProgressImageDto
            {
                Id = i.Id,
                ProgressUpdateId = i.ProgressUpdateId,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                Caption = i.Caption,
                DisplayOrder = i.DisplayOrder,
                DateTimeCreated = i.DateTimeCreated
            }).ToList() ?? new(),
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
