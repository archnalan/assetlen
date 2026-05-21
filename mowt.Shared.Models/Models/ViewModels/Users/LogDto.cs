using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.Users
{
	public class LogDto
	{
		public int Id { get; set; }

		public string? Message { get; set; }

		public string? MessageTemplate { get; set; }

		public string? Level { get; set; }

		public DateTime? TimeStamp { get; set; }

		public string? Exception { get; set; }

		public string? Properties { get; set; }

		public int? UserId { get; set; }

		public int? ShiftId { get; set; }

		public int? SaleId { get; set; }

		public int? LogTypeId { get; set; }

		public int? OldQty { get; set; }

		public int? NewQty { get; set; }

		public int? ProductId { get; set; }
	}
}
