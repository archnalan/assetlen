using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ExportDtos
{
    public class SupplierExportDto
    {
        public int? SupplierId { get; set; }

        public string? AccountNumber { get; set; }

        public string? FullName { get; set; }

        public string? Contact { get; set; }

        public string? CardNumber { get; set; }

        public string? VatNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public decimal? CreditLimit { get; set; }

        public bool? Deleted { get; set; }

        public string? Company { get; set; }
    }
}
