using mowt.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess
{
    /// <summary>
    /// Threaded replies to feedback items
    /// </summary>
    public class tbl_ProductDetailFeedbackReply : BaseEntity
    {
        /// <summary>
        /// The parent feedback this reply belongs to
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string FeedbackId { get; set; } = string.Empty;

        /// <summary>
        /// Parent reply ID for nested threading (null for top-level replies)
        /// </summary>
        [MaxLength(40)]
        public string? ParentReplyId { get; set; }

        /// <summary>
        /// User ID who posted this reply
        /// </summary>
        [MaxLength(40)]
        public string? UserId { get; set; }

        /// <summary>
        /// Display name of the replying user (cached)
        /// </summary>
        [MaxLength(100)]
        public string? UserName { get; set; }

        /// <summary>
        /// Whether this reply is from an admin
        /// </summary>
        public bool IsAdminReply { get; set; } = false;

        /// <summary>
        /// The reply message content
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to parent feedback
        /// </summary>
        [ForeignKey("FeedbackId")]
        public virtual tbl_ProductDetailFeedback? Feedback { get; set; }

        /// <summary>
        /// Navigation property to parent reply (for threading)
        /// </summary>
        [ForeignKey("ParentReplyId")]
        public virtual tbl_ProductDetailFeedbackReply? ParentReply { get; set; }

        /// <summary>
        /// Navigation property to child replies
        /// </summary>
        public virtual ICollection<tbl_ProductDetailFeedbackReply>? ChildReplies { get; set; }
    }
}
