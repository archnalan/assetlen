using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// How <b>one reader</b> arranges their own projects — the order they dragged
/// them into, and whether they pinned one to the top.
///
/// <para>
/// This is deliberately per user rather than per project. Peter funds four
/// developments and wants the one about to be poured at the top; Nalan is a
/// member of the same four and cares about the one he is standing on this
/// morning. Ordering stored on <c>tbl_Project</c> would mean one of them
/// rearranging the other's screen, which is the same class of mistake as a
/// shared "unread" flag.
/// </para>
///
/// <para>
/// Absence is the normal state. A project with no row here has never been
/// dragged, and sorts by its creation date behind everything that has — so a
/// reader who never touches the feature still gets a stable, sensible list, and
/// a newly created project lands at the bottom where the creator just left it.
/// </para>
///
/// <para>
/// <b>Top-level projects only.</b> A sub-project is drawn underneath its parent
/// wherever the parent sits; giving it an independent position would let a guest
/// wing float away from the house it belongs to.
/// </para>
/// </summary>
public class tbl_ProjectPreference : BaseEntity
{
    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>
    /// Position within the unpinned list, ascending. Sparse and non-contiguous
    /// is fine — only the relative order is read, and rewriting every row on
    /// every drop is how a reorder ends up costing a round trip per project.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Pinned to the top, above the draggable list. Capped at
    /// <see cref="MaxPins"/>: a pin board where everything is pinned is a list.
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>Pins order among themselves by when they were pinned, oldest first, so the top slot is stable.</summary>
    public DateTime? PinnedAt { get; set; }

    /// <summary>The most projects one reader may pin.</summary>
    public const int MaxPins = 3;

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("UserId")]
    public AppUser? User { get; set; }
}
