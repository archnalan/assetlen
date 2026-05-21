using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
    public class UpdateUserProfileDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Aboutme { get; set; }

        public string? Industry { get; set; }

        public string? ProfilePicUrl { get; set; }

        public string? CoverPhotoUrl { get; set; }

        public string? OriginalEmail { get; set; }

        public string? OriginalPhoneNumber { get; set; }
    }

    public class VerifyContactChangeDto
    {
        [Required]
        public string UserId { get; set; }

        public string? NewEmail { get; set; }

        public string? NewPhoneNumber { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string VerificationCode { get; set; }
    }
}
