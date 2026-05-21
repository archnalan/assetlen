using mowt.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface ICustomerDeposit
    {
        [Post("/api/CustomerDeposit/AddCustomerCreditToDB")]
        Task<IApiResponse<CustomerDepositDto>> AddCustomerCreditToDB([Body] CustomerDepositDto customerDepositDto);

        [Get("/api/CustomerDeposit/GetCustomerCreditSUMLowerThanEndDate")]
        Task<IApiResponse<decimal>> GetCustomerCreditSUMLowerThanEndDate(int customerId, DateTime endDate);

        [Get("/api/CustomerDeposit/GetCustomerDebitSUMUsingCustomerIDAndEndDate")]
        Task<IApiResponse<decimal>> GetCustomerDebitSUMUsingCustomerIDAndEndDate(int customerId, DateTime endDate);
    }
}
