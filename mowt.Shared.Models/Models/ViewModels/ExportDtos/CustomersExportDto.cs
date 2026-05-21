using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ExportDtos
{
    public class CustomersExportDto
    {
        public int? CustomerId { get; set; }
        public string? AccountNumber { get; set; }
        public string? FullName { get; set; }
        public string? Contact { get; set; }
        public string? CardNumber { get; set; }
        public string? VatNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public decimal? CreditLimit { get; set; }
        public string? Company { get; set; }
    }
}
