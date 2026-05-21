using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ExportDtos
{
    public class CategoriesExportDto
    {
        public int? CategoryId { get; set; }

        public string? Category { get; set; }

        public string? Description { get; set; }

        public bool HideInPos { get; set; }
    }
}
