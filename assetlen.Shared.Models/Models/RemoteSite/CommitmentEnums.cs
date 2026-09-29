namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>What a commitment is about (assetlen.md §3). Money, materials, dates and decisions — nothing else is worth holding.</summary>
public enum CommitmentKind
{
    Spec = 0,
    Price = 1,
    Date = 2,
    Material = 3,
    Choice = 4
}

/// <summary>
/// Idea → In discussion → Agreed → Delivered → Verified (assetlen.md §3).
/// Ordered, so compare with <c>&gt;=</c>.
/// </summary>
public enum CommitmentMaturity
{
    Idea = 0,
    InDiscussion = 1,
    Agreed = 2,
    Delivered = 3,
    Verified = 4
}

/// <summary>
/// Cleared → Query raised → Resolved. Cleared is not closed: a paid item can be
/// reopened without it reading as an accusation, and resolving it updates the
/// item rather than leaving the answer in a message.
/// </summary>
public enum CommitmentQueryState
{
    /// <summary>Nothing has been settled against it yet, and nobody has questioned it.</summary>
    None = 0,
    Cleared = 1,
    QueryRaised = 2,
    Resolved = 3
}

/// <summary>
/// Where a commitment was made. The most expensive ones on the real project
/// were never typed anywhere (whatsapp-evidence.md F5), so Verbal and Meeting
/// are first-class, not a note field.
/// </summary>
public enum CommitmentSource
{
    App = 0,
    Ingested = 1,
    Verbal = 2,
    Meeting = 3
}

/// <summary>A deliverable is one checklist line inside a funded stage.</summary>
public enum DeliverableStatus
{
    NotStarted = 0,
    InProgress = 1,
    Done = 2
}

/// <summary>What a commitment link points at. Backlinks run both ways off the same row.</summary>
public enum CommitmentLinkTarget
{
    Commitment = 0,
    Artifact = 1,
    IngestedMessage = 2,
    ProgressUpdate = 3,
    FundingEntry = 4,
    Flag = 5,
    Document = 6,
    Claim = 7,
    Variation = 8,

    /// <summary>A markup layer on an artifact — the circled line a query was asked about.</summary>
    Annotation = 9
}

/// <summary>The marks a layer is made of. Coordinates are fractions of the image, so a layer fits any rendering of the original.</summary>
public enum AnnotationShapeKind
{
    Ellipse = 0,
    Rect = 1,
    Arrow = 2,
    Path = 3
}

/// <summary>How the target stands to the commitment — the provenance strip reads these.</summary>
public enum CommitmentLinkRelation
{
    /// <summary>Where the commitment was said — a message, a minute, a drawing.</summary>
    Source = 0,

    /// <summary>Proof it happened — a photo, a delivery note, another commitment it delivers.</summary>
    Evidence = 1,

    /// <summary>A bill against it.</summary>
    Invoice = 2,

    /// <summary>The release or claim that settled it.</summary>
    Clears = 3,

    Relates = 4
}

/// <summary>An extra — a change to agreed scope, with its cost delta.</summary>
public enum VariationStatus
{
    Proposed = 0,
    Approved = 1,
    Rejected = 2,
    Withdrawn = 3
}

/// <summary>
/// A progress claim against a stage: the contractor asks, the funder clears.
/// The step between "funded" and "carried forward" in the stage ledger.
/// </summary>
public enum ClaimStatus
{
    Claimed = 0,
    Cleared = 1,
    Queried = 2,
    Withdrawn = 3
}
