using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class SalesPerDayReportResponse
    {
        public List<RepSalesPerDayDto>? SalesData { get; set; }
        public int TotalCount { get; set; }
    }
}
