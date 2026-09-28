using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── Proposals — the review queue (plan.md P5, assetlen.md Law 3) ─────────

public class ExtractionProposalDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? IngestedMessageId { get; set; }
    public ProposalKind Kind { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }

    public decimal? Amount { get; set; }
    public string? Currency { get; set; }

    /// <summary>True when there is a figure the reader is off the money for.</summary>
    public bool AmountHidden { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>The words the date was read from — "by Tuesday", "this week" — so the reader can check the arithmetic.</summary>
    public string? DateText { get; set; }

    public string? Quantity { get; set; }
    public CommitmentMaturity Maturity { get; set; }
    public ProjectSide? OwedBySide { get; set; }
    public string? PartyName { get; set; }

    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public int StagePhase { get; set; }

    /// <summary>Which trigger fired. The accept rate is instrumented per rule so a noisy one can be narrowed.</summary>
    public string? Rule { get; set; }
    public string? Engine { get; set; }
    public double Confidence { get; set; }

    /// <summary>Someone in the thread pushed back on it — it lands In discussion, never Agreed.</summary>
    public bool Contested { get; set; }
    public string? ContestNote { get; set; }

    public ProposalStatus Status { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecidedByName { get; set; }
    public string? CommitmentId { get; set; }
    public string? FlagId { get; set; }

    public string? SourceAuthor { get; set; }
    public ProjectSide? SourceSide { get; set; }
    public DateTime? SourceSentAt { get; set; }
    public string? SourceExcerpt { get; set; }
}

public class ExtractionRuleStatDto
{
    public string? Rule { get; set; }
    public int Proposed { get; set; }
    public int Accepted { get; set; }
    public int Rejected { get; set; }
    public double? AcceptRate { get; set; }
}

public class ExtractionRunDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? Engine { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int MessagesRead { get; set; }
    public int ProposalsCreated { get; set; }
    public int ReadingsCreated { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// The whole queue plus its instrumentation. <see cref="BelowThreshold"/> is the
/// plan's tripwire: under roughly two-thirds accepted, narrow the trigger rather
/// than ship a confidently wrong register.
/// </summary>
public class ExtractionQueueDto
{
    public List<ExtractionProposalDto> Proposals { get; set; } = new();
    public int PendingCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public double? AcceptRate { get; set; }
    public double Threshold { get; set; }
    public bool BelowThreshold { get; set; }
    public int MinimumSample { get; set; }
    public List<ExtractionRuleStatDto> Rules { get; set; } = new();
    public ExtractionRunDto? LastRun { get; set; }
    public int ReadingCount { get; set; }
    public bool CanDecide { get; set; }
}

public class ExtractionRunRequestDto
{
    [Required, MaxLength(40)] public string? ProjectId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

/// <summary>Accept or reject several at once — Peter clears the queue in bulk, not one nag at a time.</summary>
public class ProposalDecisionDto
{
    [Required] public List<string> ProposalIds { get; set; } = new();
    public bool Accept { get; set; }
}

/// <summary>Correct a proposal before accepting it. Only the fields sent change.</summary>
public class ProposalEditDto
{
    [Required, MaxLength(40)] public string? ProposalId { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    public ProposalKind? Kind { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? DueDate { get; set; }
    [MaxLength(40)] public string? StageId { get; set; }
}

public class ProposalDecisionResultDto
{
    public int Accepted { get; set; }
    public int Rejected { get; set; }
    public int Skipped { get; set; }
    public List<string> CommitmentIds { get; set; } = new();
    public List<string> FlagIds { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

// ─── Progress readings (works-report.md §4.2) ────────────────────────────

public class ProgressReadingDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public string? Subject { get; set; }
    public decimal Percent { get; set; }
    public DateTime ObservedAt { get; set; }
    public ProgressReadingSource SourceKind { get; set; }
    public string? SourceId { get; set; }
}

// ─── OCR ─────────────────────────────────────────────────────────────────

public class ArtifactTextDto
{
    public string? ArtifactId { get; set; }
    public ArtifactTextStatus Status { get; set; }
    public string? Engine { get; set; }
    public string? Text { get; set; }
    public int CharCount { get; set; }
    public DateTime? ExtractedAt { get; set; }
    public string? Error { get; set; }
}

public class OcrQueueResultDto
{
    public int Queued { get; set; }
    public int AlreadyRead { get; set; }
    public string? Engine { get; set; }
    public bool EngineAvailable { get; set; }
}

// ─── Media re-join (works-report.md §5) ──────────────────────────────────

public class MediaRejoinFileDto
{
    public string? FileName { get; set; }
    public MediaRejoinOutcome Outcome { get; set; }
    public string? Reason { get; set; }

    /// <summary>The moment in the file name, when it carried one. Folder names are ignored.</summary>
    public DateTime? StampedAt { get; set; }
    public string? MessageId { get; set; }
    public DateTime? MessageSentAt { get; set; }
    public string? Caption { get; set; }
    public string? ArtifactId { get; set; }
}

public class MediaRejoinReportDto
{
    public string? BatchId { get; set; }
    public string? ProjectId { get; set; }
    public int FilesReceived { get; set; }
    public int Bound { get; set; }
    public int AlreadyBound { get; set; }
    public int Duplicates { get; set; }
    public int Unbound { get; set; }

    /// <summary>Files whose human name ("intended stoppage line.jpeg") became the caption.</summary>
    public int Captioned { get; set; }

    /// <summary><c>&lt;Media omitted&gt;</c> lines still waiting for a file after this run.</summary>
    public int LinesStillOpen { get; set; }
    public List<MediaRejoinFileDto> Files { get; set; } = new();
}
