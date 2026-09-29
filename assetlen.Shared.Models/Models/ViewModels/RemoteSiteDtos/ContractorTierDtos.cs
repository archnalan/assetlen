using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── Capture (P9) ─────────────────────────────────────────────

/// <summary>
/// What the camera screen opens on: today's work, so the first tap names the
/// deliverable instead of hunting for a stage (assetlen.md §8, three-tap capture).
/// </summary>
public class CaptureTodayDto
{
    public string? ProjectId { get; set; }

    /// <summary>The stage a capture with nothing named would land on.</summary>
    public string? ActiveStageId { get; set; }
    public string? ActiveStageName { get; set; }

    public List<CaptureDeliverableDto> Deliverables { get; set; } = new();

    /// <summary>Captures already posted today, so "did I send the slab?" needs no scrolling.</summary>
    public int CapturesToday { get; set; }
}

public class CaptureDeliverableDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public StageGroup? Phase { get; set; }
    public DeliverableStatus Status { get; set; }
    public int CapturesToday { get; set; }
    public DateTime? LastCapturedAt { get; set; }
}

// ─── Curation by exception (P9) ───────────────────────────────

/// <summary>
/// One day of captures as the mediator curates it: what the cutoff will send
/// across if he does nothing, and the frames he has kept or dropped.
/// </summary>
public class CurationDraftDto
{
    public string? ProjectId { get; set; }
    public DateTime Day { get; set; }

    /// <summary>Local hour the day's selection crosses with or without him.</summary>
    public int CutoffHour { get; set; }

    public DateTime? PublishedAt { get; set; }
    public BriefPublishTrigger? PublishedBy { get; set; }
    public string? PublishedByName { get; set; }

    public int FrameTotal { get; set; }
    public int WillPublish { get; set; }
    public int AlreadyShown { get; set; }
    public int Dropped { get; set; }

    /// <summary>The name the client side will read on every frame that crosses (§10.1).</summary>
    public string? AccountableName { get; set; }

    public List<CurationBlockDto> Blocks { get; set; } = new();
}

/// <summary>Frames grouped the way the brief groups them — by the work, not by time.</summary>
public class CurationBlockDto
{
    public string? Key { get; set; }
    public string? Title { get; set; }
    public string? StageName { get; set; }
    public StageGroup? Phase { get; set; }
    public List<CurationFrameDto> Frames { get; set; } = new();
}

public class CurationFrameDto
{
    public string? ImageId { get; set; }
    public string? ProgressUpdateId { get; set; }
    public string? ArtifactId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public string? EntryNote { get; set; }

    /// <summary>True authorship. The delivery side's own record (§10.1), never sent across.</summary>
    public string? CapturedByName { get; set; }
    public DateTime At { get; set; }

    public FrameCuration Curation { get; set; }
    public Channel Channel { get; set; }

    /// <summary>Crosses at the cutoff: kept, or chosen by the rule and not dropped.</summary>
    public bool WillPublish { get; set; }

    /// <summary>Why the rule chose it, in words, when it did.</summary>
    public string? Reason { get; set; }
}

public class CurationMarkDto
{
    [Required]
    public List<string> ImageIds { get; set; } = new();

    public FrameCuration Curation { get; set; }
}

public class CurationPublishResultDto
{
    public string? ProjectId { get; set; }
    public DateTime Day { get; set; }
    public BriefPublishTrigger Trigger { get; set; }
    public int FramesExposed { get; set; }
    public int FramesDropped { get; set; }
    public DateTime PublishedAt { get; set; }

    /// <summary>False when the day had already crossed and nothing new was selected.</summary>
    public bool Changed { get; set; }
}

// ─── Claim evidence (P9) ──────────────────────────────────────

/// <summary>
/// One piece of proof on a claim — a frame, a deliverable signed off, a reading.
/// A claim that carries these is paid without a phone call (assetlen.md §7, tier 3).
/// </summary>
public class ClaimEvidenceDto
{
    public string? Id { get; set; }
    public ClaimEvidenceKind Kind { get; set; }
    public string? ArtifactId { get; set; }
    public string? ProgressImageId { get; set; }
    public string? ProgressUpdateId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? DeliverableId { get; set; }
    public string? DeliverableTitle { get; set; }
    public DeliverableStatus? DeliverableStatus { get; set; }
    public decimal? Percent { get; set; }
    public string? Caption { get; set; }
    public DateTime? At { get; set; }
}

/// <summary>What the claim form offers, with a sensible selection already made.</summary>
public class ClaimEvidenceOptionsDto
{
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public DateTime? Since { get; set; }
    public List<ClaimEvidenceDto> Frames { get; set; } = new();
    public List<ClaimEvidenceDto> Deliverables { get; set; } = new();
    public ClaimEvidenceDto? LatestReading { get; set; }

    /// <summary>Frames the rule would attach if nobody chose — the newest few.</summary>
    public List<string> SuggestedImageIds { get; set; } = new();
}

// ─── Web push (P9) ────────────────────────────────────────────

public class PushSubscriptionCreateDto
{
    [Required, MaxLength(1000)] public string? Endpoint { get; set; }
    [Required, MaxLength(200)] public string? P256dh { get; set; }
    [Required, MaxLength(100)] public string? Auth { get; set; }
    [MaxLength(300)] public string? UserAgent { get; set; }
}

public class PushStatusDto
{
    public string? PublicKey { get; set; }
    public int Subscriptions { get; set; }
    public DateTime? LastDeliveredAt { get; set; }
    public bool Enabled { get; set; }
}

public class PushDeliveryDto
{
    public string? Id { get; set; }
    public PushKind Kind { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Url { get; set; }
    public PushDeliveryStatus Status { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int? LatencyMs { get; set; }
    public int? HttpStatus { get; set; }
}
