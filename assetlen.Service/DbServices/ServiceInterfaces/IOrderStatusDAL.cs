using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface IOrderStatusDAL
	{
		Task<ServiceResult<OrderStatusDto>> AddOrderStatusToDB(OrderStatusDto o);
		Task<ServiceResult<bool>> DeleteOrderStatusSoft(string id);
		Task<ServiceResult<List<OrderStatusDto>>> GetAllOrderStatusFromDB();
		Task<ServiceResult<OrderStatusDto>> GetOrderStatusByID(string id);
		Task<ServiceResult<List<OrderStatusDto>>> GetOrderStatusForComboboxes();
		Task<ServiceResult<List<OrderStatusDto>>> SearchOrderStatus(string searchString);
		Task<ServiceResult<OrderStatusDto>> UpdateOrderStatus(string id, OrderStatusDto o);
		Task<ServiceResult<bool>> ReorderOrderStatuses(List<string> orderIds);
		Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchOrderStatusesForComboBoxes(string? keywords, string? statusId, int offSet, int limit, string? sortByColumn, bool sortAscending, CancellationToken cancellationToken);
		Task<ServiceResult<PaginationDetails<OrderStatusDto>>> SearchOrderStatusAsync(string? keywords, string? statusId, int offSet, int limit, string? sortByColumn, bool sortAscending, CancellationToken cancellationToken);
	}
}