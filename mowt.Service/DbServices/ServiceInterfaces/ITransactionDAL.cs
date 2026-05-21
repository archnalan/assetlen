using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface ITransactionDAL
	{
		Task<ServiceResult<TransactionDto>> CreateNewTransaction(TransactionDto transactDto);
		Task<ServiceResult<TransactionDto>> GetTransactionWithDetailsFromDB(string id);
		Task<ServiceResult<PaginationDetails<TransactionDto>>> GetTransactionsFromDB(int offSet, int limit, bool? completed, int? saleStatus, string sortByColumn, bool sortAscending, CancellationToken cancellation);
		Task<ServiceResult<PaginationDetails<TransactionDto>>> SearchTransactionsFromDB(string keywords, string userId, string shiftId, string customerId, string orderStatus, bool? Completed, int? saleStatus, DateTime startDate, DateTime endDate, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellation);
		Task<ServiceResult<PaginationDetails<TransactionDto>>> GetCompletedTransactionsFromDBUsingDateRange(DateTime startDate, DateTime endDate, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);

		Task<ServiceResult<TransactionDto>> GetCompletedTransactionFromDBUsingID(string saleId);
		Task<ServiceResult<decimal>> GetCustomerDebitsLowerThanEndDate(TransactionDto t, DateTime EndDate);
		Task<ServiceResult<List<TransPendingDto>>> GetLastTransactionIDFromDB(string userID);
		Task<ServiceResult<List<TransPendingDto>>> GetPendingTransactionFromDBUsingUserID(string UserID);
		Task<ServiceResult<decimal>> GetSumOfRevenueFromDForToday();
		Task<ServiceResult<decimal>> GetTotalRevenueFromDBUsingDateRange(DateTime startDate, DateTime endDate);
		Task<ServiceResult<int>> GetTotalSalesNoFromDBUsingDateRange(DateTime startDate, DateTime endDate);
		Task<ServiceResult<TransactionDto>> GetTransactionFromDB(string id);
		Task<ServiceResult<bool>> RefundTransaction(string id);
		Task<ServiceResult<List<TransPendingDto>>> SearchPendingTransactions(string keywords, string UserID, int transactionStatus, int OrderStatus);
		Task<ServiceResult<TransactionDto>> UpdateTransactionUsingTransactionID(string id, TransactionDto tDto);
		Task<ServiceResult<ImportResultSummary>> ImportTransactionsFromExcel(ImportDataDto p);
		Task<ServiceResult<MemoryStream>> GetTransactionsForCSVExportBySelectedFields(List<string> selectedColumnNames);
		Task<ServiceResult<TransactionDto>> UpdateTransactionOrderStatusAndComment(TransactionStatusUpdateDto tDto);
		Task<ServiceResult<TransactionDto>> UpdateTransactionStatusAndCreateNewTransaction(TransactionStatusUpdateDto tDto);
		Task<ServiceResult<TransactionDto>> AddCustomerToTransaction(string transactionId, string customerId);
	}
}