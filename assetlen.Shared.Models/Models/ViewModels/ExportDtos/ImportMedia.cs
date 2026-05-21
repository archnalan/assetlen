using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace assetlen.Shared.Models.Models.ViewModels.ExportDtos
{
    public class ImportMedia
    {
        public string? ImportName { get; set; }
        public string? ImportImage { get; set; }
        public IFormFile? ImportFile { get; set; }
    }
}
