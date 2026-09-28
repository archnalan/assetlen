using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>
/// Peter's surfaces (plan.md P7): the home across every project, what the reader
/// owes, and the daily brief — each assembled and filtered on the server.
/// </summary>
public interface IBriefApi
{
    [Get("/api/Brief/Home")]
    Task<IApiResponse<HomeDto>> Home([Query] int days = 7);

    [Get("/api/Brief/Owed")]
    Task<IApiResponse<List<OwedItemDto>>> Owed();

    [Get("/api/Brief/Day")]
    Task<IApiResponse<DailyBriefDto>> Day([Query] string projectId, [Query(Format = "yyyy-MM-dd")] DateTime? day = null, [Query] int days = 1);
}
