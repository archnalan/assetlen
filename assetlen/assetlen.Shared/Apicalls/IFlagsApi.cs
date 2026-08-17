using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

public interface IFlagsApi
{
    [Post("/api/Flags/AddFlag")]
    Task<IApiResponse<FlagDto>> AddFlag([Body] FlagCreateDto dto);

    [Get("/api/Flags/GetFlag")]
    Task<IApiResponse<FlagDto>> GetFlag([Query] string flagId);

    [Get("/api/Flags/GetFlagsByProject")]
    Task<IApiResponse<List<FlagDto>>> GetFlagsByProject(
        [Query] string projectId,
        [Query] FlagStatus? status = null);

    [Get("/api/Flags/GetFlagsByEntry")]
    Task<IApiResponse<List<FlagDto>>> GetFlagsByEntry([Query] string progressUpdateId);

    [Put("/api/Flags/UpdateFlag")]
    Task<IApiResponse<FlagDto>> UpdateFlag([Body] FlagUpdateDto dto);

    [Put("/api/Flags/ResolveFlag")]
    Task<IApiResponse<FlagDto>> ResolveFlag([Query] string flagId);

    /// <summary>Close every open question on a project at once. Returns how many were closed.</summary>
    [Put("/api/Flags/ResolveProjectFlags")]
    Task<IApiResponse<int>> ResolveProjectFlags([Query] string projectId);

    [Put("/api/Flags/NudgeFlag")]
    Task<IApiResponse<FlagDto>> NudgeFlag([Query] string flagId);
}
