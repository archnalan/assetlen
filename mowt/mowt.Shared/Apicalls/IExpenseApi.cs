using mowt.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface IExpenseApi
    {
        [Get("/api/Expenses/SearchExpensePerShiftAndPaymentModeID")]
        Task<IApiResponse<List<ExpensePerShiftDto>>> SearchExpensePerShiftAndPaymentModeID([Query] string shiftId, [Query] string paymentModeId);

        [Post("/api/Expenses/CreateExpense")]
        Task<IApiResponse<ExpenseDto>> CreateExpense([Body] ExpenseDto expenseDto);

    }
}
