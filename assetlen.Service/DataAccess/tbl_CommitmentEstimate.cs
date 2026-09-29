using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A figure put on a parked idea — a quote, a guess, a price seen in passing.
/// Estimates accumulate silently against the idea so that at stage kickoff it
/// comes back with its numbers, not as a blank (assetlen.md §3). Never a
/// commitment: nothing here is agreed.
/// </summary>
public class tbl_CommitmentEstimate : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? CommitmentId { get; set; }

    public decimal? Amount { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    [MaxLength(40)]
    public string? IngestedMessageId { get; set; }

    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    [MaxLength(450)]
    public string? RecordedById { get; set; }

    [ForeignKey("CommitmentId")]
    public tbl_Commitment? Commitment { get; set; }

    [ForeignKey("RecordedById")]
    public AppUser? RecordedBy { get; set; }
}
