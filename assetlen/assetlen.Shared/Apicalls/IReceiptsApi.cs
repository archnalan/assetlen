using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ReportingDto;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface IReceiptsApi
    {

        [Post("/api/Receipts/CreateOrSyncNewReceiptItems")]
        Task<IApiResponse<List<ReceiptItemDto>>> CreateOrSyncNewReceiptItems([Body] List<ReceiptItemDto> input);

        [Get("/api/Receipts/GetReceiptItemsFromDBbasedOnSlipID")]
        Task<IApiResponse<List<ReceiptItemDto>>> GetReceiptItemsFromDBbasedOnSlipID([Query] int slipId);
    }
}
