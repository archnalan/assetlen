using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.DocumentDtos
{

    /// <summary>
    /// DTO for subscription transaction history
    /// </summary>
    public class SubscriptionTransactionDto : BaseDto
    {
        [Required]
        public string SubscriptionId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 9999.99)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = string.Empty; // Payment, Refund, etc.

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // Success, Failed, Pending

        public string? PaymentMethod { get; set; }

        public string? PaymentTransactionId { get; set; }

        public string? PaymentGateway { get; set; }

        public string? FailureReason { get; set; }

        // Navigation properties
        public SubscriptionDto? Subscription { get; set; }
        public UserClaimsDto? User { get; set; }
    }

}
