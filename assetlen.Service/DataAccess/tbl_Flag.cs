using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A site issue raised against a Project. Flags are first-class so they
/// can be tracked to resolution independently of any single Site Journal
/// entry. Open Flags receive weekly nudges until Resolved or Archived.
///
/// Attachment is flexible — a Flag is always anchored to a Project, and
/// optionally to a Stage, a specific ProgressUpdate (Site Journal entry),
/// or a specific ProgressImage that surfaced the problem.
/// </summary>
public class tbl_Flag : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(40)]
    public string? ProgressUpdateId { get; set; }

    [MaxLength(40)]
    public string? ProgressImageId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public FlagStatus Status { get; set; } = FlagStatus.Open;

    public FlagSeverity Severity { get; set; } = FlagSeverity.Medium;

    public Channel Channel { get; set; } = Channel.Crew;

    [MaxLength(450)]
    public string? CreatedById { get; set; }

    [MaxLength(450)]
    public string? AssignedToId { get; set; }

    [MaxLength(450)]
    public string? ResolvedById { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? ResolvedDate { get; set; }

    /// <summary>
    /// Timestamp of the last weekly nudge sent. Drives the reminder loop.
    /// </summary>
    public DateTime? LastNudgeAt { get; set; }

    /// <summary>
    /// Set true to silence weekly nudges when a project stalls indefinitely.
    /// Different from Status==Archived (which also closes the Flag).
    /// </summary>
    public bool IsNudgeArchived { get; set; }

    // ─── Folded into the commitment model (P4) ───────────────────
    // A flag is one of two things: a query on a commitment — which is that
    // commitment in QueryRaised — or a blocker. Nothing else.

    /// <summary>The commitment this questions. Null for a blocker.</summary>
    [MaxLength(40)]
    public string? CommitmentId { get; set; }

    /// <summary>
    /// Who has to move for a blocker to clear, when they are on the roster —
    /// including an off-platform party such as the aluminium fabricator.
    /// </summary>
    [MaxLength(40)]
    public string? OwnerMemberId { get; set; }

    /// <summary>The owning party when they are not on the roster: "the epoxy team", "the power utility".</summary>
    [MaxLength(200)]
    public string? OwnerPartyName { get; set; }

    // Navigation
    [ForeignKey("CommitmentId")]
    public tbl_Commitment? Commitment { get; set; }

    [ForeignKey("OwnerMemberId")]
    public tbl_ProjectMember? OwnerMember { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("ProgressUpdateId")]
    public tbl_ProgressUpdate? ProgressUpdate { get; set; }

    [ForeignKey("ProgressImageId")]
    public tbl_ProgressImage? ProgressImage { get; set; }

    [ForeignKey("CreatedById")]
    public AppUser? CreatedBy { get; set; }

    [ForeignKey("AssignedToId")]
    public AppUser? AssignedTo { get; set; }

    [ForeignKey("ResolvedById")]
    public AppUser? ResolvedBy { get; set; }
}
