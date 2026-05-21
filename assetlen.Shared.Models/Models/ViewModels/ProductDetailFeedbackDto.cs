using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels
{
    /// <summary>
    /// DTO for displaying feedback
    /// </summary>
    public class ProductDetailFeedbackDto : BaseDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductDetailId { get; set; } = string.Empty;
        public string? FragmentId { get; set; }
        public string OriginalContentSnapshot { get; set; } = string.Empty;
        public string? SuggestedContent { get; set; }
        public int? RatingValue { get; set; }
        public string? CommentText { get; set; }
        public FeedbackType FeedbackType { get; set; }
        public FeedbackStatus Status { get; set; }
        public string? SuggestedByUserId { get; set; }
        public string? SuggestedByUserName { get; set; }
        public string? SuggestedByUserEmail { get; set; }
        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public DateTime? AppliedAt { get; set; }

        // Additional display properties
        public string? ProductName { get; set; }
        public string? SectionTitle { get; set; }
        public int ReplyCount { get; set; }
        public List<ProductDetailFeedbackReplyDto> Replies { get; set; } = new();

        // Approval tracking
        public int RequiredApprovals { get; set; } = 2;
        public int CurrentApprovals { get; set; } = 0;
        public List<FeedbackApprovalDto> Approvals { get; set; } = new();
        public bool IsFullyApproved => CurrentApprovals >= RequiredApprovals;
    }

    /// <summary>
    /// DTO for creating new feedback
    /// </summary>
    public class ProductDetailFeedbackCreateDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        public string ProductDetailId { get; set; } = string.Empty;

        public string? FragmentId { get; set; }

        [Required]
        public string OriginalContentSnapshot { get; set; } = string.Empty;

        public string? SuggestedContent { get; set; }

        [Range(1, 5)]
        public int? RatingValue { get; set; }

        public string? CommentText { get; set; }

        [Required]
        public FeedbackType FeedbackType { get; set; }
    }

    /// <summary>
    /// DTO for updating feedback status (admin action)
    /// </summary>
    public class ProductDetailFeedbackUpdateDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public FeedbackStatus Status { get; set; }

        public string? ReviewNotes { get; set; }
    }

    /// <summary>
    /// DTO for querying feedback
    /// </summary>
    public class ProductDetailFeedbackQueryDto
    {
        public string? ProductId { get; set; }
        public string? ProductDetailId { get; set; }
        public string? FragmentId { get; set; }
        public FeedbackType? FeedbackType { get; set; }
        public FeedbackStatus? Status { get; set; }
        public string? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = false;
    }

    /// <summary>
    /// DTO for displaying an approval record
    /// </summary>
    public class FeedbackApprovalDto
    {
        public string? Id { get; set; }
        public string FeedbackId { get; set; } = string.Empty;
        public string ApproverUserId { get; set; } = string.Empty;
        public string? ApproverUserName { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovalComment { get; set; }
        public DateTime ApprovedAt { get; set; }
    }

    /// <summary>
    /// DTO for initiating an approval
    /// </summary>
    public class FeedbackApprovalCreateDto
    {
        [Required]
        public string FeedbackId { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = true;

        public string? ApprovalComment { get; set; }
    }
}
