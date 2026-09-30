using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Scheduling;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One thing a work-plan line waits on: another line, something arriving by a
/// date, or drying (works-report.md §4.6). A wait taken off the plan or cleared
/// keeps its row — the waits are the record of why the project took as long as
/// it did (§4.4, Law 0).
/// </summary>
public class tbl_PlanWait : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>The line that waits.</summary>
    [MaxLength(40)]
    public string? DeliverableId { get; set; }

    public WaitKind Kind { get; set; }

    public ArrivalKind? Arrival { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>For an activity wait: the line waited on.</summary>
    [MaxLength(40)]
    public string? PredecessorId { get; set; }

    public WaitLink Link { get; set; }

    public int Days { get; set; }

    public bool CalendarDays { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? UntilDate { get; set; }

    public bool AfterMaking { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime? ClearedAt { get; set; }

    [MaxLength(450)]
    public string? ClearedById { get; set; }

    /// <summary>Taken off the plan by an edit. Kept, never deleted.</summary>
    public DateTime? RemovedAt { get; set; }

    [MaxLength(450)]
    public string? RemovedById { get; set; }

    [MaxLength(450)]
    public string? CreatedById { get; set; }

    [ForeignKey("DeliverableId")]
    public tbl_Deliverable? Deliverable { get; set; }

    [ForeignKey("PredecessorId")]
    public tbl_Deliverable? Predecessor { get; set; }
}
