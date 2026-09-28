using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A pointer from a commitment to the thing that says, proves, bills or
/// settles it. One row serves both directions — the commitment lists its
/// evidence, and the photo lists what it is evidence for (Law 2).
/// </summary>
public class tbl_CommitmentLink : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? CommitmentId { get; set; }

    public CommitmentLinkTarget TargetType { get; set; }

    [MaxLength(40)]
    public string? TargetId { get; set; }

    public CommitmentLinkRelation Relation { get; set; } = CommitmentLinkRelation.Evidence;

    [MaxLength(300)]
    public string? Note { get; set; }

    [MaxLength(450)]
    public string? CreatedById { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("CommitmentId")]
    public tbl_Commitment? Commitment { get; set; }
}
