using System;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models
{
    public class VerificationCode
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; }

        public string Code { get; set; }

        public VerificationType Type { get; set; }

        public string Contact { get; set; } // Email or Phone number

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public int AttemptCount { get; set; } = 0;

        public int ResendCount { get; set; } = 0;

        public DateTime? LastResentAt { get; set; }

        /// <summary>
        /// Used to store the password reset token when verification code is used for password reset
        /// </summary>
        public string? ResetToken { get; set; }
    }

    public enum VerificationType
    {
        Email,
        Phone
    }
}
