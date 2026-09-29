using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── Peter's surfaces (plan.md P7) ────────────────────────────────────────────
//
// Both answers are assembled on the server and already filtered to what this
// reader may see on each project. The client renders them and derives nothing.

/// <summary>
/// Whose emphasis a surface is weighted for. Emphasis orders; it never drops —
/// the funder and the representative read the same facts (assetlen.md §5, Dinah.md).
/// </summary>
public enum ReaderEmphasis
{
    /// <summary>Funds the stages: progress, money and dates lead.</summary>
    Funder = 0,

    /// <summary>The client's person on the ground: specs, finishes and choices owed lead.</summary>
    Representative = 1,

    /// <summary>The delivery side or the mediator: the brief as the client will read it.</summary>
    Delivery = 2
}

/// <summary>The four facts no curation can drop (assetlen.md §5, the truth floor).</summary>
public enum TruthFloorKind
{
    Money = 0,
    Date = 1,
    Spec = 2,
    Blocker = 3,
    DecisionOwed = 4
}

/// <summary>What a reader owes — the RFI register in Peter's words, "decisions I owe".</summary>
public enum OwedKind
{
    /// <summary>An open choice this reader's side has to make.</summary>
    Choice = 0,

    /// <summary>A spoken agreement the other side wrote down, waiting on Confirm or "That's not what we said".</summary>
    Confirmation = 1,

    /// <summary>An open question or blocker assigned to this reader.</summary>
    Question = 2,

    /// <summary>A stage claim waiting for the funder to clear it.</summary>
    Claim = 3,

    /// <summary>A proposed extra waiting for the funder's yes or no.</summary>
    Variation = 4,

    /// <summary>A release waiting to be acknowledged, or reported short and unanswered.</summary>
    Funding = 5,

    /// <summary>Commitments read from the thread, waiting for one tap into the register.</summary>
    Proposals = 6,

    /// <summary>
    /// A parked idea whose waiting has started to cost something — a lead time,
    /// a dependency, or its stage kicking off (assetlen.md Law 4). Silent otherwise.
    /// </summary>
    ParkedIdea = 7
}

public enum MovedKind
{
    Commitment = 0,
    Thread = 1,
    Frames = 2,
    Reading = 3,
    Money = 4,
    Capture = 5,
    Blocker = 6
}

// ─── Home ────────────────────────────────────────────────────────────────────

/// <summary>Every project the reader stands on, each with where it stands — one call, one membership query.</summary>
public class HomeDto
{
    public List<HomeProjectDto> Projects { get; set; } = new();

    /// <summary>Everything owed by this reader across all projects, soonest by-when first, undated last.</summary>
    public List<OwedItemDto> Owed { get; set; } = new();

    /// <summary>The start of the "what moved" window.</summary>
    public DateTime Since { get; set; }

    public string? Currency { get; set; }

    /// <summary>Summed across the projects whose money this reader sees. Null when they see none.</summary>
    public HomeMoneyDto? Portfolio { get; set; }
}

public class HomeProjectDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ParentProjectId { get; set; }
    public string? ParentName { get; set; }

    public ProjectAccessDto Standing { get; set; } = new();
    public ReaderEmphasis Emphasis { get; set; }

    /// <summary>Null when money is not part of this reader's seat on this project.</summary>
    public HomeMoneyDto? Money { get; set; }

    public int OwedCount { get; set; }
    public int OverdueCount { get; set; }

    /// <summary>The first thing owed here, so the row can name it rather than only count it.</summary>
    public OwedItemDto? NextOwed { get; set; }

    public string? CurrentStageName { get; set; }
    public StageGroup? CurrentStagePhase { get; set; }

    /// <summary>The latest reading on the stage in hand, and where it came from.</summary>
    public decimal? CurrentStagePercent { get; set; }

    public List<MovedLineDto> Moved { get; set; } = new();
    public DateTime? LastMovedAt { get; set; }

    /// <summary>Open blockers on this project, whichever side raised them — part of the truth floor.</summary>
    public int OpenBlockers { get; set; }
}

/// <summary>funded → claimed → cleared → in hand, computed from rows the way the stage ledger computes them.</summary>
public class HomeMoneyDto
{
    public string Currency { get; set; } = "UGX";
    public decimal Budget { get; set; }
    public decimal Funded { get; set; }
    public decimal PendingFunding { get; set; }
    public decimal Claimed { get; set; }
    public decimal Cleared { get; set; }
    public decimal AwaitingClearance { get; set; }

    /// <summary>Funded and not yet cleared — held by the delivery side, carried or not.</summary>
    public decimal InHand { get; set; }

    public decimal VariationsApproved { get; set; }
    public int VariationsUncosted { get; set; }
}

public class MovedLineDto
{
    public MovedKind Kind { get; set; }
    public string Text { get; set; } = "";
    public DateTime At { get; set; }
    public string? Href { get; set; }
    public string? StageName { get; set; }
    public StageGroup? StagePhase { get; set; }
}

public class OwedItemDto
{
    /// <summary>Stable across rescans — "flag:{id}", "commitment:{id}", "funding:{id}", …</summary>
    public string Key { get; set; } = "";
    public OwedKind Kind { get; set; }

    public string ProjectId { get; set; } = "";
    public string ProjectName { get; set; } = "";
    public string? StageName { get; set; }
    public StageGroup? StagePhase { get; set; }
    public string? DeliverableTitle { get; set; }

    public string Title { get; set; } = "";

    /// <summary>What waiting costs — what it holds up, what it is worth, when the work it gates starts.</summary>
    public string? Consequence { get; set; }

    public DateTime? DueBy { get; set; }
    public bool IsOverdue { get; set; }

    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public bool AmountHidden { get; set; }

    public string Href { get; set; } = "";
}

