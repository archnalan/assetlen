using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ExportDtos
{
    public class ImportDataDto
    {
        public List<ColumnMapping>? ColumnMappingsList { get; set; }
        public List<Dictionary<string, object>>? UploadedExcelContent { get; set; }
    }
}
