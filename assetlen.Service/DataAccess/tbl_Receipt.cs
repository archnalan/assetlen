using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// An actual outlay recorded against a budget line item. Sum of receipts
/// for a line gives the actual spend; PlannedAmount − sum = remaining.
/// </summary>
public class tbl_Receipt : BaseEntity
{
    [MaxLength(40)]
    public string? BudgetLineItemId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [MaxLength(200)]
    public string? VendorName { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? ReceiptImageUrl { get; set; }

    [MaxLength(450)]
    public string? CreatedById { get; set; }

    // Navigation
    [ForeignKey("BudgetLineItemId")]
    public tbl_BudgetLineItem? BudgetLineItem { get; set; }

    [ForeignKey("CreatedById")]
    public AppUser? CreatedBy { get; set; }
}
