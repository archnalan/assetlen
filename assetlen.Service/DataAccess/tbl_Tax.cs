using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Service.DataAccess;

public partial class tbl_Tax : BaseEntity
{
    //public int Id { get; set; }

    public decimal? TaxValue { get; set; }

    public string? TaxDescription { get; set; }
    public bool? Deleted { get; set; }

}
