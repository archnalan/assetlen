#if false
// REMOVED: RefundsController was entirely guarded by UserRoles.RefundItem which has been removed.
// Uncomment after confirming no functionality is needed here.
using assetlen.Service.DbServices;
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
	[Authorize(Roles = $"{UserRoles.RefundItem}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class RefundsController : ControllerBase
	{
		private readonly IRefundsDAL _refundsDAL;

		public RefundsController(IRefundsDAL refundsDAL)
		{
			_refundsDAL = refundsDAL;
		}


		[HttpPost]
		[ProducesResponseType(typeof(RefundsDto), 200)]
		public async Task<ActionResult> CreateNewRefund([FromBody] RefundsDto r)
		{
			var result = await _refundsDAL.CreateNewRefund(r);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(RefundsDto), 200)]
		public async Task<ActionResult> GetRefundBasedOnSaleId([FromQuery] string transactionId)
		{
			var result = await _refundsDAL.GetRefundBasedOnSaleId(transactionId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

	}
}
#endif
