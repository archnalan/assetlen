using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

// Per-project membership. Combined with the user's tenant-level role,
// drives "which projects can this user see / act on" at the service layer.
public class tbl_ProjectMember : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

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
