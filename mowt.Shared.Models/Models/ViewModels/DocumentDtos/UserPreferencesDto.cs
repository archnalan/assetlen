using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.DocumentDtos
{

    /// <summary>
    /// DTO for user preferences related to reading
    /// </summary>
    public class UserPreferencesDto : BaseDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Range(12, 24)]
        public int FontSize { get; set; } = 18;

        [StringLength(50)]
        public string FontFamily { get; set; } = "Georgia";

        [StringLength(20)]
        public string Theme { get; set; } = "Light";

        public bool ShowTOC { get; set; } = true;

        [Range(600, 1200)]
        public int MaxContentWidth { get; set; } = 800;

        [Range(1.0, 2.5)]
        public double LineHeight { get; set; } = 1.8;

        public bool AutoSaveProgress { get; set; } = true;

        // Navigation property
        public UserClaimsDto? User { get; set; }
    }

}
