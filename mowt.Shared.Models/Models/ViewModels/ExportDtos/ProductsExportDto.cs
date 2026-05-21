using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ExportDtos
{
    public class ProductsExportDto
    {
        public int ProductId { get; set; }

        public string? ProductCode { get; set; }

        public string? BarCode { get; set; }

        public string? ProductName { get; set; }

        public decimal? CostExclusive { get; set; }

        public decimal? CostInclusive { get; set; }

        public decimal? InStock { get; set; }

        public decimal? PriceExclusive { get; set; }

        public decimal? PriceExclusive2 { get; set; }

        public decimal? PriceInclusive { get; set; }

        public decimal? PriceInclusive2 { get; set; }

        //public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public string? Location { get; set; }

        //public string? SegmentId { get; set; }
        public string? SegmentName { get; set; }

        //public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }

        //public string? ProductImage { get; set; }


        public int CreatedBy { get; set; }

        //public bool? Deleted { get; set; }

        public bool? TrackInventory { get; set; }

        public decimal? ReOrderLevel { get; set; }

        public decimal? ReOrderQty { get; set; }

        public bool? Favourite { get; set; }

        public bool? HasSubProduct { get; set; }

        public int? IsAsubProduct { get; set; }

        public int? CompoundCostPricing { get; set; }

        //public string? TaxId { get; set; }
        public string? TaxName { get; set; }


    }

}
