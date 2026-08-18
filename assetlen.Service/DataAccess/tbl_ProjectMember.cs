using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

// Per-project membership. Combined with the user's tenant-level role,
// drives "which projects can this user see / act on" at the service layer.
//
// A project has two principal sides (Client / Contractor) and one or two
// mediators between them. Both facts live here, per project — never inferred
// from a tenant-level role, which is global and cannot vary per project.
public class tbl_ProjectMember : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>
    /// The platform user this membership belongs to. **Null for an
    /// unregistered party** — a windows fabricator, a consulting engineer, a
    /// council planner — who is attributable on commitments and in the record
    /// but will never sign in. Such a row grants no access to anyone; it exists
    /// so <c>AgreedWith</c> can point at something real.
    /// </summary>
    [MaxLength(450)]
    public string? UserId { get; set; }

    /// <summary>
    /// Display name for an unregistered party (<see cref="UserId"/> null).
    /// Ignored when <see cref="UserId"/> is set — the user's own name wins.
    /// </summary>
    [MaxLength(200)]
    public string? PartyName { get; set; }

    /// <summary>
    /// Which principal party this member belongs to. Defaults from
    /// <see cref="Specialization"/> via <c>ProjectSideDefaults.For</c>, but is
    /// stored explicitly because an architect or engineer can sit on either
    /// side depending on who retained them.
    /// </summary>
    public ProjectSide Side { get; set; } = ProjectSide.Contractor;

    /// <summary>
    /// True for the one — at most two — people who mediate between the sides.
    /// A mediator reads both surfaces and is the only member who may expose
    /// Site Diary material to the Client channel. Capped at
    /// <c>ProjectSideDefaults.MaxMediators</c> in <c>ProjectMemberDAL</c>.
    /// </summary>
    public bool IsMediator { get; set; }

    public ProjectMemberSpecialization Specialization { get; set; } = ProjectMemberSpecialization.Other;

    [MaxLength(120)]
    public string? Title { get; set; }   // free-form override when Specialization = Other

    public bool IsActive { get; set; } = true;

    public DateTime? JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    [MaxLength(450)]
    public string? AssignedById { get; set; }

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("UserId")]
    public AppUser? User { get; set; }

    [ForeignKey("AssignedById")]
    public AppUser? AssignedBy { get; set; }
}
