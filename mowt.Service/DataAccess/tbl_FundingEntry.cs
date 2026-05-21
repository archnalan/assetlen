using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess;

public class tbl_FundingEntry : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [MaxLength(450)]
    public string? PaidById { get; set; }

    [MaxLength(450)]
    public string? ConfirmedById { get; set; }

    public DateTime? ConfirmationDate { get; set; }

    public FundingStatus Status { get; set; } = FundingStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("PaidById")]
    public AppUser? PaidBy { get; set; }

    [ForeignKey("ConfirmedById")]
    public AppUser? ConfirmedBy { get; set; }
}
