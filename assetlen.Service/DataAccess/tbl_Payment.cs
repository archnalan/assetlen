using assetlen.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public partial class tbl_Payment : BaseEntity
{
    //public int Id { get; set; }

    public string? PaymentModeId { get; set; }

    public string? SaleId { get; set; }

    public decimal? Amount { get; set; }

    public string? CustomerId { get; set; }

    public string? CardRef { get; set; }

    public string? ChequeNo { get; set; }

    public string? NameOnCheque { get; set; }

    public string? BankId { get; set; }

    public DateTime? BankingDate { get; set; }

    public string? SupplierId { get; set; }

    public string? EmployeeId { get; set; }

    public string? SupplierPaymentId { get; set; }
    public decimal? Change { get; set; }

    public string? ExpenseId { get; set; }

    [ForeignKey(nameof(Id))]
    public tbl_PaymentMode? PaymentMode { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public tbl_Supplier? Supplier { get; set; }

    [ForeignKey(nameof(SupplierPaymentId))]
    public tbl_SupplierPayment? SupplierPayment { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public AppUser? Employee { get; set; }
}
