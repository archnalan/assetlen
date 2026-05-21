using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface ItaxDAL
	{
		Task<ServiceResult<taxDto>> CreateNewTax(taxDto taxDto);
		Task<ServiceResult<bool>> DeleteTaxinDBbasedOnTaxID(string taxId);
		Task<ServiceResult<List<taxDto>>> GetAllTaxFromDB();
		Task<ServiceResult<taxDto>> GetTaxFromDBbasedOnTaxID(string taxId);
		Task<ServiceResult<string>> GetTaxIDFromDBbasedOnTaxDescription(string taxDescription);
		Task<ServiceResult<bool>> GetTop1TaxFromProductsDBUsingTaxID(string taxId);
		Task<ServiceResult<bool>> GetTop1TaxFromSalesDBUsingTaxID(string taxId);
		Task<ServiceResult<bool>> HardDeleteTaxinDBbasedOnID(string taxId);
		Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchTaxesForComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
		Task<ServiceResult<List<taxDto>>> SearchTaxFromDB(string searchText);
		Task<ServiceResult<taxDto>> UpdateTaxinDBbasedOnTaxID(taxDto taxDto);
	}
}