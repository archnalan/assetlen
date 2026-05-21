using mowt.Service.DbServices;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ReportingDto;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	// REMOVED: UserRoles.SetManageShifts was removed; access now restricted to LibraryModuleLogin only.
	[Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class ShiftsController : ControllerBase
	{
		private readonly IShiftsDAL _shiftsDAL;
		public ShiftsController(IShiftsDAL shiftsDAL)
		{
			_shiftsDAL = shiftsDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(ShiftsDto), 200)]
		public async Task<ActionResult> CheckforOpenShift(string userId)
		{

			var result = await _shiftsDAL.CheckforOpenShift(userId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(ShiftsDto), 200)]
		public async Task<ActionResult> CloseShiftUsingShiftId([FromBody] ShiftsDto s)
		{

			var result = await _shiftsDAL.CloseShiftUsingShiftId(s);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(ShiftsDto), 200)]
		public async Task<ActionResult> GetActiveTransactionID(string shiftId)
		{

			var result = await _shiftsDAL.GetActiveTransactionID(shiftId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(DateTime?), 200)]
		public async Task<ActionResult> GetOldestShiftfromDB()
		{

			var result = await _shiftsDAL.GetOldestShiftfromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(DateTime?), 200)]
		public async Task<ActionResult> GetLastTransactionfromDB([FromQuery][Required] string shiftId)
		{

			var result = await _shiftsDAL.GetLastTransactionfromDB(shiftId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentModeSummaryDto>), 200)]
		[Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetShiftAmountCollectedPerPaymentModeUsingShiftID(string shiftId)
		{
			var result = await _shiftsDAL.GetShiftAmountCollectedPerPaymentModeUsingShiftID(shiftId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ShiftPerformanceDto>), 200)]
		public async Task<ActionResult> GetShiftPerformanceReport([FromQuery] DateTime reportDate, [FromQuery] string? userId = null)
		{
			var result = await _shiftsDAL.GetShiftPerformanceReport(reportDate, userId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ShiftAmountCollectedDto>), 200)]
		[Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetShiftAmountCollectedPerShift()
		{

			var result = await _shiftsDAL.GetShiftAmountCollectedPerShift();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(ShiftsDto), 200)]
		[Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetShiftsBasedOnID(string shiftId)
		{

			var result = await _shiftsDAL.GetShiftsBasedOnID(shiftId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ShiftsDto>), 200)]

		public async Task<ActionResult> GetShiftsFromDB(DateTime startDate, DateTime endDate, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
		{
			int offset1 = offset ?? 0;
			int limit1 = limit ?? 30;



			var result = await _shiftsDAL.GetShiftsFromDB(startDate, endDate, offset1, limit1, cancellation, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ShiftsDto>), 200)]
		public async Task<ActionResult> SearchShifts(DateTime startDate, DateTime endDate, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true, string keywords = "", string UserId = "", bool shiftStatus = false, [FromQuery] CancellationToken cancellation = default)
		{
			int offset1 = offset ?? 0;
			int limit1 = limit ?? 30;


			var result = await _shiftsDAL.SearchShifts(startDate, endDate, offset1, limit1, cancellation, sortByColumn, sortAscending, keywords, UserId, shiftStatus);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
		public async Task<ActionResult> SearchShiftsForComboBoxes([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true, [FromQuery] CancellationToken cancellation = default)
		{
			int offset1 = offset ?? 0;
			int limit1 = limit ?? 30;
			string keywords1 = keywords ?? "";

			var result = await _shiftsDAL.SearchShiftsForComboBoxes(keywords1, offset1, limit1, cancellation, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(ShiftsDto), 200)]
		public async Task<ActionResult> UpdateActiveTransactionInShift([Required][FromQuery] string shiftId, [Required][FromQuery] string activateSaleId)
		{
			var result = await _shiftsDAL.UpdateActiveTransactionInShift(shiftId, activateSaleId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(ShiftsDto), 200)]
		public async Task<ActionResult> CreateNewShift(ShiftsDto s)
		{

			var result = await _shiftsDAL.CreateNewShift(s);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(bool), 200)]
		public async Task<ActionResult> CanUserResumeTransactionFromShift([Required][FromQuery] string userId, [Required][FromQuery] string transactionId)
		{
			var result = await _shiftsDAL.CanUserResumeTransactionFromShift(userId, transactionId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
	}
}
