using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_OrderProcess : BaseEntity
{
    //public int Id { get; set; }

    public byte[]? Description { get; set; }

    public int? SortId { get; set; }
}
