namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// What a search result is. Results are grouped by this, never returned as a
/// flat list of messages — a message list is the pile Peter already has and
/// cannot read (plan.md P6).
/// </summary>
public enum SearchHitKind
{
    Commitment = 0,
    File = 1,
    Message = 2,
    DiaryEntry = 3,
    Stage = 4,
    Project = 5
}

/// <summary>
/// The provenance strip (assetlen.md §8): agreed → evidence → invoiced →
/// cleared → queried → resolved. Queried and resolved are the right to reopen,
/// so they are drawn after cleared rather than as a failure of it.
/// </summary>
public enum ProvenanceStep
{
    Agreed = 0,
    Evidence = 1,
    Invoiced = 2,
    Cleared = 3,
    Queried = 4,
    Resolved = 5
}

/// <summary>Where a result entered the record — the source chip (works-report §3.1).</summary>
public enum SearchOrigin
{
    Thread = 0,
    Shared = 1,
    Email = 2,
    Capture = 3,
    Document = 4,
    Register = 5,
    Spoken = 6,
    Meeting = 7,
    Project = 8
}
