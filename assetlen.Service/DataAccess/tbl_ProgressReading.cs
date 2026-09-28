using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// Progress over time (works-report.md §4.2). Append-only: "90% since 2 Sep" is a
/// stall only because the 2 Sep reading is still here beside the 22 Sep one.
/// <para>
/// A reading is an observation with a source, not a commitment, so an extracted
/// "about 80%" is written directly and never waits in the review queue.
/// </para>
/// </summary>
public class tbl_ProgressReading : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>Null when the words named no stage this project has; <see cref="Subject"/> still says what was read.</summary>
    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    public decimal Percent { get; set; }

    public DateTime ObservedAt { get; set; }

    public ProgressReadingSource SourceKind { get; set; }

    [MaxLength(40)]
    public string? SourceId { get; set; }

    /// <summary>Set for an ingested reading: it is readable by whoever may read its message.</summary>
    public ProjectSide? SourceSide { get; set; }

    [MaxLength(450)]
    public string? SourceImportedById { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }
}
