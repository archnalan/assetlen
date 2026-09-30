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
            var status = (int)response.StatusCode;
            var level = status is 403 or 404 ? LogLevel.Warning : LogLevel.Error;
            _logger.Log(level, "API Error: {ErrorContent}", errorContent);

            // The services answer a refusal in plain words ("The order loops back on itself
            // at Gypsum ceiling."); the reader is owed the reason. A server fault keeps the
            // generic line so no exception text reaches a page.
            var refusal = status is >= 400 and < 500;
            try
            {
                using var jsonDoc = JsonDocument.Parse(errorContent);
                var root = jsonDoc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return refusal && root.ValueKind == JsonValueKind.String ? root.GetString() ?? Generic : Generic;

                if (root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                    return msgProp.GetString() ?? Generic;

                // A validation refusal: its first reason.
                if (refusal && root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    foreach (var field in errors.EnumerateObject())
                        if (field.Value.ValueKind == JsonValueKind.Array && field.Value.GetArrayLength() > 0 && field.Value[0].GetString() is { } first)
                            return first;

                return Generic;
            }
            catch (JsonException)
            {
                var text = errorContent.Trim();
                return refusal && text.Length <= 300 && !text.Contains('<') ? text : Generic;
            }
        }

        private const string Generic = "An error occurred, operation not completed.";
    }
}
