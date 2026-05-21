using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.Users
{
	public class OrderStatusDto : BaseDto
	{
		//public string Id { get; set; }

		public string? OrderName { get; set; }

		public int? SortOrder { get; set; }
	}
}
