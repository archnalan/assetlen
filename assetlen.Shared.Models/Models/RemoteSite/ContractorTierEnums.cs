namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// The mediator's say over one captured frame (assetlen.md §5, curation by
/// exception). <see cref="Auto"/> leaves it to the cutoff's selection rule; the
/// other two are the only gestures he needs — keep this one, lose that one.
/// </summary>
public enum FrameCuration
{
    Auto = 0,
    Promoted = 1,
    Dropped = 2
}

/// <summary>Who sent a day's curated frames across.</summary>
public enum BriefPublishTrigger
{
    /// <summary>The mediator pressed publish.</summary>
    Mediator = 0,

    /// <summary>The cutoff passed and it went anyway — the brief does not wait for him.</summary>
    Cutoff = 1
}

/// <summary>What a claim carries so the funder can clear it without a phone call.</summary>
public enum ClaimEvidenceKind
{
    Frame = 0,
    Deliverable = 1,
    Reading = 2,
    File = 3
}

/// <summary>Why a push was sent. Kept narrow on purpose — Law 4 applies to the phone too.</summary>
public enum PushKind
{
    Test = 0,
    Capture = 1,
    Claim = 2,
    BriefPublished = 3,
    Query = 4
}

public enum PushDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Gone = 3
}
