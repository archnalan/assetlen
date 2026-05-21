using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;

namespace mowt.Service.DataAccess;

public partial class tbl_Log : BaseEntity
{
    // public int Id { get; set; }

    public string? Message { get; set; }

    public string? MessageTemplate { get; set; }

    public string? Level { get; set; }

    public DateTime? TimeStamp { get; set; }

    public string? Exception { get; set; }

    public string? Properties { get; set; }

    public string? UserId { get; set; }

    public string? ShiftId { get; set; }

    public string? SaleId { get; set; }

    public int? LogTypeId { get; set; }

    public int? OldQty { get; set; }

    public int? NewQty { get; set; }

    public string? ProductId { get; set; }
}
