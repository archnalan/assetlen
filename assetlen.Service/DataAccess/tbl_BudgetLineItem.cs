using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A line-itemed slice of a project's planned spend. Optional StageId
/// link associates the line item with a specific stage; otherwise the
/// item is project-wide (e.g. permits, contingency).
/// </summary>
public class tbl_BudgetLineItem : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public BudgetCategory Category { get; set; } = BudgetCategory.Other;

    public decimal PlannedAmount { get; set; }

    public int DisplayOrder { get; set; }

    [MaxLength(450)]
    public string? CreatedById { get; set; }

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("CreatedById")]
    public AppUser? CreatedBy { get; set; }

    [InverseProperty("BudgetLineItem")]
    public ICollection<tbl_Receipt> Receipts { get; set; } = new List<tbl_Receipt>();
}
