using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_Size : BaseEntity
{
    //public int Id { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }
}
