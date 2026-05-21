using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.DocumentDtos
{

    /// <summary>
    /// DTO for tracking user reading progress
    /// </summary>
    public class ReadingProgressDto : BaseDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        public int CurrentSection { get; set; } = 1;

        [Range(0, 100)]
        public double ProgressPercentage { get; set; }

        public DateTime LastReadDate { get; set; } = DateTime.UtcNow;

        public int TotalTimeSpentMinutes { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedDate { get; set; }

        // Navigation properties
        public UserClaimsDto? User { get; set; }
        public ProductsDto? Product { get; set; }
    }
}
