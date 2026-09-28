using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// The one object (assetlen.md §3): a spec, a price, a date, a material or a
/// choice, carrying who agreed, when, and the evidence.
/// <para>
/// Append-only in spirit. A changed commitment is a new row that
/// <see cref="SupersedesId"/> the old one, so a typo cannot invert a decision
/// and the restatement history of a date stays readable (whatsapp-evidence.md F6).
/// </para>
/// </summary>
public class tbl_Commitment : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>Stored beside the deliverable so an item agreed before its checklist exists still has a stage.</summary>
    [MaxLength(40)]
    public string? StageId { get; set; }

    [MaxLength(40)]
    public string? DeliverableId { get; set; }

    public CommitmentKind Kind { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Body { get; set; }

    public CommitmentMaturity Maturity { get; set; } = CommitmentMaturity.Agreed;

    public CommitmentQueryState QueryState { get; set; } = CommitmentQueryState.None;

    public CommitmentSource SourceChannel { get; set; } = CommitmentSource.App;

    /// <summary>
    /// The mediator at the time it was recorded — the single accountable name
    /// on everything that crosses (§10.1). Accountability is a group-by on this.
    /// </summary>
    [MaxLength(40)]
    public string? AccountableMemberId { get; set; }

    /// <summary>Who agreed on the recording side. Usually the recorder.</summary>
    [MaxLength(450)]
    public string? AgreedById { get; set; }

    /// <summary>The other party, when they are on the roster (including an off-platform row).</summary>
    [MaxLength(40)]
    public string? AgreedWithMemberId { get; set; }

    /// <summary>
    /// The other party when they are not on the roster at all — "the windows
    /// contractor, on site". Cheap field, large fidelity gain (evidence §1).
    /// </summary>
    [MaxLength(200)]
    public string? AgreedWithPartyName { get; set; }

    public DateTime? AgreedAt { get; set; }

    /// <summary>True authorship. Shown to the delivery side; the client side sees the accountable face.</summary>
    [MaxLength(450)]
    public string? RecordedById { get; set; }

    /// <summary>The side the recorder sat on — decides who the counterparty is.</summary>
    public ProjectSide? RecordedBySide { get; set; }

    public decimal? Amount { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public DateTime? DueDate { get; set; }

    public int? LeadTimeDays { get; set; }

    /// <summary>For an open choice: which side owes the decision.</summary>
    public ProjectSide? OwedBySide { get; set; }

    [MaxLength(40)]
    public string? SupersedesId { get; set; }

    /// <summary>Set on the old row when a restatement replaces it. The register shows heads only.</summary>
    public DateTime? SupersededAt { get; set; }

    [MaxLength(40)]
    public string? SupersededById { get; set; }

    public DateTime? CounterpartyConfirmedAt { get; set; }

    [MaxLength(450)]
    public string? CounterpartyConfirmedById { get; set; }

    public DateTime? DisputedAt { get; set; }

    [MaxLength(450)]
    public string? DisputedById { get; set; }

    [MaxLength(1000)]
    public string? DisputeNote { get; set; }

    [MaxLength(1000)]
    public string? ResolutionNote { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(450)]
    public string? ResolvedById { get; set; }

    public DateTime? ClearedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    /// <summary>The raw message it was read from, when it came in through ingest.</summary>
    [MaxLength(40)]
    public string? IngestedMessageId { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("StageId")]
    public tbl_Stage? Stage { get; set; }

    [ForeignKey("DeliverableId")]
    public tbl_Deliverable? Deliverable { get; set; }

    [ForeignKey("AccountableMemberId")]
    public tbl_ProjectMember? AccountableMember { get; set; }

    [ForeignKey("AgreedWithMemberId")]
    public tbl_ProjectMember? AgreedWithMember { get; set; }

    [ForeignKey("AgreedById")]
    public AppUser? AgreedBy { get; set; }

    [ForeignKey("RecordedById")]
    public AppUser? RecordedBy { get; set; }

    [ForeignKey("SupersedesId")]
    public tbl_Commitment? Supersedes { get; set; }
}
