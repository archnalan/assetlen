using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using System.ComponentModel.DataAnnotations;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface ISupplierDAL
	{
		Task<ServiceResult<SupplierDto>> AddSupplierToDB([Required] SupplierDto supplierDto);
		Task<ServiceResult<bool>> deleteSuppierUsingSupplierID(string supplierId);
		Task<ServiceResult<PaginationDetails<SupplierDto>>> GetSUpplierFromDB(int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchSupplierFromDbForComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<string>> GetSupplierIDFromDBbasedOnSuplierName(string supplierName);
		Task<ServiceResult<SupplierDto>> GetSuppliersFromDBbasedOnSuplierID(string supplierId);
		Task<ServiceResult<PaginationDetails<SupplierDto>>> SearchSupplierUsingKeywords(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<SupplierDto>> UpdateSupplierUsingSupplierID(SupplierDto supplierDto);
		Task<ServiceResult<ImportResultSummary>> ImportSuppliersFromExcel(ImportDataDto p);
		Task<ServiceResult<MemoryStream>> GetSuppliersForCSVExportBySelectedFields(List<string> selectedColumnNames);

	}
}