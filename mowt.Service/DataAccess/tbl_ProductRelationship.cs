using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_ProductRelationship : BaseEntity
{
    // public int Id { get; set; }

    public string? HasAsubProductId { get; set; }

    public string? IsAsubProductId { get; set; }

    public decimal? Qty { get; set; }

    public int? SortOrder { get; set; }
}
