using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class ProductPricing
    {
        public taxDto? Tax { get; set; }
        public string? TaxId { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "MarkUp must be a positive value.")]
        public decimal? MarkUp { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Cost Exclusive must be a positive value.")]
        public decimal? CostExclusive { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "The Cost Price must be provided.")]
        public decimal? CostInclusive { get; set; }

        public bool CostSwitch { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Price Exclusive must be a positive value.")]
        public decimal? PriceExclusive { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Price Exclusive 2 must be a positive value.")]
        public decimal? PriceExclusive2 { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "The selling Price must be provided")]
        public decimal? PriceInclusive { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Price Inclusive 2 must be a positive value.")]
        public decimal? PriceInclusive2 { get; set; }
    }
}
