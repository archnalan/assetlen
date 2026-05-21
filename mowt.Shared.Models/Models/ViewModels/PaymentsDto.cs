using mowt.Shared.Models.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
	public class PaymentsDto : BaseDto
	{
		public string? PaymentModeId { get; set; }

		public string? SaleId { get; set; }

		public decimal? Amount { get; set; }

		public string? CustomerId { get; set; }

		public string? CardRef { get; set; }

		public string? ChequeNo { get; set; }

		public string? NameOnCheque { get; set; }

		public string? BankId { get; set; }

		public DateTime? BankingDate { get; set; }

		public string? CustomerDepositId { get; set; }

		public string? SupplierId { get; set; }

		public string? EmployeeId { get; set; }

		public string? SupplierPaymentId { get; set; }

		public string? ExpenseId { get; set; }

		public decimal? Change { get; set; }

		public PaymentModeDto? PaymentMode { get; set; }

		public CustomerDto? Customer { get; set; }

		public TransactionDto? Sale { get; set; }

		public SupplierDto? Supplier { get; set; }

		public SupplierPaymentDto? SupplierPayment { get; set; }

		public CustomerDepositDto? CustomerDeposit { get; set; }

		public AppUserDto? Employee { get; set; }
	}
}
