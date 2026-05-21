using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class ProductImportDataFinalDto
    {
        public List<ColumnMapping> ColumnMappingsList { get; set; }
        public List<Dictionary<string, object>> UploadedExcelContent { get; set; }
    }
}
