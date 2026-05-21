using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ConfigurationDtos
{
    public class TaxAndDiscountDto
    {
        public string DefaultTaxId { get; set; }
        public string? SelectedTaxValue { get; set; }
        public decimal MarkupOption1 { get; set; }
        public decimal MarkupOption2 { get; set; }
        public bool PriceExclInReceipts { get; set; }
        public bool TaxInCalcProfit { get; set; }
        public bool UsePriceExcInstead { get; set; }
    }
}
