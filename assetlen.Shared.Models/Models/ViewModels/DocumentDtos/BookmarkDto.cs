using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.DocumentDtos
{

    /// <summary>
    /// DTO for user bookmarks
    /// </summary>
    public class BookmarkDto : BaseDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        public DateTime BookmarkedDate { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        // Navigation properties
        public UserClaimsDto? User { get; set; }
        public ProductsDto? Product { get; set; }
    }

}
