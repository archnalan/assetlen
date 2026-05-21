using mowt.Service.DataAccess;
using mowt.Service.DbServices;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	//[Authorize(Roles = $"{UserRoles.SetSystemConfig}",
	//	   AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class OrderStatusesController : ControllerBase
	{
		private readonly IOrderStatusDAL _orderStatusesDAL;

		public OrderStatusesController(IOrderStatusDAL orderStatusDAL)
		{
			_orderStatusesDAL = orderStatusDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<OrderStatusDto>), 200)]
		public async Task<ActionResult> GetOrderStatusForComboboxes()
		{
			var result = await _orderStatusesDAL.GetOrderStatusForComboboxes();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(List<OrderStatusDto>), 200)]
		public async Task<ActionResult> GetOrderStatusBasedOnID(string id)
		{
			var result = await _orderStatusesDAL.GetOrderStatusByID(id);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<OrderStatusDto>), 200)]
		public async Task<ActionResult> GetAllOrderStatusFromDB()
		{
			var result = await _orderStatusesDAL.GetAllOrderStatusFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		public async Task<ActionResult> DeleteOrderStatusSoft([FromQuery] string id)
		{
			var result = await _orderStatusesDAL.DeleteOrderStatusSoft(id);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(OrderStatusDto), 200)]
		public async Task<ActionResult> AddOrderStatusToDB([FromBody] OrderStatusDto statusDto)
		{

			var result = await _orderStatusesDAL.AddOrderStatusToDB(statusDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(OrderStatusDto), 200)]
		public async Task<ActionResult> UpdateOrderStatus([FromBody] OrderStatusDto statusDto)
		{
			var result = await _orderStatusesDAL.UpdateOrderStatus(statusDto.Id, statusDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<OrderStatusDto>), 200)]
		public async Task<ActionResult> SearchOrderStatus([FromQuery] string searchString)
		{
			var result = await _orderStatusesDAL.SearchOrderStatus(searchString);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);
			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(bool), 200)]
		public async Task<ActionResult> ReorderOrderStatuses([FromQuery] List<string> orderIds)
		{
			var result = await _orderStatusesDAL.ReorderOrderStatuses(orderIds);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
		public async Task<ActionResult> SearchOrderStatusesForComboBoxes([FromQuery] string? keywords = "", [FromQuery] string? statusId = "", [FromQuery] int offSet = 0, [FromQuery] int limit = 12, [FromQuery] string? sortByColumn = "", [FromQuery] bool sortAscending = true, CancellationToken cancellationToken = default)
		{
			var result = await _orderStatusesDAL.SearchOrderStatusesForComboBoxes(keywords, statusId, offSet, limit, sortByColumn, sortAscending, cancellationToken);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<OrderStatusDto>), 200)]
		public async Task<ActionResult> SearchOrderStatuses([FromQuery] string? keywords = "", [FromQuery] string? statusId = "", [FromQuery] int offSet = 0, [FromQuery] int limit = 12, [FromQuery] string? sortByColumn = "", [FromQuery] bool sortAscending = true, CancellationToken cancellationToken = default)
		{
			var result = await _orderStatusesDAL.SearchOrderStatusAsync(keywords, statusId, offSet, limit, sortByColumn, sortAscending, cancellationToken);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

	}
}
