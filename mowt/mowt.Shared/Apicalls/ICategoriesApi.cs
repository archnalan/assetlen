using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using mowt.Shared.Models.Models.ViewModels.Users;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface ICategoriesAPI
    {

        [Get("/api/Categories/GetCategoriesFromDB")]
        Task<IApiResponse<PaginationDetails<CategoryDto>>> GetCategoriesFromDB([Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Categories/SearchCategoriesFromComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchCategoriesFromComboBoxes([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Categories/GetCategoryBasedOnID")]
        Task<IApiResponse<CategoryDto>> GetCategoryBasedOnID([Query] string id);

        [Get("/api/Categories/GetCategoryIDBasedCategoryName")]
        Task<IApiResponse<CategoryDto>> GetCategoryIDBasedCategoryName(string name);

        [Post("/api/Categories/AddCategory")]
        Task<IApiResponse<CategoryDto>> AddCategory([Body] CategoryDto cashItemDto);

        [Put("/api/Categories/UpdateCategory")]
        Task<IApiResponse<CategoryDto>> UpdateCategory(string id, CategoryDto c);

        [Delete("/api/Categories/DeleteCategory")]
        Task<IApiResponse<CategoryDto>> DeleteCategory([Query] string id);

        [Get("/api/Categories/SearchCategoriesFromDB")]
        Task<IApiResponse<PaginationDetails<CategoryDto>>> SearchCategoriesFromDB([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Post("/api/Categories/GetCategoriesForCSVExportBySelectedFields")]
        Task<IApiResponse<HttpContent>> GetCategoriesForCSVExportBySelectedFields([Body] List<string> selectedColumnNames);

        [Post("/api/Categories/ImportCategoriesFromExcel")]
        Task<IApiResponse<ImportResultSummary>> ImportCategoriesFromExcel([Body] ImportDataDto p);

        [Get("/api/Categories/GetTopCategories")]
        Task<IApiResponse<List<CategoryDto>>> GetTopCategories([Query] int? limit = 10, [Query] CancellationToken cancellationToken = default);

    }
}
