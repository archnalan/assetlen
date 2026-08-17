using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_Project : BaseEntity
{
    [MaxLength(200)]
    public string? ProjectName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public decimal? TotalBudget { get; set; }

    public DateTime? ExpectedStartDate { get; set; }

    public DateTime? ExpectedCompletionDate { get; set; }

    public DateTime? RevisedCompletionDate { get; set; }

    [MaxLength(450)]
    public string? InvestorId { get; set; }

    [MaxLength(450)]
    public string? ProjectManagerId { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// The developer's account that owns this project (assetlen.md D1).
    /// <para>
    /// <b>Not the contractor's.</b> Peter buys, so the project — and every
    /// commitment, artifact and receipt hanging off it — belongs to his
    /// account. Contractors are members and can be replaced without the record
    /// moving. Every child row's <c>TenantId</c> is stamped from here rather
    /// than from whoever happens to be writing, so a guest contractor cannot
    /// write rows into their own tenant and make them vanish from the owner's view.
    /// </para>
    /// </summary>
    [MaxLength(40)]
    public string? OwnerTenantId { get; set; }

    // ─── Billing: per project, by size — never per seat ─────────────────
    // Seat pricing would grow Peter's bill every time his contractor hired a
    // labourer, discouraging exactly the behaviour the product needs.

    /// <summary>
    /// Total floor area in square metres, **including sub-projects**. The
    /// billable unit is the top-level project, so a guest wing does not become
    /// a second invoice — it enlarges the first one.
    /// </summary>
    public decimal? FloorAreaSqm { get; set; }

    /// <summary>
    /// Billing tier. Derived from <see cref="FloorAreaSqm"/> via
    /// <c>ProjectSizingPolicy.TierFor</c>, but stored so a project's bill does
    /// not silently change when a threshold is retuned.
    /// </summary>
    public ProjectSizeTier SizeTier { get; set; } = ProjectSizeTier.Small;

    public ProjectSizeSource SizeSource { get; set; } = ProjectSizeSource.Unknown;

    /// <summary>Set when someone accepted a tier increase, so the charge is never a surprise.</summary>
    [MaxLength(450)]
    public string? SizeTierConfirmedById { get; set; }

    public DateTime? SizeTierConfirmedAt { get; set; }

    /// <summary>
    /// Opaque token addressing this project's inbound mailbox — the ongoing
    /// trickle half of the front door (plan.md P3). Mail sent to
    /// <c>in+{key}@…</c> lands as ingested messages, attachments included.
    /// <para>
    /// Random and unguessable rather than derived from the project id, because
    /// the address is pasted into third-party address books and forwarded on: it
    /// is a capability, so it must be revocable by reissuing the key without
    /// touching the project.
    /// </para>
    /// </summary>
    [MaxLength(40)]
    public string? IngestEmailKey { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    /// <summary>
    /// When someone sent this project to the bin. Null for a live project.
    /// <para>
    /// <b>Deletion is a two-step with thirty days in between.</b> A project is
    /// the container for every commitment, receipt and photograph on an
    /// engagement — the one thing in this product that must never be destroyed
    /// by a mis-tap. Archiving hides it from the rail, the home screen and the
    /// dashboard totals but touches nothing underneath, so restoring is a single
    /// write. Only after <see cref="PurgeDueAt"/> passes does anything get
    /// soft-deleted, and even then the rows survive as <c>IsDeleted</c>.
    /// </para>
    /// </summary>
    public DateTime? ArchivedAt { get; set; }

    /// <summary>Who binned it. The bin lists a name, because "who threw this away" is the first question asked.</summary>
    [MaxLength(450)]
    public string? ArchivedById { get; set; }

    /// <summary>Days an archived project waits in the bin before its contents are soft-deleted.</summary>
    public const int ArchiveRetentionDays = 30;

    /// <summary>When the contents get soft-deleted, or null while the project is live.</summary>
    [NotMapped]
    public DateTime? PurgeDueAt => ArchivedAt?.AddDays(ArchiveRetentionDays);

    public bool IsFirstFreeProject { get; set; }

    public bool IsSubscriptionActive { get; set; } = true;

    [MaxLength(3)]
    public string Currency { get; set; } = "UGX";

    /// <summary>
    /// Self-referential parent for one-level Sub-project nesting
    /// (e.g. Residence → Guest Wing). Null for top-level Projects.
    /// Enforced single-level at the service layer; DB allows the
    /// chain but UI/services do not traverse beyond depth 1.
    /// </summary>
    [MaxLength(40)]
    public string? ParentProjectId { get; set; }

    // Navigation
    [ForeignKey("InvestorId")]
    public AppUser? Investor { get; set; }

    [ForeignKey("ProjectManagerId")]
    public AppUser? ProjectManager { get; set; }

    [ForeignKey("ParentProjectId")]
    public tbl_Project? ParentProject { get; set; }

    [InverseProperty("ParentProject")]
    public ICollection<tbl_Project> SubProjects { get; set; } = new List<tbl_Project>();

    [InverseProperty("Project")]
    public ICollection<tbl_Stage> Stages { get; set; } = new List<tbl_Stage>();

    [InverseProperty("Project")]
    public ICollection<tbl_FundingEntry> FundingEntries { get; set; } = new List<tbl_FundingEntry>();

    [InverseProperty("Project")]
    public ICollection<tbl_ProgressUpdate> ProgressUpdates { get; set; } = new List<tbl_ProgressUpdate>();

    [InverseProperty("Project")]
    public ICollection<tbl_ProjectSubscription> Subscriptions { get; set; } = new List<tbl_ProjectSubscription>();

    [InverseProperty("Project")]
    public ICollection<tbl_Flag> Flags { get; set; } = new List<tbl_Flag>();

    [InverseProperty("Project")]
    public ICollection<tbl_ProjectMember> Members { get; set; } = new List<tbl_ProjectMember>();
}
