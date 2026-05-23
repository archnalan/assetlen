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

    /// <summary>
    /// Visibility channel for collaboration items (Site Journal entries,
    /// Stream messages, Flags). Default is Crew (fail-closed) — items are
    /// internal to the contractor org until explicitly promoted to Client.
    /// </summary>
    public enum Channel
    {
        Crew = 0,
        Client = 1
    }

    /// <summary>
    /// Lifecycle of a Flag (site issue raised on a Journal entry or media item).
    /// Open Flags receive weekly nudges until Resolved or Archived.
    /// </summary>
    public enum FlagStatus
    {
        Open = 0,
        InProgress = 1,
        Resolved = 2,
        Archived = 3
    }

    public enum FlagSeverity
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// Category buckets for a project budget line item. Drives the
    /// breakdown chart on the Budget tab. Extending requires a UI change
    /// because the chart uses a fixed swatch palette.
    /// </summary>
    public enum BudgetCategory
    {
        Materials = 0,
        Labor = 1,
        Equipment = 2,
        Permits = 3,
        Contingency = 4,
        Other = 99
    }
}
