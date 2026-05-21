using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class ProductWithQtyDto : BaseDto
    {
        public string? ProductId { get; set; }
        [Required]
        public string? ProductName { get; set; }
        public decimal? CostInclusive { get; set; }
        public decimal? CostExclusive { get; set; }
        public decimal? PriceInclusive { get; set; }
        public decimal? Qty { get; set; }
        public bool CostPricesUpdated { get; set; } = false;
    }
}
