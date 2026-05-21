using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Shared.Services
{
    /// <summary>
    /// Provides session-level helpers for hydrating user state that may be lost on page refresh.
    /// </summary>
    public interface IUserSessionService
    {
        /// <summary>
        /// If a user is already authenticated in <see cref="ISD"/> but their
        /// <c>RolesDto</c> is null (e.g. after a browser refresh), fetches the
        /// roles from the API and re-applies them on the current user object.
        /// </summary>
        Task RestoreUserRolesAsync();

        /// <summary>
        /// Checks whether the current user satisfies <paramref name="check"/>.
        /// If not, navigates to <paramref name="redirectTo"/> so the caller can
        /// stop initializing and let the <see cref="ForbiddenRedirect"/> component
        /// handle the UI feedback. Returns <c>true</c> when the user is permitted.
        /// </summary>
        Task<bool> RequirePermissionAsync(
            Func<UserRolesDto?, bool> check,
            string redirectTo = "/admin/dashboard");

        /// <summary>
        /// Checks if the current user has <c>AdminModuleLogin</c>.  
        /// Returns <c>true</c> when permitted; navigates to "/" and returns <c>false</c> otherwise.
        /// </summary>
        Task<bool> RequireAdminLoginAsync();

        /// <summary>
        /// Checks if the current user has <c>LibraryModuleLogin</c>.  
        /// If not, logs the user out and redirects to the login page without a returnUrl.
        /// Returns <c>true</c> when permitted.
        /// </summary>
        Task<bool> RequireLibraryLoginAsync();
    }
}
