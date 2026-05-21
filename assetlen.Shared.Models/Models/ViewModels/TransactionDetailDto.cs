using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
	public class TransactionDetailDto : BaseDto
	{
		public string? ProductId { get; set; }

		[Required]
		[RegularExpression(@"^(?!0(\.0+)?$)\d+(\.\d+)?$", ErrorMessage = "Value must be greater than zero.")]
		public decimal? Qty { get; set; }

		public decimal? CostExc { get; set; }

		public decimal? CostInc { get; set; }

		public decimal? PriceInc { get; set; }

		public decimal? PriceExc { get; set; }

		public string? TaxId { get; set; }

		public decimal? TaxPercent { get; set; }

		public string? DiscountId { get; set; }

		public decimal? DiscountPercent { get; set; }

		public string? TransactionId { get; set; }

		public decimal? TotalPriceInc { get; set; }

		public decimal? TotalPriceExc { get; set; }

		public int? SortOrder { get; set; }

		public bool? CostIncState { get; set; }

		public bool? SpecialPricingUsed { get; set; }

		public string? ItemNote { get; set; }

		public DiscountDto? Discount { get; set; }

		public taxDto? Tax { get; set; }

		public ProductsDto? Product { get; set; }
	}
}
