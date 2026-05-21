using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels
{
    /// <summary>
    /// DTO for displaying a feedback reply
    /// </summary>
    public class ProductDetailFeedbackReplyDto : BaseDto
    {
        public string FeedbackId { get; set; } = string.Empty;
        public string? ParentReplyId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsAdminReply { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Nested child replies for tree structure
        /// </summary>
        public List<ProductDetailFeedbackReplyDto> ChildReplies { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a new reply
    /// </summary>
    public class ProductDetailFeedbackReplyCreateDto
    {
        [Required]
        public string FeedbackId { get; set; } = string.Empty;

        public string? ParentReplyId { get; set; }

        [Required]
        [MinLength(1)]
        public string Message { get; set; } = string.Empty;
    }
}
