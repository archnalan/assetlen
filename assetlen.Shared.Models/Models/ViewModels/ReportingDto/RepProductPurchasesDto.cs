using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepProductPurchasesDto
    {
        public DateTime DateReceived { get; set; }
        public string? GRNSupplierNumber { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string? BarCode { get; set; }
        public decimal Qty { get; set; }
        public decimal CostExclusive { get; set; }
        public decimal CostInclusive { get; set; }
        public bool PriceChanged { get; set; }
        public decimal NewCostInc { get; set; }
        public decimal NewPriceInc { get; set; }
        public string? Supplier { get; set; }
        public string? SupplierID { get; set; }
        public string? UserName { get; set; }
        public string? SegmentId { get; set; }
        public string? CategoryId { get; set; }
        public string? Category { get; set; }
        public string? Segment { get; set; }
    }

}
