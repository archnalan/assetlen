using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using Microsoft.AspNetCore.Authorization;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ICustomersApi
    {

        [Get("/api/Customer/GetCustomersFromDb")]
        Task<IApiResponse<PaginationDetails<CustomerDto>>> GetCustomersFromDb([Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Customer/SearchCustomerByKeywords")]
        Task<IApiResponse<PaginationDetails<CustomerDto>>> SearchCustomersFromDb([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Customer/SearchCustomerByKeywords")]
        Task<IApiResponse<PaginationDetails<CustomerDto>>> SearchCustomerByKeywords([Query] string? keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Customer/SearchCustomerByKeywordsForComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchCustomerByKeywordsForComboBoxes([Query] string? keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Post("/api/Customer/AddCustomer")]
        Task<IApiResponse<CustomerDto>> AddCustomer(CustomerDto customerDto);

        [Put("/api/Customer/UpdateCustomer")]
        Task<IApiResponse<CustomerDto>> UpdateCustomer(CustomerDto customerDto, [Query] string id);
        [Delete("/api/Customer/DeleteCustomerById")]
        Task<IApiResponse<CustomerDto>> DeleteCustomerById([Query] string id);
        [Get("/api/Customer/GetCustomerById")]
        Task<IApiResponse<CustomerDto>> GetCustomerById([Query] string id);

        [Post("/api/Customer/ImportCustomersFromExcel")]
        Task<IApiResponse<ImportResultSummary>> ImportCustomersFromExcel([Body] ImportDataDto p);

        [Post("/api/Customer/GetCustomersForCSVExportBySelectedFields")]
        Task<IApiResponse<HttpContent>> GetCustomersForCSVExportBySelectedFields([Body] List<string> selectedColumnNames);

    }
}
