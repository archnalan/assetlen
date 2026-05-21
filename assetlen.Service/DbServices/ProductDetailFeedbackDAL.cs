using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using System.Linq.Dynamic.Core;

namespace assetlen.Service.DbServices
{
    public class ProductDetailFeedbackDAL : IProductDetailFeedbackDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<ProductDetailFeedbackDAL> _logger;
        private readonly IFeedbackNotificationService _notificationService;

        public ProductDetailFeedbackDAL(
            mowtDbContext context,
            ILogger<ProductDetailFeedbackDAL> logger,
            IFeedbackNotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        #region Feedback CRUD

        public async Task<ServiceResult<ProductDetailFeedbackDto>> CreateFeedback(
            ProductDetailFeedbackCreateDto dto,
            string userId,
            string userName,
            string? userEmail,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = new tbl_ProductDetailFeedback
                {
                    ProductId = dto.ProductId,
                    ProductDetailId = dto.ProductDetailId,
                    FragmentId = dto.FragmentId,
                    OriginalContentSnapshot = dto.OriginalContentSnapshot,
                    SuggestedContent = dto.SuggestedContent,
                    RatingValue = dto.RatingValue,
                    CommentText = dto.CommentText,
                    FeedbackType = dto.FeedbackType,
                    Status = FeedbackStatus.Pending,
                    SuggestedByUserId = userId,
                    SuggestedByUserName = userName,
                    SuggestedByUserEmail = userEmail,
                    DateTimeCreated = DateTime.UtcNow
                };

                _context.tbl_ProductDetailFeedbacks.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                var result = entity.Adapt<ProductDetailFeedbackDto>();

                // Get additional info
                var section = await _context.tbl_ProductDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.ProductDetailId, cancellationToken);
                if (section != null)
                {
                    result.SectionTitle = section.Title;
                }

                var product = await _context.tbl_Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken);
                if (product != null)
                {
                    result.ProductName = product.ProductName;
                }

                // Trigger real-time notification
                await _notificationService.NotifyFeedbackCreated(result);

