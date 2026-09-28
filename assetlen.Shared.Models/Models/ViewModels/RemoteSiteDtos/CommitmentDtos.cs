using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── Deliverables ────────────────────────────────────────────

public class DeliverableDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public DeliverableStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedByName { get; set; }

    /// <summary>Current commitments filed against it (restated versions count once).</summary>
    public int CommitmentCount { get; set; }
}

public class DeliverableCreateDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    [Required, MaxLength(40)] public string? StageId { get; set; }
    [Required, MaxLength(200)] public string? Title { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public int DisplayOrder { get; set; }
}

public class DeliverableUpdateDto
{
    [Required, MaxLength(40)] public string? Id { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public DeliverableStatus? Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int? DisplayOrder { get; set; }
}

// ─── Commitments ─────────────────────────────────────────────

public class CommitmentDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public StageGroup? StagePhase { get; set; }
    public string? DeliverableId { get; set; }
    public string? DeliverableTitle { get; set; }

    public CommitmentKind Kind { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public CommitmentMaturity Maturity { get; set; }
    public CommitmentQueryState QueryState { get; set; }
    public CommitmentSource SourceChannel { get; set; }

    /// <summary>The single accountable name (§10.1). Always shown.</summary>
    public string? AccountableMemberId { get; set; }
    public string? AccountableName { get; set; }

    public string? AgreedByName { get; set; }
    public string? AgreedWithMemberId { get; set; }

    /// <summary>The counterparty — a roster name, or the free-text party when they have no row.</summary>
    public string? AgreedWithName { get; set; }
    public DateTime? AgreedAt { get; set; }

    /// <summary>True authorship. Null when the reader sits on the client side and the author on the bench.</summary>
    public string? RecordedByName { get; set; }
    public ProjectSide? RecordedBySide { get; set; }

    /// <summary>Null when the reader's seat does not include money — see <see cref="AmountHidden"/>.</summary>
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public bool AmountHidden { get; set; }

    public DateTime? DueDate { get; set; }
    public int? LeadTimeDays { get; set; }
    public ProjectSide? OwedBySide { get; set; }

    public string? SupersedesId { get; set; }
    public string? SupersededById { get; set; }
    public DateTime? SupersededAt { get; set; }

    /// <summary>How many earlier statements this one replaced. "Restated twice" is the finding on a date.</summary>
    public int RestatementCount { get; set; }

    public DateTime? CounterpartyConfirmedAt { get; set; }
    public string? CounterpartyConfirmedByName { get; set; }
    public DateTime? DisputedAt { get; set; }
    public string? DisputedByName { get; set; }
    public string? DisputeNote { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClearedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? IngestedMessageId { get; set; }

    /// <summary>The open query on it, when there is one — the register links straight to it.</summary>
    public string? OpenQueryFlagId { get; set; }

    public string? VariationId { get; set; }
    public VariationStatus? VariationStatus { get; set; }

    public int LinkCount { get; set; }

    // ─── What the reader may do, stamped by the server ───────────
    // Never inferred from CanWrite on the client. A spoken agreement is
    // confirmed by the other side, never by the person who wrote it down.

    public bool CanConfirm { get; set; }
    public bool CanDispute { get; set; }
    public bool CanAdvance { get; set; }
    public bool CanVerify { get; set; }
    public bool CanRaiseQuery { get; set; }
    public bool CanResolveQuery { get; set; }
    public bool CanClear { get; set; }
    public bool CanRestate { get; set; }

    /// <summary>Spoken or minuted, and the other side has not yet said yes or no.</summary>
    public bool IsAwaitingCounterparty { get; set; }

    /// <summary>A date promised and passed with the work not delivered.</summary>
    public bool IsOverdue { get; set; }

    public bool IsSuperseded => SupersededAt is not null;
}

public class CommitmentCreateDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    [MaxLength(40)] public string? StageId { get; set; }
    [MaxLength(40)] public string? DeliverableId { get; set; }
    public CommitmentKind Kind { get; set; }
    [Required, MaxLength(200)] public string? Title { get; set; }
    [MaxLength(2000)] public string? Body { get; set; }

    /// <summary>Defaults to Agreed. An Idea or an open choice is recorded as such.</summary>
    public CommitmentMaturity? Maturity { get; set; }

    public CommitmentSource SourceChannel { get; set; } = CommitmentSource.App;

    [MaxLength(40)] public string? AgreedWithMemberId { get; set; }
    [MaxLength(200)] public string? AgreedWithPartyName { get; set; }
    public DateTime? AgreedAt { get; set; }

    public decimal? Amount { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    public DateTime? DueDate { get; set; }
    public int? LeadTimeDays { get; set; }
    public ProjectSide? OwedBySide { get; set; }
    [MaxLength(40)] public string? IngestedMessageId { get; set; }
}

/// <summary>A new statement of an existing commitment. The old one is kept, marked superseded.</summary>
public class CommitmentRestateDto
{
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    [MaxLength(2000)] public string? Body { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? AgreedAt { get; set; }
    public CommitmentSource? SourceChannel { get; set; }
}

public class CommitmentNoteDto
{
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    [MaxLength(1000)] public string? Note { get; set; }
}

/// <summary>Resolving a query writes into the item — "revised to 4 bags, +UGX 480,000, agreed 6 Aug".</summary>
public class CommitmentResolveDto
{
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    [Required, MaxLength(1000)] public string? Note { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? DueDate { get; set; }
}

public class CommitmentMaturityDto
{
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    public CommitmentMaturity Maturity { get; set; }
}

public class CommitmentLinkDto : BaseDto
{
    public string? CommitmentId { get; set; }
    public string? CommitmentTitle { get; set; }
    public CommitmentKind CommitmentKind { get; set; }
    public CommitmentLinkTarget TargetType { get; set; }
    public string? TargetId { get; set; }
    public CommitmentLinkRelation Relation { get; set; }
    public string? Note { get; set; }

    /// <summary>A human label for the target — a stage release, a message excerpt, a file name.</summary>
    public string? TargetLabel { get; set; }
    public DateTime? TargetDate { get; set; }
}

public class CommitmentLinkCreateDto
{
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    public CommitmentLinkTarget TargetType { get; set; }
    [Required, MaxLength(40)] public string? TargetId { get; set; }
    public CommitmentLinkRelation Relation { get; set; } = CommitmentLinkRelation.Evidence;
    [MaxLength(300)] public string? Note { get; set; }
}

/// <summary>"What did this contractor commit to on this project, and what state is each in" — one row per accountable name.</summary>
public class AccountabilityRowDto
{
    public string? MemberId { get; set; }
    public string? Name { get; set; }
    public bool IsMediator { get; set; }
    public bool IsActive { get; set; }
    public ProjectSide? Side { get; set; }
    public int Total { get; set; }
    public int Idea { get; set; }
    public int InDiscussion { get; set; }
    public int Agreed { get; set; }
    public int Delivered { get; set; }
    public int Verified { get; set; }
    public int OpenQueries { get; set; }
    public int AwaitingConfirmation { get; set; }
    public int Overdue { get; set; }
}

/// <summary>Blockers grouped by whoever has to move — the windows team, the epoxy team, the utility.</summary>
public class BlockerOwnerRowDto
{
    public string? OwnerName { get; set; }
    public string? OwnerMemberId { get; set; }
    public int Open { get; set; }
    public int OldestDaysOpen { get; set; }
    public List<FlagDto> Items { get; set; } = new();
}

public class AccountabilityDto
{
    public string? ProjectId { get; set; }
    public List<AccountabilityRowDto> Commitments { get; set; } = new();
    public List<BlockerOwnerRowDto> Blockers { get; set; } = new();
}

// ─── Variations ──────────────────────────────────────────────

public class VariationDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public string? CommitmentId { get; set; }
    public string? Title { get; set; }
    public string? Reason { get; set; }

    /// <summary>Null means not costed. A gap on the page, never a zero.</summary>
    public decimal? CostDelta { get; set; }
    public string? Currency { get; set; }
    public int? TimeDeltaDays { get; set; }
    public VariationStatus Status { get; set; }
    public string? RaisedByName { get; set; }
    public DateTime? RaisedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? DecisionNote { get; set; }

    /// <summary>The funder decides. The side proposing an extra never approves it.</summary>
    public bool CanDecide { get; set; }
    public bool IsCosted => CostDelta is not null;
}

public class VariationCreateDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    [MaxLength(40)] public string? StageId { get; set; }

    /// <summary>An existing commitment this varies. When absent, one is created so the extra sits on the register.</summary>
    [MaxLength(40)] public string? CommitmentId { get; set; }

    [Required, MaxLength(200)] public string? Title { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }
    public decimal? CostDelta { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    public int? TimeDeltaDays { get; set; }
    public DateTime? RaisedAt { get; set; }
}

public class VariationDecisionDto
{
    [Required, MaxLength(40)] public string? VariationId { get; set; }
    public bool Approve { get; set; }

    /// <summary>The figure agreed at approval, when it was not costed before.</summary>
    public decimal? CostDelta { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

// ─── Claims and the stage ledger ─────────────────────────────

public class StageClaimDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public decimal Amount { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public string? ClaimedByName { get; set; }
    public string? Note { get; set; }
    public string? EvidenceArtifactId { get; set; }
    public ClaimStatus Status { get; set; }
    public decimal? ClearedAmount { get; set; }
    public DateTime? ClearedAt { get; set; }
    public string? ClearedByName { get; set; }
    public string? QueryNote { get; set; }

    public bool CanClear { get; set; }
    public bool CanQuery { get; set; }
    public bool CanWithdraw { get; set; }

    /// <summary>What counts against the stage once cleared.</summary>
    public decimal SettledAmount => ClearedAmount ?? Amount;
}

public class StageClaimCreateDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    [Required, MaxLength(40)] public string? StageId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? ClaimedAt { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [MaxLength(40)] public string? EvidenceArtifactId { get; set; }
}

public class StageClaimDecisionDto
{
    [Required, MaxLength(40)] public string? ClaimId { get; set; }

    /// <summary>True clears the claim; false raises a query on it.</summary>
    public bool Clear { get; set; }

    /// <summary>Cleared at a different figure than claimed. Null clears the whole claim.</summary>
    public decimal? ClearedAmount { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

/// <summary>
/// One stage in the cost report: funded → claimed → cleared → carried forward
/// (assetlen.md §6). One row per stage, never combined — "too many stages
/// combined" is the sentence this exists to answer.
/// </summary>
public class StageLedgerRowDto
{
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public string? ParentStageId { get; set; }
    public int DisplayOrder { get; set; }
    public StageGroup Phase { get; set; }
    public StageStatus Status { get; set; }

    public decimal Budget { get; set; }

    /// <summary>Releases acknowledged by the delivery side, at what landed.</summary>
    public decimal Funded { get; set; }

    /// <summary>Releases still unanswered at one end or the other.</summary>
    public decimal PendingFunding { get; set; }

    /// <summary>The previous closed stage's balance, carried into this one.</summary>
    public decimal CarriedIn { get; set; }

    public decimal Claimed { get; set; }
    public decimal Cleared { get; set; }

    /// <summary>Claimed and not yet cleared or queried away.</summary>
    public decimal AwaitingClearance { get; set; }

    /// <summary>Funded plus carried in, less cleared. What the contractor holds against this stage.</summary>
    public decimal InHand { get; set; }

    /// <summary>Set only when the stage is closed: the balance that moves to the next stage.</summary>
    public decimal? CarriedForward { get; set; }

    public decimal VariationsApproved { get; set; }
    public int VariationsProposed { get; set; }
    public int VariationsUncosted { get; set; }
}

public class StageLedgerDto
{
    public string? ProjectId { get; set; }
    public string? Currency { get; set; }
    public List<StageLedgerRowDto> Rows { get; set; } = new();

    public decimal TotalBudget { get; set; }
    public decimal TotalFunded { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalClaimed { get; set; }
    public decimal TotalCleared { get; set; }
    public decimal TotalInHand { get; set; }
    public decimal TotalVariationsApproved { get; set; }
}
