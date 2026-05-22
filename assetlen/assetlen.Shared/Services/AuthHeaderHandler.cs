using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.statics;
using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Components;

namespace assetlen.Shared.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NavigationManager _navigationManager;
        private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);
        private readonly CustomAuthStateProvider? _authStateProvider;
        private readonly ISD _sd;

        // Endpoints that should not trigger redirect on 401
        private static readonly string[] _excludedEndpoints = new[]
        {
            "/api/Authorization/RefreshToken",
            "/api/Authorization/Login",
            "/api/Authorization/Register",
            "/api/Authorization/Signup",
            "/api/Account/Login",
            "/api/Account/Register",
            "/api/Account/Signup"
        };

        public AuthHeaderHandler(IStorageService localStorage, IHttpClientFactory httpClientFactory, NavigationManager navigationManager, ISD sd)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _navigationManager = navigationManager;
            _sd = sd;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // Retrieve token and tenant ID from local storage
                var sessionModel = await _localStorage.GetItemAsync<LoginResponseDto>(StorageKeys.SessionState);

                if (sessionModel != null && !string.IsNullOrEmpty(sessionModel.token))
                {
                    // Check if token is expired or about to expire
                    if (IsTokenExpiringSoon(sessionModel.token))
                    {
                        // Attempt to refresh the token
                        sessionModel = await RefreshTokenAsync(sessionModel);
                    }

                    // Add Authorization header
                    if (!string.IsNullOrEmpty(sessionModel?.token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionModel.token);
                    }

                    // Add Tenant ID header
                    if (!string.IsNullOrEmpty(sessionModel?.TenantId))
                    {
                        if (request.Headers.TryGetValues("TenantId", out _))
                        {
                            request.Headers.Remove("TenantId");
                        }
                        request.Headers.Add("TenantId", sessionModel.TenantId);
                    }
                }

                // Call the inner handler (continue the request)
                var response = await base.SendAsync(request, cancellationToken);

                // Check for 401 Unauthorized - redirect to login
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Check if this is an excluded endpoint (login, signup, refresh)
                    var requestPath = request.RequestUri?.PathAndQuery ?? string.Empty;
                    bool isExcludedEndpoint = _excludedEndpoints.Any(endpoint =>
                        requestPath.Contains(endpoint, StringComparison.OrdinalIgnoreCase));

                    if (!isExcludedEndpoint)
                    {
                        await RedirectToLogin();
                    }
                }

                // Handle 403 Forbidden — redirect based on user role context
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    var requestPath = request.RequestUri?.PathAndQuery ?? string.Empty;
                    bool isAdminPath = requestPath.Contains("/admin", StringComparison.OrdinalIgnoreCase);
                    bool userHasAdminLogin = _sd.CurrentUser?.RolesDto?.IsTenantAdmin == true;

                    if (isAdminPath || userHasAdminLogin)
                        _navigationManager.NavigateTo("/admin/dashboard");
                    else
                        _navigationManager.NavigateTo("/books");
                }

                return response;
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task RedirectToLogin()
        {
            // Clear the stored session since it's no longer valid
            await _localStorage.RemoveItemAsync(StorageKeys.SessionState);

            // Get the current page URL for return after login
            var currentUri = _navigationManager.Uri;
            var baseUri = _navigationManager.BaseUri;

            // Get relative path for return URL
            var returnUrl = currentUri.Replace(baseUri, "/");
            if (!returnUrl.StartsWith("/"))
            {
                returnUrl = "/" + returnUrl;
            }

            // Don't redirect if already on login page
            if (!returnUrl.Contains("/login", StringComparison.OrdinalIgnoreCase))
            {
                var loginUrl = string.IsNullOrEmpty(returnUrl) || returnUrl == "/"
                    ? "/login"
                    : $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";

                _navigationManager.NavigateTo(loginUrl, forceLoad: true);
            }
        }

        private bool IsTokenExpiringSoon(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                    return true;

                var jwtToken = handler.ReadJwtToken(token);
                var expiryTime = jwtToken.ValidTo;

                // Check if token expires in the next 5 minutes or is already expired
                return expiryTime <= DateTime.UtcNow.AddMinutes(5);
            }
            catch
            {
                return true;
            }
        }

        private async Task<LoginResponseDto> RefreshTokenAsync(LoginResponseDto currentSession)
        {
            // Use semaphore to prevent multiple simultaneous refresh attempts
            await _refreshSemaphore.WaitAsync();
            try
            {
                // Check again if another thread already refreshed the token
                var latestSession = await _localStorage.GetItemAsync<LoginResponseDto>(StorageKeys.SessionState);
                if (latestSession?.token != currentSession.token)
                {
                    // Token was already refreshed by another request
                    return latestSession;
                }

                // Create a separate HTTP client without AuthHeaderHandler to avoid circular dependency
                var httpClient = _httpClientFactory.CreateClient("RefreshTokenClient");

                var refreshRequest = new RefreshTokenRequestDto
                {
                    Token = currentSession.token,
                    RefreshToken = currentSession.RefreshToken
                };

                var response = await httpClient.PostAsJsonAsync("/api/Authorization/RefreshToken", refreshRequest);

                if (response.IsSuccessStatusCode)
                {
                    var newSession = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    if (newSession != null)
                    {
                        // Save the new token
                        await _localStorage.SetItemAsync(StorageKeys.SessionState, newSession);

                        return newSession;
                    }
                }

                // If refresh fails, return the current session (will likely fail on API call)
                return currentSession;
            }
            catch
            {
                // If refresh fails, return the current session
                return currentSession;
            }
            finally
            {
                _refreshSemaphore.Release();
            }
        }
    }
}
