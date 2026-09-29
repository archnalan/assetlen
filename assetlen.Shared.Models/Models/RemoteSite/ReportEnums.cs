namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>What set an issued works report off (works-report.md §7).</summary>
public enum ReportIssueKind
{
    Manual = 0,
    Scheduled = 1,
    Milestone = 2
}

/// <summary>Where a video's poster frame has got to.</summary>
public enum PosterStatus
{
    Pending = 0,
    Done = 1,

    /// <summary>No ffmpeg on this server. The video is kept; its tile says "length unknown".</summary>
    EngineUnavailable = 2,
    Failed = 3
}

/// <summary>What a source chip opens (works-report.md §3.1).</summary>
public enum ReportSourceKind
{
    Message = 0,
    Photo = 1,
    Video = 2,
    Capture = 3,
    Release = 4,
    Claim = 5,
    Commitment = 6,
    Variation = 7,
    Blocker = 8,
    Reading = 9,
    Stage = 10,
    Deliverable = 11
}

/// <summary>The marks on the deadline strip — one axis, no rows, no Gantt.</summary>
public enum DeadlineMarkKind
{
    Set = 0,
    Restated = 1,
    Today = 2,
    Promised = 3,
    Lapsed = 4,
    ContractorDate = 5,
    Forecast = 6
}

public enum ReportTone
{
    Neutral = 0,
    Good = 1,
    Watch = 2,
    Late = 3
}

public enum AheadKind
{
    Deliverable = 0,
    DatePromise = 1,
    DecisionOwed = 2,
    NextStage = 3
}
