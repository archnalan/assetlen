using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>Markup layers on an artifact, and the question a circle asks.</summary>
public interface IAnnotationsApi
{
    [Get("/api/Annotations/GetForArtifact")]
    Task<IApiResponse<AnnotatedArtifactDto>> GetForArtifact([Query] string artifactId);

    [Post("/api/Annotations/Save")]
    Task<IApiResponse<AnnotationDto>> Save([Body] AnnotationSaveDto dto);

    [Post("/api/Annotations/Ask")]
    Task<IApiResponse<AskResultDto>> Ask([Body] AnnotationAskDto dto);

    [Put("/api/Annotations/Expose")]
    Task<IApiResponse<AnnotationDto>> Expose([Body] AnnotationExposeDto dto);
}
