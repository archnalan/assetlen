using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls
{

    public interface IFundingApi
    {
        [Post("/api/Funding/AddFundingEntry")]
        Task<IApiResponse<FundingEntryDto>> AddFundingEntry([Body] FundingEntryCreateDto dto);

        [Put("/api/Funding/ConfirmFunding")]
        Task<IApiResponse<FundingEntryDto>> ConfirmFunding([Body] FundingConfirmDto dto);

        [Get("/api/Funding/GetFundingByProject")]
        Task<IApiResponse<List<FundingEntryDto>>> GetFundingByProject([Query] string projectId);

        [Get("/api/Funding/GetFundingByStage")]
        Task<IApiResponse<List<FundingEntryDto>>> GetFundingByStage([Query] string stageId);

        [Get("/api/Funding/GetPendingConfirmations")]
        Task<IApiResponse<List<FundingEntryDto>>> GetPendingConfirmations();

        /// <summary>The funder accepts the figure the delivery side says landed.</summary>
        [Put("/api/Funding/SettleFunding")]
        Task<IApiResponse<FundingEntryDto>> SettleFunding([Body] FundingSettleDto dto);

        /// <summary>Releases stalled on this reader, at either end of the exchange.</summary>
        [Get("/api/Funding/GetFundingNeedingMe")]
        Task<IApiResponse<List<FundingEntryDto>>> GetFundingNeedingMe();
    }

}
