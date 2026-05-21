
using System.ComponentModel.DataAnnotations;

namespace mowt.Shared.Models.Models
{
    public class ResetPasswordDto
    {
        [Required]
        [MinLength(4)]
        public string Password { get; set; }

        /// <summary>
        /// User identifier - can be email or phone number
        /// </summary>
        [Required]
        public string Identifier { get; set; }

        /// <summary>
        /// Legacy field for backward compatibility
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Used when user knows their old password (profile page scenario)
        /// </summary>
        public string? OldPassword { get; set; }

        /// <summary>
        /// Used for email-based password reset
        /// </summary>
        public string? ResetToken { get; set; }

        /// <summary>
        /// Used for phone-based password reset (OTP verification)
        /// </summary>
        public string? VerificationCode { get; set; }
    }
}
