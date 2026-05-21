using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public partial class tbl_TransactionDetail : BaseEntity
{
    //public int Id { get; set; }

    public string? ProductId { get; set; }

    public decimal? Qty { get; set; }

    public decimal? CostExc { get; set; }

    public decimal? CostInc { get; set; }

    public decimal? PriceInc { get; set; }

    public decimal? PriceExc { get; set; }

    public string? TaxId { get; set; }

    public decimal? TaxPercent { get; set; }

    public string? DiscountId { get; set; }

    public decimal? DiscountPercent { get; set; }

    public string? TransactionId { get; set; }

    public decimal? TotalPriceInc { get; set; }

    public decimal? TotalPriceExc { get; set; }

    public int? SortOrder { get; set; }

    public bool? CostIncState { get; set; }

    public bool? SpecialPricingUsed { get; set; }

    public string? ImportedId { get; set; }

    public string? ItemNote { get; set; }

    //[ForeignKey(nameof(TransactionId))]
    //public tbl_Transaction? Transaction { get; set; }

    [ForeignKey(nameof(DiscountId))]
    public tbl_Discount? Discount { get; set; }

    [ForeignKey(nameof(TaxId))]
    public tbl_Tax? Tax { get; set; }

    [ForeignKey(nameof(ProductId))]
    public tbl_Product? Product { get; set; }
}
