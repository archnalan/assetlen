using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess;

public class tbl_Project : BaseEntity
{
    [MaxLength(200)]
    public string? ProjectName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public decimal? TotalBudget { get; set; }

    public DateTime? ExpectedStartDate { get; set; }

    public DateTime? ExpectedCompletionDate { get; set; }

    public DateTime? RevisedCompletionDate { get; set; }

    [MaxLength(450)]
    public string? InvestorId { get; set; }

    [MaxLength(450)]
    public string? ProjectManagerId { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    public bool IsFirstFreeProject { get; set; }

    public bool IsSubscriptionActive { get; set; } = true;

    [MaxLength(3)]
    public string Currency { get; set; } = "UGX";

    // Navigation
    [ForeignKey("InvestorId")]
    public AppUser? Investor { get; set; }

    [ForeignKey("ProjectManagerId")]
    public AppUser? ProjectManager { get; set; }

    [InverseProperty("Project")]
    public ICollection<tbl_Stage> Stages { get; set; } = new List<tbl_Stage>();

    [InverseProperty("Project")]
    public ICollection<tbl_FundingEntry> FundingEntries { get; set; } = new List<tbl_FundingEntry>();

    [InverseProperty("Project")]
    public ICollection<tbl_ProgressUpdate> ProgressUpdates { get; set; } = new List<tbl_ProgressUpdate>();

    [InverseProperty("Project")]
    public ICollection<tbl_ProjectSubscription> Subscriptions { get; set; } = new List<tbl_ProjectSubscription>();
}
