using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface ISlipsDAL
	{
		Task<ServiceResult<List<SizeDto>>> GetAllSlipdetailsFromDB();
		Task<ServiceResult<SizeDto>> GetSlipdetailsFromDBbasedOnslipID(string sizeId);
		Task<ServiceResult<SizeDto>> UpdateOrCreateSlipsUsingSlipID(SizeDto sizeDto);
	}
}