using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class ProductMedia
    {
        public string? ProductName { get; set; }
        public string? Base64Image { get; set; }
        public string? ProductImageName { get; set; }
        public string? ProductImageUrl { get; set; }
        public IFormFile? ProductFile { get; set; }
    }
}
