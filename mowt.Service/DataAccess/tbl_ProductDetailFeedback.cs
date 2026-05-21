using mowt.Shared.Models.Models;
using mowt.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess
{
    /// <summary>
    /// Feedback submitted by readers on document content fragments
    /// </summary>
    public class tbl_ProductDetailFeedback : BaseEntity
    {
        /// <summary>
        /// The book/product this feedback belongs to
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// The section (tbl_ProductDetail) this feedback belongs to
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string ProductDetailId { get; set; } = string.Empty;

        /// <summary>
        /// The unique fragment ID within the HTML content (data-fragment-id)
        /// </summary>
        [MaxLength(40)]
        public string? FragmentId { get; set; }

        /// <summary>
        /// Snapshot of the original content at time of feedback
        /// </summary>
        public string OriginalContentSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// The suggested replacement content (for SuggestedEdit type)
        /// </summary>
        public string? SuggestedContent { get; set; }

        /// <summary>
        /// Rating value (1-5) for Rating type feedback
        /// </summary>
        [Range(1, 5)]
        public int? RatingValue { get; set; }

        /// <summary>
        /// Comment text for Comment type or explanation for SuggestedEdit
        /// </summary>
        public string? CommentText { get; set; }

        /// <summary>
        /// Type of feedback: Rating, Comment, or SuggestedEdit
        /// </summary>
        public FeedbackType FeedbackType { get; set; } = FeedbackType.Comment;

        /// <summary>
        /// Current status in the review workflow
        /// </summary>
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;

        /// <summary>
        /// User ID who submitted this feedback
        /// </summary>
        [MaxLength(40)]
        public string? SuggestedByUserId { get; set; }

        /// <summary>
        /// Display name of the user who submitted (cached for performance)
        /// </summary>
        [MaxLength(100)]
        public string? SuggestedByUserName { get; set; }

        /// <summary>
        /// Email of the user for notifications
        /// </summary>
        [MaxLength(256)]
        public string? SuggestedByUserEmail { get; set; }

        /// <summary>
        /// Admin user ID who reviewed this feedback
        /// </summary>
        [MaxLength(40)]
        public string? ReviewedByUserId { get; set; }

        /// <summary>
        /// When the feedback was reviewed
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Admin notes about the review decision
        /// </summary>
        public string? ReviewNotes { get; set; }

        /// <summary>
        /// When the suggested edit was applied (if approved)
        /// </summary>
        public DateTime? AppliedAt { get; set; }

        /// <summary>
        /// Last time the user was notified about this feedback
        /// </summary>
        public DateTime? LastNotifiedAt { get; set; }

        /// <summary>
        /// Navigation property to replies
        /// </summary>
        [InverseProperty("Feedback")]
        public virtual ICollection<tbl_ProductDetailFeedbackReply>? Replies { get; set; }

        /// <summary>
        /// Navigation property to approvals (for 2-step approval process)
        /// </summary>
        [InverseProperty("Feedback")]
        public virtual ICollection<tbl_FeedbackApproval>? Approvals { get; set; }

        /// <summary>
        /// Number of approvals required before edit can be applied (default 2)
        /// </summary>
        public int RequiredApprovals { get; set; } = 2;
    }
}
