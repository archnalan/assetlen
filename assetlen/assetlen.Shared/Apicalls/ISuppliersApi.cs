using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ISuppliersApi
    {

        [Get("/api/Suppliers/GetSUpplierFromDB")]
        Task<IApiResponse<PaginationDetails<SupplierDto>>> GetSUpplierFromDB([Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Suppliers/SearchSupplierUsingKeywords")]
        Task<IApiResponse<PaginationDetails<SupplierDto>>> SearchSupplierUsingKeywords([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Suppliers/SearchSupplierFromDbForComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchSupplierFromDbForComboBoxes([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Suppliers/GetSuppliersFromDbBasedOnSupplierID")]
        Task<IApiResponse<SupplierDto>> GetSuppliersFromDbBasedOnSupplierID([Query] string supplierId);

        [Post("/api/Suppliers/AddSupplierToDB")]
        Task<IApiResponse<SupplierDto>> AddSupplierToDB(SupplierDto SupplierDto);

        [Put("/api/Suppliers/UpdateSupplierUsingSupplierID")]
        Task<IApiResponse<SupplierDto>> UpdateSupplierUsingSupplierID(SupplierDto SupplierDto);

        [Delete("/api/Suppliers/deleteSuppierUsingSupplierID")]
        Task<IApiResponse<SupplierDto>> deleteSuppierUsingSupplierID([Query] string id);

        [Post("/api/Suppliers/GetSuppliersForCSVExportBySelectedFields")]
        Task<IApiResponse<HttpContent>> GetSuppliersForCSVExportBySelectedFields([Body] List<string> selectedColumnNames);

        [Post("/api/Suppliers/ImportSuppliersFromExcel")]
        Task<IApiResponse<ImportResultSummary>> ImportSuppliersFromExcel([Body] ImportDataDto p);

    }
}
