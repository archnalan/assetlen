using mowt.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface IExpenseTypeApi
    {
        [Get("/api/ExpenseType/GetExpenseTypes")]
        Task<IApiResponse<List<ExpenseTypeDto>>> GetExpenseTypes();
        [Get("/api/ExpenseType/GetExpensesForComboBox")]
        Task<IApiResponse<List<ComboBoxDto>>> GetExpensesForComboBox();

        [Get("/api/ExpenseType/GetExpenseType")]
        Task<IApiResponse<ExpenseTypeDto>> GetExpenseType(int typeId);

        [Post("/api/ExpenseType/AddExpenseType")]
        Task<IApiResponse<ExpenseTypeDto>> AddExpenseType([Body] ExpenseTypeDto expenseTypeDto);

        [Post("/api/ExpenseType/AddMultipleExpenseTypes")]
        Task<IApiResponse<List<ExpenseTypeDto>>> AddMultipleExpenseTypes([Body] List<ExpenseTypeDto> expenseTypeDtos);

        [Put("/api/ExpenseType/UpdateExpenseType")]
        Task<IApiResponse<ExpenseTypeDto>> UpdateExpenseType([Body] ExpenseTypeDto expenseTypeDto);

        [Delete("/api/ExpenseType/DeleteExpenseType")]
        Task<IApiResponse<bool>> DeleteExpenseType(int typeId);

        [Get("/api/ExpenseType/SearchExpenseTypesForComboBoxes")]
        Task<IApiResponse<List<ComboBoxDto>>> SearchExpenseTypesForComboBoxes(string? searchText = "");
    }
}
