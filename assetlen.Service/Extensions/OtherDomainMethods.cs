using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;

namespace assetlen.Service.Extensions;

public static class OtherDomainMethods
{
    public static List<RoleStatusDto> GetRoleStatuses(this UserRolesDto userRoles)
    {
        // Each role in the canonical set surfaces as a row so admin UIs can
        // toggle assignment regardless of whether it's currently held.
        return UserRoles.All
            .Select(r => new RoleStatusDto { Name = r, Status = userRoles.Roles.Contains(r) })
            .ToList();
    }

    public static UserRolesDto GenerateUserRoles(List<string> roleNames)
    {
        // Filter to known ASSETLEN roles; ignore any stray legacy names.
        var canonical = UserRoles.All.ToHashSet();
        return new UserRolesDto
        {
            Roles = (roleNames ?? []).Where(canonical.Contains).ToList()
        };
    }
}
