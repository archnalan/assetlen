using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class ExpenseDto : BaseDto
    {
        public string? CustomerId { get; set; }

        public string? SupplierId { get; set; }

        public string? EmployeeId { get; set; }

        public string? ExpenseType { get; set; }

        public decimal? Amount { get; set; }

        public string? Comment { get; set; }

        public string? ShiftId { get; set; }

        public DateTime? DateTimePayed { get; set; }
        public List<PaymentsDto>? ExpensePayments { get; set; }
    }
}
