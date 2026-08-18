using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── Project DTOs ────────────────────────────────────────────

public class ProjectDto : BaseDto
{
    [MaxLength(200)]
    public string? ProjectName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public decimal? TotalBudget { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public DateTime? RevisedCompletionDate { get; set; }
    public string? InvestorId { get; set; }
    public string? ProjectManagerId { get; set; }
    public string? CoverImageUrl { get; set; }
    public ProjectStatus Status { get; set; }

    /// <summary>Set while this project sits in the bin. See <c>tbl_Project.ArchivedAt</c>.</summary>
    public DateTime? ArchivedAt { get; set; }

    /// <summary>When the bin empties this one, or null while it is live.</summary>
    public DateTime? PurgeDueAt { get; set; }

    public bool IsFirstFreeProject { get; set; }
    public bool IsSubscriptionActive { get; set; }
    public string Currency { get; set; } = "UGX";

    // Computed / populated by service
    public string? InvestorName { get; set; }
    public string? ProjectManagerName { get; set; }
    public decimal FundedPercentage { get; set; }
    public decimal CompletedPercentage { get; set; }
    public decimal TotalFunded { get; set; }
    public decimal TotalRemaining { get; set; }
    public string? CurrentStageName { get; set; }
    public RiskLevel RiskLevel { get; set; }

    // Sub-project nesting
    public string? ParentProjectId { get; set; }
    public string? ParentProjectName { get; set; }

    public List<StageDto> Stages { get; set; } = new();
    public List<ProjectCardDto> SubProjects { get; set; } = new();
}

public class ProjectCreateDto
{
    [MaxLength(200)]
    public string? ProjectName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public decimal? TotalBudget { get; set; }

    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ExpectedCompletionDate { get; set; }
    public string? ProjectManagerId { get; set; }
    public string Currency { get; set; } = "UGX";

    // Set when creating a Sub-project. The service enforces a one-level limit:
    // the parent referenced here must itself have ParentProjectId == null.
    public string? ParentProjectId { get; set; }

    public List<StageCreateDto> Stages { get; set; } = new();
}

// ─── Project Member DTOs ─────────────────────────────────────

public class ProjectMemberDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? UserId { get; set; }

    /// <summary>Set when this is an off-platform party with no login.</summary>
    public string? PartyName { get; set; }

    /// <summary>Which of the project's two principal parties this member belongs to.</summary>
    public ProjectSide Side { get; set; } = ProjectSide.Contractor;

    /// <summary>True for the one or two people who mediate between the sides.</summary>
    public bool IsMediator { get; set; }

    public ProjectMemberSpecialization Specialization { get; set; }

    /// <summary>Money on this project, assigned explicitly. Null follows the seat.</summary>
    public bool? HandlesMoney { get; set; }

    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }

    // Populated by service
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserProfilePicUrl { get; set; }
    public string? AssignedByName { get; set; }

    /// <summary>True when there is no platform account behind this row.</summary>
    public bool IsOffPlatform => string.IsNullOrEmpty(UserId);
}

public class ProjectMemberCreateDto
{
    [Required]
    public string? ProjectId { get; set; }

    /// <summary>
    /// UserId OR UserEmail for a platform user; leave both empty and supply
    /// <see cref="PartyName"/> for an off-platform party who is attributable
    /// on commitments but will never sign in.
    /// </summary>
    public string? UserId { get; set; }

    [EmailAddress]
    public string? UserEmail { get; set; }

    [MaxLength(200)]
    public string? PartyName { get; set; }

    public ProjectMemberSpecialization Specialization { get; set; } = ProjectMemberSpecialization.Other;

    /// <summary>Null defaults from <see cref="Specialization"/> via <c>ProjectSideDefaults.For</c>.</summary>
    public ProjectSide? Side { get; set; }

    /// <summary>
    /// Put this person on the money for this project regardless of their seat —
    /// an engineer asked to follow releases alongside the architect. Null leaves
    /// it to the seat: principals and the mediator.
    /// </summary>
    public bool? HandlesMoney { get; set; }

