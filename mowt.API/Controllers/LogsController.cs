using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class LogsController : ControllerBase
	{
		private readonly ILogsDAL _logsDAL;

		public LogsController(ILogsDAL logsDAL)
		{
			_logsDAL = logsDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<LogDto>), 200)]
		public async Task<ActionResult> GetLogsFromDB(DateTime startDate, DateTime endDate)
		{
			var result = await _logsDAL.GetLogsFromDB(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<LogDto>), 200)]
		public async Task<ActionResult> SearchLogs(DateTime startDate, DateTime endDate, string keywords, int userId, int logTypeId)
		{
			var result = await _logsDAL.SearchLogs(startDate, endDate, keywords, userId, logTypeId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

	}
}
