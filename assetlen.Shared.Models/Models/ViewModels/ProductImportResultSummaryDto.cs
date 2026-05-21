using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class ProductImportResultSummaryDto
    {
        public string Errors { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}
