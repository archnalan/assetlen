using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ═════════════════════════════════════════════════════════════════════════
// The works report (works-report.md). One shape for the live page and for an
// issued snapshot — the only difference is where the data comes from.
// Every figure here is computed from a table; prose lives only in Narrative.
// ═════════════════════════════════════════════════════════════════════════

public class WorksReportDto
{
    /// <summary>Null for the live view.</summary>
    public string? Id { get; set; }
    public string ProjectId { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string? ParentProjectName { get; set; }
    public string Currency { get; set; } = "UGX";

    public bool IsLive { get; set; }
    public DateTime AsAt { get; set; }
    public DateTime WindowFrom { get; set; }
    public string WindowLabel { get; set; } = "";
    public ProjectSide Audience { get; set; } = ProjectSide.Client;

    public ReportIssueKind? IssueKind { get; set; }
    public string? IssueReason { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? IssuedByName { get; set; }
    public string? CoveringNote { get; set; }
    public string? DeliveryNote { get; set; }
    public string? ContentSha256 { get; set; }

    /// <summary>True when the stored snapshot still hashes to <see cref="ContentSha256"/>.</summary>
    public bool? HashVerified { get; set; }
    public string? PreviousReportId { get; set; }
    public DateTime? PreviousAsAt { get; set; }

    /// <summary>The accountable face (assetlen.md §10.1).</summary>
    public string? MediatorName { get; set; }

    /// <summary>Whether the reader may issue a report on this project.</summary>
    public bool CanIssue { get; set; }

    public ReportCoverDto Cover { get; set; } = new();
    public List<HeadlineCardDto> Headlines { get; set; } = new();
    public DeadlineStripDto Deadline { get; set; } = new();
    public ProgressSummaryDto Progress { get; set; } = new();
    public List<StageGroupReportDto> StageGroups { get; set; } = new();
    public List<ReportPairDto> Changed { get; set; } = new();
    public ReportChangesDto? SinceLast { get; set; }
    public List<DecisionReportDto> Decisions { get; set; } = new();
    public List<VariationReportDto> Variations { get; set; } = new();

    /// <summary>Scope changes on record with no variation raised for them — a gap, not an empty state.</summary>
    public List<ReportGapDto> ScopeGaps { get; set; } = new();
    public List<BlockerLaneDto> Blockers { get; set; } = new();
    public AheadDto Ahead { get; set; } = new();

    /// <summary>
    /// Present only for a reader on the money. Absent — not empty — for anyone
    /// else (works-report.md §2.7), so the key is not even written.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MoneyReportDto? Money { get; set; }

    public List<FootageGroupDto> Footage { get; set; } = new();
    public List<ReportSourceDto> Sources { get; set; } = new();
    public NarrativeDto Narrative { get; set; } = new();
}

public class ReportCoverDto
{
    public string? HeroArtifactId { get; set; }
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
}

public class HeadlineCardDto
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Unit { get; set; }
    public string StateWord { get; set; } = "";
    public ReportTone Tone { get; set; }
    public string Line { get; set; } = "";
}

public class ProgressSummaryDto
{
    public decimal OverallPercent { get; set; }

    /// <summary>"budget" or "stage count" — the page says which.</summary>
    public string Weighting { get; set; } = "";
    public string WeightingNote { get; set; } = "";
    public int StageCount { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public int NotStarted { get; set; }
    public int Stalled { get; set; }
    public int StallThresholdDays { get; set; }
}

public class DeadlineStripDto
{
    public DateTime AxisFrom { get; set; }
    public DateTime AxisTo { get; set; }
    public List<DeadlineMarkDto> Marks { get; set; } = new();
    public DateTime? Promised { get; set; }
    public DateTime? ContractorDate { get; set; }
    public DateTime? Forecast { get; set; }
    public string? ForecastBasis { get; set; }

    /// <summary>Open stages the forecast cannot speak for — no pace, never guessed.</summary>
    public List<string> ForecastExcludes { get; set; } = new();
    public int LapsedCount { get; set; }
    public ReportTone Tone { get; set; }
    public string StateWord { get; set; } = "";

    /// <summary>"date commitments" when the chain exists, else "project dates".</summary>
    public string Basis { get; set; } = "";
}

public class DeadlineMarkDto
{
    public DeadlineMarkKind Kind { get; set; }
    public DateTime At { get; set; }
    public string Label { get; set; } = "";
    public List<string> SourceIds { get; set; } = new();
}

public class StageGroupReportDto
{
    public StageGroup Phase { get; set; }
    public string Label { get; set; } = "";
    public List<StageReportDto> Stages { get; set; } = new();
}

public class StageReportDto
{
    public string StageId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ParentName { get; set; }
    public StageGroup Phase { get; set; }
    public StageStatus Status { get; set; }
    public decimal? Percent { get; set; }
    public DateTime? PercentAt { get; set; }
    public List<ReadingPointDto> Readings { get; set; } = new();

    public bool IsStalled { get; set; }
    public int? StalledDays { get; set; }
    public DateTime? StalledSince { get; set; }

