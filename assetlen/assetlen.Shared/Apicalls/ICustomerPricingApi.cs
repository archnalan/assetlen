using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ICustomerPricingApi
    {
        [Get("/api/CustomerPricing/GetAllCustomerBasedPricingFromDB")]
        Task<IApiResponse<PaginationDetails<PricingsDto>>> GetAllCustomerBasedPricingFromDB([Query] int offset = 0, [Query] int limit = 100, [Query] string sortByColumn = "Id", [Query] bool sortAscending = true, CancellationToken cancellationToken = default);

        [Get("/api/CustomerPricing/GetPricingListByCustomerIdAndProductId")]
        Task<IApiResponse<List<PricingsDto>>> GetPricingListByCustomerIdAndProductId([Query] string prodId, [Query] string custId);

        [Get("/api/CustomerPricing/GetCustomerPricingByPricingId")]
        Task<IApiResponse<PricingsDto>> GetCustomerPricingByPricingId([Query] string id);

        [Post("/api/CustomerPricing/CreateNewCustomerPricing")]
        Task<IApiResponse<PricingsDto>> CreateNewCustomerPricing([Body] PricingsDto pDto);

        [Put("/api/CustomerPricing/UpdateCustomerPricing")]
        Task<IApiResponse<PricingsDto>> UpdateCustomerPricing([Body] PricingsDto c);

        [Delete("/api/CustomerPricing/RemoveCustomerPricingOnProduct")]
        Task<IApiResponse<object>> RemoveCustomerPricingOnProduct([Query] string productId);

        [Delete("/api/CustomerPricing/RemoveCustomerPricingOnCustomer")]
        Task<IApiResponse<object>> RemoveCustomerPricingOnCustomer([Query] string customerId);

        [Delete("/api/CustomerPricing/RemoveCustomerPricingById")]
        Task<IApiResponse<object>> RemoveCustomerPricingById([Query] string id);

        [Get("/api/CustomerPricing/GetCustomerPricingByCustomerId")]
        Task<IApiResponse<List<PricingsDto>>> GetCustomerPricingByCustomerId([Query] string customerId);

        [Get("/api/CustomerPricing/GetCustomerPricingListByProductID")]
        Task<IApiResponse<List<PricingsDto>>> GetCustomerPricingListByProductID([Query] string productId);

        [Get("/api/CustomerPricing/SearchCustomerBasedPricingInDb")]
        Task<IApiResponse<PaginationDetails<PricingsDto>>> SearchCustomerBasedPricingInDb([Query] string keywords, [Query] int offset = 0, [Query] int limit = 100, [Query] string sortByColumn = "Id", [Query] bool sortAscending = true, CancellationToken cancellationToken = default);

        [Post("/api/CustomerPricing/CreateUpdateCustomerPricing")]
        Task<IApiResponse<PricingsDto>> CreateUpdateCustomerPricing([Body] PricingsDto pricingDto);
    }
}
