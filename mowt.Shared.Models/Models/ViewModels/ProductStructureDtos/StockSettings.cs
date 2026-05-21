using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class StockSettings
    {
        public decimal? InStock { get; set; }

        public bool TrackInventory { get; set; } = true;

        public decimal? ReOrderLevel { get; set; }

        public decimal? ReOrderQty { get; set; }

        public bool Favourite { get; set; }

        public bool HasSubProduct { get; set; }

        public string? IsAsubProduct { get; set; }

        public int? CompoundCostPricing { get; set; }

        public bool CostIncStatus { get; set; }

        public string? CurrentProductId { get; set; }

    }
}
