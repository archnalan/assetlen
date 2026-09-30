using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Refit;
using assetlen.Shared.statics;

namespace assetlen.Tests.Client;

/// <summary>
/// A refusal reaches the reader in the server's own words — the scheduler's
/// "The order loops back on itself at Gypsum ceiling." — never as a bare
/// "operation not completed"; a server fault never shows its exception text.
/// </summary>
public class ApiResponseHandlerTests
{
    private static readonly ApiResponseHandler Handler = new(NullLogger<ApiResponseHandler>.Instance);

    private static async Task<IApiResponse<string>> Answer(HttpStatusCode status, string body, string mediaType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost/api/WorkPlan/PreviewSchedule");
        var response = new HttpResponseMessage(status) { RequestMessage = request, Content = new StringContent(body, Encoding.UTF8, mediaType) };
        var error = await ApiException.Create(request, HttpMethod.Post, response, new RefitSettings());
        return new ApiResponse<string>(response, null, new RefitSettings(), error);
    }

    [Fact]
    public async Task A_plain_text_refusal_is_shown_as_it_was_said()
    {
        var r = await Answer(HttpStatusCode.BadRequest, "The order loops back on itself at Gypsum ceiling.", "text/plain");
        Assert.Equal("The order loops back on itself at Gypsum ceiling.", Handler.GetApiErrorMessage(r));
    }

    [Fact]
    public async Task A_validation_refusal_gives_its_first_reason()
    {
        var r = await Answer(HttpStatusCode.BadRequest,
            "{\"title\":\"One or more validation errors occurred.\",\"status\":400,\"errors\":{\"Activities[0].WorkDays\":[\"The field WorkDays must be between 0 and 400.\"]}}",
            "application/problem+json");
        Assert.Equal("The field WorkDays must be between 0 and 400.", Handler.GetApiErrorMessage(r));
    }

    [Fact]
    public async Task A_json_message_is_still_read()
    {
        var r = await Answer(HttpStatusCode.Conflict, "{\"message\":\"Already restated.\"}", "application/json");
        Assert.Equal("Already restated.", Handler.GetApiErrorMessage(r));
    }

    [Fact]
    public async Task A_server_fault_keeps_its_exception_text_to_itself()
    {
        var r = await Answer(HttpStatusCode.InternalServerError, "Npgsql.PostgresException: 23505 duplicate key", "text/plain");
        Assert.DoesNotContain("Npgsql", Handler.GetApiErrorMessage(r));
    }
}
