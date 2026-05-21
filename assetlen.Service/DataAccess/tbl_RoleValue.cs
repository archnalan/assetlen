using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace assetlen.Service.DataAccess;

public partial class tbl_RoleValue : BaseEntity
{
    // public int Id { get; set; }

    public int? UserId { get; set; }

    public int? RoleId { get; set; }

    public bool? RoleValue { get; set; }
}
