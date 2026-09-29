using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Mapster;
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
	public class LogsDAL : ILogsDAL
	{
		private readonly AssetlenDbContext _context;
		private readonly ILogger<LogsDAL> _logger;

		public LogsDAL(ILogger<LogsDAL> logger, AssetlenDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		#region Read All Logs from Database
		public async Task<ServiceResult<List<LogDto>>> GetLogsFromDB(DateTime startDate, DateTime endDate)
		{
			try
			{
				var logs = await _context.tbl_Logs
								.Where(c => c.TimeStamp >= startDate && c.TimeStamp <= endDate)
								.OrderByDescending(c => c.Id)
								.ToListAsync();

				var logsDto = logs.Adapt<List<LogDto>>();

				return ServiceResult<List<LogDto>>.Success(logsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching logs in db {error}", ex);
				return ServiceResult<List<LogDto>>.Failure(new ServerErrorException($"Could not fetch logs."));
			}

		}
		#endregion

		#region Search Logs
		public async Task<ServiceResult<List<LogDto>>> SearchLogs(DateTime startDate, DateTime endDate, string keywords, int userId, int logTypeId)
		{
			try
			{
				// Whole days, inclusive — as the old CAST(TimeStamp AS DATE) BETWEEN did.
				var from = startDate.Date;
				var until = endDate.Date.AddDays(1);
				var q = _context.tbl_Logs.Where(c => c.TimeStamp >= from && c.TimeStamp < until);

				if (!string.IsNullOrEmpty(keywords))
				{
					var k = keywords.ToLower();
					q = q.Where(c => c.Id.ToLower().Contains(k)
						|| (c.Message != null && c.Message.ToLower().Contains(k))
						|| (c.SaleId != null && c.SaleId.ToLower().Contains(k))
						|| (c.ShiftId != null && c.ShiftId.ToLower().Contains(k)));
				}
				if (userId > 0)
				{
					var uid = userId.ToString();
					q = q.Where(c => c.UserId == uid);
				}
				if (logTypeId > 0)
				{
					q = q.Where(c => c.LogTypeId == logTypeId);
				}

				var logs = await q.OrderByDescending(c => c.Id).ToListAsync();

				var logsDto = logs.Adapt<List<LogDto>>();

				return ServiceResult<List<LogDto>>.Success(logsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error searching logs in db {error}", ex);
				return ServiceResult<List<LogDto>>.Failure(new ServerErrorException($"Could not search logs."));
			}
		}
		#endregion
	}
}
