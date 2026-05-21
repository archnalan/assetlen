using System.ComponentModel.DataAnnotations;

namespace mowt.Shared.Models.Models.ViewModels.Users
{
    public class SendVerificationCodeDto
    {
        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string UserId { get; set; }
    }

    public class VerifyCodeDto
    {
        [Required]
        public string Identifier { get; set; } // Can be UserId, Email, Phone, or Username

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; }
    }
}
