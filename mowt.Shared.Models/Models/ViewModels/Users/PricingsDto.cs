using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.Users
{
	public class PricingsDto : BaseDto
	{
		public string? CustomerId { get; set; }
		[Required]
		public string? CustomerName { get; set; }

		public string? ProductId { get; set; }
		[Required]
		public string? ProductName { get; set; }

		public string? PriceGroupId { get; set; }

		public decimal? PriceInc { get; set; }

		public decimal? PriceExc { get; set; }

		public string? TaxId { get; set; }

		public int? SortOrder { get; set; }

		public decimal? CostInc { get; set; }

		public decimal? CostExc { get; set; }

		public bool Selected { get; set; }
	}
}
