using mowt.Shared.Models.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

namespace mowt.Shared.Models.Models
{

    public interface ITenantProvider
    {
        string GetTenantId();
        string GetUserId();
        UserClaimsDto GetCurrentUser();
        bool IsSuperAdmin();
    }

    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTenantId()
        {
            //var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["TenantId"].FirstOrDefault();

            var identity = _httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;

            if (identity != null)
            {

                //var token = context.SecurityToken as JsonWebToken;
                //var tenantIdFromToken = token.GetPayloadValue<string>("TenantId");


                //if (!string.IsNullOrEmpty(tenantIdFromToken))
                //{
                //    //Logger.LogInformation("Here is the client id from the request {clientId}", clientId);
                //    if (tenantIdFromToken != TenantId)
                //    {
                //        context.Fail("Invalid TenantId");
                //        context.Response.StatusCode = 401;
                //        context.Response.ContentType = "application/json";
                //        var result = System.Text.Json.JsonSerializer.Serialize(new { message = $"Invalid TenantId header or Token" });
                //        return context.Response.WriteAsync(result);
                //    }
                //}
                var userClaims = identity.Claims;
                var tenantId = userClaims.FirstOrDefault(x => x.Type.ToLower() == "tenantid")?.Value;
                if (string.IsNullOrEmpty(tenantId))
                {


                }
                return tenantId;
            }
            return "";
        }



        public string GetUserId()
        {
            return GetCurrentUser()?.Id ?? string.Empty;
        }

        public bool IsSuperAdmin()
        {
            var identity = _httpContextAccessor.HttpContext?.User;
            return identity?.IsInRole("mowtSuperAdmin") ?? false;
        }
        public UserClaimsDto GetCurrentUser()
        {
            var identity = _httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;
                var userDetails = userClaims.FirstOrDefault(x => x.Type.ToLower() == "user")?.Value;
                if (userDetails == null) return null;
                var userClaimsDto = JsonSerializer.Deserialize<UserClaimsDto>(userDetails);
                return userClaimsDto;

            }
            return null;
        }
    }
}
