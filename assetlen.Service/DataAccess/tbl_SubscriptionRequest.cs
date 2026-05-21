using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.statics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess
{
    /// <summary>
    /// Represents an enterprise subscription application submitted by an organisation
    /// </summary>
    public class tbl_SubscriptionRequest : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string OrganisationName { get; set; } = string.Empty;

        public EnterpriseEntityType EntityType { get; set; } = EnterpriseEntityType.Company;

        [Required]
        [MaxLength(150)]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? ContactPhone { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(400)]
        public string? Address { get; set; }

        public int RequestedSeats { get; set; } = 1;

        public string? AdditionalNotes { get; set; }

        // Submitted-by user (nullable — walk-in or anonymous)
        [MaxLength(40)]
        public string? SubmittedByUserId { get; set; }

        [MaxLength(200)]
        public string? SubmittedByUserName { get; set; }

        [MaxLength(255)]
        public string? SubmittedByEmail { get; set; }

        public SubscriptionRequestStatus Status { get; set; } = SubscriptionRequestStatus.Pending;

        // Quote fields
        public decimal? QuotedAmount { get; set; }

        [MaxLength(10)]
        public string? QuoteCurrency { get; set; } = "UGX";

        public string? QuoteNotes { get; set; }

        public DateTime? QuotedDate { get; set; }

        [MaxLength(40)]
        public string? QuotedByUserId { get; set; }

        [MaxLength(200)]
        public string? QuotedByUserName { get; set; }

        // Payment confirmation
        public DateTime? PaymentConfirmedDate { get; set; }

        [MaxLength(200)]
        public string? PaymentReference { get; set; }

        [MaxLength(40)]
        public string? PaymentConfirmedByUserId { get; set; }

        // Subscription period
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        public string? AdminNotes { get; set; }

        // Navigation
        public virtual ICollection<tbl_SubscriptionSeat> Seats { get; set; } = new List<tbl_SubscriptionSeat>();
    }

    /// <summary>
    /// A single email seat provisioned under an enterprise subscription request
    /// </summary>
    public class tbl_SubscriptionSeat : BaseEntity
    {
        [Required]
        [MaxLength(40)]
        public string RequestId { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? ActivatedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        /// <summary>Linked AppUser.Id once the email address registers in the system</summary>
        [MaxLength(40)]
        public string? LinkedUserId { get; set; }

        [ForeignKey(nameof(RequestId))]
        public virtual tbl_SubscriptionRequest? Request { get; set; }
    }
}
