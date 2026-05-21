using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels.ReportingDto;
using Hangfire;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IReportsDAL
    {
        Task<ServiceResult<bool>> DeleteAfileFromServerPC(string fullFilePath);
        Task<ServiceResult<decimal>> GetCustomerBalanceAsync(string customerId, DateTime endDate);
        Task<ServiceResult<List<RepCustomerStatementDto>>> GetCustomerStatementAsync(DateTime startDate, DateTime endDate, string customerID);
        Task<ServiceResult<List<RepExpensesDto>>> GetExpensesPerUserAsync(string UserID, DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<string>>> GetNumberOfFilesInBackupFolder(string backUpFolderPath);
        Task<ServiceResult<List<RepProductPurchasesDto>>> GetProductPurchasesAsyc(DateTime startDate, DateTime endDate, string SupplierID, string CategoryID, string SegmentID, string? keyWords);
        Task<ServiceResult<List<RepProductInStockDto>>> GetProductsInStockReport(string supplierId, string categoryId, string segmentId);
        Task<ServiceResult<List<RepProductSalesDto>>> GetProductsSoldReport(DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<RepSalesPerCustomerDto>>> GetSalesPerCustomerAsync(DateTime startDate, DateTime endDate, string CustomerID);
        Task<ServiceResult<List<RepSaleDetailDto>>> GetSaleDetail(DateTime startDate, DateTime endDate, string? userId);
        Task<ServiceResult<List<RepSalesPerCategoryAndSegmentDto>>> GetSalesPerCategoryAndSegmentAsync(DateTime startDate, DateTime endDate, string CategoryID, string SegmentID);
        Task<ServiceResult<List<RepSalesPerDayDashboardDto>>> GetSalesPerDayDashboard(DateTime startDate, DateTime endDate);
        Task<ServiceResult<SalesPerDayReportResponse>> GetSalesPerDayReport(DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<RepStockMovementDto>>> GetStockMovementReport(string productID, DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<RepSupplierStatementDto>>> GetSupplierStatementAsync(DateTime startDate, DateTime endDate, string supplierID);
        Task<ServiceResult<List<MonthlySalesDto>>> GetSalesPerMonth(DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<RepExpensesDto>>> GetExpensesAsync(DateTime startDate, DateTime endDate, string? expenseTypeId, string? userId);
        Task<ServiceResult<List<YearlySalesDto>>> GetSalesPerYear(DateTime startDate, DateTime endDate);
        Task<ServiceResult<List<ProductSalesDto>>> GetTopSellingProducts(DateTime startDate, DateTime endDate, int topN);
        Task<ServiceResult<List<SalesPerDayDto>>> GetUserSalesPerDayReport(DateTime startDate, DateTime endDate);
    }
}