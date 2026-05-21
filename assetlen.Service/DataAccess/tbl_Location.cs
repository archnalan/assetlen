using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace assetlen.Service.DataAccess;

public partial class tbl_Location : BaseEntity
{
    //public int Id { get; set; }

    public string? Location { get; set; }

    //public bool? IsDeleted { get; set; }
}
