using mowt.Service.DbServices;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.GenerateReports}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class SlipsController : ControllerBase
	{
		private readonly ISlipsDAL _slipsDAL;

		public SlipsController(ISlipsDAL slipsDAL)
		{
			_slipsDAL = slipsDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(SizeDto), 200)]
		public async Task<ActionResult> GetAllSlipdetailsFromDB()
		{

			var result = await _slipsDAL.GetAllSlipdetailsFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(SizeDto), 200)]
		public async Task<ActionResult> GetSlipdetailsFromDBbasedOnslipID([FromQuery] string sizeId)
		{

			var result = await _slipsDAL.GetSlipdetailsFromDBbasedOnslipID(sizeId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(SizeDto), 200)]
		public async Task<ActionResult> UpdateOrCreateSlipsUsingSlipID([FromBody] SizeDto sizeDto)
		{

			var result = await _slipsDAL.UpdateOrCreateSlipsUsingSlipID(sizeDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

	}
}
