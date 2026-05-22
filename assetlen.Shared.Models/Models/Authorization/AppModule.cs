namespace assetlen.Shared.Models.Models.Authorization;

// Mirrors the folder layout under assetlen.Shared/Modules/.
// Primary axis of the role-permission matrix in RolePermissions.
public enum AppModule
{
    Identity = 1,
    Projects,
    Journal,
    Streams,
    Timeline,
    Finance,
    Documents,
    Lookbook,
    Search,
    Integrations
}