                return ServiceResult<ProductDetailFeedbackDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback");
                return ServiceResult<ProductDetailFeedbackDto>.Failure(
                    new ServerErrorException("Error creating feedback"));
            }
        }

        public async Task<ServiceResult<ProductDetailFeedbackDto>> GetFeedbackById(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_ProductDetailFeedbacks
                    .AsNoTracking()
                    .Include(x => x.Replies!.Where(r => r.IsDeleted != true))
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (entity == null)
                    return ServiceResult<ProductDetailFeedbackDto>.Failure(
                        new NotFoundException("Feedback not found"));

                var result = entity.Adapt<ProductDetailFeedbackDto>();
                result.Replies = BuildReplyTree(entity.Replies?.ToList() ?? new());
                result.ReplyCount = entity.Replies?.Count ?? 0;

                // Get section and product info
                await EnrichFeedbackDto(result, cancellationToken);

                return ServiceResult<ProductDetailFeedbackDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback by id");
                return ServiceResult<ProductDetailFeedbackDto>.Failure(
                    new ServerErrorException("Error getting feedback"));
            }
        }

        public async Task<ServiceResult<PaginationDetails<ProductDetailFeedbackDto>>> GetFeedback(
            ProductDetailFeedbackQueryDto query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var queryable = _context.tbl_ProductDetailFeedbacks
                    .AsNoTracking()
                    .Include(x => x.Replies)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(query.ProductId))
                    queryable = queryable.Where(x => x.ProductId == query.ProductId);

                if (!string.IsNullOrEmpty(query.ProductDetailId))
                    queryable = queryable.Where(x => x.ProductDetailId == query.ProductDetailId);

                if (!string.IsNullOrEmpty(query.FragmentId))
                    queryable = queryable.Where(x => x.FragmentId == query.FragmentId);

                if (query.FeedbackType.HasValue)
                    queryable = queryable.Where(x => x.FeedbackType == query.FeedbackType.Value);

                if (query.Status.HasValue)
                    queryable = queryable.Where(x => x.Status == query.Status.Value);

                if (!string.IsNullOrEmpty(query.UserId))
                    queryable = queryable.Where(x => x.SuggestedByUserId == query.UserId);

                if (query.FromDate.HasValue)
                    queryable = queryable.Where(x => x.DateTimeCreated >= query.FromDate.Value);

                if (query.ToDate.HasValue)
                    queryable = queryable.Where(x => x.DateTimeCreated <= query.ToDate.Value);

                var totalCount = await queryable.CountAsync(cancellationToken);

                // Apply sorting
                var sortColumn = string.IsNullOrEmpty(query.SortBy) ? "DateTimeCreated" : query.SortBy;
                var sortDirection = query.SortAscending ? "asc" : "desc";
                queryable = queryable.OrderBy($"{sortColumn} {sortDirection}");

                // Apply pagination
                var entities = await queryable
                    .Skip(query.Offset)
                    .Take(query.Limit)
                    .ToListAsync(cancellationToken);

                var dtos = entities.Select(e =>
                {
                    var dto = e.Adapt<ProductDetailFeedbackDto>();
                    dto.ReplyCount = e.Replies?.Count(r => r.IsDeleted != true) ?? 0;
                    return dto;
                }).ToList();

                // Enrich with product/section info
                foreach (var dto in dtos)
                {
                    await EnrichFeedbackDto(dto, cancellationToken);
                }

                var result = new PaginationDetails<ProductDetailFeedbackDto>
                {
                    TotalSize = totalCount,
                    Limit = query.Limit,
                    OffSet = query.Offset,
                    Data = dtos,
                    IsNext = totalCount > query.Offset + query.Limit
                };

                return ServiceResult<PaginationDetails<ProductDetailFeedbackDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying feedback");
                return ServiceResult<PaginationDetails<ProductDetailFeedbackDto>>.Failure(
                    new ServerErrorException("Error querying feedback"));
            }
        }

        public async Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetFeedbackByProductId(
            string productId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await _context.tbl_ProductDetailFeedbacks
                    .AsNoTracking()
                    .Where(x => x.ProductId == productId)
                    .Include(x => x.Replies!.Where(r => r.IsDeleted != true))
                    .OrderByDescending(x => x.DateTimeCreated)
                    .ToListAsync(cancellationToken);

                var result = entities.Select(e =>
                {
                    var dto = e.Adapt<ProductDetailFeedbackDto>();
                    dto.Replies = BuildReplyTree(e.Replies?.ToList() ?? new());
                    dto.ReplyCount = e.Replies?.Count ?? 0;
                    return dto;
                }).ToList();

                return ServiceResult<List<ProductDetailFeedbackDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback by product id");
                return ServiceResult<List<ProductDetailFeedbackDto>>.Failure(
                    new ServerErrorException("Error getting feedback"));
            }
        }

        public async Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetFeedbackByProductDetailId(
            string productDetailId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await _context.tbl_ProductDetailFeedbacks
                    .AsNoTracking()
                    .Where(x => x.ProductDetailId == productDetailId)
                    .Include(x => x.Replies!.Where(r => r.IsDeleted != true))
                    .OrderByDescending(x => x.DateTimeCreated)
                    .ToListAsync(cancellationToken);

                var result = entities.Select(e =>
                {
                    var dto = e.Adapt<ProductDetailFeedbackDto>();
                    dto.Replies = BuildReplyTree(e.Replies?.ToList() ?? new());
                    dto.ReplyCount = e.Replies?.Count ?? 0;
                    return dto;
                }).ToList();

                return ServiceResult<List<ProductDetailFeedbackDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback by product detail id");
                return ServiceResult<List<ProductDetailFeedbackDto>>.Failure(
                    new ServerErrorException("Error getting feedback"));
            }
        }

        public async Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetFeedbackByFragment(
            string productDetailId,
            string fragmentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await _context.tbl_ProductDetailFeedbacks
                    .AsNoTracking()
                    .Where(x => x.ProductDetailId == productDetailId && x.FragmentId == fragmentId)
                    .Include(x => x.Replies!.Where(r => r.IsDeleted != true))
                    .OrderByDescending(x => x.DateTimeCreated)
                    .ToListAsync(cancellationToken);

                var result = entities.Select(e =>
                {
                    var dto = e.Adapt<ProductDetailFeedbackDto>();
                    dto.Replies = BuildReplyTree(e.Replies?.ToList() ?? new());
                    dto.ReplyCount = e.Replies?.Count ?? 0;
                    return dto;
                }).ToList();

                return ServiceResult<List<ProductDetailFeedbackDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback by fragment");
                return ServiceResult<List<ProductDetailFeedbackDto>>.Failure(
                    new ServerErrorException("Error getting feedback"));
            }
        }

        public async Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetMyFeedback(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await _context.tbl_ProductDetailFeedbacks
                    .AsNoTracking()
                    .Where(x => x.SuggestedByUserId == userId)
                    .Include(x => x.Replies!.Where(r => r.IsDeleted != true))
                    .OrderByDescending(x => x.DateTimeCreated)
                    .ToListAsync(cancellationToken);

                var result = entities.Select(e =>
                {
                    var dto = e.Adapt<ProductDetailFeedbackDto>();
                    dto.Replies = BuildReplyTree(e.Replies?.ToList() ?? new());
                    dto.ReplyCount = e.Replies?.Count ?? 0;
                    return dto;
                }).ToList();

                // Enrich with product/section info
                foreach (var dto in result)
                {
                    await EnrichFeedbackDto(dto, cancellationToken);
                }

                return ServiceResult<List<ProductDetailFeedbackDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user feedback");
                return ServiceResult<List<ProductDetailFeedbackDto>>.Failure(
                    new ServerErrorException("Error getting feedback"));
            }
        }

        public async Task<ServiceResult<bool>> DeleteFeedback(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_ProductDetailFeedbacks
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (entity == null)
                    return ServiceResult<bool>.Failure(new NotFoundException("Feedback not found"));

                entity.IsDeleted = true;
                entity.DateTimeModified = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback");
                return ServiceResult<bool>.Failure(new ServerErrorException("Error deleting feedback"));
            }
        }

        #endregion

        #region Admin Actions

        public async Task<ServiceResult<ProductDetailFeedbackDto>> UpdateFeedbackStatus(
            ProductDetailFeedbackUpdateDto dto,
            string reviewerUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_ProductDetailFeedbacks
                    .Include(x => x.Replies)
                    .FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken);

                if (entity == null)
                    return ServiceResult<ProductDetailFeedbackDto>.Failure(
                        new NotFoundException("Feedback not found"));

                var oldStatus = entity.Status;
                entity.Status = dto.Status;
                entity.ReviewedByUserId = reviewerUserId;
                entity.ReviewedAt = DateTime.UtcNow;
                entity.ReviewNotes = dto.ReviewNotes;
                entity.DateTimeModified = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                var result = entity.Adapt<ProductDetailFeedbackDto>();
                result.Replies = BuildReplyTree(entity.Replies?.ToList() ?? new());

                // Notify user of status change
                await _notificationService.NotifyStatusChanged(result, oldStatus, dto.Status);

                return ServiceResult<ProductDetailFeedbackDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feedback status");
                return ServiceResult<ProductDetailFeedbackDto>.Failure(
                    new ServerErrorException("Error updating feedback status"));
            }
        }

        public async Task<ServiceResult<bool>> ApplySuggestedEdit(
            string feedbackId,
            string adminUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var feedback = await _context.tbl_ProductDetailFeedbacks
                    .FirstOrDefaultAsync(x => x.Id == feedbackId, cancellationToken);

                if (feedback == null)
                    return ServiceResult<bool>.Failure(new NotFoundException("Feedback not found"));

                if (feedback.FeedbackType != FeedbackType.SuggestedEdit)
                    return ServiceResult<bool>.Failure(
                        new BadRequestException("Only suggested edits can be applied"));

                if (string.IsNullOrEmpty(feedback.SuggestedContent))
                    return ServiceResult<bool>.Failure(
                        new BadRequestException("No suggested content to apply"));

                // Get the section
                var section = await _context.tbl_ProductDetails
                    .FirstOrDefaultAsync(x => x.Id == feedback.ProductDetailId, cancellationToken);

                if (section == null)
                    return ServiceResult<bool>.Failure(new NotFoundException("Section not found"));

                // Apply the edit - replace fragment content in HTML
                if (!string.IsNullOrEmpty(feedback.FragmentId))
                {
                    section.Content = ReplaceFragmentContent(
                        section.Content,
                        feedback.FragmentId,
                        feedback.SuggestedContent);
                }
                else
                {
                    // If no fragment ID, replace entire section content
                    section.Content = feedback.SuggestedContent;
                }

                section.DateTimeModified = DateTime.UtcNow;

                // Update feedback status
                feedback.Status = FeedbackStatus.Approved;
                feedback.AppliedAt = DateTime.UtcNow;
                feedback.ReviewedByUserId = adminUserId;
                feedback.ReviewedAt = DateTime.UtcNow;
                feedback.DateTimeModified = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                // Notify user
                var feedbackDto = feedback.Adapt<ProductDetailFeedbackDto>();
                await _notificationService.NotifyEditApplied(feedbackDto);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying suggested edit");
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Error applying suggested edit"));
            }
        }

        #endregion

        #region Replies

        public async Task<ServiceResult<ProductDetailFeedbackReplyDto>> CreateReply(
            ProductDetailFeedbackReplyCreateDto dto,
            string userId,
            string userName,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verify feedback exists
                var feedback = await _context.tbl_ProductDetailFeedbacks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.FeedbackId, cancellationToken);

                if (feedback == null)
                    return ServiceResult<ProductDetailFeedbackReplyDto>.Failure(
                        new NotFoundException("Feedback not found"));

                var entity = new tbl_ProductDetailFeedbackReply
                {
                    FeedbackId = dto.FeedbackId,
                    ParentReplyId = dto.ParentReplyId,
                    UserId = userId,
                    UserName = userName,
                    IsAdminReply = isAdmin,
                    Message = dto.Message,
                    DateTimeCreated = DateTime.UtcNow
                };

                _context.tbl_ProductDetailFeedbackReplies.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                var result = entity.Adapt<ProductDetailFeedbackReplyDto>();

                // Notify via real-time and email
                await _notificationService.NotifyNewReply(feedback, result, isAdmin);

                return ServiceResult<ProductDetailFeedbackReplyDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reply");
                return ServiceResult<ProductDetailFeedbackReplyDto>.Failure(
                    new ServerErrorException("Error creating reply"));
            }
        }

        public async Task<ServiceResult<List<ProductDetailFeedbackReplyDto>>> GetRepliesByFeedbackId(
            string feedbackId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entities = await _context.tbl_ProductDetailFeedbackReplies
                    .AsNoTracking()
                    .Where(x => x.FeedbackId == feedbackId)
                    .OrderBy(x => x.DateTimeCreated)
                    .ToListAsync(cancellationToken);

                var result = BuildReplyTree(entities);
                return ServiceResult<List<ProductDetailFeedbackReplyDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting replies");
                return ServiceResult<List<ProductDetailFeedbackReplyDto>>.Failure(
                    new ServerErrorException("Error getting replies"));
            }
        }

        public async Task<ServiceResult<bool>> DeleteReply(
            string replyId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_ProductDetailFeedbackReplies
                    .FirstOrDefaultAsync(x => x.Id == replyId, cancellationToken);

                if (entity == null)
                    return ServiceResult<bool>.Failure(new NotFoundException("Reply not found"));

                entity.IsDeleted = true;
                entity.DateTimeModified = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reply");
                return ServiceResult<bool>.Failure(new ServerErrorException("Error deleting reply"));
            }
        }

        #endregion

        #region Approval Workflow

        public async Task<ServiceResult<FeedbackApprovalDto>> InitiateApproval(
            FeedbackApprovalCreateDto dto,
            string userId,
            string userName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if feedback exists
                var feedback = await _context.tbl_ProductDetailFeedbacks
                    .Include(f => f.Approvals)
                    .FirstOrDefaultAsync(x => x.Id == dto.FeedbackId, cancellationToken);

                if (feedback == null)
                    return ServiceResult<FeedbackApprovalDto>.Failure(new NotFoundException("Feedback not found"));

                // Check if user has already approved
                var existingApproval = feedback.Approvals?
                    .FirstOrDefault(a => a.ApproverUserId == userId && a.IsDeleted != true);

                if (existingApproval != null)
                    return ServiceResult<FeedbackApprovalDto>.Failure(
                        new BadRequestException("You have already submitted an approval for this feedback"));

                // Create the approval record
                var approval = new tbl_FeedbackApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    FeedbackId = dto.FeedbackId,
                    ApproverUserId = userId,
                    ApproverUserName = userName,
                    IsApproved = dto.IsApproved,
                    ApprovalComment = dto.ApprovalComment,
                    ApprovedAt = DateTime.UtcNow,
                    DateTimeCreated = DateTime.UtcNow,
                    TenantId = feedback.TenantId
                };

                _context.tbl_FeedbackApprovals.Add(approval);

                // Count current approvals (including this new one)
                var approvalCount = (feedback.Approvals?.Count(a => a.IsApproved && a.IsDeleted != true) ?? 0)
                    + (dto.IsApproved ? 1 : 0);

                // Check if we've reached the required approvals
                if (approvalCount >= feedback.RequiredApprovals && dto.IsApproved)
                {
                    // Fully approved - apply the edit if it's a suggested edit
                    feedback.Status = FeedbackStatus.Approved;
                    feedback.ReviewedAt = DateTime.UtcNow;
                    feedback.ReviewedByUserId = userId;

                    if (feedback.FeedbackType == FeedbackType.SuggestedEdit && !feedback.AppliedAt.HasValue)
                    {
                        // Auto-apply the edit now that it's fully approved
                        await ApplyEditToDocument(feedback, cancellationToken);
                        feedback.AppliedAt = DateTime.UtcNow;
                        feedback.Status = FeedbackStatus.Resolved;
                    }
                }
                else if (approvalCount == 1 && dto.IsApproved)
                {
                    // First approval - move to Under Review
                    feedback.Status = FeedbackStatus.UnderReview;
                }

                feedback.DateTimeModified = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                var result = approval.Adapt<FeedbackApprovalDto>();
                return ServiceResult<FeedbackApprovalDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating approval");
                return ServiceResult<FeedbackApprovalDto>.Failure(
                    new ServerErrorException("Error initiating approval"));
            }
        }

        public async Task<ServiceResult<List<FeedbackApprovalDto>>> GetApprovalsByFeedbackId(
            string feedbackId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var approvals = await _context.tbl_FeedbackApprovals
                    .AsNoTracking()
                    .Where(a => a.FeedbackId == feedbackId && a.IsDeleted != true)
                    .OrderByDescending(a => a.ApprovedAt)
                    .ToListAsync(cancellationToken);

                var result = approvals.Adapt<List<FeedbackApprovalDto>>();
                return ServiceResult<List<FeedbackApprovalDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approvals");
                return ServiceResult<List<FeedbackApprovalDto>>.Failure(
                    new ServerErrorException("Error getting approvals"));
            }
        }

        public async Task<ServiceResult<bool>> HasUserApproved(
            string feedbackId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var hasApproved = await _context.tbl_FeedbackApprovals
                    .AsNoTracking()
                    .AnyAsync(a => a.FeedbackId == feedbackId
                        && a.ApproverUserId == userId
                        && a.IsDeleted != true, cancellationToken);

                return ServiceResult<bool>.Success(hasApproved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user approval");
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Error checking approval status"));
            }
        }

        private async Task ApplyEditToDocument(tbl_ProductDetailFeedback feedback, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(feedback.FragmentId) || string.IsNullOrEmpty(feedback.SuggestedContent))
                return;

            var section = await _context.tbl_ProductDetails
                .FirstOrDefaultAsync(x => x.Id == feedback.ProductDetailId, cancellationToken);

            if (section == null || string.IsNullOrEmpty(section.Content))
                return;

            // Replace the fragment content with the suggested content
            section.Content = ReplaceFragmentContent(section.Content, feedback.FragmentId, feedback.SuggestedContent);
            section.DateTimeModified = DateTime.UtcNow;
        }

        #endregion

        #region Statistics

        public async Task<ServiceResult<FeedbackStatsDto>> GetFeedbackStats(
            string? productId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.tbl_ProductDetailFeedbacks.AsNoTracking();

                if (!string.IsNullOrEmpty(productId))
                    query = query.Where(x => x.ProductId == productId);

                var stats = new FeedbackStatsDto
                {
                    TotalFeedback = await query.CountAsync(cancellationToken),
                    PendingCount = await query.CountAsync(x => x.Status == FeedbackStatus.Pending, cancellationToken),
                    ApprovedCount = await query.CountAsync(x => x.Status == FeedbackStatus.Approved, cancellationToken),
                    RejectedCount = await query.CountAsync(x => x.Status == FeedbackStatus.Rejected, cancellationToken),
                    CommentsCount = await query.CountAsync(x => x.FeedbackType == FeedbackType.Comment, cancellationToken),
                    RatingsCount = await query.CountAsync(x => x.FeedbackType == FeedbackType.Rating, cancellationToken),
                    SuggestedEditsCount = await query.CountAsync(x => x.FeedbackType == FeedbackType.SuggestedEdit, cancellationToken),
                    AverageRating = await query
                        .Where(x => x.FeedbackType == FeedbackType.Rating && x.RatingValue.HasValue)
                        .AverageAsync(x => (double?)x.RatingValue ?? 0, cancellationToken)
                };

                return ServiceResult<FeedbackStatsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback stats");
                return ServiceResult<FeedbackStatsDto>.Failure(
                    new ServerErrorException("Error getting feedback stats"));
            }
        }

        #endregion

        #region Private Helpers

        private List<ProductDetailFeedbackReplyDto> BuildReplyTree(List<tbl_ProductDetailFeedbackReply> flatReplies)
        {
            var lookup = flatReplies
                .Select(r => r.Adapt<ProductDetailFeedbackReplyDto>())
                .ToDictionary(r => r.Id!);

            var roots = new List<ProductDetailFeedbackReplyDto>();

            foreach (var reply in lookup.Values)
            {
                if (string.IsNullOrEmpty(reply.ParentReplyId))
                {
                    roots.Add(reply);
                }
                else if (lookup.TryGetValue(reply.ParentReplyId, out var parent))
                {
                    parent.ChildReplies.Add(reply);
                }
            }

            return roots.OrderBy(r => r.DateTimeCreated).ToList();
        }

        private async Task EnrichFeedbackDto(ProductDetailFeedbackDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(dto.SectionTitle))
            {
                var section = await _context.tbl_ProductDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.ProductDetailId, cancellationToken);
                dto.SectionTitle = section?.Title;
            }

            if (string.IsNullOrEmpty(dto.ProductName))
            {
                var product = await _context.tbl_Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken);
                dto.ProductName = product?.ProductName;
            }
        }

        private string ReplaceFragmentContent(string html, string fragmentId, string newContent)
        {
            // Simple regex-based replacement for fragment content
            // Pattern: <tag data-fragment-id="GUID">...content...</tag>
            var pattern = $@"(<[^>]+data-fragment-id=""{fragmentId}""[^>]*>)(.*?)(</[^>]+>)";
            var replacement = $"$1{System.Web.HttpUtility.HtmlEncode(newContent)}$3";

            return System.Text.RegularExpressions.Regex.Replace(
                html,
                pattern,
                replacement,
                System.Text.RegularExpressions.RegexOptions.Singleline);
        }

        #endregion
    }
}
