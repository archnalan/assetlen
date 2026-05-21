using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepExpensesDto
    {
        public DateTime TransDate { get; set; }
        public string UserName { get; set; }
        public string? ShiftID { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string? PaymentMode { get; set; }
        public string? Category { get; set; }
    }
}
