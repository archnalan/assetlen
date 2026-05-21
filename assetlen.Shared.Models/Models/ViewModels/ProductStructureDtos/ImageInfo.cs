using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class ImageInfo
    {
        public string? ImageName { get; set; }
        public Guid? ImageGuid { get; set; }
        public string? ImageUniqueName { get; set; }
        public string? ImageFullPath { get; set; }
        public string? ContentType { get; set; }
    }
}
