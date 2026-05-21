using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels.Users;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface ILogsDAL
	{
		Task<ServiceResult<List<LogDto>>> GetLogsFromDB(DateTime startDate, DateTime endDate);
		Task<ServiceResult<List<LogDto>>> SearchLogs(DateTime startDate, DateTime endDate, string keywords, int userId, int logTypeId);
	}
}