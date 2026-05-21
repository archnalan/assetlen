using mowt.Shared.Apicalls;
using mowt.Shared.Models.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace mowt.Shared.Services
{
    /// <summary>
    /// Restores user-role state that is lost when the browser refreshes the page.
    /// Also provides simple permission-gate helpers consumed by layouts and pages.
    /// </summary>
    public class UserSessionService : IUserSessionService
    {
        private readonly IAuthorizationApi _auth;
        private readonly ISD _sd;
        private readonly NavigationManager _nav;
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly ILogger<UserSessionService> _logger;

        public UserSessionService(
            IAuthorizationApi auth,
            ISD sd,
            NavigationManager nav,
            CustomAuthStateProvider authStateProvider,
            ILogger<UserSessionService> logger)
        {
            _auth = auth;
            _sd = sd;
            _nav = nav;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task RestoreUserRolesAsync()
        {
            var user = _sd.CurrentUser;
            if (user is null || string.IsNullOrEmpty(user.Id)) return;
            if (user.RolesDto is not null) return;

            try
            {
                var result = await _auth.GetRolesForUserByUserId(user.Id);
                if (result.IsSuccessStatusCode && result.Content is not null)
                {
                    user.RolesDto = result.Content;
                    _sd.SetUser(user);
                }
                else
                {
                    _logger.LogWarning(
                        "RestoreUserRolesAsync: failed for user {UserId}. StatusCode={StatusCode}",
                        user.Id, result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RestoreUserRolesAsync: unexpected error for user {UserId}", user?.Id);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RequirePermissionAsync(
            Func<UserRolesDto?, bool> check,
            string redirectTo = "/admin/dashboard")
        {
            await RestoreUserRolesAsync();
            var roles = _sd.CurrentUser?.RolesDto;
            if (check(roles)) return true;

            _nav.NavigateTo(redirectTo);
            return false;
        }

        /// <inheritdoc />
        public async Task<bool> RequireAdminLoginAsync()
        {
            await RestoreUserRolesAsync();
            var roles = _sd.CurrentUser?.RolesDto;
            if (roles?.AdminModuleLogin == true) return true;

            _nav.NavigateTo("/");
            return false;
        }

        /// <inheritdoc />
        public async Task<bool> RequireLibraryLoginAsync()
        {
            var user = _sd.CurrentUser;
            // Not authenticated at all — fine for public library pages.
            if (user is null) return true;

            await RestoreUserRolesAsync();
            var roles = _sd.CurrentUser?.RolesDto;
            if (roles?.LibraryModuleLogin == true) return true;

            // Authenticated but no library access — log out and send to login (no returnUrl).
            try
            {
                _sd.RemoveUser();
                await _authStateProvider.MarkUserAsLoggedOut();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RequireLibraryLoginAsync: error during logout");
            }

            _nav.NavigateTo("/login");
            return false;
        }
    }
}
