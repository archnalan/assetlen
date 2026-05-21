using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class BankDto : BaseDto
    {
        [Required(ErrorMessage = "Bank name is required.")]
        [StringLength(200, ErrorMessage = "Bank name cannot exceed 200 characters.")]
        public string BankName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Swift code cannot exceed 200 characters.")]
        public string? SwiftCode { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
    }

    public class BanksExportDto
    {
        public string? Id { get; set; }
        public string? BankName { get; set; }
        public string? SwiftCode { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }
}