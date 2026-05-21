using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Refit;

namespace assetlen.Shared.Apicalls
{
    public interface IOrderStatusesApi
    {
        [Get("/api/OrderStatuses/GetOrderStatusForComboboxes")]
        Task<IApiResponse<List<OrderStatusDto>>> GetOrderStatusForComboboxes();

        [Get("/api/OrderStatuses/GetOrderStatusBasedOnID")]
        Task<IApiResponse<OrderStatusDto>> GetOrderStatusBasedOnID(string id);

        [Get("/api/OrderStatuses/GetAllOrderStatusFromDB")]
        Task<IApiResponse<List<OrderStatusDto>>> GetAllOrderStatusFromDB();

        [Delete("/api/OrderStatuses/DeleteOrderStatusSoft")]
        Task<IApiResponse<bool>> DeleteOrderStatusSoft([Query] string id);

        [Post("/api/OrderStatuses/AddOrderStatusToDB")]
        Task<IApiResponse<OrderStatusDto>> AddOrderStatusToDB([Body] OrderStatusDto statusDto);

        [Put("/api/OrderStatuses/UpdateOrderStatus")]
        Task<IApiResponse<OrderStatusDto>> UpdateOrderStatus([Body] OrderStatusDto statusDto);

        [Get("/api/OrderStatuses/SearchOrderStatus")]
        Task<IApiResponse<List<OrderStatusDto>>> SearchOrderStatus([Query] string searchString);

        [Put("/api/OrderStatuses/ReorderOrderStatuses")]
        Task<IApiResponse<bool>> ReorderOrderStatuses([Query(CollectionFormat.Multi)] List<string> orderIds);

        [Get("/api/OrderStatuses/SearchOrderStatusesForComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchOrderStatusesForComboBoxes(
            [Query] string? keywords,
            [Query] string? statusId,
            [Query] int offSet,
            [Query] int limit,
            [Query] string sortByColumn,
            [Query] bool sortAscending,
            CancellationToken cancellationToken);

        [Get("/api/OrderStatuses/SearchOrderStatuses")]
        Task<IApiResponse<PaginationDetails<OrderStatusDto>>> SearchOrderStatuses(
            [Query] string? keywords,
            [Query] string? statusId,
            [Query] int offSet,
            [Query] int limit,
            [Query] string? sortByColumn,
            [Query] bool sortAscendin,
            CancellationToken cancellationToken);

    }
}
