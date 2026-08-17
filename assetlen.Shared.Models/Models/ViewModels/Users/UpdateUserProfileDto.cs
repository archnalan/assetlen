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

    /// <summary>What the caller learns after a code is sent.</summary>
    public class ContactChallengeDto
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The code itself, on a Development host only. A real inbox is not
        /// wired up locally, so without this the change flow cannot be walked
        /// end to end. Always null anywhere else.
        /// </summary>
        public string? DevCode { get; set; }
    }

    /// <summary>
    /// Whether a username can be claimed, and what to offer instead when it
    /// cannot. Suggestions are pre-checked, so anything returned here is free.
    /// </summary>
    public class UserNameAvailabilityDto
    {
        public string UserName { get; set; } = string.Empty;

        public bool Available { get; set; }

        /// <summary>Why not, in words the reader can act on. Null when available.</summary>
        public string? Reason { get; set; }

        public List<string> Suggestions { get; set; } = new();
    }

    public class UpdateUserNameDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(64, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;
    }
}
