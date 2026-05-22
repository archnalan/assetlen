using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace assetlen.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class ReceiptsController : ControllerBase
	{
		private readonly IReceiptsDAL _receiptsDAL;

		public ReceiptsController(IReceiptsDAL receiptsDAL)
		{
			_receiptsDAL = receiptsDAL;
		}



		[HttpGet]
		[ProducesResponseType(typeof(List<ReceiptDto>), 200)]
		[Authorize(Roles = $"{UserRoles.Crew}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetReceiptItemsFromDBbasedOnSlipID(int slipId)
		{
			var result = await _receiptsDAL.GetReceiptItemsFromDBbasedOnSlipID(slipId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[Authorize(Roles = $"{UserRoles.Contractor}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[ProducesResponseType(typeof(ReceiptDto), 200)]
		public async Task<ActionResult> CreateOrSyncNewReceiptItems([FromBody] List<ReceiptItemDto> rtDto)
		{
			var result = await _receiptsDAL.CreateOrSyncNewReceiptItems(rtDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}


	}
}
