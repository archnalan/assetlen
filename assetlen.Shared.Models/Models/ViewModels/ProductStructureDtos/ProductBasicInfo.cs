using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class ProductBasicInfo
    {
        [Key]
        public string? Id { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Document Name must be at least 3 characters long.")]
        public string? ProductName { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Description must be at least 3 characters long.")]
        public string? Description { get; set; }

        public string? ProductCode { get; set; }

        public string? BarCode { get; set; }

        public string? SupplierId { get; set; }

        public string? CategoryId { get; set; }

        public string? SegmentId { get; set; }

        public string? Location { get; set; }

        public int? AccessLevel { get; set; }

    }
}
