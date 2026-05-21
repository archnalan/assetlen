using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface ITransactionsApi
    {
        [Get("/api/Transactions/GetCompletedTransactionsFromDB")]
        Task<IApiResponse<PaginationDetails<TransactionDto>>> GetCompletedTransactionsFromDB([Query] int? offset, [Query] int? limit, [Query] string sortByColumn = null, [Query] bool sortAscending = true, [Query] CancellationToken cancellation = default);

        [Get("/api/Transactions/SearchTransactionsFromDB")]
        Task<IApiResponse<PaginationDetails<TransactionDto>>> SearchTransactionsFromDB([Query] string? keywords, [Query] string? userId, [Query] string? shiftId, [Query] string? customerId, [Query] string? orderStatus, [Query] bool? completed, [Query] int? saleStatus, [Query] DateTime? startDate, [Query] DateTime? endDate, [Query] int? offset, [Query] int? limit, [Query] string sortByColumn = null, [Query] bool sortAscending = true, [Query] CancellationToken cancellation = default);

        [Get("/api/Transactions/GetCompletedTransactionFromDBUsingID")]
        Task<IApiResponse<TransactionDto>> GetCompletedTransactionFromDBUsingID([Query] string saleId);

        [Get("/api/Transactions/GetTransactionWithDetailsFromDB")]
        Task<IApiResponse<TransactionDto>> GetTransactionWithDetailsFromDB([Query] string id);

        [Get("/api/Transactions/GetTransactionById")]
        Task<IApiResponse<TransactionDto>> GetTransactionById([Query] string id);

        [Put("/api/Transactions/UpdateTransactionUsingTransactionID")]
        Task<IApiResponse<TransactionDto>> UpdateTransactionUsingTransactionID([Query] string id, [Body] TransactionDto transact);

        [Put("/api/Transactions/UpdateTransactionOrderStatusAndComment")]
        Task<IApiResponse<TransactionDto>> UpdateTransactionOrderStatusAndComment([Body] TransactionStatusUpdateDto transact);

        [Put("/api/Transactions/UpdateTransactionStatusAndCreateNewTransaction")]
        Task<IApiResponse<TransactionDto>> UpdateTransactionStatusAndCreateNewTransaction([Body] TransactionStatusUpdateDto transact);

        [Put("/api/Transactions/AddCustomerToTransaction")]
        Task<IApiResponse<TransactionDto>> AddCustomerToTransaction([Query] string transactionId, [Query] string customerId);

        [Post("/api/Transactions/GetTransactionsForCSVExportBySelectedFields")]
        Task<IApiResponse<HttpContent>> GetTransactionsForCSVExportBySelectedFields([Body] List<string> selectedColumnNames);

        [Post("/api/Transactions/ImportTransactionsFromExcel")]
        Task<IApiResponse<ImportResultSummary>> ImportTransactionsFromExcel([Body] ImportDataDto p);

        [Get("/api/Transactions/GetTransactionsFromDB")]
        Task<IApiResponse<PaginationDetails<TransactionDto>>> GetTransactionsFromDB([Query] int? offSet, [Query] int? limit, [Query] bool? completed, [Query] int? saleStatus, [Query] string sortByColumn = null, [Query] bool sortAscending = true, [Query] CancellationToken cancellation = default);

    }
}
