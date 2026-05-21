using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class ProductRelationshipDto : BaseDto
    {
        public string? HasAsubProductId { get; set; }

        public string? IsAsubProductId { get; set; }

        public decimal? Qty { get; set; }

        public int? SortOrder { get; set; }
    }
}
