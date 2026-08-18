using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_Stage : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(200)]
    public string? StageName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? BudgetAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpectedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public decimal? CompletionPercentage { get; set; }

    public int DisplayOrder { get; set; }

    public StageStatus Status { get; set; } = StageStatus.NotStarted;

    /// <summary>
    /// The major stage this one sits under. One level only, as with projects:
    /// the point is a short list of phases a reader can hold in their head, with
    /// the detail folded underneath — not a tree nobody can navigate.
    /// </summary>
    [MaxLength(40)]
    public string? ParentStageId { get; set; }

    /// <summary>
    /// The catalogue entry this stage was taken from, or null when the reader
    /// named it themselves. Stored so the catalogue can grey out what this
    /// project already uses instead of letting somebody add "Roofing" twice.
    /// </summary>
    [MaxLength(60)]
    public string? CatalogueKey { get; set; }

    /// <summary>
    /// Which phase of the build this belongs to. Decides the accent colour, and
    /// it is stored rather than derived from <see cref="CatalogueKey"/> because a
    /// custom stage needs one too — the reader picks the phase when they name it.
    /// </summary>
    public StageGroup Phase { get; set; } = StageGroup.Custom;

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("ParentStageId")]
    public tbl_Stage? ParentStage { get; set; }

    [InverseProperty("ParentStage")]
    public ICollection<tbl_Stage> SubStages { get; set; } = new List<tbl_Stage>();

    [InverseProperty("Stage")]
    public ICollection<tbl_FundingEntry> FundingEntries { get; set; } = new List<tbl_FundingEntry>();

    [InverseProperty("Stage")]
    public ICollection<tbl_ProgressUpdate> ProgressUpdates { get; set; } = new List<tbl_ProgressUpdate>();
}
