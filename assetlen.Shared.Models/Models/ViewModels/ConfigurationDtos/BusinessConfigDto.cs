using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ConfigurationDtos
{
    public class BusinessConfigDto
    {
        [Required]
        public string BusinessName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string TaxIdNo { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;

        [StringLength(5, MinimumLength = 5, ErrorMessage = "BarcodePrefix must be exactly 5 characters.")]
        public string BarcodePrefix { get; set; } = string.Empty;
        public bool AutoPrintReceipt { get; set; }
        public bool AutoSyncwithOnline { get; set; }
        public bool EnableBillingItemNotes { get; set; }
        public bool EnableOrdersItemNotes { get; set; }
        public bool IndicateDiscountDetails { get; set; }
        public bool AutoStartShift { get; set; }
        public bool LinkSalesPerson { get; set; }
        public bool PreventOutOfStockSale { get; set; }
        public bool LowStockLevelNotification { get; set; }
        public bool IncreaseQtyInsteadOfDuplicate { get; set; }
        public int AutoLogOutMinutes { get; set; }
        public int NoOfDecimalPlaces { get; set; }
        public bool UseCustomerSpecificPricing { get; set; }
    }
}