    /// <summary>The thread still reports work on it after the reading stopped moving.</summary>
    public DateTime? StillReportedAt { get; set; }

    public DateTime? PlannedEnd { get; set; }
    public DateTime? BaselineEnd { get; set; }
    public DateTime? Forecast { get; set; }
    public string? ForecastBasis { get; set; }
    public int? SlipDays { get; set; }
    public int? ForecastVsPlanDays { get; set; }
    public int DeliverablesDone { get; set; }
    public int DeliverablesTotal { get; set; }
    public List<ReportFrameDto> Frames { get; set; } = new();
    public List<string> Gaps { get; set; } = new();
    public List<string> SourceIds { get; set; } = new();
}

public class ReadingPointDto
{
    public DateTime At { get; set; }
    public decimal Percent { get; set; }
    public string SourceId { get; set; } = "";
}

public class ReportFrameDto
{
    public string ArtifactId { get; set; } = "";
    public DateTime At { get; set; }
    public string? Caption { get; set; }
    public bool IsVideo { get; set; }
    public string? PosterArtifactId { get; set; }
    public double? DurationSeconds { get; set; }
    public PosterStatus? PosterStatus { get; set; }
    public string SourceId { get; set; } = "";
}

public class ReportPairDto
{
    public string StageName { get; set; } = "";
    public StageGroup Phase { get; set; }
    public ReportFrameDto Before { get; set; } = new();
    public ReportFrameDto After { get; set; } = new();
    public int DaysApart { get; set; }

    /// <summary>"same view" when the frames were matched by what they show, else "first and latest".</summary>
    public string Basis { get; set; } = "";
}

public class ReportChangesDto
{
    public string PreviousReportId { get; set; } = "";
    public DateTime PreviousAsAt { get; set; }
    public decimal OverallFrom { get; set; }
    public decimal OverallTo { get; set; }
    public List<StageMoveDto> StageMoves { get; set; } = new();
    public List<string> NewDecisions { get; set; } = new();
    public List<string> NewVariations { get; set; } = new();
    public List<string> BlockersOpened { get; set; } = new();
    public List<string> BlockersCleared { get; set; } = new();
    public int LapsedFrom { get; set; }
    public int LapsedTo { get; set; }
    public DateTime? ForecastFrom { get; set; }
    public DateTime? ForecastTo { get; set; }

    /// <summary>Null for a reader off the money.</summary>
    public decimal? FundedDelta { get; set; }
}

public class StageMoveDto
{
    public string StageName { get; set; } = "";
    public decimal? From { get; set; }
    public decimal? To { get; set; }
}

public class DecisionReportDto
{
    public string Id { get; set; } = "";
    public CommitmentKind Kind { get; set; }
    public string Title { get; set; } = "";
    public string? StageName { get; set; }
    public StageGroup? Phase { get; set; }
    public string? AgreedByName { get; set; }
    public string? AgreedWith { get; set; }
    public DateTime? AgreedAt { get; set; }
    public CommitmentMaturity Maturity { get; set; }
    public CommitmentQueryState QueryState { get; set; }
    public bool IsNew { get; set; }
    public int Restatements { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public bool AmountHidden { get; set; }
    public List<string> Gaps { get; set; } = new();
    public List<string> SourceIds { get; set; } = new();
}

public class VariationReportDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Reason { get; set; }
    public string? StageName { get; set; }
    public StageGroup? Phase { get; set; }
    public decimal? CostDelta { get; set; }
    public string? Currency { get; set; }
    public bool AmountHidden { get; set; }
    public int? TimeDeltaDays { get; set; }
    public VariationStatus Status { get; set; }
    public DateTime? RaisedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public bool IsNew { get; set; }
    public List<string> Gaps { get; set; } = new();
    public List<string> SourceIds { get; set; } = new();
}

public class ReportGapDto
{
    public string Key { get; set; } = "";
    public string Text { get; set; } = "";
    public string? StageName { get; set; }
    public List<string> SourceIds { get; set; } = new();
}

public class BlockerLaneDto
{
    public string Owner { get; set; } = "";
    public int OldestDays { get; set; }
    public List<BlockerReportDto> Items { get; set; } = new();
}

public class BlockerReportDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";

