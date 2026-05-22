namespace assetlen.Shared.Models.Models.RemoteSite;

// Per-project specialization. Documentation + service-layer hint; does NOT
// grant authorization on its own — that comes from the user's tenant-level
// UserRoles. A user can hold different specializations across projects.
public enum ProjectMemberSpecialization
{
    Lead = 0,           // the Manager running this project
    Foreman = 1,        // site supervisor
    Engineer = 2,
    Architect = 3,
    Inspector = 4,      // quality / safety
    Photographer = 5,   // media uploader
    Subcontractor = 6,  // external trade worker (electrical, plumbing, etc.)
    Observer = 7,       // read-only watcher
    ClientOwner = 8,    // principal client / property owner
    Other = 99          // ad-hoc role
}
