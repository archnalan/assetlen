namespace assetlen.Shared.Models.Models.ViewModels
{
    /// <summary>
    /// Statistics about feedback
    /// </summary>
    public class FeedbackStatsDto
    {
        public int TotalFeedback { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CommentsCount { get; set; }
        public int RatingsCount { get; set; }
        public int SuggestedEditsCount { get; set; }
        public double AverageRating { get; set; }
    }
}
