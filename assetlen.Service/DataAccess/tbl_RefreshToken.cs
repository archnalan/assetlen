using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Shared.Models.Models
{
    public class tbl_RefreshToken:BaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Token { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByToken { get; set; }

        public string? IpAddress { get; set; }

        public string? DeviceType { get; set; }

        public string? BrowserType { get; set; }

        public string TenantId { get; set; }

        // Login History Properties (merged from UserLoginHistory)
        public string DeviceFingerprint { get; set; }

        public DateTime FirstLoginAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public int LoginCount { get; set; } = 1;

        // Token Properties
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsRevoked => RevokedAt != null;

        public bool IsActive => !IsRevoked && !IsExpired;

        public bool IsInGracePeriod => RevokedAt != null && DateTime.UtcNow <= RevokedAt.Value.AddMinutes(1);
    }
}