using mowt.Shared.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mowt.Service.DataAccess;

public partial class tbl_Discount : BaseEntity
{
    //[Key]
    //public int Id { get; set; }
    public string? DiscountName { get; set; }
    public decimal? DiscountValue { get; set; }
    public bool? isValuePercentage { get; set; }
    public bool? Active { get; set; }
}
