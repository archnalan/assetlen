using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{

    public class CustomerDto : BaseDto
    {
        [DisplayName("Account ID")]
        [StringLength(120)]
        public string? AccountNumber { get; set; }
        [DisplayName("Full Name")]
        [MinLength(3, ErrorMessage = "Fullname is too short")]
        [StringLength(120, ErrorMessage = "Fullname too long (120 character limit)")]
        [Required]
        public string? FullName { get; set; }
        [DisplayName("Contact")]
        [MinLength(6)]
        [AllowNull]
        [StringLength(120)]
        public string? Contact { get; set; }
        [DisplayName("Card number")]
        [StringLength(120)]
        public string? CardNumber { get; set; }
        [DisplayName("VAT Number")]
        [StringLength(120)]
        public string? VatNumber { get; set; }
        [DisplayName("Email")]
        [EmailAddress]
        [StringLength(120)]
        public string? Email { get; set; }
        [DisplayName("Address")]
        [StringLength(120)]
        public string? Address { get; set; }
        [DisplayName("Credit Limit")]
        [StringLength(120)]
        public decimal? CreditLimit { get; set; }
        [DisplayName("Company")]
        public string? Company { get; set; }
        [JsonIgnore]
        public virtual ICollection<TransactionDto>? Transactions { get; set; }
    }
}
