using assetlen.Shared.Models.statics;

namespace assetlen.Shared.Models.Models.ViewModels
{
    /// <summary>
    /// Real-time event payload for feedback updates via SignalR
    /// </summary>
    public class FeedbackRealtimeEventDto
    {
        /// <summary>
        /// Type of event
        /// </summary>
        public FeedbackEventType EventType { get; set; }

        /// <summary>
        /// The feedback ID this event relates to
        /// </summary>
        public string FeedbackId { get; set; } = string.Empty;

        /// <summary>
        /// The product/book ID
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// The section ID
        /// </summary>
        public string ProductDetailId { get; set; } = string.Empty;

        /// <summary>
        /// The fragment ID within the section
        /// </summary>
        public string? FragmentId { get; set; }

        /// <summary>
        /// New reply data (if event is NewReply)
        /// </summary>
        public ProductDetailFeedbackReplyDto? NewReply { get; set; }

        /// <summary>
        /// Status change info (if event is StatusChanged)
        /// </summary>
        public FeedbackStatus? NewStatus { get; set; }
        public FeedbackStatus? OldStatus { get; set; }

        /// <summary>
        /// User ID who triggered the event
        /// </summary>
        public string? TriggeredByUserId { get; set; }

        /// <summary>
        /// User name who triggered the event
        /// </summary>
        public string? TriggeredByUserName { get; set; }

        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Types of real-time feedback events
    /// </summary>
    public enum FeedbackEventType
    {
        NewReply = 1,
        StatusChanged = 2,
        FeedbackCreated = 3,
        FeedbackDeleted = 4
    }
}
