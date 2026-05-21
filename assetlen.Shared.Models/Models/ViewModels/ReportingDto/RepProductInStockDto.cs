using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepProductInStockDto
    {
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public decimal? InStock { get; set; }
        public decimal? CostExc { get; set; }
        public decimal? PriceExc { get; set; }
        public decimal? PriceInc { get; set; }
        public string? CategoryId { get; set; }
        public string? Category { get; set; }
        public string? SegmentID { get; set; }
        public string? Segment { get; set; }
        public string? SupplierID { get; set; }
        public string? Supplier { get; set; }
    }
}
