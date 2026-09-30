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

    public string? ProjectName { get; set; }
    public StageGroup? StagePhase { get; set; }

    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public int? WorkDays { get; set; }

    /// <summary>A role — "Masons", "Aluminium team" — never a person.</summary>
    public string? Trade { get; set; }
    public string? Area { get; set; }

    /// <summary>The photo the line was ticked on; null while it is not done, or when it was set done without one.</summary>
    public string? CompletionArtifactId { get; set; }
    public string? CompletionThumbnailUrl { get; set; }
    public string? CompletionImageUrl { get; set; }

    /// <summary>Stamped by the server for this reader; never inferred on the client.</summary>
    public bool CanTick { get; set; }

    /// <summary>Every tick and reopening, oldest first. Nothing is removed from it.</summary>
    public List<DeliverableEventDto> History { get; set; } = new();
}

public class DeliverableEventDto
{
    public DeliverableEventKind Kind { get; set; }
    public DateTime OccurredAt { get; set; }

    /// <summary>The true name on the delivery side; the accountable face on the client side.</summary>
    public string? ByName { get; set; }
    public string? ArtifactId { get; set; }
    public string? ThumbnailUrl { get; set; }
}

/// <summary>The project-wide work plan: every line of the house and its sub-projects, for one reader.</summary>
public class WorkPlanDto
{
    public string? ProjectId { get; set; }
    public bool CanTick { get; set; }
    public bool CanEdit { get; set; }
    public List<DeliverableDto> Items { get; set; } = new();
}

public class DeliverableCreateDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    [Required, MaxLength(40)] public string? StageId { get; set; }
    [Required, MaxLength(200)] public string? Title { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public int? WorkDays { get; set; }
    [MaxLength(80)] public string? Trade { get; set; }
    [MaxLength(80)] public string? Area { get; set; }
}

public class DeliverableUpdateDto
{
    [Required, MaxLength(40)] public string? Id { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public DeliverableStatus? Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public int? WorkDays { get; set; }
    [MaxLength(80)] public string? Trade { get; set; }
    [MaxLength(80)] public string? Area { get; set; }
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

    // ─── What the answer changed (P8) ────────────────────────────
    // A resolved query that moved the figure reads "revised to 4 bags,
    // +UGX 480,000" on the item itself, not in a message (assetlen.md §3).

    /// <summary>The figure of the statement this one replaced. Masked with <see cref="Amount"/>.</summary>
    public decimal? PreviousAmount { get; set; }
    public DateTime? PreviousDueDate { get; set; }

    /// <summary>When the statement this one replaced had been cleared — "cleared is not closed".</summary>
    public DateTime? PreviousClearedAt { get; set; }

    /// <summary>The marked-up artifact the latest query on this item (or an earlier statement of it) was asked on.</summary>
    public string? MarkupArtifactId { get; set; }
    public string? MarkupLayerId { get; set; }
    public string? MarkupNote { get; set; }

    // ─── Parked ideas and decide-by (P8, Law 4) ──────────────────

    /// <summary>Work that must not start before this is decided — the driveway that the gate duct runs under.</summary>
    public string? DependsOnStageId { get; set; }
    public string? DependsOnStageName { get; set; }

    /// <summary>Computed backwards from the stage start and lead time. Null while waiting costs nothing.</summary>
    public DateTime? DecideBy { get; set; }
    public string? DecideByReason { get; set; }

    /// <summary>True only when waiting has started to cost something. An idea is never nagged before that.</summary>
    public bool IsSurfaced { get; set; }

    public int EstimateCount { get; set; }
    public decimal? EstimateLow { get; set; }
    public decimal? EstimateHigh { get; set; }
    public decimal? LatestEstimate { get; set; }
    public string? EstimateCurrency { get; set; }

    public bool IsParkedIdea => Maturity == CommitmentMaturity.Idea && SupersededAt is null;

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
    public bool CanAddEstimate { get; set; }
    public bool CanPark { get; set; }

    /// <summary>May put a figure on the idea — the money seat only.</summary>
    public bool CanPriceEstimate { get; set; }

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

    /// <summary>For a parked idea: the stage that must not start before it is decided.</summary>
    [MaxLength(40)] public string? DependsOnStageId { get; set; }
}

/// <summary>
/// Re-file an idea: which stage it waits for, what must not start first, how
/// long it takes once decided. Planning, not a statement of what was agreed —
/// so it edits in place, and only while nothing has been agreed.
/// </summary>
public class CommitmentParkDto
{
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    [MaxLength(40)] public string? StageId { get; set; }
    [MaxLength(40)] public string? DependsOnStageId { get; set; }
    public int? LeadTimeDays { get; set; }

    /// <summary>Set to clear the dependency rather than leave it unchanged.</summary>
    public bool ClearDependency { get; set; }
}

/// <summary>A figure somebody put on an idea — a quote, a guess, a price seen. Accumulates silently.</summary>
public class CommitmentEstimateDto : BaseDto
{
    public string? CommitmentId { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public bool AmountHidden { get; set; }
    public string? Note { get; set; }
    public string? RecordedByName { get; set; }
    public DateTime? RecordedAt { get; set; }
    public string? IngestedMessageId { get; set; }
    public string? ArtifactId { get; set; }
}

public class CommitmentEstimateCreateDto
{
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    public decimal? Amount { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [MaxLength(40)] public string? IngestedMessageId { get; set; }
    [MaxLength(40)] public string? ArtifactId { get; set; }
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

    /// <summary>The file behind an artifact or markup link, so the reader can open it and mark it up.</summary>
    public string? ArtifactId { get; set; }
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

    /// <summary>The proof the claim carries — frames, deliverables signed off, the latest reading.</summary>
    public List<ClaimEvidenceDto> Evidence { get; set; } = new();
}

public class StageClaimCreateDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    [Required, MaxLength(40)] public string? StageId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? ClaimedAt { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [MaxLength(40)] public string? EvidenceArtifactId { get; set; }

    /// <summary>Captured frames to carry. Attaching one sends it across in the accountable face's name.</summary>
    public List<string> EvidenceImageIds { get; set; } = new();

    public List<string> EvidenceDeliverableIds { get; set; } = new();

    public bool AttachLatestReading { get; set; }
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
