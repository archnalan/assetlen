using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface IExpenseDAL
	{
		Task<ServiceResult<ExpenseDto>> CreateExpense(ExpenseDto expenseDto);
		Task<ServiceResult<List<ExpensePerShiftDto>>> SearchExpensePerShiftAndPaymentModeID(string shiftID, int paymentModeID);
	}
}