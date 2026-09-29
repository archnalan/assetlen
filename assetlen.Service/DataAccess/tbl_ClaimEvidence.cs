using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One piece of proof a claim carries. A claim with its own evidence is paid
/// without a phone call (assetlen.md §7): the funder sees the frames, the
/// deliverables signed off and the reading on the claim itself.
/// </summary>
public class tbl_ClaimEvidence : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? ClaimId { get; set; }

    public ClaimEvidenceKind Kind { get; set; }

    [MaxLength(40)]
    public string? ProgressImageId { get; set; }

    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    [MaxLength(40)]
    public string? DeliverableId { get; set; }

    [MaxLength(40)]
    public string? ProgressReadingId { get; set; }

    public int DisplayOrder { get; set; }

    [ForeignKey("ClaimId")]
    public tbl_StageClaim? Claim { get; set; }

    [ForeignKey("ProgressImageId")]
    public tbl_ProgressImage? ProgressImage { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }

    [ForeignKey("DeliverableId")]
    public tbl_Deliverable? Deliverable { get; set; }

    [ForeignKey("ProgressReadingId")]
    public tbl_ProgressReading? ProgressReading { get; set; }
}
