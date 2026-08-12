using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A controlled document that gets reissued — a drawing, a schedule, a bill.
/// The document is the stable identity; each issue is a
/// <see cref="tbl_ArtifactRevision"/> pointing at the artifact for that issue.
/// <para>
/// The current revision is pinned and superseded ones are archived, never
/// deleted. This exists because building to a superseded drawing is a real,
/// observed defect, not a hypothetical one.
/// </para>
/// </summary>
public class tbl_Document : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    public DocumentKind Kind { get; set; } = DocumentKind.Other;

    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>The revision on top. Every reader gets this one unless they ask for history.</summary>
    [MaxLength(40)]
    public string? CurrentRevisionId { get; set; }

    /// <summary>
    /// Whether the client side may see this document at all. Independent of
    /// per-revision refs — a drawing can be shared while an internal quotation
    /// is not.
    /// </summary>
    public Channel Channel { get; set; } = Channel.Crew;

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [InverseProperty("Document")]
    public ICollection<tbl_ArtifactRevision> Revisions { get; set; } = new List<tbl_ArtifactRevision>();
}
