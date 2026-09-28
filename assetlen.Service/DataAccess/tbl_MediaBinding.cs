using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A loose file re-joined to the <c>&lt;Media omitted&gt;</c> line it belongs to
/// (works-report.md §5).
/// <para>
/// A separate row rather than a write to <c>tbl_IngestedMessage.ArtifactId</c>:
/// the ingested record is raw and nothing edits it (CLAUDE.md §4.3). The binding
/// is a later claim about that record, made from a filename stamp, and it stays
/// distinguishable from what the export itself carried.
/// </para>
/// </summary>
public class tbl_MediaBinding : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? IngestedMessageId { get; set; }

    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    /// <summary>The re-join run that made it.</summary>
    [MaxLength(40)]
    public string? BatchId { get; set; }

    [MaxLength(300)]
    public string? FileName { get; set; }

    public DateTime? StampedAt { get; set; }

    [ForeignKey("IngestedMessageId")]
    public tbl_IngestedMessage? IngestedMessage { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }
}
