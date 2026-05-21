using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class StockNotifyDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal ReOrderLevel { get; set; }
        public bool IsLowStock => CurrentStock <= ReOrderLevel;
    }
}
