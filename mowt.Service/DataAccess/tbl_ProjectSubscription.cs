using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess;

public class tbl_ProjectSubscription : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(450)]
    public string? InvestorId { get; set; }

    [MaxLength(200)]
    public string? StripeSubscriptionId { get; set; }

    [MaxLength(200)]
    public string? StripeCustomerId { get; set; }

    public DateTime? CurrentPeriodStart { get; set; }

    public DateTime? CurrentPeriodEnd { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Free;

    public decimal MonthlyAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "UGX";

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("InvestorId")]
    public AppUser? Investor { get; set; }
}
