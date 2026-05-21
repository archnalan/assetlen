using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepSalesPerDayDto
    {
        public DateTime Date { get; set; }
        public int NoOfTransactions { get; set; }
        public decimal? TotalCostExc { get; set; }
        public decimal? TotalPriceExc { get; set; }
        public decimal? TotalPriceInc { get; set; }
        public decimal? Profit { get; set; }
        public decimal? Loss { get; set; }
        public decimal? Tax { get; set; }
        public int TotalCount { get; set; }
    }
}
