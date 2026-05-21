using assetlen.Shared.Models.statics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels
{
    // ─────────────────────────────────────────────────────
    // Main read DTO
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Full enterprise subscription request DTO returned by the API
    /// </summary>
    public class SubscriptionRequestDto : BaseDto
    {
        public string OrganisationName { get; set; } = string.Empty;
        public EnterpriseEntityType EntityType { get; set; } = EnterpriseEntityType.Company;
        public string ContactPersonName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public int RequestedSeats { get; set; } = 1;
        public string? AdditionalNotes { get; set; }

        /// <summary>Submitting user (null for anonymous walk-in applications)</summary>
        public string? SubmittedByUserId { get; set; }
        public string? SubmittedByUserName { get; set; }
        public string? SubmittedByEmail { get; set; }

        public SubscriptionRequestStatus Status { get; set; } = SubscriptionRequestStatus.Pending;

        // Quote details (filled by admin)
        public decimal? QuotedAmount { get; set; }
        public string? QuoteCurrency { get; set; } = "UGX";
        public string? QuoteNotes { get; set; }
        public DateTime? QuotedDate { get; set; }
        public string? QuotedByUserId { get; set; }
        public string? QuotedByUserName { get; set; }

        // Payment confirmation (filled by admin)
        public DateTime? PaymentConfirmedDate { get; set; }
        public string? PaymentReference { get; set; }
        public string? PaymentConfirmedByUserId { get; set; }

        // Subscription period (set upon activation)
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        public string? AdminNotes { get; set; }

        public List<SubscriptionSeatDto> Seats { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────
    // Create DTO  (submitted by the applicant)
    // ─────────────────────────────────────────────────────

    public class SubscriptionRequestCreateDto
    {
        [Required(ErrorMessage = "Organisation name is required")]
        [MaxLength(200)]
        public string OrganisationName { get; set; } = string.Empty;

        [Required]
        public EnterpriseEntityType EntityType { get; set; } = EnterpriseEntityType.Company;

        [Required(ErrorMessage = "Contact person name is required")]
        [MaxLength(150)]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress]
        [MaxLength(255)]
        public string ContactEmail { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [MaxLength(30)]
        public string? ContactPhone { get; set; }

        [Url(ErrorMessage = "Please enter a valid website URL (e.g. https://example.go.ug).")]
        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(400)]
        public string? Address { get; set; }

        [Range(1, 10000, ErrorMessage = "Please enter a valid number of seats.")]
        public int RequestedSeats { get; set; } = 1;

        [MaxLength(2000)]
        public string? AdditionalNotes { get; set; }
    }

    // ─────────────────────────────────────────────────────
    // Quote DTO  (admin issues a quote)
    // ─────────────────────────────────────────────────────

    public class SubscriptionRequestQuoteDto
    {
        [Required]
        public string RequestId { get; set; } = string.Empty;

        [Required]
        [Range(1, 99_999_999_999)]
        public decimal QuotedAmount { get; set; }

        [MaxLength(10)]
        public string QuoteCurrency { get; set; } = "UGX";

        [MaxLength(2000)]
        public string? QuoteNotes { get; set; }
    }

    // ─────────────────────────────────────────────────────
    // Confirm Payment DTO  (admin records payment)
    // ─────────────────────────────────────────────────────

    public class SubscriptionRequestPaymentDto
    {
        [Required]
        public string RequestId { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? PaymentReference { get; set; }

        /// <summary>Start of the subscription granted</summary>
        public DateTime SubscriptionStartDate { get; set; } = DateTime.Today;

        /// <summary>End of the subscription granted</summary>
        public DateTime SubscriptionEndDate { get; set; } = DateTime.Today.AddYears(1);
    }

    // ─────────────────────────────────────────────────────
    // Status update DTO  (admin changes status with a note)
    // ─────────────────────────────────────────────────────

    public class SubscriptionRequestStatusUpdateDto
    {
        [Required]
        public string RequestId { get; set; } = string.Empty;

        [Required]
        public SubscriptionRequestStatus NewStatus { get; set; }

        [MaxLength(2000)]
        public string? AdminNotes { get; set; }
    }

    // ─────────────────────────────────────────────────────
    // Seat DTO  (individual email seats within an enterprise subscription)
    // ─────────────────────────────────────────────────────

    public class SubscriptionSeatDto : BaseDto
    {
        public string RequestId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ActivatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        /// <summary>Linked AppUser Id once the user registers/logs in</summary>
        public string? LinkedUserId { get; set; }
    }

    public class SubscriptionSeatCreateDto
    {
        [Required]
        public string RequestId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DisplayName { get; set; }
    }

    public class SubscriptionSeatBulkCreateDto
    {
        [Required]
        public string RequestId { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<SubscriptionSeatCreateDto> Seats { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────
    // Stats DTO  (for the admin dashboard)
    // ─────────────────────────────────────────────────────

    public class SubscriptionRequestStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int UnderReview { get; set; }
        public int Quoted { get; set; }
        public int Active { get; set; }
        public int Declined { get; set; }
        public int TotalSeats { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ─────────────────────────────────────────────────────
    // Query DTO  (for filtering lists)
    // ─────────────────────────────────────────────────────

    public class SubscriptionRequestQueryDto
    {
        public string? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
        public string? EntityTypeFilter { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 50;
        public string? SortByColumn { get; set; }
        public bool SortAscending { get; set; } = false;
    }
}
