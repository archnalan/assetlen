using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepProductSalesDto
    {
        public string ProductName { get; set; }
        public string? ProductCode { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? TotalPriceExc { get; set; }
        public decimal? TotalPriceInc { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Profit { get; set; }
    }
}
