using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class ShiftPerformanceDto
    {
        public string Cashier { get; set; }
        public decimal Amount { get; set; }
        public DateTime ShiftStartDate { get; set; }
    }

}
