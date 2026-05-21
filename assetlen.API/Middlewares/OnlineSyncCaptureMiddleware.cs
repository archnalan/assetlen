using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Newtonsoft.Json;
using System;

namespace assetlen.API.Middlewares
{

    // Online API Middleware
    public class OnlineSyncCaptureMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<OnlineSyncCaptureMiddleware> _logger;


        public OnlineSyncCaptureMiddleware(RequestDelegate next, ILogger<OnlineSyncCaptureMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AssetlenDbContext db)
        {
            var headers = context.Request.Headers
                        .Where(h => !h.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(h => h.Key, h => h.Value.ToString());
            //remove the authorization header from the headers
            headers.Remove("Authorization");
            if (headers.ContainsKey("IsOnlineSync") && headers["IsOnlineSync"].ToLower() == "true")
            {

                if (!context.Request.Headers.TryGetValue("X-Offline-Api-Key", out var apiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized request. Invalid X-Offline-Api-Key");
                    return;
                }
                if (!context.Request.Headers.TryGetValue("X-UserId", out var userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized request. Invalid X-UserId");
                    return;
                }
                if (!context.Request.Headers.TryGetValue("TenantId", out var tenantId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized request. Invalid TenantId");
                    return;
                }
                //create service for IAthorizationDAL from context
                var authorizationDAL = context.RequestServices.GetRequiredService<IAuthorizationDAL>();
                var result = await authorizationDAL.ValidateApiKeyAsync(apiKey, tenantId, userId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Unauthorized request. Invalid API Key: {apiKey}, UserId: {userId}, TenantId: {tenantId}", apiKey, userId, tenantId);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(result.Error.Message);
                    return;
                }

                // SET HEADER HERE - BEFORE ANY NEXT MIDDLEWARE
                context.Request.Headers["Authorization"] = $"Bearer {result.Data}";

                _logger.LogInformation("Set JWT: {jwt}", result.Data);
                //log x-offline-api-key header and jwt for the request
                _logger.LogInformation("continuing rest with Token: {token}, created JWT: {jwt}", apiKey, context.Request.Headers["Authorization"]);
            }

            // Skip non-mutation requests (GET/HEAD/OPTIONS)
            if (!IsWriteRequest(context.Request.Method, context.Request.Path, headers))
            {


                await _next(context);
                return;
            }
            var tenantProvider = context.RequestServices.GetRequiredService<ITenantProvider>();

            // Enable buffering to read the request body
            context.Request.EnableBuffering();
            var originalBody = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;


            await _next(context);

            // Log only if the response is successful
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                // Capture request details
                var tenantId = tenantProvider.GetTenantId();
                var method = context.Request.Method;
                var path = context.Request.Path;
                context.Request.Body.Position = 0;
                var payload = await new StreamReader(context.Request.Body).ReadToEndAsync();
                string token = context?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Log to sync table
                db.tbl_SyncLogs.Add(new tbl_SyncLog
                {
                    TenantId = tenantId,
                    Method = method,
                    Endpoint = path,
                    Payload = payload,
                    Headers = JsonConvert.SerializeObject(headers), // originally Dictionary<string, string>? but kepts as string
                    Timestamp = DateTime.UtcNow,
                    UserJwt = token,

                });
                await db.SaveChangesAsync();
            }

            // Reset the response body
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBody);
        }

        private static bool IsWriteRequest(string method, string path, Dictionary<string, string> headers)
        {
            //create list of unallowedpaths
            var unallowedPaths = new List<string> {
                "/api/authorization/login",
                 "/api/FileProcessing/ProcessExcelFile",
                 "/api/Authorization/IssueSyncKey",
                                                        "/sync/"
            };
            var correctMethod = method is "POST" or "PUT" or "PATCH" or "DELETE";

            //check if headers contain a header for "IssyncRequest" and if it is true
            var isSyncRequest = headers.ContainsKey("IsOnlineSync") && headers["IsOnlineSync"].ToLower() == "true";

            return correctMethod && !unallowedPaths.Any(p => path.ToLower().Contains(p)) && !isSyncRequest;

        }
    }
}
