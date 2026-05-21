using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ReportingDto;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface IShiftsDAL
	{
		Task<ServiceResult<ShiftsDto>> CheckforOpenShift(string userId);
		Task<ServiceResult<ShiftsDto>> CloseShiftUsingShiftId(ShiftsDto s);
		Task<ServiceResult<ShiftsDto>> CreateNewShift(ShiftsDto s);
		Task<ServiceResult<ShiftsDto>> GetActiveShiftsforUserperUserId(string userId);
		Task<ServiceResult<string>> GetActiveTransactionID(string shiftId);
		Task<ServiceResult<List<ShiftPerformanceDto>>> GetShiftPerformanceReport(DateTime reportDate, string? userId = null);
		Task<ServiceResult<TransactionDto>> GetLastTransactionfromDB(string shiftId);
		Task<ServiceResult<DateTime?>> GetOldestShiftfromDB();
		Task<ServiceResult<List<PaymentModeSummaryDto>>> GetShiftAmountCollectedPerPaymentModeUsingShiftID(string shiftId);
		Task<ServiceResult<List<ShiftAmountCollectedDto>>> GetShiftAmountCollectedPerShift();
		Task<ServiceResult<ShiftsDto>> GetShiftsBasedOnID(string shiftId);
		Task<ServiceResult<PaginationDetails<ShiftsDto>>> GetShiftsFromDB(DateTime startDate, DateTime endDate, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<PaginationDetails<ShiftsDto>>> SearchShifts(DateTime startDate, DateTime endDate, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending, string keywords = "", string UserId = "", bool shiftStatus = false);
		Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchShiftsForComboBoxes(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<ShiftsDto>> UpdateActiveTransactionInShift(string shiftId, string activateId);
		Task<ServiceResult<ShiftsDto>> UpdateShiftsUsingShiftId(ShiftsDto s);
		Task<ServiceResult<bool>> CanUserResumeTransactionFromShift(string userId, string transactionId);
	}
}