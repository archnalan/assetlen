using assetlen.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface IPaymentsApi
    {
        [Get("/api/Payments/GetPaymentsBasedOnSaleID")]
        Task<IApiResponse<List<PaymentsDto>>> GetPaymentsBasedOnSaleID([Query] string saleId);

        [Get("/api/Payments/GetSumOfPaymentsBasedOnSaleID")]
        Task<IApiResponse<List<PaymentsDto>>> GetSumOfPaymentsBasedOnSaleID([Query] string saleId);

        [Get("/api/Payments/GetAllPaymentModes")]
        Task<IApiResponse<List<PaymentModeDto>>> GetAllPaymentModes();
        [Post("/api/Payments/AddPaymentsAndCloseSale")]
        Task<IApiResponse<TransactionDto>> AddPaymentsAndCloseSale(List<PaymentsDto> payments);
    }
}
