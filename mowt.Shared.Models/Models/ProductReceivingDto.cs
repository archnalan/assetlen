using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Shared.Models.Models
{
	public class ProductReceivingDto : BaseDto
	{
		public string? ProductId { get; set; }

		public decimal? Qty { get; set; }

		public DateTime? DateReceived { get; set; }

		public string? GrnsupplierNumber { get; set; }

		public string? ReceivedBy { get; set; }

		public bool? PriceChanged { get; set; }

		public decimal? NewCostInc { get; set; }

		public decimal? NewPriceInc { get; set; }

		public DateTime? PriceChangeScheduled { get; set; }

		public int? OrderId { get; set; }

		public bool? CreditSupplierAcc { get; set; }

		public string? SupplierAccount { get; set; }

		public decimal? CostExc { get; set; }

		public decimal? CostInc { get; set; }
		public ProductsDto? Product { get; set; }
	}
}
