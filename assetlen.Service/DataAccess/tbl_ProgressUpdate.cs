using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_ProgressUpdate : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public decimal? CompletionPercentage { get; set; }

    public bool HasIssues { get; set; }

    [MaxLength(450)]
    public string? CreatedById { get; set; }

    public ApprovalStatus? ApprovalStatus { get; set; }

    /// <summary>
    /// Visibility channel. Default Crew — entries are internal until the
    /// contractor publishes them to the Client channel. See [[Channel]].
    /// </summary>
    public Channel Channel { get; set; } = Channel.Crew;

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("CreatedById")]
    public AppUser? CreatedBy { get; set; }

    [InverseProperty("ProgressUpdate")]
    public ICollection<tbl_ProgressImage> Images { get; set; } = new List<tbl_ProgressImage>();

    [InverseProperty("ProgressUpdate")]
    public ICollection<tbl_ProgressComment> Comments { get; set; } = new List<tbl_ProgressComment>();

    [InverseProperty("ProgressUpdate")]
    public ICollection<tbl_Flag> Flags { get; set; } = new List<tbl_Flag>();
}
