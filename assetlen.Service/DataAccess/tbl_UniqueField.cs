using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace assetlen.Service.DataAccess;

public partial class tbl_UniqueField : BaseEntity
{
    public string UniqueField { get; set; } = null!;

    public int? Number { get; set; }
}
