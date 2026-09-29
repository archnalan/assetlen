using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>Curation by exception — the mediator keeps or drops frames; the cutoff sends the rest.</summary>
public interface ICurationApi
{
    [Get("/api/Curation/Draft")]
    Task<IApiResponse<CurationDraftDto>> Draft([Query] string projectId, [Query] string? day = null);

    [Put("/api/Curation/Mark")]
    Task<IApiResponse<CurationDraftDto>> Mark([Body] CurationMarkDto dto);

    [Post("/api/Curation/Publish")]
    Task<IApiResponse<CurationPublishResultDto>> Publish([Query] string projectId, [Query] string? day = null);
}
