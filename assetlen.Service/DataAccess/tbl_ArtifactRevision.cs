using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One issue of a <see cref="tbl_Document"/>. Append-only: reissuing adds a
/// row and repoints <c>tbl_Document.CurrentRevisionId</c>. Nothing is
/// overwritten, so <em>"which drawing was current when we poured that slab"</em>
/// stays answerable months later.
/// </summary>
public class tbl_ArtifactRevision : BaseEntity
{
    [MaxLength(40)]
    public string? DocumentId { get; set; }

    /// <summary>The file for this issue. Hash-shared with any identical upload.</summary>
    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    /// <summary>1-based, contiguous, assigned server-side.</summary>
    public int RevisionNo { get; set; }

    [MaxLength(450)]
    public string? IssuedById { get; set; }

    public DateTime? IssuedAt { get; set; }

    /// <summary>What changed and why — the seed of a variation record in P3.</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Set when a later revision replaces this one. Null on the current
    /// revision. Kept explicit rather than derived from <c>RevisionNo</c> so a
    /// superseded revision reads as superseded on its own row.
    /// </summary>
    [MaxLength(40)]
    public string? SupersededByRevisionId { get; set; }

    // Navigation
    [ForeignKey("DocumentId")]
    public tbl_Document? Document { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }

    [ForeignKey("IssuedById")]
    public AppUser? IssuedBy { get; set; }
}
