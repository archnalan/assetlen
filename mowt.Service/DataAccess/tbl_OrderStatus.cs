using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_OrderStatus : BaseEntity
{
    // public int Id { get; set; }

    public string? OrderName { get; set; }

    public int? SortOrder { get; set; }

    //public bool? IsDeleted { get; set; }
}
