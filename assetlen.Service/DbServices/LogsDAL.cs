using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Mapster;
using Microsoft.Data.SqlClient;
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
				string sql = "SELECT * FROM tbl_Logs WHERE CAST(TimeStamp AS DATE) BETWEEN @startDate AND @endDate";

				if (!string.IsNullOrEmpty(keywords))
				{
					sql += " AND (Id LIKE @keywords OR Message LIKE @keywords OR SaleId LIKE @keywords OR ShiftId LIKE @keywords)";
				}
				if (userId > 0)
				{
					sql += " AND UserId = @userId";
				}
				if (logTypeId > 0)
				{
					sql += " AND LogTypeId = @logTypeId";
				}
				sql += " ORDER BY Id DESC";

				var parameters = new[]
				{
					new SqlParameter("@startDate", startDate.ToString("yyyyMMdd")),
					new SqlParameter("@endDate", endDate.ToString("yyyyMMdd")),
					new SqlParameter("@keywords", $"%{keywords}%"),
					new SqlParameter("@userId", userId),
					new SqlParameter("@logTypeId", logTypeId)
				};

				var logs = await _context.tbl_Logs
									.FromSqlRaw(sql, parameters)
									.ToListAsync();

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
