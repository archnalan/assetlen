using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class PaymentModes : BaseDto
    {
        public string ModeName { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Balance { get; set; }
    }
}
