using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>Web push for this person's devices.</summary>
public interface IPushApi
{
    [Get("/api/Push/Status")]
    Task<IApiResponse<PushStatusDto>> Status();

    [Post("/api/Push/Subscribe")]
    Task<IApiResponse<PushStatusDto>> Subscribe([Body] PushSubscriptionCreateDto dto);

    [Delete("/api/Push/Unsubscribe")]
    Task<IApiResponse<PushStatusDto>> Unsubscribe([Query] string endpoint);

    [Post("/api/Push/Test")]
    Task<IApiResponse<object>> Test();

    [Get("/api/Push/Deliveries")]
    Task<IApiResponse<List<PushDeliveryDto>>> Deliveries([Query] int take = 20);
}
