using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
	public class ExpensePerShiftDto
	{
		//public int Id { get; set; }
		public string PaymentModeID { get; set; }
		public string shiftID { get; set; }
		public decimal Amount { get; set; }
	}
}
