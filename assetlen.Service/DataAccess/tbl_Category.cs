using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public partial class tbl_Category : BaseEntity
{
    //public int Id { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }

    public bool? HideInPos { get; set; }
    public bool? Deleted { get; set; }

}
