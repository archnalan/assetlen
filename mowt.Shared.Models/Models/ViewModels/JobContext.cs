using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{

    public class JobContext
    {
        public string Method { get; set; }
        public string RequestPath { get; set; }
        public string? RequestBody { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string OnlineApiUrl { get; set; }

        public static JobContext FromRequest(HttpRequest request, object? body)
        {
            return new JobContext
            {
                Method = request.Method,
                RequestPath = request.Path,
                RequestBody = body is not null ? System.Text.Json.JsonSerializer.Serialize(body) : null,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                OnlineApiUrl = "https://api.mowt.com" // Configurable
            };
        }
    }

}
