#if false
// REMOVED: CustomerDepositController was entirely guarded by UserRoles.CustomerManagement which has been removed.
// Uncomment after confirming no functionality is needed here.
using assetlen.Service.DbServices;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using assetlen.Shared.Models.statics;

namespace assetlen.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.CustomerManagement},{UserRoles.CustomerManagement}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class CustomerDepositController : ControllerBase
	{
		private readonly ICustomerDepositDAL _customerDepositDAL;

		public CustomerDepositController(ICustomerDepositDAL customerDepositDAL)
		{
			_customerDepositDAL = customerDepositDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetCustomerCreditSUMLowerThanEndDate(int customerId, DateTime endDate)
		{
			var result = await _customerDepositDAL.GetCustomerCreditSumLowerThanEndDate(customerId, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetCustomerDebitSUMUsingCustomerIDAndEndDate(int customerId, DateTime endDate)
		{
			var result = await _customerDepositDAL.GetCustomerDebitSumUsingCustomerIDAndEndDate(customerId, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(CustomerDepositDto), 200)]
		public async Task<ActionResult> AddCustomerCreditToDB(CustomerDepositDto depositDto)
		{
			var result = await _customerDepositDAL.AddCustomerCreditToDB(depositDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

	}
}
#endif
