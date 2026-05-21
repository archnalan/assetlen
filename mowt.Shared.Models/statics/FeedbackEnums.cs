namespace mowt.Shared.Models.statics
{
    /// <summary>
    /// Type of feedback a reader can submit on a document fragment
    /// </summary>
    public enum FeedbackType
    {
        /// <summary>
        /// Simple rating (1-5 stars)
        /// </summary>
        Rating = 1,

        /// <summary>
        /// General comment about the content
        /// </summary>
        Comment = 2,

        /// <summary>
        /// Suggested edit with replacement content
        /// </summary>
        SuggestedEdit = 3
    }

    /// <summary>
    /// Status of feedback in the review workflow
    /// </summary>
    public enum FeedbackStatus
    {
        /// <summary>
        /// Newly submitted, awaiting review
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Under review by admin
        /// </summary>
        UnderReview = 1,

        /// <summary>
        /// Approved (for suggested edits, content will be applied)
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Rejected by admin
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Resolved/closed
        /// </summary>
        Resolved = 4
    }
}
