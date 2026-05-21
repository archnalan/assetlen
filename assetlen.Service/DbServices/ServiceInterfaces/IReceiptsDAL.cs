using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface IReceiptsDAL
    {

        Task<ServiceResult<List<ReceiptItemDto>>> CreateOrSyncNewReceiptItems(List<ReceiptItemDto> rDto);
        Task<ServiceResult<List<ReceiptItemDto>>> GetReceiptItemsFromDBbasedOnSlipID(int slipId);

    }
}