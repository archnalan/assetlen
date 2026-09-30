using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One knock-off or reopening of a work-plan line, kept for good. A reopened
/// line loses its tick, never the photo it was ticked on or who did it — the
/// record of what was called done and then undone is what a dispute pulls.
/// </summary>
public class tbl_DeliverableEvent : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? DeliverableId { get; set; }

    public DeliverableEventKind Kind { get; set; }

    public DateTime OccurredAt { get; set; }

    [MaxLength(450)]
    public string? ById { get; set; }

    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    [MaxLength(40)]
    public string? ProgressUpdateId { get; set; }

    [ForeignKey("DeliverableId")]
    public tbl_Deliverable? Deliverable { get; set; }

    [ForeignKey("ById")]
    public AppUser? By { get; set; }
}
