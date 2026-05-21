using mowt.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models
{

    public class CategorySection
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public List<ProductsDto> Books { get; set; } = new();
        public string ColorTheme { get; set; } = string.Empty;
    }
}
