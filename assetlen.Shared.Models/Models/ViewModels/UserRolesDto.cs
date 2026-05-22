using assetlen.Shared.Models.statics;

namespace assetlen.Shared.Models.Models.ViewModels;

// Transport for a user's assigned roles. Wire format = a flat list of
// role-name strings (matches ASP.NET Identity's native storage).
// Convenience accessors below avoid string-literal comparisons at call sites.
public class UserRolesDto
{
    public List<string> Roles { get; set; } = [];

    public bool SystemAdmin => Roles.Contains(UserRoles.SystemAdmin);
    public bool Contractor => Roles.Contains(UserRoles.Contractor);
    public bool Manager => Roles.Contains(UserRoles.Manager);
    public bool Crew => Roles.Contains(UserRoles.Crew);
    public bool Client => Roles.Contains(UserRoles.Client);
    public bool Guest => Roles.Contains(UserRoles.Guest);

    public bool IsTenantAdmin => Contractor || SystemAdmin;
    public bool CanSeeFinancials => Contractor || Manager || Client || SystemAdmin;
    public bool IsInternal => Contractor || Manager || Crew || SystemAdmin;
    public bool IsExternal => Client || Guest;
}
