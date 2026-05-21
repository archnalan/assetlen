using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ExportDtos
{
    public class TransactionDetailExportDto
    {
        public string DetailId { get; set; }

        //public string? ProductId { get; set; }
        public decimal? Qty { get; set; }

        public decimal? CostExc { get; set; }

        public decimal? CostInc { get; set; }

        public decimal? PriceInc { get; set; }

        public decimal? PriceExc { get; set; }

        public string? TaxName { get; set; }
        //public string? TaxId { get; set; }
        public string? ProductName { get; set; }
        public string? PaymentType { get; set; }

        //public int? DiscountId { get; set; }

        public decimal? TaxPercent { get; set; }

        public decimal? DiscountValue { get; set; }

        public decimal? DiscountPercent { get; set; }

        public string? TransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? TotalPriceInc { get; set; }

        public decimal? TotalPriceExc { get; set; }

        public int? SortOrder { get; set; }

        public bool? CostIncState { get; set; }

        public bool? SpecialPricingUsed { get; set; }

        public string? SoldBy { get; set; }

        public string? CustomerName { get; set; }

        public string? TransactionComment { get; set; }
        public int? TransactionStatus { get; set; }

    }
}
