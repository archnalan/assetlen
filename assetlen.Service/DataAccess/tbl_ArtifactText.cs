using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// The text read out of one artifact (assetlen.md Law 3 — OCR every receipt and
/// invoice). One row per artifact, written by the OCR queue, never by a person.
/// <para>
/// A receipt that only ever existed as a photo inside a WhatsApp export becomes
/// findable by its vendor name only through this table (plan.md P6's exit).
/// </para>
/// </summary>
public class tbl_ArtifactText : BaseEntity
{
    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    [MaxLength(40)]
    public string? ProjectId { get; set; }

    public ArtifactTextStatus Status { get; set; } = ArtifactTextStatus.Pending;

    [MaxLength(40)]
    public string? Engine { get; set; }

    /// <summary>Unbounded: a scanned bill runs to pages.</summary>
    public string? Text { get; set; }

    public int CharCount { get; set; }

    public int Attempts { get; set; }

    public DateTime? ExtractedAt { get; set; }

    [MaxLength(1000)]
    public string? Error { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }
}
