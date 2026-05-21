using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess;

public partial class tbl_Expense : BaseEntity
{
    // public int Id { get; set; }

    public string? CustomerId { get; set; }

    public string? SupplierId { get; set; }

    public string? EmployeeId { get; set; }

    public string? ExpenseType { get; set; }

    public decimal? Amount { get; set; }

    public string? Comment { get; set; }

    public string? ShiftId { get; set; }

    public DateTime? DateTimePayed { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public tbl_Supplier? Supplier { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public tbl_Customer? Customer { get; set; }
    [ForeignKey(nameof(ShiftId))]
    public tbl_Shift? Shift { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public AppUser? Employee { get; set; }

    [ForeignKey(nameof(ExpenseType))]
    public tbl_ExpenseType? ExpenseTypeData { get; set; }

}
