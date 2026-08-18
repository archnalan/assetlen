using System.Text.Json;
using System.Text.Json.Serialization;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface IPandoraSmsService
    {
        Task<PandoraSmsResponse> SendSmsAsync(string number, string message);
    }

    /// <summary>
    /// What the Pandora gateway actually answers.
    /// <para>
    /// This used to declare <c>ErrorMessage</c>, <c>Message</c> and <c>Status</c>,
    /// none of which the gateway sends — it returns <c>statusCode</c>,
    /// <c>success</c>, a <c>messages</c> array and a <c>data</c> payload. Only
    /// <c>Success</c> ever bound, so every failure logged a blank reason and a
    /// one-word "Invalid SMS type" from the gateway reached the developer as a
    /// bare 500 with nothing to go on. The reason is the whole value of the
    /// response; it is mapped now.
    /// </para>
    /// </summary>
    public class PandoraSmsResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>The gateway returns an array whether it succeeded or failed.</summary>
        [JsonPropertyName("messages")]
        public List<string> Messages { get; set; } = new();

        /// <summary>
        /// Shape varies: an empty array on failure, an object carrying
        /// <c>sms_cost</c> and <c>balance</c> on success. Held loosely so one
        /// shape cannot make the other throw during deserialisation.
        /// </summary>
        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }

        /// <summary>The gateway's own words, for the log. Never null on a failure.</summary>
        [JsonIgnore]
        public string ErrorMessage =>
            Messages is { Count: > 0 } ? string.Join("; ", Messages) : $"Gateway returned {StatusCode} with no message";

        /// <summary>Remaining credit, when the gateway reported it. Useful in a log line that says why nothing was delivered.</summary>
        [JsonIgnore]
        public int? Balance =>
            Data is { ValueKind: JsonValueKind.Object } d && d.TryGetProperty("balance", out var b) && b.TryGetInt32(out var v)
                ? v : null;
    }
}
