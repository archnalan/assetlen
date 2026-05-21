using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
	public class PaymentAccountDto : BaseDto
	{
		//public int Id { get; set; }

		public int PaymentTypeId { get; set; }

		public string? PaymentAccountName { get; set; }

		public decimal? OpeningBalance { get; set; }
	}
}
