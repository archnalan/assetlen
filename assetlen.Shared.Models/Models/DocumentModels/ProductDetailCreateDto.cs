using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.DocumentModels
{
    public class ProductDetailCreateDto
    {
        public string ProductId { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int? SortOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
