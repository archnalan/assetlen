using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A house's schedule (works-report.md §4.6): its site calendar and the last
/// plan the engine computed for it. One per top-level project; the guest wing's
/// lines are scheduled with the house they belong to. The committed handover is
/// not here — it is a Date commitment, so every restatement of it is kept.
/// </summary>
public class tbl_WorkSchedule : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>The site's rest day, as <see cref="DayOfWeek"/>.</summary>
    public int RestDay { get; set; } = (int)DayOfWeek.Saturday;

    /// <summary>Public holidays and site breaks, ISO days, comma-separated.</summary>
    [MaxLength(2000)]
    public string? Holidays { get; set; }

    /// <summary>A wait that outlives its date runs on by its cause's measured lag (§4.4).</summary>
    public bool ExtendLateWaits { get; set; } = true;

    public DateTime? LastComputedAt { get; set; }

    [MaxLength(450)]
    public string? LastSavedById { get; set; }

    public DateTime? LastSavedAt { get; set; }

    public DateTime? WorksComplete { get; set; }

    /// <summary>Works complete before the last computation that moved it, so the movement is read first.</summary>
    public DateTime? PreviousWorksComplete { get; set; }

    public DateTime? WorksCompleteMovedAt { get; set; }

    public int? ReserveDays { get; set; }

    /// <summary>The line ids that set the date, first first.</summary>
    [MaxLength(4000)]
    public string? CriticalPath { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }
}
