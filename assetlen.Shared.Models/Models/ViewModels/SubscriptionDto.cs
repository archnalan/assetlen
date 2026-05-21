using System;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels
{
    /// <summary>
    /// DTO for user subscriptions to the e-book service
    /// </summary>
    public class SubscriptionDto : BaseDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string PlanId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 9999.99)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string BillingPeriod { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public DateTime? RenewalDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool AutoRenew { get; set; } = true;

        public string? PaymentMethod { get; set; }

        public string? PaymentTransactionId { get; set; }

        public DateTime? CancelledDate { get; set; }

        public string? CancellationReason { get; set; }

        // Navigation properties
        public UserClaimsDto? User { get; set; }
    }

}
