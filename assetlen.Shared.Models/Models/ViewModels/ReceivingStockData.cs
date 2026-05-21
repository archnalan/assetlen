using assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class ReceivingStockData
    {
        public List<ProductReceivingDto> productsReceiving { get; set; }
        public List<StockParam> stockParams { get; set; }
        public List<CostPriceChange>? costChanges { get; set; }
    }
    public class StockParam
    {
        public string ProductId { get; set; }
        public decimal? InStockAmount { get; set; }
    }
    public class CostPriceChange
    {
        public string productId { get; set; }
        public decimal? costExc { get; set; }
        public decimal? costInc { get; set; }
    }

}
