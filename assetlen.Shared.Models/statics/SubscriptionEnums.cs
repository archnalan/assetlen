namespace assetlen.Shared.Models.statics
{
    /// <summary>
    /// Status of an enterprise subscription request in the review workflow
    /// </summary>
    public enum SubscriptionRequestStatus
    {
        /// <summary>Newly submitted, awaiting review</summary>
        Pending = 0,

        /// <summary>Ministry staff is reviewing the request</summary>
        UnderReview = 1,

        /// <summary>A quote has been issued to the applicant</summary>
        Quoted = 2,

        /// <summary>Payment has been confirmed against the quote</summary>
        PaymentConfirmed = 3,

        /// <summary>Subscription is active — user seats have been provisioned</summary>
        Active = 4,

        /// <summary>Request declined or cancelled</summary>
        Declined = 5,

        /// <summary>Subscription period has expired</summary>
        Expired = 6
    }

    /// <summary>
    /// Category of the applying entity
    /// </summary>
    public enum EnterpriseEntityType
    {
        Company = 1,
        School = 2,
        Government = 3,
        NGO = 4,
        Other = 5
    }
}
