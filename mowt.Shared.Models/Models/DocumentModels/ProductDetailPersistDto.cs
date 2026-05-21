using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.DocumentModels
{
    public class ProductDetailPersistDto
    {
        public string? TempId { get; set; }
        public string? ExistingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int? DesiredOrder { get; set; }
        public string? ParentId { get; set; }
    }
}
