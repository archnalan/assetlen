using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepSalesPerCustomerDto
    {
        public DateTime TransactionDate { get; set; }
        public string? TransactionId { get; set; }
        public string? ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal TotalPriceExc { get; set; }
        public decimal TotalPriceInc { get; set; }
    }
}
