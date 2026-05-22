using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Shared.Services
{
    // Session-level helpers for hydrating user role state and gating navigation.
    public interface IUserSessionService
    {
        // Re-fetches roles for the current user if RolesDto is null (post-refresh).
        Task RestoreUserRolesAsync();

        // Generic gate. Returns true if check passes; otherwise navigates to
        // redirectTo and returns false.
        Task<bool> RequirePermissionAsync(
            Func<UserRolesDto?, bool> check,
            string redirectTo = "/");

        // Contractor or SystemAdmin only. Navigates to "/" on failure.
        Task<bool> RequireTenantAdminAsync();

        // Any internal user (Contractor / Manager / Crew / SystemAdmin).
        // Logs the user out and redirects to /login on failure.
        Task<bool> RequireInternalUserAsync();
    }
}
