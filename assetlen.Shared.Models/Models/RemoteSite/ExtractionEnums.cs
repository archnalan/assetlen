namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// What a proposal read from the thread would become if accepted (assetlen.md Law 3).
/// The first five mirror <see cref="CommitmentKind"/> value for value; a blocker
/// becomes a flag with a named owner rather than a commitment.
/// </summary>
public enum ProposalKind
{
    Spec = 0,
    Price = 1,
    Date = 2,
    Material = 3,
    Choice = 4,
    Blocker = 10
}

/// <summary>A proposal waits for a person. Nothing extracted becomes truth on its own.</summary>
public enum ProposalStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

/// <summary>Where a piece of text read out of an artifact stands.</summary>
public enum ArtifactTextStatus
{
    /// <summary>Queued; the OCR job has not reached it yet.</summary>
    Pending = 0,

    /// <summary>Text was read.</summary>
    Done = 1,

    /// <summary>Read, and there was nothing to read — most site photos.</summary>
    NoText = 2,

    /// <summary>A kind of file no engine here can read (a video, an archive).</summary>
    Unsupported = 3,

    /// <summary>
    /// No OCR engine is configured on this server. Kept distinct from
    /// <see cref="NoText"/> so an empty result is never mistaken for an empty receipt;
    /// re-queue once an engine exists.
    /// </summary>
    EngineUnavailable = 4,

    Failed = 5
}

/// <summary>
/// Where a progress reading came from (works-report.md §4.2). Readings are
/// append-only: a stall is only visible if yesterday's 90% is still there
/// beside today's.
/// </summary>
public enum ProgressReadingSource
{
    Stage = 0,
    Capture = 1,
    Ingested = 2,
    Manual = 3
}

/// <summary>What happened to one loose file offered to the media re-join.</summary>
public enum MediaRejoinOutcome
{
    /// <summary>Bound to a <c>&lt;Media omitted&gt;</c> line by its filename stamp.</summary>
    Bound = 0,

    /// <summary>The same bytes as another file in this upload, or already on the project.</summary>
    Duplicate = 1,

    /// <summary>Already bound by an earlier re-join. Nothing new was written.</summary>
    AlreadyBound = 2,

    /// <summary>Kept as an artifact, attached to no message, and reported.</summary>
    Unbound = 3
}
