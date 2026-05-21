using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls
{
    public interface IStagesApi
    {
        [Post("/api/Stages/CreateStage")]
        Task<IApiResponse<StageDto>> CreateStage([Query] string projectId, [Body] StageCreateDto dto);

        [Put("/api/Stages/UpdateStage")]
        Task<IApiResponse<StageDto>> UpdateStage([Body] StageDto dto);

        [Delete("/api/Stages/DeleteStage")]
        Task<IApiResponse<bool>> DeleteStage([Query] string stageId);

        [Get("/api/Stages/GetStagesByProjectId")]
        Task<IApiResponse<List<StageDto>>> GetStagesByProjectId([Query] string projectId);

        [Get("/api/Stages/GetStageById")]
        Task<IApiResponse<StageDto>> GetStageById([Query] string stageId);
    }
}