    /// <summary>
    /// Null defaults from <see cref="Specialization"/>. Setting true when the
    /// project already has two mediators is rejected with 409.
    /// </summary>
    public bool? IsMediator { get; set; }

    [MaxLength(120)]
    public string? Title { get; set; }
}

/// <summary>
/// Change an existing member's standing. Every field is optional — null means
/// "leave alone", so moving someone across sides does not silently reset their
/// title or specialization.
/// </summary>
public class ProjectMemberUpdateDto
{
    [Required]
    public string? MemberId { get; set; }

    /// <summary>Move this person to the other side of the project.</summary>
    public ProjectSide? Side { get; set; }

    /// <summary>
    /// Appoint or stand down. Appointing a third mediator is rejected with 409;
    /// standing down the last one is rejected too, since nobody would be left
    /// able to expose anything to the client.
    /// </summary>
    public bool? IsMediator { get; set; }

    public ProjectMemberSpecialization? Specialization { get; set; }

    /// <summary>Grant or withdraw money on this project. Null leaves it unchanged.</summary>
    public bool? HandlesMoney { get; set; }

    [MaxLength(120)]
    public string? Title { get; set; }
}

/// <summary>
/// The caller's own standing on one project — the answer to "may I see the Site
/// Log here, and may I put something in front of the client?"
/// <para>
/// Sent to the UI so it can render the right surface without re-deriving the
/// rules. It is a mirror of the server's decision, never the decision itself:
/// every endpoint re-resolves access regardless of what the client believes.
/// </para>
/// </summary>
public class ProjectAccessDto
{
    public string? ProjectId { get; set; }
    public ProjectAccessLevel Level { get; set; }

    /// <summary>Null when the caller has no standing on this project at all.</summary>
    public ProjectSide? Side { get; set; }

    public bool IsMediator { get; set; }
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanManage { get; set; }

    /// <summary>Contractor-side members and mediators only.</summary>
    public bool CanSeeSiteLog { get; set; }

    /// <summary>The exposure gate — mediator, owner or manager.</summary>
    public bool CanExposeToClient { get; set; }

    /// <summary>What this member was brought on to do. Null for an owner with no membership row.</summary>
    public ProjectMemberSpecialization? Specialization { get; set; }

    /// <summary>How deep the seat reaches — a principal sees the engagement, a support seat sees their own job.</summary>
    public ProjectSeat Seat { get; set; }

    public bool CanSeeMoney { get; set; }
    public bool CanSeeBrief { get; set; }
    public bool CanSeeDocuments { get; set; }
    public bool CanSeeHistory { get; set; }
    public bool CanCapture { get; set; }
    public bool CanSeeRegister { get; set; }

    /// <summary>The photographer's day starts at the camera, not at a dashboard.</summary>
    public bool LandsOnCapture { get; set; }

    /// <summary>
    /// Mirror one resolved standing. Every capability is copied from
    /// <see cref="ProjectAccess"/> rather than re-derived here — a second copy of
    /// these rules on the wire is how a tab and its endpoint start disagreeing.
    /// </summary>
    public static ProjectAccessDto From(ProjectAccess access, string? projectId) => new()
    {
        ProjectId = projectId,
        Level = access.Level,
        Side = access.Side,
        IsMediator = access.IsMediator,
        CanRead = access.CanRead,
        CanWrite = access.CanWrite,
        CanManage = access.CanManage,
        CanSeeSiteLog = access.CanSeeSiteLog,
        CanExposeToClient = access.CanExposeToClient,
        Specialization = access.Specialization,
        Seat = access.Seat,
        CanSeeMoney = access.CanSeeMoney,
        CanSeeBrief = access.CanSeeBrief,
        CanSeeDocuments = access.CanSeeDocuments,
        CanSeeHistory = access.CanSeeHistory,
        CanCapture = access.CanCapture,
        CanSeeRegister = access.CanSeeRegister,
        LandsOnCapture = access.LandsOnCapture
    };
}

