using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace assetlen.Service.DataAccess;

public partial class tbl_SupplierPayment : BaseEntity
{
    //public int Id { get; set; }

    public string? UserId { get; set; }

    public string? SupplierId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? DateTimePayed { get; set; }

    public string? PaymentId { get; set; }
}
