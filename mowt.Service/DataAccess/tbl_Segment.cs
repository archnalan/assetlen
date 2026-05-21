using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_Segment : BaseEntity
{
    // public int Id { get; set; }

    public string? Segment { get; set; }

    public string? Description { get; set; }

    public bool? HideInPos { get; set; }
}
