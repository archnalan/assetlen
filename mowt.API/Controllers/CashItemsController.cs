using mowt.Service.DataAccess;
using mowt.Service.DbServices;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using mowt.Shared.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.AdminModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class CashItemsController : ControllerBase
	{
		private readonly ICashItemsDAL _cashItemsDAL;

		public CashItemsController(ICashItemsDAL cashItemsDAL)
		{
			_cashItemsDAL = cashItemsDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<CashItemsDto>), 200)]
		[Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetCashItemsFromDB()
		{
			var result = await _cashItemsDAL.GetCashItemsFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(CashItemsDto), 200)]
		[Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetCashItemBasedOnID([FromQuery] string id)
		{
			var result = await _cashItemsDAL.GetCashItemBasedOnID(id);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(CashItemsDto), 200)]
		public async Task<ActionResult> AddCashItem([FromBody] CashItemsDto cashItemDto)
		{
			var result = await _cashItemsDAL.AddCashItem(cashItemDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(CashItemsDto), 200)]
		public async Task<ActionResult> UpdateCashItem([FromQuery] string id, [FromBody] CashItemsDto c)
		{
			var result = await _cashItemsDAL.UpdateCashItem(id, c);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		public async Task<ActionResult> DeleteCashItem([FromQuery] string id)
		{
			var result = await _cashItemsDAL.DeleteCashItem(id);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

	}
}
