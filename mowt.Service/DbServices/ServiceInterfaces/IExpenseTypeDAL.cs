using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IExpenseTypeDAL
    {
        Task<ServiceResult<List<ComboBoxDto>>> GetExpensesForComboBox();
        Task<ServiceResult<List<ExpenseTypeDto>>> GetExpenseTypes();
        Task<ServiceResult<ExpenseTypeDto>> GetExpenseType(string typeId);
        Task<ServiceResult<ExpenseTypeDto>> AddExpenseType(ExpenseTypeDto expenseTypeDto);
        Task<ServiceResult<List<ExpenseTypeDto>>> AddMultipleExpenseTypes([Required] List<ExpenseTypeDto> expenseTypeDtoList);
        Task<ServiceResult<ExpenseTypeDto>> UpdateExpenseType(ExpenseTypeDto expenseTypeDto);
        Task<ServiceResult<bool>> DeleteExpenseType(string typeId);
        Task<ServiceResult<List<ComboBoxDto>>> SearchExpenseTypesForComboBoxes(string? searchText = "");
    }
}
