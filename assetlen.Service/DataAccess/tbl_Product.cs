using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public partial class tbl_Product : BaseEntity
{
    //[Key]
    // public int Id { get; set; }

    public string? ProductCode { get; set; }

    public string? BarCode { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public decimal? CostExclusive { get; set; }

    public decimal? CostInclusive { get; set; }

    public decimal? InStock { get; set; }

    public decimal? PriceExclusive { get; set; }

    public decimal? PriceExclusive2 { get; set; }

    public decimal? PriceInclusive { get; set; }

    public decimal? PriceInclusive2 { get; set; }

    public string? CategoryId { get; set; }

    public string? Location { get; set; }

    public string? SegmentId { get; set; }

    public string? SupplierId { get; set; }

    public string? ProductImage { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string CreatedBy { get; set; }

    public bool? Deleted { get; set; }

    public bool? TrackInventory { get; set; }

    public decimal? ReOrderLevel { get; set; }

    public decimal? ReOrderQty { get; set; }

    public bool? HasSubProduct { get; set; }

    public string? IsAsubProduct { get; set; }

    public int? CompoundCostPricing { get; set; }

    public string? TaxId { get; set; }

    public bool? CostIncStatus { get; set; }

    public int? AccessLevel { get; set; } = 0;

    [ForeignKey("TaxId")] public tbl_Tax? Tax { get; set; }
}
