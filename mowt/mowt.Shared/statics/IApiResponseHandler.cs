using Refit;

namespace mowt.Shared.statics
{
    public interface IApiResponseHandler
    {
        T? ExtractContent<T>(IApiResponse<T> response);
        string GetApiErrorMessage<T>(IApiResponse<T> response);
    }
}