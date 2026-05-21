using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_ProductReceiving : BaseEntity
{
    // public int Id { get; set; }

    public string? ProductId { get; set; }

    public decimal? Qty { get; set; }

    public DateTime? DateReceived { get; set; }

    public string? GrnsupplierNumber { get; set; }

    public string? ReceivedBy { get; set; }

    public bool? PriceChanged { get; set; }

    public decimal? NewCostInc { get; set; }

    public decimal? NewPriceInc { get; set; }

    public DateTime? PriceChangeScheduled { get; set; }

    public string? OrderId { get; set; }

    public bool? CreditSupplierAcc { get; set; }

    public string? SupplierAccount { get; set; }

    public decimal? CostExc { get; set; }

    public decimal? CostInc { get; set; }
}
