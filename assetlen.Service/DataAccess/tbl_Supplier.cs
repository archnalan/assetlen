using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace assetlen.Service.DataAccess;

public partial class tbl_Supplier : BaseEntity
{
    //public int Id { get; set; }

    public string? AccountNumber { get; set; }

    public string? FullName { get; set; }

    public string? Contact { get; set; }

    public string? CardNumber { get; set; }

    public string? VatNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public decimal? CreditLimit { get; set; }

    public bool? Deleted { get; set; }

    public string? Company { get; set; }
}
