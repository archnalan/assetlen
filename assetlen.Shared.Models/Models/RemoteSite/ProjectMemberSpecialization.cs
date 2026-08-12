namespace assetlen.Shared.Models.Models.RemoteSite;

// Per-project specialization. Documentation + service-layer hint; does NOT
// grant authorization on its own — that comes from the user's tenant-level
// UserRoles combined with tbl_ProjectMember.Side. A user can hold different
// specializations across projects.
//
// Each value has a default ProjectSide — see ProjectSideDefaults.For().
public enum ProjectMemberSpecialization
{
    Lead = 0,                   // the Manager running this project — mediates by default
    Foreman = 1,                // site supervisor
    Engineer = 2,               // site / structural engineer
    Architect = 3,              // designs and directs — the canonical mediator
    Inspector = 4,              // quality / safety
    Photographer = 5,           // media uploader
    Subcontractor = 6,          // external trade worker (electrical, plumbing, etc.)
    Observer = 7,               // read-only watcher
    ClientOwner = 8,            // principal client / property owner — funds stages
    ClientRepresentative = 9,   // the client's person on the ground; decides finishes
    Other = 99                  // ad-hoc role
}
