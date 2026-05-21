using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.DocumentModels
{
    public class ProductDetailUpdateDto
    {
        public string Id { get; set; } = string.Empty;
        public int? Level { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
