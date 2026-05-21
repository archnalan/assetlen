using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_Stage : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(200)]
    public string? StageName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal? BudgetAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpectedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public decimal? CompletionPercentage { get; set; }

    public int DisplayOrder { get; set; }

    public StageStatus Status { get; set; } = StageStatus.NotStarted;

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [InverseProperty("Stage")]
    public ICollection<tbl_FundingEntry> FundingEntries { get; set; } = new List<tbl_FundingEntry>();

    [InverseProperty("Stage")]
    public ICollection<tbl_ProgressUpdate> ProgressUpdates { get; set; } = new List<tbl_ProgressUpdate>();
}
