using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A progress claim against a stage — "claim stage, Peter approves"
/// (assetlen.md §6). The middle of funded → claimed → cleared → carried forward.
/// Either side may record one: under Law 0 the developer enters the invoice he
/// was forwarded, and the ledger must still reconcile.
/// </summary>
public class tbl_StageClaim : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? ClaimedAt { get; set; }

    [MaxLength(450)]
    public string? ClaimedById { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    [MaxLength(40)]
    public string? EvidenceArtifactId { get; set; }

    public ClaimStatus Status { get; set; } = ClaimStatus.Claimed;

    /// <summary>What was cleared, when it differs from the claim. Null means the whole claim.</summary>
    public decimal? ClearedAmount { get; set; }

    public DateTime? ClearedAt { get; set; }

    [MaxLength(450)]
    public string? ClearedById { get; set; }

    [MaxLength(500)]
    public string? QueryNote { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("ClaimedById")]
    public AppUser? ClaimedBy { get; set; }

    [ForeignKey("ClearedById")]
    public AppUser? ClearedBy { get; set; }
}