// ─── Stage DTOs ──────────────────────────────────────────────

public class StageDto : BaseDto
{
    public string? ProjectId { get; set; }

    [MaxLength(200)]
    public string? StageName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? BudgetAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public int DisplayOrder { get; set; }
    public StageStatus Status { get; set; }

    // ─── Grouping and phase ──────────────────────────────────────

    /// <summary>The major stage this one sits under. One level only.</summary>
    public string? ParentStageId { get; set; }

    /// <summary>Set when this stage came from the catalogue — drives deduplication.</summary>
    public string? CatalogueKey { get; set; }

    /// <summary>Which phase of the build this is, and therefore which accent it wears.</summary>
    public StageGroup Phase { get; set; } = StageGroup.Custom;

    /// <summary>Sub-stages, filled in when the caller asked for the grouped shape.</summary>
    public List<StageDto> SubStages { get; set; } = new();

    /// <summary>The class that carries this stage's accent — see app.css §26.</summary>
    public string AccentClass => $"al-stage--{(int)Phase}";

    public string PhaseName => StageCatalogue.GroupName(Phase);

    // Computed
    public decimal FundedAmount { get; set; }
    public decimal FundedPercentage { get; set; }
    public decimal RemainingBalance { get; set; }
    public int DaysAheadOrBehind { get; set; }
}

public class StageCreateDto
{
    [MaxLength(200)]
    public string? StageName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? BudgetAmount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>Nest this under an existing major stage. One level only; a sub-stage cannot take a sub-stage.</summary>
    public string? ParentStageId { get; set; }

    /// <summary>
    /// The catalogue entry being used, when it came from the catalogue. Name,
    /// description and phase are filled in from it if the caller leaves them
    /// blank, and it is what stops the same stage being added twice.
    /// </summary>
    [MaxLength(60)]
    public string? CatalogueKey { get; set; }

    /// <summary>Only consulted for a custom stage — a catalogue stage brings its own phase.</summary>
    public StageGroup? Phase { get; set; }
}

// ─── Funding DTOs ────────────────────────────────────────────

public class FundingEntryDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaidById { get; set; }
    public string? ConfirmedById { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public FundingStatus Status { get; set; }
    public string? Notes { get; set; }

    // ─── The back-and-forth ──────────────────────────────────────

    /// <summary>The currency the funder typed in; null on entries recorded before it was asked for.</summary>
    public string? DeclaredCurrency { get; set; }

    /// <summary>The figure as typed, before conversion.</summary>
    public decimal? DeclaredAmount { get; set; }

    public decimal? ExchangeRate { get; set; }

    /// <summary>What the delivery side says landed, in the project's currency. Null until they answer.</summary>
    public decimal? ReceivedAmount { get; set; }

    public string? ReceiptNote { get; set; }
    public string? EvidenceArtifactId { get; set; }
    public string? EvidenceFileName { get; set; }
    public DateTime? SettledAt { get; set; }

    // Populated by service
    public string? PaidByName { get; set; }
    public string? ConfirmedByName { get; set; }
    public string? StageName { get; set; }
    public string? ProjectName { get; set; }

    // ─── Who may act on this one ─────────────────────────────────
    // Reading the ledger and moving it are different rights. Anyone the project
    // put on the money follows every release here; acting on one belongs to the
    // named party on that side and nobody else, so the server says so rather
    // than the client guessing and earning a 403.

    /// <summary>True only for the delivery side's responsible party, on a release still pending.</summary>
    public bool CanConfirm { get; set; }

    /// <summary>True only for the funder whose money it was, on a release reported short.</summary>
    public bool CanSettle { get; set; }

    /// <summary>
    /// The figure this release is worth to the project — what landed once
    /// somebody has said, otherwise what was sent. This is the number totals are
    /// built from, so a release that lost money in transit stops overstating the
    /// stage the moment it is acknowledged.
    /// </summary>
    public decimal SettledAmount => ReceivedAmount ?? Amount;

    /// <summary>How far the received figure fell short. Zero when they agree or nobody has answered.</summary>
    public decimal Shortfall => ReceivedAmount is { } got ? Amount - got : 0m;

    /// <summary>True while the two figures disagree and the funder has not accepted the gap.</summary>
    public bool HasGap => Status == FundingStatus.AmountQueried;

    /// <summary>Nothing more is owed on this one — it is agreed, one way or the other.</summary>
    public bool IsClosed => Status is FundingStatus.Confirmed or FundingStatus.Settled or FundingStatus.Rejected;

    /// <summary>True when the funder converted from another currency, so the UI can show both figures.</summary>
    public bool WasConverted =>
        DeclaredAmount is not null && !string.IsNullOrEmpty(DeclaredCurrency) && DeclaredAmount != Amount;
}

