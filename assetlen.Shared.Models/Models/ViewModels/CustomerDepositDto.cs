using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
	public class CustomerDepositDto : BaseDto
	{
		public string? DrawerId { get; set; }
		[Required]
		public string? CustomerId { get; set; }
		[Required]
		public decimal? Amount { get; set; }

		public string? Comment { get; set; }

		public DateTime? DateTimeDeposited { get; set; }

		public decimal? Change { get; set; }

		public string? UserId { get; set; }

		public List<PaymentsDto>? DepositPayments { get; set; }
	}
}
