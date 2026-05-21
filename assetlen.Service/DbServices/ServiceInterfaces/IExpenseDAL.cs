using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface IExpenseDAL
	{
		Task<ServiceResult<ExpenseDto>> CreateExpense(ExpenseDto expenseDto);
		Task<ServiceResult<List<ExpensePerShiftDto>>> SearchExpensePerShiftAndPaymentModeID(string shiftID, int paymentModeID);
	}
}