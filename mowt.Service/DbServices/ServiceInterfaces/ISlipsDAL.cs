using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface ISlipsDAL
	{
		Task<ServiceResult<List<SizeDto>>> GetAllSlipdetailsFromDB();
		Task<ServiceResult<SizeDto>> GetSlipdetailsFromDBbasedOnslipID(string sizeId);
		Task<ServiceResult<SizeDto>> UpdateOrCreateSlipsUsingSlipID(SizeDto sizeDto);
	}
}