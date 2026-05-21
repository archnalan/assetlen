using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Service.DataAccess;

public partial class tbl_Configuration : BaseEntity
{
    public string? StringValue { get; set; }

    public int ConfigId { get; set; }

    [ForeignKey(nameof(TenantId))]
    public tbl_Tenant? Tenant { get; set; }
}
