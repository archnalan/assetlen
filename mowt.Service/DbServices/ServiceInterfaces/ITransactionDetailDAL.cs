using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface ITransactionDetailDAL
	{
		Task<ServiceResult<List<TransactionDetailDto>>> CreateOrSyncNewTransactionDetails(List<TransactionDetailDto> tdDto);
		Task<ServiceResult<bool>> DeleteTransactionDetailBasedOnTransactionID(string transId);
		Task<ServiceResult<bool>> DeleteTransactionDetailPerDetailID(string detailID);
		Task<ServiceResult<TransactionDetailDto>> GetTransactionDetailBasedOnDetailID(string detailID);
		Task<ServiceResult<TransactionDetailDto>> GetTransactionDetailWithRelatedDataFromDB(string detailId);
		Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionID(string transId, bool? completed = null, string? statusOrder = null, int? saleStatus = null);
		Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionIDandProdID(string transId, string prodId);
		Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionIDandSortID(string transId, int sortOrder);
		Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionIDandSpecialPricing(string transId, bool spPricing);
		Task<ServiceResult<decimal>> GetTransactionTotalExc(string transId);
		Task<ServiceResult<decimal>> GetTransactionTotalInc(string transId);
		Task<ServiceResult<TransactionDetailDto>> UpdateTransactionDetail(TransactionDetailDto tdDto);
		Task<ServiceResult<MemoryStream>> GetTransactionDetailsForCSVExportBySelectedFields(List<string> selectedColumnNames);
		Task<ServiceResult<ImportResultSummary>> ImportTransactionDetailsFromExcel(ImportDataDto p);
	}
}