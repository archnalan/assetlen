using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One canonical file, uploaded once, living at a permanent address.
/// <para>
/// assetlen.md Law 2: <em>one canonical artifact, many pointers</em>. Every
/// later mention is a <see cref="tbl_ArtifactRef"/>, never a copy. Re-uploads
/// are matched on <see cref="Sha256"/> and return the artifact that already
/// exists — <em>"this is already Receipt R-014"</em>. Repetition is a tax on
/// the absence of addressing; fix addressing and it disappears.
/// </para>
/// <para>
/// The artifact carries no caption, no ordering and <b>no visibility</b>.
/// Those belong to the ref, because the same file can be Crew-only in the Site
/// Log and exposed in the Client Brief without being duplicated.
/// </para>
/// </summary>
public class tbl_Artifact : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>
    /// Lowercase hex SHA-256 of the file bytes. Unique per tenant — the same
    /// photo sent by two people is one artifact with two refs.
    /// </summary>
    [MaxLength(64)]
    public string? Sha256 { get; set; }

    public long ByteSize { get; set; }

    [MaxLength(100)]
    public string? MimeType { get; set; }

    /// <summary>
    /// Content-addressed relative path, <c>artifacts/{sha[0..2]}/{sha}</c>.
    /// Relative so the storage root can move to Blob or Drive behind
    /// <c>IArtifactStorage</c> without rewriting rows.
    /// </summary>
    [MaxLength(500)]
    public string? StoragePath { get; set; }

    /// <summary>Generated on ingest for images. Null for non-image types.</summary>
    [MaxLength(500)]
    public string? ThumbnailPath { get; set; }

    [MaxLength(260)]
    public string? OriginalFileName { get; set; }

    [MaxLength(450)]
    public string? UploadedById { get; set; }

    /// <summary>
    /// When the photo was taken, not when it was uploaded. Real capture is
    /// retrospective — a batch posted at 23:40 may be of work done at 09:00 —
    /// so ordering a brief by upload time misrepresents the day.
    /// </summary>
    public DateTime? CapturedAt { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("UploadedById")]
    public AppUser? UploadedBy { get; set; }

    [InverseProperty("Artifact")]
    public ICollection<tbl_ArtifactRef> References { get; set; } = new List<tbl_ArtifactRef>();
}
