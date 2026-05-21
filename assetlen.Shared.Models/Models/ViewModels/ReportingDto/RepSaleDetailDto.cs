using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepSaleDetailDto
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionId { get; set; }
        public string? ProductCode { get; set; }
        public string? BarCode { get; set; }
        public string? ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? PriceInc { get; set; }
        public decimal? TotalPriceInc { get; set; }
        public decimal? Tax { get; set; }
        public string? Supplier { get; set; }
    }
}
