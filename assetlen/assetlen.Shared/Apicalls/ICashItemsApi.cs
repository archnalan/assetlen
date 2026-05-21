using assetlen.Shared.Models.Models.ViewModels.ReportingDto;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ICashItemsApi
    {
        [Get("/api/CashItems/GetCashItemsFromDB")]
        Task<IApiResponse<List<CashItemsDto>>> GetCashItemsFromDB();
        [Get("/api/CashItems/GetCashItemBasedOnID")]
        Task<ApiResponse<CashItemsDto>> GetCashItemBasedOnID([Query] string id);

        [Post("/api/CashItems/AddCashItem")]
        Task<ApiResponse<CashItemsDto>> AddCashItem([Body] CashItemsDto cashItemDto);

        [Put("/api/CashItems/UpdateCashItem")]
        Task<ApiResponse<CashItemsDto>> UpdateCashItem([Query] string id, [Body] CashItemsDto cashItemDto);

        [Delete("/api/CashItems/DeleteCashItem")]
        Task<ApiResponse<bool>> DeleteCashItem([Query] string id);

    }
}