// ─── The daily brief ─────────────────────────────────────────────────────────

/// <summary>
/// One page per project per day, grouped by deliverable, assembled with no
/// curator from what was imported and captured. The truth floor is injected
/// and nothing about it is optional.
/// </summary>
public class DailyBriefDto
{
    public string ProjectId { get; set; } = "";
    public string? ProjectName { get; set; }

    /// <summary>The last day of the window. The brief covers <see cref="From"/> up to the end of this day.</summary>
    public DateTime Day { get; set; }
    public DateTime From { get; set; }
    public int Days { get; set; }

    public ReaderEmphasis Emphasis { get; set; }
    public ProjectSide? ReaderSide { get; set; }

    /// <summary>The accountable face every crossing item is attributed to (assetlen.md §10.1).</summary>
    public string? MediatorName { get; set; }

    /// <summary>The truth-floor rule, stated to both parties once, plainly.</summary>
    public string Rule { get; set; } = "";

    /// <summary>When this day's brief publishes, touched or not.</summary>
    public DateTime PublishesAt { get; set; }
    public bool IsPublished { get; set; }

    /// <summary>True when nobody on the delivery side shaped any of it — the page still stands (Law 0).</summary>
    public bool AssembledWithoutCurator { get; set; }

    /// <summary>Ordered for this reader's emphasis. Every kind is present for every reader who may see it.</summary>
    public List<TruthFloorSectionDto> TruthFloor { get; set; } = new();

    public List<BriefBlockDto> Blocks { get; set; } = new();

    public BriefCountsDto Counts { get; set; } = new();
}

public class TruthFloorSectionDto
{
    public TruthFloorKind Kind { get; set; }
    public string Label { get; set; } = "";
    public List<TruthItemDto> Items { get; set; } = new();
}

public class TruthItemDto
{
    public string Key { get; set; } = "";
    public TruthFloorKind Kind { get; set; }
    public string Title { get; set; } = "";
    public string? Detail { get; set; }
    public DateTime? At { get; set; }

    /// <summary>For a date that moved: the previous statement.</summary>
    public DateTime? PreviousDate { get; set; }
    public DateTime? NewDate { get; set; }

    public string? StageName { get; set; }
    public StageGroup? StagePhase { get; set; }
    public string? DeliverableTitle { get; set; }

    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public bool AmountHidden { get; set; }

    /// <summary>"In your register", "Read from the thread — not yet confirmed", "Recorded in the ledger".</summary>
    public string Source { get; set; } = "";

    /// <summary>The accountable face for this reader.</summary>
    public string? Who { get; set; }

    /// <summary>True when this reached the reader only because of the floor — it sits on the delivery side's channel.</summary>
    public bool CrossedByFloor { get; set; }

    /// <summary>True for a figure or date read from the thread that nobody has confirmed into the register yet.</summary>
    public bool Unconfirmed { get; set; }

    public DateTime? DueBy { get; set; }
    public bool IsOverdue { get; set; }

    public string? Href { get; set; }
}

/// <summary>One deliverable's day — or its stage's, when the work named no deliverable.</summary>
public class BriefBlockDto
{
    public string Key { get; set; } = "";
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public StageGroup? StagePhase { get; set; }
    public StageStatus? StageStatus { get; set; }
    public string? DeliverableId { get; set; }
    public string? DeliverableTitle { get; set; }
    public DeliverableStatus? DeliverableStatus { get; set; }

    /// <summary>Set when only the stage catalogue named this work: it is not a stage on the project yet, and the block says so.</summary>
    public string? CatalogueKey { get; set; }

    /// <summary>The latest reading, and the one before the window, when both exist.</summary>
    public decimal? Percent { get; set; }
    public decimal? PercentBefore { get; set; }
    public string? PercentSource { get; set; }

    /// <summary>Why this block leads for this reader, when it does.</summary>
    public string? EmphasisReason { get; set; }

    /// <summary>Same view, earlier and now. The answer to seventeen frames that read as "nothing much changed".</summary>
    public List<VantagePairDto> Pairs { get; set; } = new();

    /// <summary>Frames not in a pair, newest first, capped.</summary>
    public List<BriefFrameDto> Frames { get; set; } = new();
    public int FrameTotal { get; set; }

    public List<BriefNoteDto> Notes { get; set; } = new();

    /// <summary>Commitments on this deliverable that were agreed, restated, delivered or queried in the window.</summary>
    public List<TruthItemDto> Commitments { get; set; } = new();
}

public class VantagePairDto
{
    public BriefFrameDto Before { get; set; } = new();
    public BriefFrameDto After { get; set; } = new();
    public int DaysApart { get; set; }

    /// <summary>0 (identical framing) to 1. The pair is only offered under a threshold.</summary>
    public double Distance { get; set; }
}

public class BriefFrameDto
{
    public string ArtifactId { get; set; } = "";
    public string? MimeType { get; set; }
    public bool HasThumbnail { get; set; }
    public DateTime At { get; set; }
    public string? Caption { get; set; }

    /// <summary>"From the thread" or "Captured on site".</summary>
    public string Origin { get; set; } = "";
    public string? Href { get; set; }
}

public class BriefNoteDto
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime At { get; set; }
    public string? Who { get; set; }
    public string Origin { get; set; } = "";
    public string? Href { get; set; }
}

public class BriefCountsDto
{
    public int MessagesRead { get; set; }

    /// <summary>"Okay", "Noted", "Good progress" — read, and deliberately given no space.</summary>
    public int AcknowledgementsSetAside { get; set; }

    public int Frames { get; set; }
    public int Paired { get; set; }
    public int Captures { get; set; }

    /// <summary>Material that named no stage or deliverable the filer could recognise.</summary>
    public int Unfiled { get; set; }
}
