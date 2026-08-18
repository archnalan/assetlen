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

    /// <summary>
    /// Where one release has got to between the two people who care about it.
    /// <para>
    /// A transfer is declared by whoever sent it and acknowledged by whoever was
    /// meant to receive it, and the two figures are not always the same — bank
    /// charges, a bad rate, a partial transfer. The gap is the whole point:
    /// somebody has to say out loud what actually landed, and the other party
    /// has to accept it or take it up. Nothing here is settled by one side.
    /// </para>
    /// </summary>
    public enum FundingStatus
    {
        /// <summary>Declared by the funder. Waiting on the delivery side to say what landed.</summary>
        Pending = 0,

        /// <summary>The delivery side confirmed the declared figure arrived in full.</summary>
        Confirmed = 1,

        /// <summary>The delivery side says nothing arrived.</summary>
        Rejected = 2,

        /// <summary>
        /// The delivery side reported a different figure. Waiting on the funder
        /// to accept it or take up the shortfall.
        /// </summary>
        AmountQueried = 3,

        /// <summary>The funder accepted a received figure that differed from what they sent.</summary>
        Settled = 4
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

    /// <summary>
    /// What a user may do inside one project. Resolved from ownership plus
    /// <c>tbl_ProjectMember</c> by <c>IProjectAccessService</c> — never inline
    /// in a DAL. Roles say <em>what</em> a user may do; this says <em>where</em>.
    /// Both must pass. Levels are ordered, so compare with <c>&gt;=</c>.
    /// </summary>
    public enum ProjectAccessLevel
    {
        /// <summary>Not a stakeholder. The project must appear not to exist.</summary>
        None = 0,
        /// <summary>Active member marked <c>Observer</c> — may look, not touch.</summary>
        Read = 1,
        /// <summary>Active member — may capture, comment and raise queries.</summary>
        Write = 2,
        /// <summary>Project owner or manager (or the parent's, for a sub-project).</summary>
        Manage = 3
    }
}
