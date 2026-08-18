using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_FundingEntry : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? StageId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [MaxLength(450)]
    public string? PaidById { get; set; }

    [MaxLength(450)]
    public string? ConfirmedById { get; set; }

    public DateTime? ConfirmationDate { get; set; }

    public FundingStatus Status { get; set; } = FundingStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // ─── The back-and-forth ──────────────────────────────────────
    // Amount above is the declared figure in the project's currency and is what
    // every total is built from. Everything here records the gap between what
    // was sent and what arrived, because that gap is a conversation the two
    // parties otherwise have on the phone and never write down.

    /// <summary>
    /// The currency the funder actually typed in. Money crosses a border on this
    /// project, and "I sent 4,000" means nothing without it.
    /// </summary>
    [MaxLength(3)]
    public string? DeclaredCurrency { get; set; }

    /// <summary>The figure as typed, in <see cref="DeclaredCurrency"/>. Equals Amount when no conversion happened.</summary>
    public decimal? DeclaredAmount { get; set; }

    /// <summary>Project-currency units per one unit of the declared currency, at the moment it was declared.</summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// What the delivery side says actually landed, in the project's currency.
    /// Null until they answer. The difference from <see cref="Amount"/> is what
    /// the funder is being asked to accept or take up.
    /// </summary>
    public decimal? ReceivedAmount { get; set; }

    /// <summary>Why the figure differs — bank charges, a partial transfer, a rate.</summary>
    [MaxLength(500)]
    public string? ReceiptNote { get; set; }

    /// <summary>
    /// Optional proof of the transfer — an artifact id, not a URL, so the slip
    /// is hash-matched and stored once like every other file (CLAUDE.md §1).
    /// Shown to the delivery side while they decide what landed.
    /// </summary>
    [MaxLength(40)]
    public string? EvidenceArtifactId { get; set; }

    [MaxLength(260)]
    public string? EvidenceFileName { get; set; }

    /// <summary>Set when the funder accepted a received figure that differed from what they sent.</summary>
    public DateTime? SettledAt { get; set; }

    [MaxLength(450)]
    public string? SettledById { get; set; }

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("PaidById")]
    public AppUser? PaidBy { get; set; }

    [ForeignKey("ConfirmedById")]
    public AppUser? ConfirmedBy { get; set; }
}
