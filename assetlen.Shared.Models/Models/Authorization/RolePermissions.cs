using assetlen.Shared.Models.statics;

namespace assetlen.Shared.Models.Models.Authorization;

// Canonical role × module matrix. Module-level only. Per-project scoping
// (e.g. Manager seeing only their projects) is enforced at the service layer
// via tbl_ProjectMember. Channel filtering (Crew vs Client) is enforced
// server-side on Site Journal and Stream queries.
public static class RolePermissions
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<AppModule, ModuleAccess>> Map =
        new Dictionary<string, IReadOnlyDictionary<AppModule, ModuleAccess>>
        {
            [UserRoles.SystemAdmin] = AllModules(ModuleAccess.Admin),
            [UserRoles.Contractor]  = AllModules(ModuleAccess.Admin),

            [UserRoles.Manager] = new Dictionary<AppModule, ModuleAccess>
            {
                [AppModule.Identity]     = ModuleAccess.Read,
                [AppModule.Projects]     = ModuleAccess.Write,
                [AppModule.Journal]      = ModuleAccess.Admin,
                [AppModule.Streams]      = ModuleAccess.Admin,
                [AppModule.Timeline]     = ModuleAccess.Write,
                [AppModule.Finance]      = ModuleAccess.Read,
                [AppModule.Documents]    = ModuleAccess.Write,
                [AppModule.Lookbook]     = ModuleAccess.Write,
                [AppModule.Search]       = ModuleAccess.Read,
                [AppModule.Integrations] = ModuleAccess.Read
            },

            [UserRoles.Crew] = new Dictionary<AppModule, ModuleAccess>
            {
                [AppModule.Identity]  = ModuleAccess.Read,
                [AppModule.Projects]  = ModuleAccess.Read,
                [AppModule.Journal]   = ModuleAccess.Write,
                [AppModule.Streams]   = ModuleAccess.Write,
                [AppModule.Timeline]  = ModuleAccess.Read,
                [AppModule.Documents] = ModuleAccess.Read,
                [AppModule.Search]    = ModuleAccess.Read
            },

            [UserRoles.Client] = new Dictionary<AppModule, ModuleAccess>
            {
                [AppModule.Identity]  = ModuleAccess.Read,
                [AppModule.Projects]  = ModuleAccess.Read,
                [AppModule.Journal]   = ModuleAccess.Read,
                [AppModule.Streams]   = ModuleAccess.Write,
                [AppModule.Timeline]  = ModuleAccess.Read,
                [AppModule.Finance]   = ModuleAccess.Read,
                [AppModule.Documents] = ModuleAccess.Read,
                [AppModule.Lookbook]  = ModuleAccess.Read
            },

            [UserRoles.Guest] = new Dictionary<AppModule, ModuleAccess>
            {
                [AppModule.Identity]  = ModuleAccess.Read,
                [AppModule.Projects]  = ModuleAccess.Read,
                [AppModule.Journal]   = ModuleAccess.Read,
                [AppModule.Streams]   = ModuleAccess.Read,
                [AppModule.Timeline]  = ModuleAccess.Read,
                [AppModule.Documents] = ModuleAccess.Read,
                [AppModule.Lookbook]  = ModuleAccess.Read
            }
        };

    public static bool HasAccess(IEnumerable<string> userRoles, AppModule module, ModuleAccess required)
    {
        if (required == ModuleAccess.None) return true;
        return GetMaxLevel(userRoles, module) >= required;
    }

    public static ModuleAccess GetMaxLevel(IEnumerable<string> userRoles, AppModule module)
    {
        var max = ModuleAccess.None;
        foreach (var role in userRoles)
        {
            if (!Map.TryGetValue(role, out var perms)) continue;
            if (!perms.TryGetValue(module, out var level)) continue;
            if (level > max) max = level;
        }
        return max;
    }

    public static IEnumerable<AppModule> AccessibleModules(IEnumerable<string> userRoles)
    {
        foreach (AppModule module in Enum.GetValues<AppModule>())
        {
            if (HasAccess(userRoles, module, ModuleAccess.Read))
                yield return module;
        }
    }

    private static IReadOnlyDictionary<AppModule, ModuleAccess> AllModules(ModuleAccess level)
    {
        var dict = new Dictionary<AppModule, ModuleAccess>();
        foreach (AppModule m in Enum.GetValues<AppModule>())
            dict[m] = level;
        return dict;
    }
}