    /// <summary>Null when the blocker crossed from the crew channel — the floor carries it, not the crew's wording.</summary>
    public string? Detail { get; set; }
    public DateTime Since { get; set; }
    public int DaysOpen { get; set; }
    public string? StageName { get; set; }
    public StageGroup? Phase { get; set; }
    public DateTime? LastChase { get; set; }
    public bool Crossed { get; set; }
    public List<string> SourceIds { get; set; } = new();
}

public class AheadDto
{
    public int HorizonDays { get; set; } = 28;
    public List<AheadItemDto> Items { get; set; } = new();
}

public class AheadItemDto
{
    public string Key { get; set; } = "";
    public AheadKind Kind { get; set; }
    public string Title { get; set; } = "";
    public DateTime? DueBy { get; set; }
    public int? DaysAway { get; set; }
    public string? Consequence { get; set; }
    public ProjectSide? OwedBy { get; set; }
    public string? StageName { get; set; }
    public StageGroup? Phase { get; set; }
    public List<string> SourceIds { get; set; } = new();
}

public class MoneyReportDto
{
    public string Currency { get; set; } = "UGX";
    public List<MoneyRowDto> Rows { get; set; } = new();
    public decimal TotalBudget { get; set; }
    public decimal TotalFunded { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalClaimed { get; set; }
    public decimal TotalCleared { get; set; }
    public decimal TotalInHand { get; set; }
    public List<ReleaseReportDto> Releases { get; set; } = new();
    public List<string> Gaps { get; set; } = new();
}

public class MoneyRowDto
{
    public string StageId { get; set; } = "";
    public string StageName { get; set; } = "";
    public StageGroup Phase { get; set; }
    public decimal Budget { get; set; }
    public decimal Funded { get; set; }
    public decimal Pending { get; set; }
    public decimal Claimed { get; set; }
    public decimal Cleared { get; set; }
    public decimal InHand { get; set; }
    public decimal? CarriedForward { get; set; }
}

public class ReleaseReportDto
{
    public string Id { get; set; } = "";
    public string? StageName { get; set; }
    public decimal Amount { get; set; }
    public decimal? ReceivedAmount { get; set; }
    public DateTime? At { get; set; }
    public FundingStatus Status { get; set; }
    public string? Gap { get; set; }
    public string SourceId { get; set; } = "";
}

public class FootageGroupDto
{
    public string? StageName { get; set; }
    public StageGroup? Phase { get; set; }
    public int Total { get; set; }
    public List<ReportFrameDto> Frames { get; set; } = new();
}

public class ReportSourceDto
{
    public string Id { get; set; } = "";

    /// <summary>Short printed reference, "S12", resolved in the appendix.</summary>
    public string Ref { get; set; } = "";
    public ReportSourceKind Kind { get; set; }
    public DateTime? At { get; set; }
    public string Label { get; set; } = "";
    public string? Excerpt { get; set; }
    public string? Href { get; set; }
    public string? ArtifactId { get; set; }
}

public class NarrativeDto
{
    /// <summary>"template", or the model that drafted the accepted sentences.</summary>
    public string Engine { get; set; } = "template";
    public bool DraftingEnabled { get; set; }
    public int Accepted { get; set; }
    public int Rejected { get; set; }
    public List<string> RejectionReasons { get; set; } = new();
    public List<NarrativeTargetDto> Targets { get; set; } = new();
}

public class NarrativeTargetDto
{
    /// <summary>"cover", "answer", "stage:{id}", "decision:{id}", "variation:{id}", "blocker:{id}".</summary>
    public string TargetId { get; set; } = "";
    public bool Templated { get; set; }
    public List<NarrativeSentenceDto> Sentences { get; set; } = new();
}

public class NarrativeSentenceDto
{
    public string Text { get; set; } = "";
    public List<string> SourceIds { get; set; } = new();
}

// ─── Issuing and history ─────────────────────────────────────

public class WorksReportSummaryDto
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public DateTime AsAt { get; set; }
    public DateTime WindowFrom { get; set; }
    public ProjectSide Audience { get; set; }
    public ReportIssueKind IssueKind { get; set; }
    public string? IssueReason { get; set; }
    public DateTime IssuedAt { get; set; }
    public string? IssuedByName { get; set; }
    public string? ContentSha256 { get; set; }
    public decimal OverallPercent { get; set; }
    public string? DeadlineState { get; set; }
    public string? DeliveryNote { get; set; }
}

public class IssueReportDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }

    /// <summary>Defaults to now. A past day issues the report as the record stood at the end of it.</summary>
    public DateTime? AsAt { get; set; }
    public ProjectSide? Audience { get; set; }
    [MaxLength(1000)] public string? CoveringNote { get; set; }
}

public class ReportSettingsDto
{
    public string ProjectId { get; set; } = "";
    public bool DraftingEnabled { get; set; }

    /// <summary>Whether this server has a model configured at all.</summary>
    public bool DraftingAvailable { get; set; }
    public bool CanChange { get; set; }
    public string Statement { get; set; } = "";
}

public class ReportSettingsUpdateDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    public bool DraftingEnabled { get; set; }
}

public class ReportScheduleRunDto
{
    public DateTime AsAt { get; set; }
    public int Weekly { get; set; }
    public int Milestones { get; set; }
    public List<string> ReportIds { get; set; } = new();
    public List<string> Notes { get; set; } = new();
}

/// <summary>A draft put to the narrative validator against one target of the live report.</summary>
public class NarrativeCheckDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    public NarrativeTargetDto Draft { get; set; } = new();
}

public class NarrativeCheckResultDto
{
    public bool TargetFound { get; set; }
    public int Accepted { get; set; }
    public List<string> Reasons { get; set; } = new();
}
