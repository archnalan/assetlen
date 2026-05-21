using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using mowt.Shared.Models.Models.ViewModels.Users;
using Refit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace mowt.Shared.Apicalls
{
    public interface IBanksApi
    {
        [Get("/api/Banks/GetBanksFromDB")]
        Task<IApiResponse<PaginationDetails<BankDto>>> GetBanksFromDB(
            [Query] int offSet = 0,
            [Query] int limit = 12,
            [Query] string? sortByColumn = null,
            [Query] bool sortAscending = false,
            [Query] CancellationToken cancellationToken = default);

        [Get("/api/Banks/SearchBanksFromComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchBanksFromComboBoxes(
            [Query] string keywords = "",
            [Query] int offSet = 0,
            [Query] int limit = 12,
            [Query] string? sortByColumn = null,
            [Query] bool sortAscending = false,
            [Query] CancellationToken cancellationToken = default);

        [Get("/api/Banks/GetBankById")]
        Task<IApiResponse<BankDto>> GetBankById([Query] string id);

        [Get("/api/Banks/GetBankByName")]
        Task<IApiResponse<BankDto>> GetBankByName([Query] string name);

        [Get("/api/Banks/GetBankIDBasedOnBankName")]
        Task<IApiResponse<string>> GetBankIDBasedOnBankName([Query] string bankName);

        [Post("/api/Banks/AddBank")]
        Task<IApiResponse<BankDto>> AddBank([Body] BankDto bank);

        [Put("/api/Banks/UpdateBank")]
        Task<IApiResponse<BankDto>> UpdateBank([Body] BankDto bank);

        [Delete("/api/Banks/DeleteBank")]
        Task<IApiResponse<bool>> DeleteBank([Query] string id);

        [Get("/api/Banks/SearchBanksFromDB")]
        Task<IApiResponse<PaginationDetails<BankDto>>> SearchBanksFromDB(
            [Query] string keywords = "",
            [Query] int offSet = 0,
            [Query] int limit = 12,
            [Query] string? sortByColumn = null,
            [Query] bool sortAscending = false,
            [Query] CancellationToken cancellationToken = default);

        [Post("/api/Banks/GetBanksForCSVExportBySelectedFields")]
        Task<IApiResponse<HttpContent>> GetBanksForCSVExportBySelectedFields([Body] List<string> selectedColumnNames);

        [Post("/api/Banks/ImportBanksFromExcel")]
        Task<IApiResponse<ImportResultSummary>> ImportBanksFromExcel([Body] ImportDataDto p);
    }
}