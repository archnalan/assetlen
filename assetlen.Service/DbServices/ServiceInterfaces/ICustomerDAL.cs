using assetlen.Service.DataAccess;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface ICustomerDAL
	{
		Task<ServiceResult<CustomerDto>> AddCustomer(CustomerDto customerDto);
		Task<ServiceResult<bool>> DeleteCustomerById(string id);
		Task<ServiceResult<CustomerDto>> GetCustomerById(string id);
		Task<ServiceResult<PaginationDetails<CustomerDto>>> GetCustomersFromDB(int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<PaginationDetails<CustomerDto>>> SearchCustomerByKeywords(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<CustomerDto>> UpdateCustomer(string id, CustomerDto updateCustomer);
		Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchCustomerByKeywordsForComboBoxes(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<int> GetTotalCustomerCount(CancellationToken cancellation);
		Task<ServiceResult<ImportResultSummary>> ImportCustomersFromExcel(ImportDataDto p);
		Task<ServiceResult<MemoryStream>> GetCustomersForCSVExportBySelectedFields(List<string> selectedColumnNames);

	}
}