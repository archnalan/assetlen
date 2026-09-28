using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// Something extraction read in the thread and proposes to put on the register
/// (plan.md P5). It becomes a commitment — or, for a blocker, a flag — only when
/// a person accepts it; until then it is a suggestion with its source attached.
/// <para>
/// Rejected rows are kept. They are the denominator of the accept rate, and the
/// <see cref="Fingerprint"/> they hold is what stops a re-run proposing the same
/// thing to someone who already said no.
/// </para>
/// </summary>
public class tbl_ExtractionProposal : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? RunId { get; set; }

    [MaxLength(40)]
    public string? IngestedMessageId { get; set; }

    /// <summary>Hash of message, rule and wording. Unique per project, so re-running extraction is idempotent.</summary>
    [MaxLength(64)]
    public string? Fingerprint { get; set; }

    public ProposalKind Kind { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Detail { get; set; }

    public decimal? Amount { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public DateTime? DueDate { get; set; }

    [MaxLength(100)]
    public string? DateText { get; set; }

    [MaxLength(100)]
    public string? Quantity { get; set; }

    public CommitmentMaturity Maturity { get; set; } = CommitmentMaturity.Agreed;

    public ProjectSide? OwedBySide { get; set; }

    [MaxLength(200)]
    public string? PartyName { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(60)]
    public string? Rule { get; set; }

    [MaxLength(40)]
    public string? Engine { get; set; }

    public double Confidence { get; set; }

    public bool Contested { get; set; }

    [MaxLength(500)]
    public string? ContestNote { get; set; }

    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

    [MaxLength(450)]
    public string? DecidedById { get; set; }

    public DateTime? DecidedAt { get; set; }

    [MaxLength(40)]
    public string? CommitmentId { get; set; }

    [MaxLength(40)]
    public string? FlagId { get; set; }

    /// <summary>
    /// Copied from the source batch and stored, like <c>tbl_IngestBatch.ImportedSide</c>
    /// itself: a proposal is exactly as readable as the message it was read from,
    /// and no later roster edit changes that.
    /// </summary>
    public ProjectSide SourceSide { get; set; } = ProjectSide.Client;

    [MaxLength(450)]
    public string? SourceImportedById { get; set; }

    public DateTime? SourceSentAt { get; set; }

    [ForeignKey("IngestedMessageId")]
    public tbl_IngestedMessage? IngestedMessage { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("DecidedById")]
    public AppUser? DecidedBy { get; set; }
}
