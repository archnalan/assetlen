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
	// REMOVED: UserRoles.RecordExpenses was removed; access now restricted to AdminModuleLogin only.
	[Authorize(Roles = $"{UserRoles.AdminModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class ExpensesController : ControllerBase
	{
		private readonly IExpenseDAL _expenseDAL;

		public ExpensesController(IExpenseDAL cashItemsDAL)
		{
			_expenseDAL = cashItemsDAL;
		}

		[HttpPost]
		[ProducesResponseType(typeof(ExpenseDto), 200)]
		public async Task<ActionResult> CreateExpense(ExpenseDto ex)
		{
			var result = await _expenseDAL.CreateExpense(ex);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(ExpenseDto), 200)]
		public async Task<ActionResult> SearchExpensePerShiftAndPaymentModeID(string shiftId, int paymentModeId)
		{
			var result = await _expenseDAL.SearchExpensePerShiftAndPaymentModeID(shiftId, paymentModeId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
	}
}
