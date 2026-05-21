using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class SalesPerDayDto
    {
        public DateTime Date { get; set; }
        public string UserName { get; set; }
        public int NoOfTransactions { get; set; }
        public decimal TotalPriceInc { get; set; }
        public decimal Percentage { get; set; }
        public decimal? Profit { get; set; }
    }
}
