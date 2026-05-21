using assetlen.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface IRefundsApi
    {
        [Post("/api/Refunds/CreateNewRefund")]
        Task<IApiResponse<RefundsDto>> CreateNewRefund([Body] RefundsDto r);

        [Get("/api/Refunds/GetRefundBasedOnSaleId")]
        Task<IApiResponse<RefundsDto>> GetRefundBasedOnSaleId([Query] string transactionId);

    }
}
