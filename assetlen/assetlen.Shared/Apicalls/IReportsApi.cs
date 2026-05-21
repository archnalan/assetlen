using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ReportingDto;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace assetlen.Shared.Apicalls
{
    public interface IReportsApi
    {
        [Get("/api/Reports/GetCustomerStatement")]
        Task<IApiResponse<List<RepCustomerStatementDto>>> GetCustomerStatement([Query] DateTime startDate, [Query] DateTime endDate, [Query] string customerID);

        [Get("/api/Reports/GetSalesPerCustomer")]
        Task<IApiResponse<List<RepSalesPerCustomerDto>>> GetSalesPerCustomer([Query] DateTime startDate, [Query] DateTime endDate, [Query] string CustomerID);

        [Get("/api/Reports/GetSupplierStatement")]
        Task<IApiResponse<List<RepSupplierStatementDto>>> GetSupplierStatement([Query] DateTime startDate, [Query] DateTime endDate, [Query] string supplierID);

        [Get("/api/Reports/GetSalesPerDayReport")]
        Task<IApiResponse<SalesPerDayReportResponse>> GetSalesPerDayReport([Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/api/Reports/GetSalesPerDayDashboard")]
        Task<IApiResponse<List<RepSalesPerDayDashboardDto>>> GetSalesPerDayDashboard([Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/api/Reports/GetSalesPerMonth")]
        Task<IApiResponse<List<MonthlySalesDto>>> GetSalesPerMonth([Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/api/Reports/GetProductsSoldReport")]
        Task<IApiResponse<List<RepProductSalesDto>>> GetProductsSoldReport([Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/api/Reports/GetStockMovementReport")]
        Task<IApiResponse<List<RepStockMovementDto>>> GetStockMovementReport([Query] string productID, [Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/api/Reports/GetSalesPerCategoryAndSegment")]
        Task<IApiResponse<List<RepSalesPerCategoryAndSegmentDto>>> GetSalesPerCategoryAndSegment([Query] DateTime startDate, [Query] DateTime endDate, [Query] string? CategoryID, [Query] string? SegmentID);

        [Get("/api/Reports/GetProductPurchases")]
        Task<IApiResponse<List<RepProductPurchasesDto>>> GetProductPurchases([Query] DateTime startDate, [Query] DateTime endDate, [Query] string? SupplierID, [Query] string? CategoryID, [Query] string? SegmentID, [Query] string? keyWords);

        [Get("/api/Reports/GetExpensesPerUser")]
        Task<IApiResponse<List<RepExpensesDto>>> GetExpensesPerUser([Query] string UserID, [Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/api/Reports/GetCustomerBalance")]
        Task<IApiResponse<decimal>> GetCustomerBalance([Query] string customerId, [Query] DateTime endDate);

        [Get("/api/Reports/GetProductsInStockReport")]
        Task<IApiResponse<List<RepProductInStockDto>>> GetProductsInStockReport([Query] string? supplierId, [Query] string? CategoryID, [Query] string? SegmentID);

        [Get("/api/Reports/GetExpenses")]
        Task<IApiResponse<List<RepExpensesDto>>> GetExpenses([Query] DateTime startDate, [Query] DateTime endDate, [Query] string? expenseTypeId, [Query] string? userId);

        [Get("/api/Reports/GetSaleDetail")]
        Task<IApiResponse<List<RepSaleDetailDto>>> GetSaleDetail([Query] DateTime startDate, [Query] DateTime endDate, [Query] string? userId);

        [Get("/api/Reports/GetSalesPerYear")]
        Task<IApiResponse<List<YearlySalesDto>>> GetSalesPerYear([Query] DateTime startDate, [Query] DateTime endDate);

        [Get("/api/Reports/GetTopSellingProducts")]
        Task<IApiResponse<List<ProductSalesDto>>> GetTopSellingProducts([Query] DateTime startDate, [Query] DateTime endDate, [Query] int topN);

        [Get("/api/Reports/GetUserSalesPerDayReport")]
        Task<IApiResponse<List<SalesPerDayDto>>> GetUserSalesPerDayReport([Query] DateTime startDate, [Query] DateTime endDate);
    }
}
