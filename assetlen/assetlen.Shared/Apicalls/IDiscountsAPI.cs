using assetlen.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface IDiscountsAPI
    {
        [Get("/api/Discounts/GetDiscountsFromDB")]
        Task<IApiResponse<PaginationDetails<DiscountDto>>> GetDiscountsFromDB([Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Discounts/GetDiscountsFromComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> GetDiscountsFromComboBoxes([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Discounts/SearchDiscountsFromComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchDiscountsFromComboBoxes([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default, [Query] bool? isActive = false);

        [Post("/api/Discounts/GetDiscountByID")]
        Task<IApiResponse<DiscountDto>> GetDiscountByID([Query] int id);

        [Post("/api/Discounts/GetDiscountByValue")]
        Task<IApiResponse<DiscountDto>> GetDiscountIDByValue(string name);

        [Post("/api/Discounts/AddDiscount")]
        Task<IApiResponse<DiscountDto>> AddDiscount([Body] DiscountCreateDto discount);

        [Put("/api/Discounts/UpdateDiscount")]
        Task<IApiResponse<DiscountDto>> UpdateDiscount(int id, DiscountDto discount);

        [Delete("/api/Discounts/DeleteDiscount")]
        Task<IApiResponse<DiscountDto>> DeleteDiscount([Query] int id);
    }
}