public class FundingEntryCreateDto
{
    public string? ProjectId { get; set; }

    public string? StageId { get; set; }

    /// <summary>The figure as typed, in <see cref="Currency"/>.</summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// What the funder typed in. Defaults to the project's currency; anything
    /// else must carry <see cref="ExchangeRate"/> so the ledger stays in one
    /// currency and the original figure is still on the record.
    /// </summary>
    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>Project-currency units per one unit of <see cref="Currency"/>. Ignored when they match.</summary>
    public decimal? ExchangeRate { get; set; }

    public DateTime? PaymentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>Optional proof of the transfer, as a stored artifact id.</summary>
    [MaxLength(40)]
    public string? EvidenceArtifactId { get; set; }

    [MaxLength(260)]
    public string? EvidenceFileName { get; set; }
}

public class FundingConfirmDto
{
    public string? FundingEntryId { get; set; }

    public bool IsConfirmed { get; set; } = true;

    /// <summary>
    /// What actually landed, in the project's currency. Null means "the declared
    /// figure arrived in full" — the one-tap answer, which is the common case.
    /// A different figure opens the gap for the funder to accept or take up.
    /// </summary>
    public decimal? ReceivedAmount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>The funder's answer to a reported shortfall: accept the figure and close it.</summary>
public class FundingSettleDto
{
    public string? FundingEntryId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

// ─── Progress DTOs ───────────────────────────────────────────

public class ProgressUpdateDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? Description { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public bool HasIssues { get; set; }
    public string? CreatedById { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public Channel Channel { get; set; } = Channel.Crew;

    // Populated
    public string? CreatedByName { get; set; }
    public string? StageName { get; set; }

    /// <summary>
    /// Frames this reader may see. For a client-side reader this is the exposed
    /// subset, not the whole batch — compare against <see cref="ImageCount"/>.
    /// </summary>
    public List<ProgressImageDto> Images { get; set; } = new();

    /// <summary>
    /// Total frames on the entry regardless of exposure. Lets the mediator's UI
    /// show "3 of 18 shared" without a second round trip. A client-side reader
    /// receives this too — being told the feed is filtered is Peter's stated
    /// trust condition, and hiding the denominator would break it.
    /// </summary>
    public int ImageCount { get; set; }

    public List<ProgressCommentDto> Comments { get; set; } = new();
}

public class ProgressUpdateCreateDto
{
    public string? ProjectId { get; set; }

    public string? StageId { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, 100)]
    public decimal? CompletionPercentage { get; set; }

    public bool HasIssues { get; set; }

    /// <summary>
    /// Crew (default, fail-closed) keeps the entry internal. Client promotes
    /// it into the curated client-facing view.
    /// </summary>
    public Channel Channel { get; set; } = Channel.Crew;

    /// <summary>
    /// Base64-encoded images with optional captions.
    /// Max 5 per update.
    /// </summary>
    public List<ProgressImageUploadDto> Images { get; set; } = new();
}

public class ProgressImageDto : BaseDto
{
    public string? ProgressUpdateId { get; set; }

    /// <summary>Pointer to the canonical file. Null only on pre-P2 rows.</summary>
    public string? ArtifactId { get; set; }

    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Visibility of this frame. A client-side reader only ever receives
    /// frames already at <see cref="Channel.Client"/>, so this reads
    /// <c>Client</c> for everything they can see.
    /// </summary>
    public Channel Channel { get; set; } = Channel.Crew;

