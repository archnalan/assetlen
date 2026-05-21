using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public partial class tbl_PaymentAccount : BaseEntity
{
    //public int Id { get; set; }

    public int PaymentTypeId { get; set; }

    public string? PaymentAccountName { get; set; }

    public decimal? OpeningBalance { get; set; }

}
