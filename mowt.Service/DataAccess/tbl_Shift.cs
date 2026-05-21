using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess;

public partial class tbl_Shift : BaseEntity
{
    //public int Id { get; set; }

    public string? UserId { get; set; }

    public DateTime? DateTimeOpened { get; set; }

    public decimal? OpeningBalance { get; set; }

    public decimal? CurrentBalance { get; set; }

    public decimal? ShiftEndAmount { get; set; }

    public DateTime? DateTimeClosed { get; set; }

    public string? ActiveId { get; set; }

    public string? SubActiveId { get; set; }

    public decimal? ShiftEndCash { get; set; }

    public decimal? ShiftEndCard { get; set; }

    public decimal? ShiftEndCheque { get; set; }

    public string? Comment { get; set; }

    public bool? DrawerStatus { get; set; }

    public decimal? ShiftEndBank { get; set; }

    public decimal? ShiftEndAcc { get; set; }
    [ForeignKey("UserId")] public AppUser? User { get; set; }
    public List<tbl_ShiftClosureSummary>? ShiftclosureSummary { get; set; }

}
