using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class RefundsDto : BaseDto
    {
        public string? SaleId { get; set; }

        public decimal? RefundAmount { get; set; }

        public DateTime? RefundDateTime { get; set; }

        public int? RefundedBy { get; set; }

        public string? RefundComment { get; set; }

        public int? ShiftId { get; set; }

        public string? ToCustomerId { get; set; }
    }
}
