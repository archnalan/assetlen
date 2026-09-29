using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// Markup as a layer on the original (assetlen.md Law 2) — Peter's fourth
/// search: circle the thing, ask why. Visibility follows the file underneath,
/// then the layer's own channel; a layer that asked about the register is
/// shown only to the seats that read the register.
/// </summary>
public interface IAnnotationDAL
{
    Task<ServiceResult<AnnotatedArtifactDto>> GetForArtifact(string artifactId, string userId, CancellationToken ct = default);

    /// <summary>A new layer, or the next version of one of your own. The earlier version is kept.</summary>
    Task<ServiceResult<AnnotationDto>> Save(AnnotationSaveDto dto, string userId, CancellationToken ct = default);

    /// <summary>A circle and a question: the query lands on the commitment, the circle stays on the file.</summary>
    Task<ServiceResult<AskResultDto>> Ask(AnnotationAskDto dto, string userId, CancellationToken ct = default);

    /// <summary>Move a delivery-side layer to the client side, or back. The mediator's call.</summary>
    Task<ServiceResult<AnnotationDto>> Expose(AnnotationExposeDto dto, string userId, CancellationToken ct = default);
}
