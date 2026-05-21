using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepSalesPerCategoryAndSegmentDto
    {
        public string ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string? BarCode { get; set; }
        public decimal? Quantity { get; set; }
        public decimal TotalCostExclusive { get; set; }
        public decimal TotalPriceExc { get; set; }
        public decimal TotalPriceInc { get; set; }
        public string? Category { get; set; }
        public string? Segment { get; set; }
    }
}
