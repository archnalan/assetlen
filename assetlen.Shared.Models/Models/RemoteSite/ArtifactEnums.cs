namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// What an <c>tbl_ArtifactRef</c> points at. The artifact itself is anonymous —
/// it is bytes at a permanent address. The ref says where it is being *used*,
/// which is what carries the caption, the ordering and the exposure decision.
/// </summary>
public enum ArtifactTargetType
{
    /// <summary>A Site Log capture (<c>tbl_ProgressUpdate</c>).</summary>
    ProgressUpdate = 0,
    /// <summary>A revision of a drawing, schedule or bill (<c>tbl_Document</c>).</summary>
    Document = 1,
    /// <summary>Evidence on a Commitment. Reserved for P3.</summary>
    Commitment = 2,
    /// <summary>A block in a published Client Brief. Reserved for P5.</summary>
    Brief = 3,
    /// <summary>A receipt or invoice image.</summary>
    Receipt = 4
}

/// <summary>
/// Kinds of controlled document that carry a revision chain. The current
/// revision is pinned; superseded revisions are archived, never deleted —
/// building to a superseded drawing is a real, observed defect.
/// </summary>
public enum DocumentKind
{
    ArchitecturalDrawing = 0,
    StructuralDrawing = 1,
    /// <summary>Door / window / wardrobe schedules.</summary>
    Schedule = 2,
    BillOfQuantities = 3,
    Permit = 4,
    Quotation = 5,
    Other = 99
}
