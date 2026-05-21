using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ReportingDto
{
    public class YearlySalesDto
    {
        public int Year { get; set; }
        public decimal TotalPriceInc { get; set; }
        public decimal Percentage { get; set; }
        public string DisplayName => $"{Year}";
    }
}
