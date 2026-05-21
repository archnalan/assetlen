namespace assetlen.Shared.Models.Models.RemoteSite
{
    public enum ProjectStatus
    {
        Active = 0,
        Completed = 1,
        OnHold = 2,
        Cancelled = 3
    }

    public enum StageStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }

    public enum FundingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Rejected = 2
    }

    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        NeedsAttention = 2
    }

    public enum SubscriptionStatus
    {
        Active = 0,
        Cancelled = 1,
        PastDue = 2,
        Free = 3
    }

    public enum RiskLevel
    {
        Green = 0,
        Yellow = 1,
        Red = 2
    }
}