    public string? ExposedById { get; set; }
    public DateTime? ExposedAt { get; set; }

    public List<ProgressCommentDto> Comments { get; set; } = new();
}

/// <summary>
/// Expose or withdraw specific frames on one entry. The mediator's core
/// curation gesture — pick three of eighteen, not all or nothing.
/// </summary>
public class ProgressImageExposureDto
{
    [Required]
    public List<string> ImageIds { get; set; } = new();

    public Channel Channel { get; set; } = Channel.Crew;
}

public class ProgressImageUploadDto
{
    public string? Base64Image { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    [MaxLength(300)]
    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>Client-side preview data URI (not sent to server)</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? PreviewUrl { get; set; }
}

// ─── Comment DTOs ────────────────────────────────────────────

public class ProgressCommentDto : BaseDto
{
    public string? ProgressUpdateId { get; set; }
    public string? ProgressImageId { get; set; }
    public string? CommentText { get; set; }
    public string? AuthorId { get; set; }
    public string? ParentCommentId { get; set; }

    // Populated
    public string? AuthorName { get; set; }
    public string? AuthorProfilePicUrl { get; set; }
    public List<ProgressCommentDto> Replies { get; set; } = new();
}

public class ProgressCommentCreateDto
{
    public string? ProgressUpdateId { get; set; }
    public string? ProgressImageId { get; set; }

    [MaxLength(2000)]
    public string? CommentText { get; set; }

    public string? ParentCommentId { get; set; }
}

public class ProgressApprovalDto
{
    public string? ProgressUpdateId { get; set; }

    public ApprovalStatus Status { get; set; }
}

// ─── Streams broadcast envelope ──────────────────────────────

public class StreamCommentEvent
{
    public string? StreamId { get; set; }
    public Channel Channel { get; set; } = Channel.Crew;
    public ProgressCommentDto? Comment { get; set; }
}

// ─── Budget DTOs ─────────────────────────────────────────────

public class BudgetLineItemDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public BudgetCategory Category { get; set; } = BudgetCategory.Other;
    public decimal PlannedAmount { get; set; }
    public int DisplayOrder { get; set; }

    // Computed
    public string? StageName { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Remaining => PlannedAmount - TotalSpent;
    public int ReceiptCount { get; set; }
}

public class BudgetLineItemCreateDto
{
    [Required]
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    [Required]
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public BudgetCategory Category { get; set; } = BudgetCategory.Other;

    [Range(0, double.MaxValue)]
    public decimal PlannedAmount { get; set; }
}

public class BudgetLineItemUpdateDto
{
    [Required]
    [MaxLength(40)]
    public string? Id { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public BudgetCategory? Category { get; set; }

    public decimal? PlannedAmount { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }
}

public class ReceiptDto : BaseDto
{
    public string? BudgetLineItemId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? VendorName { get; set; }
    public string? Notes { get; set; }
    public string? ReceiptImageUrl { get; set; }

