using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// An extra: what changed, why, what it costs, and who approved it
/// (assetlen.md §6). Eight costed variations in the evidence thread — an added
/// floor among them — and none recorded; the 29 Jul reconciliation failure was
/// caused by that (F3 is caused by F4).
/// </summary>
public class tbl_Variation : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    /// <summary>Every variation is also a commitment, so it sits on the register with its evidence.</summary>
    [MaxLength(40)]
    public string? CommitmentId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    /// <summary>Null means nobody has costed it — shown as a gap, never as zero.</summary>
    public decimal? CostDelta { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public int? TimeDeltaDays { get; set; }

    public VariationStatus Status { get; set; } = VariationStatus.Proposed;

    [MaxLength(450)]
    public string? RaisedById { get; set; }

    public DateTime? RaisedAt { get; set; }

    [MaxLength(450)]
    public string? ApprovedById { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? DecisionNote { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("CommitmentId")]
    public tbl_Commitment? Commitment { get; set; }

    [ForeignKey("RaisedById")]
    public AppUser? RaisedBy { get; set; }

    [ForeignKey("ApprovedById")]
    public AppUser? ApprovedBy { get; set; }
}
