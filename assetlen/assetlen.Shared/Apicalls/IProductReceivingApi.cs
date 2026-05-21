using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface IProductReceivingApi
    {
        [Get("/api/ProductReceiving/GetProductReceivingDetailFromDBPerGRNumber")]
        Task<IApiResponse<List<ProductReceivingDto>>> GetProductReceivingDetailFromDBPerGRNumber(string GRNumber);

        [Get("/api/ProductReceiving/GetProductsReceivedFromDBUsingDateRange")]
        Task<IApiResponse<List<ProductReceivingDto>>> GetProductsReceivedFromDBUsingDateRange(DateTime startDate, DateTime endDate);

        [Post("/api/ProductReceiving/AddProductReceivingDetailToDB")]
        Task<IApiResponse<ProductReceivingDto>> AddProductReceivingDetailToDB(ProductReceivingDto prDto);

        [Post("/api/ProductReceiving/ReceiveMultipleProducts")]
        Task<IApiResponse<List<ProductReceivingDto>>> ReceiveMultipleProducts(ReceivingStockData recData);

        [Get("/api/ProductReceiving/SearchProductReceivingDetailFromDB")]
        Task<IApiResponse<PaginationDetails<ProductReceivingDto>>> SearchProductReceivingDetailFromDB(int? receiveStockId, int? supplierAccount, string? keywords, string? barCode, int? offset, int? limit, CancellationToken token);
    }
}
