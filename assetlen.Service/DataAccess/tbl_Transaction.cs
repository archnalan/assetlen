using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public partial class tbl_Transaction : BaseEntity
{
    //public int Id { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? SoldBy { get; set; }

    public decimal? SaleTotal { get; set; }

    public decimal? Change { get; set; }

    public string? ShiftId { get; set; }

    public string? CustomerId { get; set; }

    public int? TransactionStatus { get; set; }

    public string? SaleAgentId { get; set; }

    public string? QuotationId { get; set; }

    public string? OrderStatus { get; set; }

    public string? ImportedId { get; set; }

    public string? TransactionComment { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public tbl_Customer? Customer { get; set; }

    [ForeignKey(nameof(SoldBy))]
    public AppUser? Seller { get; set; }

    [ForeignKey(nameof(SaleAgentId))]
    public AppUser? SaleAgent { get; set; }
    public ICollection<tbl_TransactionDetail>? TransactionDetails { get; set; }
}
