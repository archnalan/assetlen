using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace assetlen.Shared.statics
{
    public class ApiResponseHandler : IApiResponseHandler
    {
        private readonly ILogger<ApiResponseHandler> _logger;

        public ApiResponseHandler(ILogger<ApiResponseHandler> logger)
        {
            _logger = logger;
        }

        public T? ExtractContent<T>(IApiResponse<T> response)
        {
            if (!response.IsSuccessStatusCode)
                return default;

            return response.Content;
        }

        public string GetApiErrorMessage<T>(IApiResponse<T> response)
        {
            var errorContent = response.Error?.Content;

            if (string.IsNullOrWhiteSpace(errorContent))
            {
                return "An unknown error occurred.";
            }
            // 404 is how the server answers a project or section this reader was
            // not invited into (a refusal would confirm it exists) — expected, not a fault.
            var level = (int)response.StatusCode is 403 or 404 ? LogLevel.Warning : LogLevel.Error;
            _logger.Log(level, "API Error: {ErrorContent}", errorContent);
            try
            {
                using var jsonDoc = JsonDocument.Parse(errorContent);
                var root = jsonDoc.RootElement;

                string? message = root.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString()
                    : "An error occurred, operation not completed.";

                _logger.Log(level, "API Error: {Message}", message);
                return message ?? "An error occurred, operation not completed.";
            }
            catch (JsonException)
            {
                _logger.Log(level, "Failed to parse error response.");
                return "Error! Operation not completed";
            }
        }
    }
}
