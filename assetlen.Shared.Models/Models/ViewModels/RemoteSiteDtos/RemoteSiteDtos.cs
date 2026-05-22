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

    public List<StageDto> Stages { get; set; } = new();
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

    public List<StageCreateDto> Stages { get; set; } = new();
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

    // Populated
    public string? CreatedByName { get; set; }
    public string? StageName { get; set; }
    public List<ProgressImageDto> Images { get; set; } = new();
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
    /// Base64-encoded images with optional captions.
    /// Max 5 per update.
    /// </summary>
    public List<ProgressImageUploadDto> Images { get; set; } = new();
}

public class ProgressImageDto : BaseDto
{
    public string? ProgressUpdateId { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public List<ProgressCommentDto> Comments { get; set; } = new();
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
