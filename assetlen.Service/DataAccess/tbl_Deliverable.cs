using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One checklist line inside a funded stage — five to eight per stage
/// (assetlen.md §3). It is what capture and commitments are aimed at, so
/// nothing on a project floats.
/// </summary>
public class tbl_Deliverable : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public DeliverableStatus Status { get; set; } = DeliverableStatus.NotStarted;

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(450)]
    public string? CompletedById { get; set; }

    // The work plan (works-report.md §4.5): when the line is planned on site, by
    // which trade — a role, never a person — and under which area it reads.
    public DateTime? PlannedStart { get; set; }

    public DateTime? PlannedEnd { get; set; }

    public int? WorkDays { get; set; }

    [MaxLength(80)]
    public string? Trade { get; set; }

    [MaxLength(80)]
    public string? Area { get; set; }

    // The scheduler's inputs (works-report.md §4.6). Once a project has a schedule,
    // PlannedStart and PlannedEnd are the last computed result, never typed.

    /// <summary>Working days made off site before the site work can start.</summary>
    public int? MakeDays { get; set; }

    /// <summary>Calendar days the finished work cures before anything follows it.</summary>
    public int? CureDays { get; set; }

    /// <summary>Not before this day — the project's calendar day, carried unshifted.</summary>
    public DateTime? EarliestStart { get; set; }

    /// <summary>The team whose queue this line waits in, and its place there.</summary>
    [MaxLength(80)]
    public string? TeamKey { get; set; }

    public int? QueueOrder { get; set; }

    /// <summary>The day work started on site. What it waited on no longer holds it.</summary>
    public DateTime? ActualStart { get; set; }

    /// <summary>A known finish — "the doors run to 21 Oct", or "needs two more days".</summary>
    public DateTime? PinnedFinish { get; set; }

    /// <summary>Where the last computation put the off-site making, for the lanes.</summary>
    public DateTime? PlannedMakeStart { get; set; }

    public DateTime? PlannedMakeEnd { get; set; }

    /// <summary>The photo the tick was made on. No photo, no tick (works-report.md §4.5).</summary>
    [MaxLength(40)]
    public string? CompletionArtifactId { get; set; }

    [ForeignKey("CompletionArtifactId")]
    public tbl_Artifact? CompletionArtifact { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("CompletedById")]
    public AppUser? CompletedBy { get; set; }
}
