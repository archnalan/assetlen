using assetlen.Service.DataAccess;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface ICategoryDAL
	{
		Task<ServiceResult<CategoryDto>> AddCategory(CategoryDto c);
		Task<ServiceResult<bool>> DeleteCategoryById(string id);
		Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchCategoriesFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);

		Task<ServiceResult<PaginationDetails<CategoryDto>>> GetCategoriesFromDB(int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<CategoryDto>> GetCategoryById(string id);
		Task<ServiceResult<CategoryDto>> GetCategoryByName(string name);
		Task<ServiceResult<string>> GetCategoryIDBasedOnCategoryName(string categoryName);
		Task<ServiceResult<CategoryDto>> UpdateCategory(string id, CategoryDto cDto);
		Task<ServiceResult<PaginationDetails<CategoryDto>>> SearchCategoriesFromDB(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<MemoryStream>> GetCategoriesForCSVExportBySelectedFields(List<string> selectedColumnNames);
		Task<ServiceResult<ImportResultSummary>> ImportCategoriesFromExcel(ImportDataDto p);
		Task<ServiceResult<List<CategoryDto>>> GetTopCategories(int limit, CancellationToken cancellationToken);

	}
}