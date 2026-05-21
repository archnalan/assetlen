using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.DocumentDtos
{

    /// <summary>
    /// DTO for book reviews and ratings
    /// </summary>
    public class BookReviewDto : BaseDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(100)]
        public string? ReviewTitle { get; set; }

        [StringLength(2000)]
        public string? ReviewText { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public bool IsVerifiedPurchase { get; set; }

        public int HelpfulCount { get; set; }

        public int NotHelpfulCount { get; set; }

        // Navigation properties
        public ProductsDto? Product { get; set; }
        public UserClaimsDto? User { get; set; }
    }
}
