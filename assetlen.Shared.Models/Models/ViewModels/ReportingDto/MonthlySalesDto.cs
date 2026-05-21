using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{
    public class MonthlySalesDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalPriceInc { get; set; }
        public decimal Percentage { get; set; }
        public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
        public string DisplayName => $"{MonthName} {Year}";
    }
}
