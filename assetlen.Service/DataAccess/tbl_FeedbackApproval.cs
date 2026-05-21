using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess
{
    /// <summary>
    /// Tracks individual approvals for feedback items requiring multi-admin approval
    /// Two approvals are required before a suggested edit can be applied
    /// </summary>
    public class tbl_FeedbackApproval : BaseEntity
    {
        /// <summary>
        /// The feedback item being approved
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string FeedbackId { get; set; } = string.Empty;

        /// <summary>
        /// The admin user who submitted this approval
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string ApproverUserId { get; set; } = string.Empty;

        /// <summary>
        /// The display name of the approver (cached)
        /// </summary>
        [MaxLength(100)]
        public string? ApproverUserName { get; set; }

        /// <summary>
        /// True if approving, false if rejecting
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Optional comment from the approver
        /// </summary>
        public string? ApprovalComment { get; set; }

        /// <summary>
        /// When this approval was submitted
        /// </summary>
        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to the feedback
        /// </summary>
        [ForeignKey("FeedbackId")]
        public virtual tbl_ProductDetailFeedback? Feedback { get; set; }
    }
}
