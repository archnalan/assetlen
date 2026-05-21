using mowt.ServiceHandler;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface IProductReceivingDAL
	{
		Task<ServiceResult<ProductReceivingDto>> AddOProductReceivingDetailToDB(ProductReceivingDto p);
		Task<ServiceResult<List<ProductReceivingDto>>> ReceiveMultipleProducts(List<ProductReceivingDto> p, List<StockParam> stockParamList, List<CostPriceChange>? costChanges);
		Task<ServiceResult<List<ProductReceivingDto>>> GetProductReceivingDetailFromDBPerGRNnumber(string GRNSupplierNumber);
		Task<ServiceResult<List<ProductReceivingDto>>> GetProductsReceivedFromDBUsingDateRange(DateTime startDate, DateTime endDate);
		Task<ServiceResult<List<ProductReceivingDto>>> GetProductsReceivedFromDBUsingDateRangeAndGRNSupplierNumber(DateTime startDate, DateTime endDate, string GRNSupplierNumber);
		Task<ServiceResult<PaginationDetails<ProductReceivingDto>>> SearchProductReceivingDetailFromDB(string? receiveStockId, string? supplierAccount, string? keywords, string? barCode, int offset, int limit, CancellationToken token);
	}
}