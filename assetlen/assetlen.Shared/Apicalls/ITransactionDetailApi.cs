using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.ReportingDto;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ITransactionDetailApi
    {

        [Post("/api/TransactionDetails/CreateOrSyncNewTransactionDetails")]
        Task<IApiResponse<List<TransactionDetailDto>>> CreateOrSyncNewTransactionDetails([Body] List<TransactionDetailDto> tdDto);

        [Get("/api/TransactionDetails/GetTransactionDetailBasedOnTransactionID")]
        Task<IApiResponse<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionID(
            [Query] string transId,
            [Query] bool? completed = null,
            [Query] string? statusOrder = null,
            [Query] int? saleStatus = null);

        [Post("/api/TransactionDetails/GetTransactionDetailsForCSVExportBySelectedFields")]
        Task<IApiResponse<HttpContent>> GetTransactionDetailsForCSVExportBySelectedFields([Body] List<string> selectedColumnNames);

        [Post("/api/TransactionDetails/ImportTransactionDetailsFromExcel")]
        Task<IApiResponse<ImportResultSummary>> ImportTransactionDetailsFromExcel([Body] ImportDataDto p);

    }
}
