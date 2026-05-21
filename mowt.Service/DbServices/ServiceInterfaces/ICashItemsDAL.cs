using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface ICashItemsDAL
	{
		Task<ServiceResult<List<CashItemsDto>>> GetCashItemsFromDB();
		Task<ServiceResult<CashItemsDto>> GetCashItemBasedOnID(string cashItemId);
		Task<ServiceResult<CashItemsDto>> AddCashItem(CashItemsDto c);
		Task<ServiceResult<CashItemsDto>> UpdateCashItem(string id, CashItemsDto c);
		Task<ServiceResult<bool>> DeleteCashItem(string id);
	}
}
