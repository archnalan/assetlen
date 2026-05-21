using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
	public class CashItemsDto : BaseDto
	{
		//public int Id { get; set; }
		public decimal? Amount { get; set; }
		public decimal? Count { get; set; }
		public decimal? TotalCounted { get; set; }
	}
}
