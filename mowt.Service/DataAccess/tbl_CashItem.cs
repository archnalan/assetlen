using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_CashItem : BaseEntity
{
    //public int Id { get; set; }

    public decimal? Amount { get; set; }
}
