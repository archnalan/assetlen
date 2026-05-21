using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;

namespace mowt.Shared.Apicalls
{
    public interface IProductsAPI
    {
        [Get("/api/Products/GetProductsFromDB")]
        Task<IApiResponse<PaginationDetails<ProductsDto>>> GetProductsFromDB([Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Products/GetProductsBasedOnID")]
        Task<IApiResponse<ProductsDto>> GetProductsBasedOnID([Query] string productId);

        [Get("/api/Products/GetTrendingProducts")]
        Task<IApiResponse<List<ProductsDto>>> GetTrendingProducts([Query] int? trendingCount);

        [Get("/api/Products/GetSubProductContentByParentID")]
        Task<IApiResponse<List<ProductWithQtyDto>>> GetSubProductContentByParentID([Query] string parentProductId);

        [Get("/api/Products/GetProductsBasedOnProdCode")]
        Task<IApiResponse<ProductsDto>> GetProductsBasedOnProdCode([Query][Required] string productCode);

        [Post("/api/Products/AddProduct")]
        Task<IApiResponse<ProductsDto>> AddProduct([Body] ProductCreateDto p);
        [Multipart]
        [Post("/api/Products/ProcessExcelFile")]
        Task<IApiResponse<List<Dictionary<string, object>>>> ProcessExcelFile(StreamPart ProductFile, string ProductName);

        [Put("/api/Products/UpdateProduct")]
        Task<IApiResponse<ProductsDto>> UpdateProduct([Body] ProductsDto p);

        [Delete("/api/Products/DeleteProducts")]
        Task<IApiResponse<bool>> DeleteProducts([Query] string productId);

        [Get("/api/Products/SearchProducts")]
        Task<IApiResponse<PaginationDetails<ProductsDto>>> SearchProducts([Query] string? keywords, [Query] string? categoryId, [Query] string? segmentId, [Query] string? supplierID, [Query] bool? hasSubProduct, [Query] decimal? inStock, [Query] int? offSet = 0, [Query] int? limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Products/SearchProductsForComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchProductsForComboBoxes([Query] string? keywords, [Query] string? categoryId, [Query] string? segmentId, [Query] string? supplierID, [Query] bool? hasSubProduct, [Query] decimal? inStock, [Query] int? offSet = 0, [Query] int? limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Put("/api/Products/UpdateProductStock")]
        Task<IApiResponse<ProductsDto>> UpdateProductStock([Query] string productId, [Query] decimal inStockAmount);
        [Post("/api/Products/GetProductsForCSVExportBasedOnSelectedFields")]
        Task<IApiResponse<HttpContent>> GetProductsForCSVExportBasedOnSelectedFields([Body] List<string> availableColumnNames, [Query] string segmentId, [Query] string categoryId, [Query] string supplierId);

        [Post("/api/Products/UpdateProductOnImportFromExcel")]
        Task<IApiResponse<ProductImportResultSummaryDto>> UpdateProductOnImportFromExcel([Body] ProductImportDataFinalDto p);

        [Put("/api/Products/UpdateProductStockList")]
        Task<IApiResponse<bool>> UpdateProductStockList([Body] List<StockParam> stockParamsList);

        [Put("/api/Products/UpdateProductPrices")]
        Task<IApiResponse<ProductsDto>> UpdateProductPrices([Query][Required] string productId, [Body][Required] ProductPricing pricing);

        [Put("/api/Products/UpdateProductBarcodes")]
        Task<IApiResponse<ProductBarcodeDto>> UpdateProductBarcodes([Body] List<ProductBarcodeDto> products);

        [Put("/api/Products/UpdateProductCostPrices")]
        Task<IApiResponse<bool>> UpdateProductCostPrices([Body] List<CostPriceChange> costChange);

        [Get("/api/Products/GetProductStockLevel")]
        Task<IApiResponse<StockNotifyDto>> GetProductStockLevel([Query][Required] string productId);

        [Get("/api/Products/GetStockLevelNotification")]
        Task<IApiResponse<List<StockNotifyDto>>> GetStockLevelNotification();

        [Get("/api/Products/GetBooksByCategoryId")]
        Task<IApiResponse<PaginationDetails<ProductsDto>>> GetBooksByCategoryId([Query][Required] string categoryId, [Query] int? offSet = 0, [Query] int? limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Products/GetFreeBooks")]
        Task<IApiResponse<PaginationDetails<ProductsDto>>> GetFreeBooks([Query] int? offSet = 0, [Query] int? limit = 12, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Products/GetMyCreatedBooks")]
        Task<IApiResponse<PaginationDetails<ProductsDto>>> GetMyCreatedBooks([Query] int? offSet = 0, [Query] int? limit = 200, [Query] CancellationToken cancellationToken = default);
    }
}