    // Populated
    public string? LineItemTitle { get; set; }
    public string? CreatedByName { get; set; }
}

public class ReceiptCreateDto
{
    [Required]
    [MaxLength(40)]
    public string? BudgetLineItemId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [MaxLength(200)]
    public string? VendorName { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? ReceiptImageUrl { get; set; }
}

public class ProjectBudgetSummaryDto
{
    public string? ProjectId { get; set; }
    public decimal ProjectBudget { get; set; }
    public decimal TotalPlanned { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Remaining => TotalPlanned - TotalSpent;
    public decimal SpendPercent =>
        TotalPlanned > 0 ? Math.Round(TotalSpent / TotalPlanned * 100, 1) : 0;
    public decimal AllocationPercent =>
        ProjectBudget > 0 ? Math.Round(TotalPlanned / ProjectBudget * 100, 1) : 0;
    public Dictionary<BudgetCategory, decimal> PlannedByCategory { get; set; } = new();
    public Dictionary<BudgetCategory, decimal> SpentByCategory { get; set; } = new();
    public List<BudgetLineItemDto> LineItems { get; set; } = new();
}

// ─── Flag DTOs ───────────────────────────────────────────────

public class FlagDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? ProgressUpdateId { get; set; }
    public string? ProgressImageId { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public FlagStatus Status { get; set; } = FlagStatus.Open;
    public FlagSeverity Severity { get; set; } = FlagSeverity.Medium;
    public Channel Channel { get; set; } = Channel.Crew;

    public string? CreatedById { get; set; }
    public string? AssignedToId { get; set; }
    public string? ResolvedById { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime? LastNudgeAt { get; set; }
    public bool IsNudgeArchived { get; set; }

    // Populated
    public string? ProjectName { get; set; }
    public string? StageName { get; set; }
    public string? CreatedByName { get; set; }
    public string? AssignedToName { get; set; }
    public string? ResolvedByName { get; set; }
}

public class FlagCreateDto
{
    [Required]
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(40)]
    public string? ProgressUpdateId { get; set; }

    [MaxLength(40)]
    public string? ProgressImageId { get; set; }

    [Required]
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public FlagSeverity Severity { get; set; } = FlagSeverity.Medium;
    public Channel Channel { get; set; } = Channel.Crew;

    [MaxLength(450)]
    public string? AssignedToId { get; set; }

    public DateTime? DueDate { get; set; }
}

public class FlagUpdateDto
{
    [Required]
    [MaxLength(40)]
    public string? Id { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public FlagStatus? Status { get; set; }
    public FlagSeverity? Severity { get; set; }

    [MaxLength(450)]
    public string? AssignedToId { get; set; }

    public DateTime? DueDate { get; set; }
}

// ─── Subscription DTOs ───────────────────────────────────────

public class ProjectSubscriptionDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? InvestorId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string Currency { get; set; } = "UGX";
}

// ─── Portfolio / Dashboard DTOs ──────────────────────────────

public class PortfolioSummaryDto
{
    public decimal TotalCapitalDeployed { get; set; }
    public int ActiveProjectsCount { get; set; }
    public int ProjectsAtRiskCount { get; set; }
    public decimal TotalPortfolioCompletion { get; set; }
    public List<ProjectCardDto> Projects { get; set; } = new();
}

public class ProjectCardDto
{
    public string Id { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal FundedPercentage { get; set; }
    public decimal CompletedPercentage { get; set; }
    public string TimelineStatus { get; set; } = "On Track";
    public DateTime? LastUpdateDate { get; set; }
    public string? LatestImageUrl { get; set; }
    public List<string> RecentImageUrls { get; set; } = new();
    public string? CurrentStageName { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public bool IsSubscriptionActive { get; set; }
    public ProjectStatus Status { get; set; }
    public string Currency { get; set; } = "UGX";
    public decimal TotalBudget { get; set; }
    public decimal TotalFunded { get; set; }

    public string? ParentProjectId { get; set; }
    public int SubProjectCount { get; set; }
    public List<ProjectCardDto> SubProjects { get; set; } = new();

    /// <summary>
    /// The cover deliberately chosen for this project, as opposed to whatever
    /// was photographed most recently. Carried separately from
    /// <see cref="LatestImageUrl"/> so the chrome can tell the two apart: a
    /// chosen cover is stable enough to recognise a project by, a site photo is
    /// not, and "clear the image" must only clear the one the reader set.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Set on a sub-project that is borrowing its parent's cover. The wing is
    /// part of the house and looks like it until someone gives it a face of its
    /// own; the flag exists so "clear image" on the wing does not offer to clear
    /// something that belongs to the parent.
    /// </summary>
    public bool CoverInherited { get; set; }

    /// <summary>Open questions on this project — the number the context menu offers to clear.</summary>
    public int OpenIssueCount { get; set; }

    // ─── This reader's arrangement ───────────────────────────────
    // Per user, never per project: two people on one engagement each order
    // their own screen. See tbl_ProjectPreference.

    public bool IsPinned { get; set; }

    /// <summary>Position among unpinned projects, ascending. Server-assigned; the client sends it back on a drop.</summary>
    public int SortOrder { get; set; }

    // ─── The bin ─────────────────────────────────────────────────

    public DateTime? ArchivedAt { get; set; }

    /// <summary>Who sent it to the bin, for the archive list. Null while live.</summary>
    public string? ArchivedByName { get; set; }

    /// <summary>When the contents get soft-deleted. Null while live.</summary>
    public DateTime? PurgeDueAt { get; set; }

    /// <summary>Whole days left before the purge, floored at zero. Null while live.</summary>
    public int? DaysUntilPurge { get; set; }

    /// <summary>
    /// This reader's standing on this project, resolved server-side with the card.
    /// <para>
    /// The context menu and the card chrome have to know whether to offer
    /// settings, a cover change or the open-question count, and the dashboard is
    /// the one screen that renders many projects at once — a per-card round trip
    /// to <c>GetMyStanding</c> would be one request per tile. Null only on a card
    /// assembled without a caller.
    /// </para>
    /// </summary>
    public ProjectAccessDto? Standing { get; set; }
}

// ─── Arranging and binning projects ──────────────────────────

/// <summary>One project's new position, sent as part of a whole-list drop.</summary>
public class ProjectOrderItemDto
{
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Zero-based index within the unpinned list, in the order the reader dropped them.</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// The result of one drag. The whole unpinned list is sent rather than the moved
/// project alone: a single index is ambiguous the moment two devices reorder at
/// once, and re-sending eight ids costs less than reconciling that.
/// </summary>
public class ProjectOrderUpdateDto
{
    public List<ProjectOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Point a project at an artifact for its cover, or clear it. Null
/// <see cref="ArtifactId"/> means clear — the project falls back to its parent's
/// cover if it has one, and to the mark if it does not.
/// </summary>
public class ProjectCoverUpdateDto
{
    public string ProjectId { get; set; } = string.Empty;
    public string? ArtifactId { get; set; }
}

// ─── PM Dashboard DTOs ───────────────────────────────────────

public class PMDashboardDto
{
    public List<ProjectCardDto> AssignedProjects { get; set; } = new();
    public List<FundingEntryDto> PendingConfirmations { get; set; } = new();
    public List<ProgressCommentDto> RecentComments { get; set; } = new();
    public int TotalAssigned { get; set; }
    public int PendingCount { get; set; }
    public int PendingFundingCount => PendingCount;
    public int CommentsNeedingResponse { get; set; }
}

// ─── Timeline DTO ────────────────────────────────────────────

public class TimelineEntryDto
{
    public string StageId { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public DateTime? PlannedStart { get; set; }
    public DateTime? PlannedEnd { get; set; }
    public decimal ActualProgress { get; set; }
    public StageStatus Status { get; set; }
    public int DaysAheadOrBehind { get; set; }
    public string StatusBadge { get; set; } = "On Track";

    // Convenience properties for the timeline UI
    public string Title => StageName;
    public string? Description => Status switch
    {
        StageStatus.Completed => $"Completed — {ActualProgress}%",
        StageStatus.InProgress => $"In Progress — {ActualProgress}%",
        _ => "Not Started"
    };
    public DateTime Date => PlannedStart ?? DateTime.MinValue;
    public string EventType => StatusBadge;
    public bool IsCompleted => Status == StageStatus.Completed;
    public bool IsCurrent => Status == StageStatus.InProgress;
}

// ─── Analytics DTO ───────────────────────────────────────────

public class ProjectAnalyticsDto
{
    public DateTime? LastLogin { get; set; }
    public DateTime? LastUpdate { get; set; }
    public int UpdatesThisWeek { get; set; }
    public int TotalUpdates { get; set; }
    public decimal FundingPercentage { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int TimelineVarianceDays { get; set; }
    public RiskLevel RiskLevel { get; set; }
}
