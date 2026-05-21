using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IReceiptsDAL
    {

        Task<ServiceResult<List<ReceiptItemDto>>> CreateOrSyncNewReceiptItems(List<ReceiptItemDto> rDto);
        Task<ServiceResult<List<ReceiptItemDto>>> GetReceiptItemsFromDBbasedOnSlipID(int slipId);

    }
}