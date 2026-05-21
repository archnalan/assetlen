using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface ICustomerPricingDAL
	{
		Task<ServiceResult<PricingsDto>> AddCustomerPricing(PricingsDto cupDto);
		Task<ServiceResult<bool>> DeleteCustomerPricingListByProductId(string productId);
		Task<ServiceResult<PricingsDto>> GetCustomerPricingByID(string id);
		Task<ServiceResult<List<PricingsDto>>> GetPricingListByCustomerIdAndProductId(string customerId, string productId);
		Task<ServiceResult<List<PricingsDto>>> GetCustomerPricingListByCustomerID(string customerId);
		Task<ServiceResult<List<PricingsDto>>> GetCustomerPricingListByProductID(string productId);
		Task<ServiceResult<PricingsDto>> UpdateCustomerPricing(PricingsDto cu);
		Task<ServiceResult<bool>> DeleteCustomerPricingById(string id);
		Task<ServiceResult<bool>> DeleteCustomerPricingListByCustomerId(string customerId);
		Task<ServiceResult<PaginationDetails<PricingsDto>>> GetAllCustomerBasedPricingFromDB(int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<PaginationDetails<PricingsDto>>> SearchCustomerBasedPricingInDb(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
		Task<ServiceResult<PricingsDto>> CreateUpdateCustomerPricing(PricingsDto pricingDto);
	}
}