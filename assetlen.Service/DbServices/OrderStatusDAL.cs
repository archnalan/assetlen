using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
	public class OrderStatusDAL : IOrderStatusDAL
	{
		private readonly mowtDbContext _context;
		private readonly ILogger<OrderStatusDAL> _logger;

		public OrderStatusDAL(mowtDbContext context, ILogger<OrderStatusDAL> logger)
		{
			_context = context;
			_logger = logger;
		}

		#region Method for reading Orderstatus for ComboBoxes' Sake
		public async Task<ServiceResult<List<OrderStatusDto>>> GetOrderStatusForComboboxes()
		{
			try
			{
				var orderStatuses = await _context.tbl_OrderStatuses.OrderBy(c => c.SortOrder).ToListAsync();

				var orderStatusesDto = orderStatuses.Adapt<List<OrderStatusDto>>();

				return ServiceResult<List<OrderStatusDto>>.Success(orderStatusesDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching order statuses for comboboxes: {Error}", ex);
				return ServiceResult<List<OrderStatusDto>>.Failure(
					new ServerErrorException("Could not fetch order statuses."));
			}
		}
		#endregion

		#region Method for reading Orderstatus based on ID
		public async Task<ServiceResult<OrderStatusDto>> GetOrderStatusByID(string id)
		{
			try
			{
				var orderStatus = await _context.tbl_OrderStatuses.FindAsync(id);

				if (orderStatus == null)
				{
					_logger.LogError("Order status with ID: {OrderStatusId} not found.", id);
					return ServiceResult<OrderStatusDto>.Failure(
						new NotFoundException($"Order status with ID {id} does not exist."));
				}

				return ServiceResult<OrderStatusDto>.Success(orderStatus.Adapt<OrderStatusDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching order status with ID {OrderStatusId}: {Error}", id, ex);
				return ServiceResult<OrderStatusDto>.Failure(
					new ServerErrorException("Could not fetch order status."));
			}
		}
		#endregion

		#region Method for reading AllOrderstatus
		public async Task<ServiceResult<List<OrderStatusDto>>> GetAllOrderStatusFromDB()
		{
			try
			{
				var orderStatuses = await _context.tbl_OrderStatuses.OrderBy(c => c.SortOrder).ToListAsync();

				var orderStatusesDto = orderStatuses.Adapt<List<OrderStatusDto>>();

				return ServiceResult<List<OrderStatusDto>>.Success(orderStatusesDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching all order statuses: {Error}", ex);
				return ServiceResult<List<OrderStatusDto>>.Failure(
					new ServerErrorException("Could not fetch order statuses."));
			}
		}
		#endregion

		#region Delete Order Detail softdelete 
		public async Task<ServiceResult<bool>> DeleteOrderStatusSoft(string id)
		{
			var orderStatusInDb = await _context.tbl_OrderStatuses.FirstOrDefaultAsync(x => x.Id == id);

			if (orderStatusInDb == null) return ServiceResult<bool>
					.Failure(new NotFoundException($"Order status with ID: {id} not found."));
			try
			{
				//soft delete
				orderStatusInDb.IsDeleted = true;

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while deleting order status with ID {OrderStatusId}: {Error}", id, ex);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete order status."));
			}
		}
		#endregion

		#region update OrderStatus in DB
		public async Task<ServiceResult<OrderStatusDto>> UpdateOrderStatus(string id, OrderStatusDto o)
		{
			if (o == null) return ServiceResult<OrderStatusDto>.Failure(
				new BadRequestException("Order status data is required"));

			if (o.Id != id) return ServiceResult<OrderStatusDto>.Failure(
				   new BadRequestException($"Order status with ID: {id} is not the same as Order status with ID: {o.Id}"));

			var orderStatusInDb = await _context.tbl_OrderStatuses.FirstOrDefaultAsync(c => c.Id == id);

			if (orderStatusInDb == null) return ServiceResult<OrderStatusDto>.Failure(
			   new NotFoundException($"Order status with ID: {id} not found."));

			try
			{
				orderStatusInDb.OrderName = o.OrderName ?? orderStatusInDb.OrderName;
				orderStatusInDb.SortOrder = o.SortOrder ?? orderStatusInDb.SortOrder;

				_context.tbl_OrderStatuses.Update(orderStatusInDb);
				await _context.SaveChangesAsync();

				var updatedStatusDto = orderStatusInDb.Adapt<OrderStatusDto>();

				return ServiceResult<OrderStatusDto>.Success(updatedStatusDto);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while updating order status with ID {OrderStatusId}: {Error}", id, ex);
				return ServiceResult<OrderStatusDto>.Failure(
					new ServerErrorException("Could not update order status."));
			}
		}
		#endregion

		#region Method for adding OrderStatus to the DB
		public async Task<ServiceResult<OrderStatusDto>> AddOrderStatusToDB(OrderStatusDto o)
		{
			if (o == null) return ServiceResult<OrderStatusDto>.Failure(
				new BadRequestException("Order status data is required"));
			try
			{
				var maxSortOrder = await _context.tbl_OrderStatuses.MaxAsync(c => c.SortOrder);
				o.SortOrder = maxSortOrder + 1;
				var status = o.Adapt<tbl_OrderStatus>();

				await _context.AddAsync(status);

				await _context.SaveChangesAsync();

				return ServiceResult<OrderStatusDto>.Success(status.Adapt<OrderStatusDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while creating order status {OrderName}: {Error}", o.OrderName, ex);
				return ServiceResult<OrderStatusDto>.Failure(
					new ServerErrorException("Could not create order status."));
			}
		}
		#endregion

		#region Search OrderStatus from DB
		public async Task<ServiceResult<List<OrderStatusDto>>> SearchOrderStatus(string searchString)
		{
			try
			{
				var orderStatuses = await _context.tbl_OrderStatuses.AsNoTracking()
					.Where(c => c.OrderName != null && c.OrderName.Contains(searchString))
					.OrderBy(c => c.SortOrder)
					.ToListAsync();

				var orderStatusesDto = orderStatuses.Adapt<List<OrderStatusDto>>();
				return ServiceResult<List<OrderStatusDto>>.Success(orderStatusesDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching order statuses with search string '{SearchString}': {Error}", searchString, ex);
				return ServiceResult<List<OrderStatusDto>>.Failure(
					new ServerErrorException("Could not search order statuses."));
			}
		}
		#endregion

		#region Reorder OrderStatuses in DB
		public async Task<ServiceResult<bool>> ReorderOrderStatuses(List<string> orderIds)
		{
			if (orderIds == null || orderIds.Count == 0)
				return ServiceResult<bool>.Failure(new BadRequestException("Order IDs are required"));

			try
			{
				var orderStatuses = await _context.tbl_OrderStatuses
					.Where(x => orderIds.Contains(x.Id))
					.ToListAsync();

				if (orderStatuses.Count != orderIds.Count)
					return ServiceResult<bool>.Failure(new BadRequestException("Some Order IDs do not exist in the database"));

				for (int i = 0; i < orderIds.Count; i++)
				{
					var orderStatus = orderStatuses.FirstOrDefault(x => x.Id == orderIds[i]);
					if (orderStatus != null)
					{
						orderStatus.SortOrder = i + 1; // Assign sequential SortOrder
					}
				}

				await _context.SaveChangesAsync();
				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while reordering order statuses: {Error}", ex);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not reorder order statuses."));
			}
		}
		#endregion

		#region Search Order status for ComboBoxes
		public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchOrderStatusesForComboBoxes(string? keywords, string? statusId, int offSet, int limit, string? sortByColumn, bool sortAscending, CancellationToken cancellationToken)
		{
			try
			{
				IQueryable<tbl_OrderStatus> query = _context.tbl_OrderStatuses;

				if (!string.IsNullOrEmpty(keywords))
				{
					query = query.Where(c => c.Id.ToString().Contains(keywords) ||
										  (c.OrderName != null && c.OrderName.Contains(keywords)));
				}
				if (!string.IsNullOrEmpty(statusId))
				{
					query = query.Where(c => c.Id == statusId);
				}
				var orderStatuses = await query.AsNoTracking()
											.OrderBy(c => c.SortOrder)
											.Select(x => new ComboBoxDto
											{
												Id = x.Id,
												IdString = x.Id.ToString(),
												ValueText = x.OrderName ?? string.Empty
											})
											.ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

				return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(orderStatuses);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching order statuses for comboboxes with keywords '{Keywords}': {Error}", keywords, ex);
				return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
					new ServerErrorException("Could not search order statuses."));
			}
		}
		#endregion

		#region Search order statuses
		public async Task<ServiceResult<PaginationDetails<OrderStatusDto>>> SearchOrderStatusAsync(string? keywords, string? statusId, int offSet, int limit, string? sortByColumn, bool sortAscending, CancellationToken cancellationToken)
		{
			try
			{
				IQueryable<tbl_OrderStatus> query = _context.tbl_OrderStatuses;
				if (!string.IsNullOrEmpty(keywords))
				{
					query = query.Where(c => c.OrderName != null && c.OrderName.Contains(keywords));
				}
				if (!string.IsNullOrEmpty(statusId))
				{
					query = query.Where(c => c.Id == statusId);
				}
				var orderStatuses = await query.AsNoTracking()
												.OrderBy(c => c.SortOrder)
												.Select(x => new OrderStatusDto
												{
													Id = x.Id.ToString(),
													OrderName = x.OrderName,
													SortOrder = x.SortOrder
												})
												.ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
				return ServiceResult<PaginationDetails<OrderStatusDto>>.Success(orderStatuses);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching order statuses with keywords '{Keywords}': {Error}", keywords, ex);
				return ServiceResult<PaginationDetails<OrderStatusDto>>.Failure(
					new ServerErrorException("Could not search order statuses."));
			}

		}

		#endregion
	}
}
