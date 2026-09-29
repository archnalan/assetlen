using Refit;

namespace assetlen.Shared.statics;

public static class RefitErrorContent
{
    // Refit 16 widened IApiResponse.Error to ApiExceptionBase; only an ApiException carries the
    // server's answer. A request that never reached the server has no body, so this is null.
    extension(ApiExceptionBase error)
    {
        public string? Content => (error as ApiException)?.Content;
    }
}
