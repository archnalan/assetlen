using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace assetlen.Service.DataAccess;

public partial class tbl_CustomerPricing : BaseEntity
{
    //public int Id { get; set; }

    public string? CustomerId { get; set; }

    public string? ProductId { get; set; }

    public string? PriceGroupId { get; set; }

    public decimal? PriceInc { get; set; }

    public decimal? PriceExc { get; set; }

    public string? TaxId { get; set; }

    //public bool? IsDeleted { get; set; }

    public int? SortOrder { get; set; }

    public decimal? CostInc { get; set; }

    public decimal? CostExc { get; set; }
}
