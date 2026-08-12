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
    /// Null defaults from <see cref="Specialization"/>. Setting true when the
    /// project already has two mediators is rejected with 409.
    /// </summary>
    public bool? IsMediator { get; set; }

    [MaxLength(120)]
    public string? Title { get; set; }
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

    // Populated by service
    public string? PaidByName { get; set; }
    public string? ConfirmedByName { get; set; }
    public string? StageName { get; set; }
    public string? ProjectName { get; set; }
}

public class FundingEntryCreateDto
{
    public string? ProjectId { get; set; }

    public string? StageId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class FundingConfirmDto
{
    public string? FundingEntryId { get; set; }

    public bool IsConfirmed { get; set; } = true;

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
