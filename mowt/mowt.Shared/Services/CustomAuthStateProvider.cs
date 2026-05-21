using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using mowt.Shared.Models.Models.ViewModels.Users;
using Microsoft.AspNetCore.Components;
using mowt.Shared.Models.Models;
using System.Text.Json;

namespace mowt.Shared.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {

        private readonly IStorageService localStorage;
        public readonly NavigationManager _navigationManager;

        public CustomAuthStateProvider(IStorageService localStorage, NavigationManager navigationManager)
        {
            this.localStorage = localStorage;
            _navigationManager = navigationManager;
        }
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var sessionModel = (await localStorage.GetItemAsync<LoginResponseDto>("sessionState"));
            var identity = sessionModel == null ? new ClaimsIdentity() : GetClaimsIdentity(sessionModel.token);
            var user = new ClaimsPrincipal(identity);

            //Console.WriteLine($"LocalToken read from storage {Newtonsoft.Json.JsonConvert.SerializeObject(sessionModel)}");
            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(LoginResponseDto model)
        {
            await localStorage.SetItemAsync("sessionState", model);
            var identity = GetClaimsIdentity(model.token);
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        public async Task<UserClaimsDto> GetAuthenticatedUser(LoginResponseDto model)
        {

            var jwtToken = ParseClaimsFromJwt(model.token);
            var userclaim = jwtToken.FirstOrDefault(c => c.Type.ToLower() == "user");

            return JsonSerializer.Deserialize<UserClaimsDto>(userclaim.Value ?? "null");
        }

        public async Task<ClaimsPrincipal> GetUserClaimsFromToken(LoginResponseDto model)
        {

            var identity = GetClaimsIdentity(model.token);
            var user = new ClaimsPrincipal(identity);
            return user;
        }
        private ClaimsIdentity GetClaimsIdentity(string token)
        {

            var jwtToken = ParseClaimsFromJwt(token);
            var claims = jwtToken;
            return CleanedupClaims(claims);
        }

        public async Task MarkUserAsLoggedOut()
        {
            await localStorage.RemoveItemAsync("sessionState");
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

        }
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));

        }
        private ClaimsIdentity CleanedupClaims(IEnumerable<Claim> claimsIn)
        {
            var claims = new List<Claim>();



            // Add standard claims
            //claims.AddRange(claimsIn);

            // Find and handle the "roles" claim correctly
            var rolesClaim = claimsIn.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "roles");
            if (rolesClaim != null)
            {


                // Check if roles are in JSON array format
                if (rolesClaim.Value.StartsWith("[") && rolesClaim.Value.EndsWith("]"))
                {
                    var roles = JsonSerializer.Deserialize<string[]>(rolesClaim.Value);
                    if (roles != null)
                    {
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                    }
                }

            }

            return new ClaimsIdentity(claims, "jwt");
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }


}
