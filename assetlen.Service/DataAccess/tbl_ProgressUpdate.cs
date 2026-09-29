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

    /// <summary>The deliverable the capture was aimed at. Nothing floats (CLAUDE.md §1).</summary>
    [MaxLength(40)]
    public string? DeliverableId { get; set; }

    /// <summary>The offline queue's key, so a retry after a lost reply never posts twice.</summary>
    [MaxLength(64)]
    public string? ClientCaptureId { get; set; }

    /// <summary>When it was shot. A capture queued offline at 22:00 belongs to that day, not the day the signal came back.</summary>
    public DateTime? CapturedAt { get; set; }

    [MaxLength(40)]
    public string? VoiceArtifactId { get; set; }

    // Navigation
    [ForeignKey("DeliverableId")]
    public tbl_Deliverable? Deliverable { get; set; }

    [ForeignKey("VoiceArtifactId")]
    public tbl_Artifact? VoiceArtifact { get; set; }

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
