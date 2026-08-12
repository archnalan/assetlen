using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One human, one login, many accounts (assetlen.md §10.2).
/// <para>
/// Before this, <c>AppUser.TenantId</c> was a single scalar stamped into the
/// JWT, so a person belonged to exactly one organisation forever. That is fine
/// while the contractor owns everything and wrong the moment the developer does:
/// Nalan works for several developers and must appear as a guest in each,
/// without a second email address.
/// </para>
/// <para>
/// <c>AppUser.TenantId</c> survives as the user's <em>default</em> account —
/// which one they land in — not as the truth about where they may act.
/// </para>
/// </summary>
/// <remarks>
/// <b>Deliberately not tenant-scoped.</b> The global query filter would hide a
/// user's memberships in every account except the one they are currently in —
/// which is precisely the list sign-in needs before a tenant has been chosen.
/// Indexes are configured in <c>AssetlenDbContext</c> alongside the others.
/// </remarks>
public class tbl_TenantMembership : BaseEntity
{
    [MaxLength(450)]
    public string? UserId { get; set; }

    /// <summary>
    /// The account this membership grants standing in. Deliberately shadows the
    /// inherited <c>TenantId</c>: for this table the tenant <em>is</em> the
    /// subject, not the owner, so the row must not be filtered out of the very
    /// tenant it describes.
    /// </summary>
    [MaxLength(40)]
    public override string? TenantId { get; set; }

    /// <summary>
    /// Comma-separated tenant-level roles held in <em>this</em> account. A
    /// person can be a developer in their own and delivery-side in another.
    /// Per-project standing still comes from <c>tbl_ProjectMember</c>.
    /// </summary>
    [MaxLength(300)]
    public string? Roles { get; set; }

    /// <summary>Where this user lands at sign-in. Exactly one per user should be true.</summary>
    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? JoinedAt { get; set; }

    [MaxLength(450)]
    public string? InvitedById { get; set; }

    // Navigation
    [ForeignKey("UserId")]
    public AppUser? User { get; set; }
}
