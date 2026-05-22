using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
    /// <summary>
    /// DTO for requesting promotion of a general user to employee status.
    /// Requires approval from another admin before taking effect.
    /// </summary>
    public class UserPromotionRequestDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? UserFullName { get; set; }

        public string? RequestedByUserId { get; set; }

        public string? RequestedByUserName { get; set; }

        public string? Comment { get; set; }

        public PromotionStatus Status { get; set; } = PromotionStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public string? ApprovedByUserId { get; set; }

        public string? ApprovedByUserName { get; set; }

        public string? ApprovalComment { get; set; }
    }

    public enum PromotionStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    /// <summary>
    /// DTO for approving or rejecting a user promotion request.
    /// </summary>
    public class UserPromotionApprovalDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public bool IsApproved { get; set; }

        public string? ApprovalComment { get; set; }
    }

    /// <summary>
    /// DTO for admin-initiated password reset (bypasses exponential backoff).
    /// </summary>
    public class AdminPasswordResetDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// "email" or "otp" — determines how the reset link/code is delivered.
        /// </summary>
        public string ResetMethod { get; set; } = "email";
    }

    /// <summary>
    /// DTO for toggling user account status (disable/enable/soft-delete).
    /// </summary>
    public class UserAccountStatusDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The desired action: "disable", "enable", "softdelete", "restore"
        /// </summary>
        [Required]
        public string Action { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }

    /// <summary>
    /// Enriched user response DTO that includes account status info for admin views.
    /// </summary>
    public class UserAdminViewDto : BaseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FullName => $"{FirstName} {LastName}".Trim();
        public string? Address { get; set; }
        public string? Aboutme { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public string? Jobtitle { get; set; }
        public string? CardNumber { get; set; }
        public string[] defaultRole { get; set; } = new[] { "Contractor" };

        // Account status fields
        public bool IsDisabled { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEmployee { get; set; }
        public bool HasPendingPromotion { get; set; }
        public PromotionStatus? PromotionStatus { get; set; }
        public string? SubscriptionPlan { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public string Initials => string.IsNullOrWhiteSpace(FullName)
            ? "?"
            : string.Concat(FullName.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(x => x[0].ToString().ToUpper()));
    }
}
