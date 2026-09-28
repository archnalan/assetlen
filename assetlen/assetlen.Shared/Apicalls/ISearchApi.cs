using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>
/// Search (plan.md P6): one question, answered grouped by object and already
/// filtered to what the reader may see on each project. The client renders it.
/// </summary>
public interface ISearchApi
{
    [Get("/api/Search/Query")]
    Task<IApiResponse<SearchResultDto>> Query([Query] string q, [Query] string? projectId = null, [Query] int take = 20);
}
