using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using Microsoft.AspNetCore.Http;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IProductsDAL
    {
        Task<ServiceResult<ProductsDto>> AddProduct(ProductCreateDto p);
        Task<ServiceResult<bool>> DeleteProducts(string productId);
        Task<ServiceResult<ProductsDto>> GetProductsBasedOnBarcode(string barCode);
        Task<ServiceResult<ProductsDto>> GetProductsBasedOnID(string productId);
        Task<ServiceResult<List<ProductWithQtyDto>>> GetSubProductContentByParentID(string parentProductId);
        Task<ServiceResult<ProductsDto>> GetProductsBasedOnProdCode(string productCode);
        Task<ServiceResult<MemoryStream>> GetProductsForCSVExportBasedOnSelectedFields(string segmentId, string categoryId, string supplierId, List<string> availableColumnNames);
        Task<ServiceResult<PaginationDetails<ProductsDto>>> GetProductsFromDB(int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<List<ProductsDto>>> GetTrendingProducts(int trendingCount);
        Task<ServiceResult<PaginationDetails<ProductsDto>>> GetBooksByCategoryId(string categoryId, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<PaginationDetails<ProductsDto>>> GetFreeBooks(int offset, int limit, CancellationToken cancellationToken);
        Task<ServiceResult<PaginationDetails<ProductsDto>>> SearchProducts(string keywords, string categoryId, string segmentId, string supplierID, bool? hasSubProduct, decimal inStock, int offSet, int limit, CancellationToken cancellationToken, string? sortByColumn, bool sortAscending);
        Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchProductsForComboBoxes(string keywords, string categoryId, string segmentId, string supplierID, bool hasSubProduct, decimal inStock, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<List<ProductsDto>>> SearchTOP15Products(string keywords, string categoryId, string segmentId, string supplierID);
        Task<ServiceResult<ProductsDto>> UpdateProduct(ProductsDto p);
        Task<ServiceResult<List<ProductBarcodeDto>>> UpdateProductBarcodes(List<ProductBarcodeDto> p);
        Task<ServiceResult<ProductsDto>> UpdateProductPrices(string productId, ProductPricing prices);
        Task<ServiceResult<ProductsDto>> UpdateProductOnImportUsingBarCode(ProductsDto p);
        Task<ServiceResult<ProductsDto>> UpdateProductOnImportUsingProductCode(ProductsDto p);
        Task<ServiceResult<List<Dictionary<string, object>>>> ProcessExcelFile(ProductMedia p);
        Task<ServiceResult<ProductImportResultSummaryDto>> UpdateProductOnImportFromExcel(ProductImportDataFinalDto p);
        Task<ServiceResult<bool>> UpdateProductStock(string productId, decimal inStockAmount);
        Task<ServiceResult<bool>> UpdateProductStockList(List<StockParam> stockParamsList);
        Task<ServiceResult<bool>> UpdateStockFromProductReceiving(List<StockParam> stockParamsList);
        Task<ServiceResult<bool>> UpdateProductCostPrices(List<CostPriceChange> costChange);
        Task<ServiceResult<StockNotifyDto>> GetProductStockLevel(string productId);
        Task<ServiceResult<List<StockNotifyDto>>> GetStockLevelNotification();
        Task<ServiceResult<PaginationDetails<ProductsDto>>> GetBooksByCreatedBy(string userId, int offSet, int limit, CancellationToken cancellationToken);
    }
}