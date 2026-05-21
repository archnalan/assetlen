using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_UniqueField : BaseEntity
{
    public string UniqueField { get; set; } = null!;

    public int? Number { get; set; }
}
