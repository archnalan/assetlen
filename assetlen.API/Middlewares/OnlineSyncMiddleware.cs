using assetlen.Service.DbServices;
using Hangfire;
using System.Security.Claims;
using System.Text;
using System.Text.Json; // Required for JSON parsing

namespace assetlen.API.Middlewares
{
    public class OnlineSyncMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public OnlineSyncMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Request.Headers
                .Where(h => !h.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            // Skip non-mutation requests
            if (!IsWriteRequest(context.Request.Method, context.Request.Path, headers))
            {
                await _next(context);
                return;
            }

            // Enable request body buffering
            context.Request.EnableBuffering();
            string requestBody = "";
            using (var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset for downstream readers
            }

            // Capture the original response stream
            var originalResponseBody = context.Response.Body;
            using var responseCaptureStream = new MemoryStream();
            context.Response.Body = responseCaptureStream;

            try
            {
                // Process the request
                await _next(context);

                // Handle successful responses (200-299)
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    // Reset response stream for reading
                    responseCaptureStream.Seek(0, SeekOrigin.Begin);
                    var responseBodyContent = await new StreamReader(responseCaptureStream).ReadToEndAsync();

                    // For POST requests: Update request body with new ID
                    if (context.Request.Method == "POST" &&
                        context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        string? newId = GetIdFromJson(responseBodyContent);
                        if (!string.IsNullOrEmpty(newId))
                        {
                            requestBody = UpdateJsonId(requestBody, newId);
                        }
                    }

                    // Prepare sync job details
                    var method = context.Request.Method;
                    var path = context.Request.Path + context.Request.QueryString;
                    headers.TryAdd("IsOnlineSync", "true");



                    // Build online URL
                    var onlineBaseUrl = _configuration["OnlineApi:BaseUrl"];
                    var onlineUrl = $"{onlineBaseUrl}{path}";

                    // Enqueue background job with updated request body
                    BackgroundJob.Enqueue<SyncDAL>(s => s.SyncWithOnlineApi(
                        onlineUrl,
                        method,
                        headers,
                        requestBody));
                }

                // Reset stream position for copying
                responseCaptureStream.Seek(0, SeekOrigin.Begin);
                await responseCaptureStream.CopyToAsync(originalResponseBody);
            }
            finally
            {
                // Restore the original response stream
                context.Response.Body = originalResponseBody;
            }
        }

        private static bool IsWriteRequest(string method, string path, Dictionary<string, string> headers)
        {
            var unallowedPaths = new List<string>
            {
                "/api/authorization/login",
                "/api/FileProcessing/ProcessExcelFile",
                "/sync/"
            };
            var correctMethod = method is "POST" or "PUT" or "PATCH" or "DELETE";
            var isSyncRequest = headers.ContainsKey("IsOnlineSync") &&
                                headers["IsOnlineSync"].Equals("true", StringComparison.OrdinalIgnoreCase);

            return correctMethod && !unallowedPaths.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase)) && !isSyncRequest;
        }

        private static string? GetIdFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // Handle JSON array response
                if (root.ValueKind == JsonValueKind.Array)
                {
                    // For arrays, return null since we can't get a single ID
                    return null;
                }

                // Handle JSON object response
                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in root.EnumerateObject())
                    {
                        if (property.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                        {
                            return property.Value.ValueKind switch
                            {
                                JsonValueKind.String => property.Value.GetString(),
                                _ => property.Value.ToString()
                            };
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Invalid JSON; ignore
            }
            return null;
        }
        private static string UpdateJsonId(string json, string newId)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // Handle JSON array request
                if (root.ValueKind == JsonValueKind.Array)
                {
                    using var ms = new MemoryStream();
                    using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false }))
                    {
                        writer.WriteStartArray();

                        foreach (var element in root.EnumerateArray())
                        {
                            if (element.ValueKind == JsonValueKind.Object)
                            {
                                writer.WriteStartObject();
                                bool idWritten = false;

                                foreach (var prop in element.EnumerateObject())
                                {
                                    if (prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                                    {
                                        writer.WriteString("id", newId);
                                        idWritten = true;
                                    }
                                    else
                                    {
                                        prop.WriteTo(writer);
                                    }
                                }

                                if (!idWritten)
                                {
                                    writer.WriteString("id", newId);
                                }

                                writer.WriteEndObject();
                            }
                            else
                            {
                                element.WriteTo(writer);
                            }
                        }

                        writer.WriteEndArray();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }

                // Handle JSON object request
                if (root.ValueKind == JsonValueKind.Object)
                {
                    using var ms = new MemoryStream();
                    using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false }))
                    {
                        writer.WriteStartObject();

                        bool idWritten = false;
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                            {
                                writer.WriteString("id", newId);
                                idWritten = true;
                            }
                            else
                            {
                                prop.WriteTo(writer);
                            }
                        }

                        if (!idWritten)
                            writer.WriteString("id", newId);

                        writer.WriteEndObject();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }

                return json;
            }
            catch (JsonException)
            {
                return json; // Return original on error
            }
        }

        // Helper for case-insensitive JSON property search
        private static readonly JsonSerializerOptions JsonPropertyCaseInsensitive =
            new() { PropertyNameCaseInsensitive = true };
    }
}