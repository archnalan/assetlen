using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ReportingDto;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace mowt.Service.DbServices
{
    public class ReportsDAL : IReportsDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<ReportsDAL> _logger;
        private readonly ITenantProvider _tenantProvider;
        public ReportsDAL(ILogger<ReportsDAL> logger, mowtDbContext context, ITenantProvider tenantProvider)
        {
            _logger = logger;
            _context = context;
            _tenantProvider = tenantProvider;
        }
        private string TenantId => _tenantProvider.GetTenantId();

        #region Products Sold Report
        public async Task<ServiceResult<List<RepProductSalesDto>>> GetProductsSoldReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return ServiceResult<List<RepProductSalesDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date."));

                var result = await (
                    from td in _context.tbl_TransactionDetails
                    join p in _context.tbl_Products on td.ProductId equals p.Id
                    join t in _context.tbl_Transactions on td.TransactionId equals t.Id
                    where t.TransactionDate >= startDate
                       && t.TransactionDate <= endDate
                       && t.TransactionStatus >= 10
                    group new { td, p } by new { p.ProductName, p.ProductCode } into g
                    orderby g.Key.ProductName
                    select new RepProductSalesDto
                    {
                        ProductName = g.Key.ProductName ?? "",
                        ProductCode = g.Key.ProductCode,
                        Quantity = g.Sum(x => x.td.Qty ?? 0),
                        TotalCost = g.Sum(x => x.td.CostExc ?? 0),
                        TotalPriceExc = g.Sum(x => x.td.TotalPriceExc ?? 0),
                        TotalPriceInc = g.Sum(x => x.td.TotalPriceInc ?? 0),
                        Tax = g.Sum(x => (x.td.TotalPriceInc ?? 0) - (x.td.TotalPriceExc ?? 0)),
                        Profit = g.Sum(x => (x.td.TotalPriceExc ?? 0) - ((x.td.CostExc ?? 0) * (x.td.Qty ?? 0)))
                    }
                ).ToListAsync();

                return ServiceResult<List<RepProductSalesDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running product sales report: {ex}", ex);
                return ServiceResult<List<RepProductSalesDto>>.Failure(
                    new ServerErrorException("Error while generating product sales report."));
            }
        }
        #endregion

        #region Method for getting Sales per day Report
        public async Task<ServiceResult<SalesPerDayReportResponse>> GetSalesPerDayReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<SalesPerDayReportResponse>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }

                string sql = @"
				  WITH SalesData AS (
                        SELECT
                            CAST(t.transactionDate AS DATE) AS [Date],
                            COUNT(DISTINCT t.Id) AS [NoOfTransactions], 
                            SUM(td.costExc * td.qty) AS [TotalCostExc],
                            SUM(td.totalPriceExc) AS [TotalPriceExc], 
                            SUM(td.totalPriceInc) AS [TotalPriceInc],  
                            -- Calculates PROFIT (only positive margins):
                            SUM(CASE 
                                WHEN (td.totalPriceExc - (td.costExc * td.qty)) >= 0 
                                THEN (td.totalPriceExc - (td.costExc * td.qty)) 
                                ELSE 0 
                            END) AS [Profit],
                            -- Calculates LOSS (absolute value of negative margins):
                            SUM(CASE 
                                WHEN (td.totalPriceExc - (td.costExc * td.qty)) < 0 
                                THEN ABS(td.totalPriceExc - (td.costExc * td.qty)) 
                                ELSE 0 
                            END) AS [Loss],
                            SUM(td.totalPriceInc - td.totalPriceExc) AS [Tax], 
                            COUNT(*) OVER() AS TotalCount
                        FROM tbl_transactionDetail td
                        INNER JOIN tbl_transaction t 
                            ON td.transactionID = t.Id
                        WHERE t.transactionDate BETWEEN @StartDate AND @EndDate
                            AND t.transactionStatus IN (10, 11, 13)
                            AND t.TenantId = @TenantId
                            AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
                            AND td.TenantId = @TenantId
                            AND (td.IsDeleted = 0 OR td.IsDeleted IS NULL)
                        GROUP BY CAST(t.transactionDate AS DATE)
                    )
                    SELECT * FROM SalesData
                    ORDER BY [Date]";

                var parameters = new[]
                {
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@TenantId", TenantId),
                };

                var result = await _context.Database
                    .SqlQueryRaw<RepSalesPerDayDto>(sql, parameters)
                    .ToListAsync();
                var salesPerDay = new SalesPerDayReportResponse
                {
                    SalesData = result,
                    TotalCount = result.FirstOrDefault()?.TotalCount ?? 0
                };
                return ServiceResult<SalesPerDayReportResponse>.Success(salesPerDay);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running sales per day report: {ex}", ex);
                return ServiceResult<SalesPerDayReportResponse>.Failure(
                    new ServerErrorException("Error while running sales per day report."));
            }
        }
        #endregion

        #region Method for getting Sales per day Data for Admin dashboard
        public async Task<ServiceResult<List<RepSalesPerDayDashboardDto>>> GetSalesPerDayDashboard(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepSalesPerDayDashboardDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }
                var query = @"
					 SELECT
					     CAST(t.transactionDate AS DATE) AS Date,
					     SUM(td.priceInc) AS TotalPriceInc
					 FROM
					     tbl_transactionDetail td
					     INNER JOIN tbl_transaction t
					         ON td.transactionID = t.Id
					 WHERE
					     t.transactionDate BETWEEN @p0 AND @p1
					     AND t.transactionStatus IN (10, 11, 13)
					     AND t.TenantId = @p2
					     AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
					     AND (td.IsDeleted = 0 OR td.IsDeleted IS NULL)
					 GROUP BY
					     CAST(t.transactionDate AS DATE)
					 ORDER BY
					     CAST(t.transactionDate AS DATE)";

                var parameters = new[]
                {
                    new SqlParameter("@p0", startDate),
                    new SqlParameter("@p1", endDate),
                    new SqlParameter("@p2", TenantId)
                };
                var result = await _context.Database
                    .SqlQueryRaw<RepSalesPerDayDashboardDto>(query, parameters)
                    .ToListAsync();

                return ServiceResult<List<RepSalesPerDayDashboardDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report : {ex}", ex);
                return ServiceResult<List<RepSalesPerDayDashboardDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Method for getting Products instock Report

        public async Task<ServiceResult<List<RepProductInStockDto>>> GetProductsInStockReport(string supplierId, string categoryId, string segmentId)
        {
            try
            {
                if (!string.IsNullOrEmpty(categoryId))
                {
                    bool categoryExists = await _context.tbl_Categories
                   .AnyAsync(c => c.Id == categoryId);
                    if (!categoryExists)
                    {
                        return ServiceResult<List<RepProductInStockDto>>.Failure(
                            new NotFoundException($"Category with ID {categoryId} not found.")
                        );
                    }
                }
                if (!string.IsNullOrEmpty(segmentId))
                {
                    bool segmentExists = await _context.tbl_Segments.AnyAsync(
                        s => s.Id == segmentId);

                    if (!segmentExists)
                    {
                        return ServiceResult<List<RepProductInStockDto>>.Failure(
                            new NotFoundException($"Segment with ID {segmentId} not found.")
                        );
                    }
                }
                if (!string.IsNullOrEmpty(supplierId))
                {
                    bool supplierExists = await _context.tbl_Suppliers
                        .AnyAsync(s => s.Id == supplierId);
                    if (!supplierExists)
                    {
                        return ServiceResult<List<RepProductInStockDto>>.Failure(
                            new NotFoundException($"Supplier with ID {supplierId} not found.")
                        );
                    }
                }

                string sql = @"
					SELECT
                        tbl_products.productName AS [ProductName],
                        tbl_products.productCode AS [ProductCode],
                        tbl_products.inStock AS [InStock],
                        COALESCE(tbl_products.costExclusive, 0) * COALESCE(tbl_products.inStock, 0) AS [CostExc],
                    				COALESCE(tbl_products.priceExclusive, 0) * COALESCE(tbl_products.inStock, 0) AS [PriceExc],
                    				COALESCE(tbl_products.priceInclusive, 0) * COALESCE(tbl_products.inStock, 0) AS [PriceInc],
                    				tbl_category.Id AS [CategoryId],
                        tbl_category.category AS [Category],
                        tbl_segment.Id AS [SegmentId],
                        tbl_segment.segment AS [Segment],
                        tbl_Supplier.Id AS [SupplierId],
                        tbl_Supplier.FullName AS [Supplier]
                    FROM
                        tbl_products
                        INNER JOIN tbl_Supplier ON tbl_products.supplierId = tbl_Supplier.Id
                        INNER JOIN tbl_segment ON tbl_products.segmentId = tbl_segment.Id
                        INNER JOIN tbl_category ON tbl_products.categoryId = tbl_category.Id
                    WHERE
                        (tbl_products.IsDeleted = 0 OR tbl_products.IsDeleted IS NULL)
                        AND (tbl_Supplier.IsDeleted = 0 OR tbl_Supplier.IsDeleted IS NULL)
                        AND (tbl_segment.IsDeleted = 0 OR tbl_segment.IsDeleted IS NULL)
                        AND (tbl_category.IsDeleted = 0 OR tbl_category.IsDeleted IS NULL)
                        AND tbl_products.TenantId = @TenantId
                        AND tbl_Supplier.TenantId = @TenantId
                        AND tbl_segment.TenantId = @TenantId
                        AND tbl_category.TenantId = @TenantId
                        AND (@SupplierID IS NULL OR @SupplierID = '' OR @SupplierID = '-1' OR tbl_Supplier.Id = @SupplierID)
                        AND (@CategoryID IS NULL OR @CategoryID = '' OR @CategoryID = '-1' OR tbl_category.Id = @CategoryID)
                        AND ( @SegmentID IS NULL OR @SegmentID = '' OR @SegmentID = '-1' OR tbl_segment.Id = @SegmentID)
                    ORDER BY
                        tbl_products.productName";

                var parameters = new[]
                {
                    new SqlParameter("@SupplierID", supplierId),
                    new SqlParameter("@CategoryID", categoryId),
                    new SqlParameter("@SegmentID", segmentId),
                    new SqlParameter("@TenantId", TenantId)
                };

                var result = await _context.Database
                    .SqlQueryRaw<RepProductInStockDto>(sql, parameters)
                    .ToListAsync();

                return ServiceResult<List<RepProductInStockDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running products in stock report: {ex}", ex);
                return ServiceResult<List<RepProductInStockDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Method for getting SaleDetails Report
        public async Task<ServiceResult<List<RepSaleDetailDto>>> GetSaleDetail(DateTime startDate, DateTime endDate, string? userID)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepSaleDetailDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }

                if (!string.IsNullOrEmpty(userID))
                {
                    bool userExists = await _context.Users
                        .AnyAsync(c => c.Id == userID);

                    if (!userExists)
                    {
                        return ServiceResult<List<RepSaleDetailDto>>.Failure(
                            new NotFoundException($"User with ID {userID} not found.")
                        );
                    }
                }

                var result = await (
                    from td in _context.tbl_TransactionDetails
                    join t in _context.tbl_Transactions on td.TransactionId equals t.Id
                    join p in _context.tbl_Products on td.ProductId equals p.Id
                    join tx in _context.tbl_Taxes on td.TaxId equals tx.Id
                    join s in _context.tbl_Suppliers on p.SupplierId equals s.Id
                    where t.TransactionDate >= startDate
                       && t.TransactionDate <= endDate
                       && (new[] { 10, 11, 13 }).Contains(t.TransactionStatus ?? 0)
                       && (string.IsNullOrEmpty(userID) || t.SoldBy == userID)
                    orderby t.TransactionDate
                    select new RepSaleDetailDto
                    {
                        TransactionDate = t.TransactionDate ?? DateTime.MinValue,
                        TransactionId = t.Id,
                        ProductCode = p.ProductCode,
                        BarCode = p.BarCode,
                        ProductName = p.ProductName ?? "",
                        Quantity = td.Qty,
                        PriceInc = td.PriceInc,
                        TotalPriceInc = td.TotalPriceInc,
                        Tax = tx.TaxValue,
                        Supplier = s.FullName ?? ""
                    }
                ).ToListAsync();

                return ServiceResult<List<RepSaleDetailDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running sale detail report: {ex}", ex);
                return ServiceResult<List<RepSaleDetailDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Get Monthly sale details report
        public async Task<ServiceResult<List<MonthlySalesDto>>> GetSalesPerMonth(DateTime startDate, DateTime endDate)
        {
            try
            {
                var dailySales = await GetSalesPerDayDashboard(startDate, endDate);
                if (!dailySales.IsSuccess)
                    return ServiceResult<List<MonthlySalesDto>>.Failure(dailySales.Error);

                var monthlySales = dailySales.Data
                    .GroupBy(d => new { d.Date.Year, d.Date.Month })
                    .Select(g => new MonthlySalesDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalPriceInc = g.Sum(x => x.TotalPriceInc ?? 0m)
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList();
                decimal totalSum = monthlySales.Sum(m => m.TotalPriceInc);

                var result = monthlySales.Select(m => new MonthlySalesDto
                {
                    Year = m.Year,
                    Month = m.Month,
                    TotalPriceInc = m.TotalPriceInc,
                    Percentage = totalSum != 0 ?
                   (m.TotalPriceInc / totalSum) * 100 : 0
                })
                 .ToList();

                return ServiceResult<List<MonthlySalesDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report : {ex}", ex);
                return ServiceResult<List<MonthlySalesDto>>.Failure(new ServerErrorException(ex.Message));
            }
        }
        #endregion

        #region Get Yearly sale details report
        public async Task<ServiceResult<List<YearlySalesDto>>> GetSalesPerYear(DateTime startDate, DateTime endDate)
        {
            try
            {
                var dailySales = await GetSalesPerDayDashboard(startDate, endDate);
                if (!dailySales.IsSuccess)
                    return ServiceResult<List<YearlySalesDto>>.Failure(dailySales.Error);

                var yearlySalesList = dailySales.Data
                    .GroupBy(d => d.Date.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        Total = g.Sum(x => x.TotalPriceInc ?? 0m)
                    })
                    .OrderBy(y => y.Year)
                    .ToList();

                decimal totalSum = yearlySalesList.Sum(y => y.Total);

                var yearlySales = yearlySalesList
                    .Select(y => new YearlySalesDto
                    {
                        Year = y.Year,
                        TotalPriceInc = y.Total,
                        Percentage = totalSum != 0 ? (y.Total / totalSum) * 100 : 0
                    })
                    .ToList();

                return ServiceResult<List<YearlySalesDto>>.Success(yearlySales);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running yearly sales report: {ex}", ex);
                return ServiceResult<List<YearlySalesDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Method for getting SaleDetails Per customer
        public async Task<ServiceResult<List<RepSalesPerCustomerDto>>> GetSalesPerCustomerAsync(DateTime startDate, DateTime endDate, string customerId)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepSalesPerCustomerDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }

                bool customerExists = await _context.tbl_Customers
                    .AnyAsync(c => c.Id == customerId);

                if (!customerExists)
                {
                    return ServiceResult<List<RepSalesPerCustomerDto>>.Failure(
                        new NotFoundException($"Customer with ID {customerId} not found.")
                    );
                }

                var result = await (
                    from t in _context.tbl_Transactions
                    join c in _context.tbl_Customers on t.CustomerId equals c.Id
                    join td in _context.tbl_TransactionDetails on t.Id equals td.TransactionId
                    join p in _context.tbl_Products on td.ProductId equals p.Id
                    where t.TransactionDate >= startDate
                       && t.TransactionDate <= endDate
                       && (new[] { 10, 11, 13 }).Contains(t.TransactionStatus ?? 0)
                       && c.Id == customerId
                    orderby t.TransactionDate
                    select new RepSalesPerCustomerDto
                    {
                        TransactionDate = t.TransactionDate ?? DateTime.MinValue,
                        TransactionId = t.Id,
                        ProductName = p.ProductName,
                        Quantity = td.Qty,
                        TotalPriceExc = td.TotalPriceExc ?? 0,
                        TotalPriceInc = td.TotalPriceInc ?? 0
                    }
                ).ToListAsync();

                return ServiceResult<List<RepSalesPerCustomerDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running sales per customer report: {ex}", ex);
                return ServiceResult<List<RepSalesPerCustomerDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Method for getting Stock Movement report per product

        public async Task<ServiceResult<List<RepStockMovementDto>>> GetStockMovementReport(string productID, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepStockMovementDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }

                bool productExists = await _context.tbl_Products
                .AnyAsync(c => c.Id == productID);

                if (!productExists)
                {
                    return ServiceResult<List<RepStockMovementDto>>.Failure(
                        new NotFoundException($"Product with ID {productID} not found.")
                    );
                }
                string sql = @"
				   WITH StockEvents AS (
                    -- Stock take and CsvImport (combined)
                    SELECT
                        COALESCE(p.productName, '') AS [Description],
                        l.TimeStamp AS [TransDate],
                        p.productCode AS [ProductCode],
                        p.barCode AS [Barcode],
                        CASE l.LogTypeId WHEN 6 THEN 'Stocktake' WHEN 7 THEN 'CsvImport' END AS [EventType],
                        l.Id AS [EventTypeId],
                        COALESCE(l.oldQty, 0) AS [OldQty],
                        0 AS [ChangeQty],
                        COALESCE(l.newQty, 0) AS [NewQty],
                        COALESCE(u.UserName, '') AS [User],
                        p.Id AS [ProductId],
                        p.TenantId
                    FROM
                        tbl_Logs l
                        INNER JOIN tbl_products p ON l.productId = p.Id
                        INNER JOIN Users u ON l.UserId = u.Id
                    WHERE
                        l.LogTypeId IN (6, 7)
                        AND p.Id = @ProductId
                
                    UNION ALL
                
                    -- Refunds
                    SELECT
                        COALESCE(p.productName, '') AS [Description],
                        r.refundDateTime AS [TransDate],
                        p.productCode AS [ProductCode],
                        p.barCode AS [Barcode],
                        'Refund' AS [EventType],
                        r.Id AS [EventTypeId],
                        0 AS [OldQty],
                        COALESCE(td.qty, 0) AS [ChangeQty],
                        0 AS [NewQty],
                        COALESCE(u.UserName, '') AS [User],
                        td.productID AS [ProductId],
                        t.TenantId
                    FROM
                        tbl_Refunds r
                        INNER JOIN tbl_transaction t ON r.saleID = t.Id
                        INNER JOIN tbl_transactionDetail td ON td.transactionID = t.Id
                        INNER JOIN tbl_products p ON p.Id = td.productID
                        INNER JOIN Users u ON r.refundedBy = u.Id
                    WHERE
                        t.transactionStatus = 7
                        AND p.deleted = 0
                        AND td.productID = @ProductId
                
                    UNION ALL
                
                    -- Sales
                    SELECT
                        COALESCE(p.productName, '') AS [Description],
                        t.transactionDate AS [TransDate],
                        p.productCode AS [ProductCode],
                        p.barCode AS [Barcode],
                        'Sale' AS [EventType],
                        t.Id AS [EventTypeId],
                        0 AS [OldQty],
                        COALESCE(td.qty, 0) * -1 AS [ChangeQty],
                        0 AS [NewQty],
                        COALESCE(u.UserName, '') AS [User],
                        p.Id AS ProductId,
                        t.TenantId
                    FROM
                        tbl_transaction t
                        INNER JOIN tbl_transactionDetail td ON td.transactionID = t.Id
                        INNER JOIN tbl_products p ON p.Id = td.productID
                        INNER JOIN Users u ON t.soldBy = u.Id
                    WHERE
                        p.deleted = 0
                        AND p.Id = @ProductId
                        AND t.transactionStatus IN (10, 11, 13)
                
                    UNION ALL
                
                    -- Stock Receiving
                    SELECT
                        COALESCE(p.productName, '') AS [Description],
                        pr.DateReceived AS [TransDate],
                        p.productCode AS [ProductCode],
                        p.barCode AS [Barcode],
                        'StockReceiving' AS [EventType],
                        pr.Id AS [EventTypeId],
                        0 AS [OldQty],
                        COALESCE(pr.Qty, 0) AS [ChangeQty],
                        0 AS [NewQty],
                        COALESCE(u.UserName, '') AS [User],
                        pr.ProductID AS [ProductId],
                        p.TenantId
                    FROM
                        tbl_ProductReceiving pr
                        INNER JOIN tbl_products p ON p.Id = pr.ProductID
                        INNER JOIN Users u ON pr.ReceivedBy = u.Id
                    WHERE
                       pr.ProductID = @ProductId
                ),
                StockMovementWithRunningTotal AS (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY TransDate, ProductId) AS StockMovementId,
                        [Description],
                        TransDate,
                        ProductCode,
                        Barcode,
                        EventType,
                        EventTypeId,
                        OldQty,
                        ChangeQty,
                        NewQty,
                        [User],
                        ProductId,
                        SUM(CASE WHEN ChangeQty = 0 THEN NewQty ELSE ChangeQty END) 
                            OVER (PARTITION BY ProductId ORDER BY TransDate ROWS UNBOUNDED PRECEDING) AS RunningNewQty
                    FROM StockEvents
                    WHERE TransDate >= COALESCE(
                        (SELECT MIN(TransDate) FROM StockEvents WHERE EventType IN ('Stocktake', 'CsvImport')),
                        GETDATE()
                    )
                )
                SELECT
                    [Description],
                    TransDate,
                    ProductCode,
                    Barcode,
                    EventType,
                    EventTypeId,
                    LAG(RunningNewQty, 1, 0) OVER (PARTITION BY ProductId ORDER BY TransDate) AS OldQty,
                    ChangeQty,
                    RunningNewQty AS NewQty,
                    [User],
                    ProductId
                FROM StockMovementWithRunningTotal
                WHERE TransDate BETWEEN @startDate AND @endDate
                ORDER BY TransDate, ProductId";

                var parameters = new[]
                {
                    new SqlParameter("@ProductId", productID),
                    new SqlParameter("@startDate", startDate),
                    new SqlParameter("@endDate", endDate),
                };

                var result = await _context.Database
                    .SqlQueryRaw<RepStockMovementDto>(sql, parameters)
                    .ToListAsync();

                return ServiceResult<List<RepStockMovementDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running stock movement report: {ex}", ex);
                return ServiceResult<List<RepStockMovementDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Method for getting Sales Per Category
        public async Task<ServiceResult<List<RepSalesPerCategoryAndSegmentDto>>> GetSalesPerCategoryAndSegmentAsync(
            DateTime startDate, DateTime endDate, string CategoryID, string SegmentID)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepSalesPerCategoryAndSegmentDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }

                if (!string.IsNullOrEmpty(CategoryID) && CategoryID != "-1")
                {
                    bool categoryExists = await _context.tbl_Categories
                        .AnyAsync(c => c.Id == CategoryID);

                    if (!categoryExists)
                    {
                        return ServiceResult<List<RepSalesPerCategoryAndSegmentDto>>.Failure(
                            new NotFoundException($"Category with ID {CategoryID} not found.")
                        );
                    }
                }

                if (!string.IsNullOrEmpty(SegmentID) && SegmentID != "-1")
                {
                    bool segmentExists = await _context.tbl_Segments
                        .AnyAsync(s => s.Id == SegmentID);

                    if (!segmentExists)
                    {
                        return ServiceResult<List<RepSalesPerCategoryAndSegmentDto>>.Failure(
                            new NotFoundException($"Segment with ID {SegmentID} not found.")
                        );
                    }
                }

                bool filterByCategory = !string.IsNullOrEmpty(CategoryID) && CategoryID != "-1";
                bool filterBySegment = !string.IsNullOrEmpty(SegmentID) && SegmentID != "-1";

                var result = await (
                    from t in _context.tbl_Transactions
                    join td in _context.tbl_TransactionDetails on t.Id equals td.TransactionId
                    join p in _context.tbl_Products on td.ProductId equals p.Id
                    join c in _context.tbl_Categories on p.CategoryId equals c.Id
                    join s in _context.tbl_Segments on p.SegmentId equals s.Id
                    where t.TransactionDate >= startDate
                       && t.TransactionDate <= endDate
                       && (new[] { 10, 11, 13 }).Contains(t.TransactionStatus ?? 0)
                       && (!filterByCategory || c.Id == CategoryID)
                       && (!filterBySegment || s.Id == SegmentID)
                    group new { td, p, c, s } by new
                    {
                        p.ProductName,
                        p.ProductCode,
                        p.BarCode,
                        Category = c.Category,
                        Segment = s.Segment
                    } into g
                    orderby g.Key.ProductName
                    select new RepSalesPerCategoryAndSegmentDto
                    {
                        ProductName = g.Key.ProductName ?? "",
                        ProductCode = g.Key.ProductCode,
                        BarCode = g.Key.BarCode,
                        Quantity = g.Sum(x => x.td.Qty),
                        TotalCostExclusive = g.Sum(x => x.p.CostExclusive ?? 0),
                        TotalPriceExc = g.Sum(x => x.td.TotalPriceExc ?? 0),
                        TotalPriceInc = g.Sum(x => x.td.TotalPriceInc ?? 0),
                        Category = g.Key.Category ?? "",
                        Segment = g.Key.Segment ?? ""
                    }
                ).ToListAsync();

                return ServiceResult<List<RepSalesPerCategoryAndSegmentDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running sales per category and segment report: {ex}", ex);
                return ServiceResult<List<RepSalesPerCategoryAndSegmentDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Method for getting Product Purchases

        public async Task<ServiceResult<List<RepProductPurchasesDto>>> GetProductPurchasesAsyc(
                        DateTime startDate, DateTime endDate, string SupplierID, string CategoryID, string SegmentID, string? keyWords)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepProductPurchasesDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }
                if (!string.IsNullOrEmpty(SupplierID))
                {
                    bool supplierExists = await _context.tbl_Suppliers.AnyAsync(
                    s => s.Id == SupplierID && s.TenantId == TenantId);

                    if (!supplierExists)
                    {
                        return ServiceResult<List<RepProductPurchasesDto>>.Failure(
                            new NotFoundException($"Supplier with ID {SupplierID} not found.")
                        );
                    }
                }
                if (!string.IsNullOrEmpty(CategoryID))
                {
                    bool categoryExists = await _context.tbl_Categories.AnyAsync(
                    c => c.Id == CategoryID && c.TenantId == TenantId);
                    if (!categoryExists)
                    {
                        return ServiceResult<List<RepProductPurchasesDto>>.Failure(
                            new NotFoundException($"Category with ID {CategoryID} not found.")
                        );
                    }
                }
                if (!string.IsNullOrEmpty(SegmentID))
                {
                    bool segmentExists = await _context.tbl_Segments.AnyAsync(
                    s => s.Id == SegmentID && s.TenantId == TenantId);
                    if (!segmentExists)
                    {
                        return ServiceResult<List<RepProductPurchasesDto>>.Failure(
                            new NotFoundException($"Segment with ID {SegmentID} not found.")
                        );
                    }
                }

                var sql = @"
					 SELECT
					     pr.DateReceived,
					     pr.GrnsupplierNumber,
					     p.productName AS ProductName,
					     p.productCode AS ProductCode,
					     p.barCode AS BarCode,
					     pr.Qty,
					     p.costExclusive AS CostExclusive,
					     p.costInclusive AS CostInclusive,
					     pr.PriceChanged,
					     pr.NewCostInc,
					     pr.NewPriceInc,
					     s.FullName AS Supplier,
					     pr.supplierAccount AS SupplierID,
					     u.FirstName AS UserName,
					     p.segmentId AS SegmentId,
					     p.categoryId AS CategoryId,
					     c.category AS Category,
					     seg.segment AS Segment
					 FROM
					     tbl_ProductReceiving pr
					     INNER JOIN tbl_products p ON pr.ProductID = p.Id
					     INNER JOIN Users u ON pr.ReceivedBy = u.Id
					     INNER JOIN tbl_Supplier s ON pr.supplierAccount = s.Id
					     INNER JOIN tbl_category c ON p.categoryId = c.Id
					     INNER JOIN tbl_segment seg ON p.segmentId = seg.Id
					 WHERE
					     pr.DateReceived BETWEEN @startDate AND @endDate
					     AND (@GRNSupplierNumber IS NULL OR pr.GrnsupplierNumber LIKE @GRNSupplierNumber)
					     AND (@CategoryID IS NULL OR @CategoryID = '' OR p.categoryId = @CategoryID)
					     AND (@SegmentID IS NULL OR @SegmentID = '' OR p.segmentId = @SegmentID)
					     AND (@SupplierID IS NULL OR @SupplierID = '' OR pr.supplierAccount = @SupplierID)
					     AND (pr.TenantId = @TenantId AND (pr.IsDeleted = 0 OR pr.IsDeleted IS NULL))
					     AND (p.TenantId = @TenantId AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL))
					     AND (u.TenantId = @TenantId AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL))
					     AND (s.TenantId = @TenantId AND (s.IsDeleted = 0 OR s.IsDeleted IS NULL))
					     AND (c.TenantId = @TenantId AND (c.IsDeleted = 0 OR c.IsDeleted IS NULL))
					     AND (seg.TenantId = @TenantId AND (seg.IsDeleted = 0 OR seg.IsDeleted IS NULL))
					 ORDER BY
					     pr.Id";

                var result = await _context.Database
                    .SqlQueryRaw<RepProductPurchasesDto>(sql,
                        new SqlParameter("@startDate", startDate),
                        new SqlParameter("@endDate", endDate),
                        new SqlParameter("@SupplierID", SupplierID),
                        new SqlParameter("@CategoryID", CategoryID),
                        new SqlParameter("@SegmentID", SegmentID),
                        new SqlParameter("@GRNSupplierNumber", keyWords ?? (object)DBNull.Value),
                        new SqlParameter("@TenantId", TenantId))
                    .ToListAsync();

                return ServiceResult<List<RepProductPurchasesDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report: {ex}", ex);
                return ServiceResult<List<RepProductPurchasesDto>>.Failure(
                    new ServerErrorException(ex.Message));
            }
        }

        #endregion

        #region Method for getting Expenses Per User

        public async Task<ServiceResult<List<RepExpensesDto>>> GetExpensesPerUserAsync(string UserID, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepExpensesDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }
                bool userExists = await _context.Users.AnyAsync(c => c.Id == UserID);

                if (!userExists)
                {
                    return ServiceResult<List<RepExpensesDto>>.Failure(
                        new NotFoundException($"User with ID {UserID} not found.")
                    );
                }

                var sql = @"
		            SELECT
                        e.dateTimePayed AS TransDate,
                        u.userName AS UserName,
                        e.ShiftID,
                        e.Amount,
                        e.Comment AS Reason,
                        pm.Description AS PaymentMode,
                        et.Description AS Category
                    FROM
                        tbl_Expense e
                        INNER JOIN tbl_Payments p ON e.Id = p.ExpenseID
                        INNER JOIN tbl_paymentMode pm ON p.PaymentModeID = pm.PaymentModeID
                        INNER JOIN tbl_shifts s ON e.ShiftID = s.Id
                        INNER JOIN [dbo].[Users] u ON s.userId = u.Id
                        INNER JOIN tbl_ExpenseType et ON e.ExpenseType = et.Id
                    WHERE
                        e.dateTimePayed BETWEEN @startDate AND @endDate
                        AND s.userId = @userID 
                        AND e.TenantId = @TenantId AND (e.IsDeleted = 0 OR e.IsDeleted IS NULL)
                        AND p.TenantId = @TenantId AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
                        AND pm.TenantId = @TenantId AND (pm.IsDeleted = 0 OR pm.IsDeleted IS NULL)
                        AND s.TenantId = @TenantId AND (s.IsDeleted = 0 OR s.IsDeleted IS NULL)
                        AND u.TenantId = @TenantId AND (u.IsDeleted = 0 OR u.IsDeleted IS NULL)
                        AND et.TenantId = @TenantId AND (et.IsDeleted = 0 OR et.IsDeleted IS NULL)
                    ORDER BY
                        e.dateTimePayed";

                var result = await _context.Database
                    .SqlQueryRaw<RepExpensesDto>(sql,
                        new SqlParameter("@startDate", startDate),
                        new SqlParameter("@endDate", endDate),
                        new SqlParameter("@userID", UserID),
                        new SqlParameter("@TenantId", TenantId))
                    .ToListAsync();

                return ServiceResult<List<RepExpensesDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report: {ex}", ex);
                return ServiceResult<List<RepExpensesDto>>.Failure(new ServerErrorException("Error while generating report."));
            }
        }

        #endregion

        #region for getting Expenses
        public async Task<ServiceResult<List<RepExpensesDto>>> GetExpensesAsync(DateTime startDate, DateTime endDate, string? expenseTypeId, string? userId)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepExpensesDto>>.Failure(
                        new BadRequestException("Start date cannot be later than end date.")
                    );
                }

                if (!string.IsNullOrEmpty(expenseTypeId))
                {
                    bool expenseTypeExists = await _context.tbl_ExpenseTypes
                        .AnyAsync(et => et.Id == expenseTypeId);
                    if (!expenseTypeExists)
                    {
                        return ServiceResult<List<RepExpensesDto>>.Failure(
                            new NotFoundException($"Expense type with ID {expenseTypeId} not found.")
                        );
                    }
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    bool userExists = await _context.Users
                        .AnyAsync(u => u.Id == userId);
                    if (!userExists)
                    {
                        return ServiceResult<List<RepExpensesDto>>.Failure(
                            new NotFoundException($"User with ID {userId} not found.")
                        );
                    }
                }

                var result = await (
                    from e in _context.tbl_Expenses
                    join p in _context.tbl_Payments on e.Id equals p.ExpenseId
                    join pm in _context.tbl_PaymentModes on p.PaymentModeId equals pm.Id
                    join s in _context.tbl_Shifts on e.ShiftId equals s.Id
                    join u in _context.Users on s.UserId equals u.Id
                    join et in _context.tbl_ExpenseTypes on e.ExpenseType equals et.Id
                    where e.DateTimePayed >= startDate
                        && e.DateTimePayed <= endDate
                        && et.Description != null
                        && (string.IsNullOrEmpty(expenseTypeId) || et.Id == expenseTypeId)
                        && (string.IsNullOrEmpty(userId) || u.Id == userId)
                    orderby e.DateTimePayed
                    select new RepExpensesDto
                    {
                        TransDate = e.DateTimePayed ?? DateTime.MinValue,
                        UserName = u.UserName ?? "",
                        ShiftID = e.ShiftId,
                        Amount = e.Amount ?? 0,
                        Reason = e.Comment,
                        PaymentMode = pm.Description,
                        Category = et.Description
                    }
                ).ToListAsync();

                return ServiceResult<List<RepExpensesDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving general expenses for period {StartDate} to {EndDate}", startDate, endDate);
                return ServiceResult<List<RepExpensesDto>>.Failure(new ServerErrorException("An error occurred while retrieving general expenses."));
            }
        }
        #endregion

        #region Method for getting Customer Statement 

        public async Task<ServiceResult<List<RepCustomerStatementDto>>> GetCustomerStatementAsync(DateTime startDate, DateTime endDate, string customerId)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepCustomerStatementDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }
                bool customerExists = await _context.tbl_Customers
               .AnyAsync(c => c.Id == customerId);

                if (!customerExists)
                {
                    return ServiceResult<List<RepCustomerStatementDto>>.Failure(
                        new NotFoundException($"Customer with ID {customerId} not found.")
                    );
                }
                var sql = @"
				WITH CustomerTransactions AS (
				    SELECT
				        t.TransactionDate AS TransDate,
				        'Sale: ' + CONVERT(VARCHAR(150), t.Id) AS Description,
				        t.SaleTotal AS Debit,
				        0 AS Credit,
				        0 AS Balance
				    FROM
				        tbl_transaction t
				    WHERE
				        t.TransactionDate <= @EndDate
				        AND t.CustomerId = @CustomerID
				        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				        AND t.TenantId = @TenantId

				    UNION ALL

				    SELECT
				        cd.DateTimeDeposited AS TransDate,
				        'Payment: ' + CONVERT(VARCHAR(150), cd.Id) AS Description,
				        0 AS Debit,
				        cd.Amount AS Credit,
				        0 AS Balance
				    FROM
				        tbl_customerDeposit cd
				    WHERE
				        cd.DateTimeDeposited <= @EndDate
				        AND cd.Amount > 0
				        AND cd.CustomerId = @CustomerID
				        AND (cd.IsDeleted = 0 OR cd.IsDeleted IS NULL)
				        AND cd.TenantId = @TenantId

				    UNION ALL

				    SELECT
				        t.TransactionDate AS TransDate,
				        'Payment: ' + CONVERT(VARCHAR(150), pm.Description) AS Description,
				        0 AS Debit,
				        p.Amount AS Credit,
				        0 AS Balance
				    FROM
				        tbl_Payments p
				        INNER JOIN tbl_transaction t ON p.SaleId = t.Id
				        INNER JOIN tbl_paymentMode pm ON p.PaymentModeId = pm.PaymentModeID
				    WHERE
				        t.TransactionDate <= @EndDate
				        AND t.CustomerId = @CustomerID
				        AND p.PaymentModeId NOT IN (1, 2)
				        AND p.Amount > 0
				        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				        AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
				        AND (pm.IsDeleted = 0 OR pm.IsDeleted IS NULL)
				        AND t.TenantId = @TenantId
				        AND p.TenantId = @TenantId
				        AND pm.TenantId = @TenantId

				    UNION ALL

				    SELECT
				        t.TransactionDate AS TransDate,
				        'Payment: ' + CONVERT(VARCHAR(150), pm.Description) AS Description,
				        0 AS Debit,
				        (p.Amount - t.Change) AS Credit,
				        0 AS Balance
				    FROM
				        tbl_Payments p
				        INNER JOIN tbl_transaction t ON p.SaleId = t.Id
				        INNER JOIN tbl_paymentMode pm ON p.PaymentModeId = pm.PaymentModeID
				    WHERE
				        t.TransactionDate <= @EndDate
				        AND t.CustomerId = @CustomerID
				        AND p.PaymentModeId = 1
				        AND p.Amount > 0
				        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				        AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
				        AND (pm.IsDeleted = 0 OR pm.IsDeleted IS NULL)
				        AND t.TenantId = @TenantId
				        AND p.TenantId = @TenantId
				        AND pm.TenantId = @TenantId

				    UNION ALL

				    SELECT
				        cd.DateTimeDeposited AS TransDate,
				        'Acc Withdraw ' + CONVERT(VARCHAR(150), cd.Id) AS Description,
				        (0 - cd.Amount) AS Debit,
				        0 AS Credit,
				        0 AS Balance
				    FROM
				        tbl_customerDeposit cd
				    WHERE
				        cd.DateTimeDeposited <= @EndDate
				        AND cd.Amount <= 0
				        AND cd.CustomerId = @CustomerID
				        AND (cd.IsDeleted = 0 OR cd.IsDeleted IS NULL)
				        AND cd.TenantId = @TenantId

				    UNION ALL

				    SELECT
				        r.RefundDateTime AS TransDate,
				        'Refund: ' + CONVERT(VARCHAR(150), r.Id) AS Description,
				        0 AS Debit,
				        r.RefundAmount AS Credit,
				        0 AS Balance
				    FROM
				        tbl_Refunds r
				    WHERE
				        r.RefundDateTime <= @EndDate
				        AND r.ToCustomerId = @CustomerID
				        AND (r.IsDeleted = 0 OR r.IsDeleted IS NULL)
				        AND r.TenantId = @TenantId
				),
				BalanceBF AS (
				    SELECT COALESCE(SUM(Credit - Debit), 0) AS BalanceBf
				    FROM CustomerTransactions
				    WHERE TransDate < @StartDate
				),
				Statement AS (
				    SELECT
				        TransDate,
				        Description,
				        Debit,
				        Credit,
				        BalanceBf AS Balance
				    FROM BalanceBF
				    CROSS JOIN (SELECT @StartDate AS TransDate, 'Balance BF' AS Description, 0 AS Debit, 0 AS Credit) AS BF

				    UNION ALL

				    SELECT
				        TransDate,
				        Description,
				        Debit,
				        Credit,
				        0 AS Balance
				    FROM CustomerTransactions
				    WHERE TransDate >= @StartDate AND TransDate <= @EndDate
				)
				SELECT
				    TransDate,
				    Description,
				    Debit,
				    Credit,
				    SUM(Balance + Debit - Credit) OVER (ORDER BY TransDate ROWS UNBOUNDED PRECEDING) AS Balance
				FROM Statement
				ORDER BY TransDate;";

                var result = await _context.Database
                    .SqlQueryRaw<RepCustomerStatementDto>(sql,
                        new SqlParameter("@StartDate", startDate),
                        new SqlParameter("@EndDate", endDate),
                        new SqlParameter("@CustomerID", customerId),
                        new SqlParameter("@TenantId", TenantId))
                    .ToListAsync();

                return ServiceResult<List<RepCustomerStatementDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report: {ex}", ex);
                return ServiceResult<List<RepCustomerStatementDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }

        #endregion

        #region Method for getting Customer Statement

        public async Task<ServiceResult<decimal>> GetCustomerBalanceAsync(string customerId, DateTime endDate)
        {
            try
            {
                bool customerExists = await _context.tbl_Customers
                .AnyAsync(c => c.Id == customerId && c.TenantId == TenantId);

                if (!customerExists)
                {
                    return ServiceResult<decimal>.Failure(
                        new NotFoundException($"Customer with ID {customerId} not found.")
                    );
                }
                var sql = @"
				  SELECT COALESCE(SUM(Debit - Credit), 0) AS Value
				  FROM (
				      -- Sales
				      SELECT saleTotal AS Debit, 0 AS Credit
				      FROM tbl_transaction
				      WHERE transactionDate <= @EndDate 
				        AND customerId = @CustomerID
				        AND TenantId = @TenantId 
				        AND (IsDeleted = 0 OR IsDeleted IS NULL)

				      UNION ALL

				      -- Customer Deposits (positive amounts)
				      SELECT 0 AS Debit, Amount AS Credit
				      FROM tbl_customerDeposit
				      WHERE dateTimeDeposited <= @EndDate 
				        AND Amount > 0 
				        AND customerID = @CustomerID
				        AND TenantId = @TenantId 
				        AND (IsDeleted = 0 OR IsDeleted IS NULL)

				      UNION ALL

				      -- Payments (non-cash, non-card)
				      SELECT 0 AS Debit, p.Amount AS Credit
				      FROM tbl_Payments p
				      INNER JOIN tbl_transaction t ON p.saleID = t.Id
				      INNER JOIN tbl_paymentMode pm ON p.PaymentModeID = pm.PaymentModeID
				      WHERE t.transactionDate <= @EndDate 
				        AND t.customerId = @CustomerID
				        AND p.PaymentModeID NOT IN (1, 2) 
				        AND p.Amount > 0
				        AND t.TenantId = @TenantId 
				        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				        AND p.TenantId = @TenantId 
				        AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
				        AND pm.TenantId = @TenantId 
				        AND (pm.IsDeleted = 0 OR pm.IsDeleted IS NULL)

				      UNION ALL

				      -- Cash Payments (adjusted for change)
				      SELECT 0 AS Debit, (p.Amount - t.change) AS Credit
				      FROM tbl_Payments p
				      INNER JOIN tbl_transaction t ON p.saleID = t.Id
				      INNER JOIN tbl_paymentMode pm ON p.PaymentModeID = pm.PaymentModeID
				      WHERE t.transactionDate <= @EndDate 
				        AND t.customerId = @CustomerID
				        AND p.PaymentModeID = 1 
				        AND p.Amount > 0
				        AND t.TenantId = @TenantId 
				        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				        AND p.TenantId = @TenantId 
				        AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
				        AND pm.TenantId = @TenantId 
				        AND (pm.IsDeleted = 0 OR pm.IsDeleted IS NULL)

				      UNION ALL

				      -- Account Withdrawals (negative deposits)
				      SELECT (0 - Amount) AS Debit, 0 AS Credit
				      FROM tbl_customerDeposit
				      WHERE dateTimeDeposited <= @EndDate 
				        AND Amount <= 0 
				        AND customerID = @CustomerID
				        AND TenantId = @TenantId 
				        AND (IsDeleted = 0 OR IsDeleted IS NULL)

				      UNION ALL

				      -- Refunds
				      SELECT 0 AS Debit, refundAmount AS Credit
				      FROM tbl_Refunds
				      WHERE refundDateTime <= @EndDate 
				        AND toCustomerID = @CustomerID
				        AND TenantId = @TenantId 
				        AND (IsDeleted = 0 OR IsDeleted IS NULL)
				  ) AS Transactions";

                var balance = await _context.Database
                    .SqlQueryRaw<decimal>(sql,
                        new SqlParameter("@CustomerID", customerId),
                        new SqlParameter("@EndDate", endDate),
                        new SqlParameter("@TenantId", TenantId))
                    .FirstOrDefaultAsync();

                return ServiceResult<decimal>.Success(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while calculating customer balance: {ex}", ex);
                return ServiceResult<decimal>.Failure(new ServerErrorException("Error while generating report."));
            }
        }

        #endregion

        #region Method for getting Supplier Statement 
        public async Task<ServiceResult<List<RepSupplierStatementDto>>> GetSupplierStatementAsync(DateTime startDate, DateTime endDate, string supplierID)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<RepSupplierStatementDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }
                bool supplierExists = await _context.tbl_Suppliers.AnyAsync(
                    s => s.Id == supplierID);

                if (!supplierExists)
                {
                    return ServiceResult<List<RepSupplierStatementDto>>.Failure(
                        new NotFoundException($"Supplier with ID {supplierID} not found."));
                }

                var sql = @"
					 WITH SupplierTransactions AS (
                    -- Purchases
                    SELECT
                        t.TransactionDate AS TransDate,
                        'Purchase: ' + CAST(t.Id AS VARCHAR(150)) AS Description,
                        t.SaleTotal AS Debit,
                        0 AS Credit,
                        0 AS Balance
                    FROM
                        tbl_Transaction t
                    WHERE
                        t.TransactionDate <= @EndDate
                        AND t.TenantId = @TenantId
                        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
                
                    UNION ALL
                
                    -- Supplier Payments (positive amounts)
                    SELECT
                        sp.DateTimePayed AS TransDate,
                        'Payment: ' + CAST(sp.Id AS VARCHAR(150)) AS Description,
                        0 AS Debit,
                        sp.Amount AS Credit,
                        0 AS Balance
                    FROM
                        tbl_SupplierPayment sp
                    WHERE
                        sp.DateTimePayed <= @EndDate
                        AND sp.Amount > 0
                        AND sp.SupplierId = @SupplierID
                        AND sp.TenantId = @TenantId
                        AND (sp.IsDeleted = 0 OR sp.IsDeleted IS NULL)
                
                    UNION ALL
                
                    -- Payments (non-cash, non-card)
                    SELECT
                        t.TransactionDate AS TransDate,
                        'Payment: ' + CAST(pm.Description AS VARCHAR(150)) AS Description,
                        0 AS Debit,
                        p.Amount AS Credit,
                        0 AS Balance
                    FROM
                        tbl_Payments p
                        INNER JOIN tbl_Transaction t ON p.SaleId = t.Id
                        INNER JOIN tbl_PaymentMode pm ON p.PaymentModeId = pm.PaymentModeId
                    WHERE
                        t.TransactionDate <= @EndDate
                        AND p.PaymentModeId NOT IN (1, 2)
                        AND p.Amount > 0
                        AND t.TenantId = @TenantId
                        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
                        AND p.TenantId = @TenantId
                        AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
                        AND pm.TenantId = @TenantId
                        AND (pm.IsDeleted = 0 OR pm.IsDeleted IS NULL)
                
                    UNION ALL
                
                    -- Cash Payments (adjusted for change)
                    SELECT
                        t.TransactionDate AS TransDate,
                        'Payment: ' + CAST(pm.Description AS VARCHAR(150)) AS Description,
                        0 AS Debit,
                        (p.Amount - t.Change) AS Credit,
                        0 AS Balance
                    FROM
                        tbl_Payments p
                        INNER JOIN tbl_Transaction t ON p.SaleId = t.Id
                        INNER JOIN tbl_PaymentMode pm ON p.PaymentModeId = pm.PaymentModeID
                    WHERE
                        t.TransactionDate <= @EndDate
                        AND p.PaymentModeId = 1
                        AND p.Amount > 0
                        AND t.TenantId = @TenantId
                        AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
                        AND p.TenantId = @TenantId
                        AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
                        AND pm.TenantId = @TenantId
                        AND (pm.IsDeleted = 0 OR pm.IsDeleted IS NULL)
                
                    UNION ALL
                
                    -- Account Withdrawals (negative payments)
                    SELECT
                        sp.DateTimePayed AS TransDate,
                        'Acc Withdraw ' + CAST(sp.Id AS VARCHAR(150)) AS Description,
                        (0 - sp.Amount) AS Debit,
                        0 AS Credit,
                        0 AS Balance
                    FROM
                        tbl_SupplierPayment sp
                    WHERE
                        sp.DateTimePayed <= @EndDate
                        AND sp.Amount <= 0
                        AND sp.SupplierId = @SupplierID
                        AND sp.TenantId = @TenantId
                        AND (sp.IsDeleted = 0 OR sp.IsDeleted IS NULL)
                
                    UNION ALL
                
                    -- Refunds
                    SELECT
                        r.RefundDateTime AS TransDate,
                        'Refund: ' + CAST(r.Id AS VARCHAR(150)) AS Description,
                        0 AS Debit,
                        r.RefundAmount AS Credit,
                        0 AS Balance
                    FROM
                        tbl_Refunds r
                    WHERE
                        r.RefundDateTime <= @EndDate
                        AND r.ToCustomerId = @SupplierID
                        AND r.TenantId = @TenantId
                        AND (r.IsDeleted = 0 OR r.IsDeleted IS NULL)
                ),
                BalanceBF AS (
                    SELECT COALESCE(SUM(Credit - Debit), 0) AS BalanceBf
                    FROM SupplierTransactions
                    WHERE TransDate < @StartDate
                ),
                Statement AS (
                    SELECT
                        TransDate,
                        Description,
                        Debit,
                        Credit,
                        BalanceBf AS Balance
                    FROM BalanceBF
                    CROSS JOIN (SELECT @StartDate AS TransDate, 'Balance BF' AS Description, 0 AS Debit, 0 AS Credit) AS BF
                
                    UNION ALL
                
                    SELECT
                        TransDate,
                        Description,
                        Debit,
                        Credit,
                        0 AS Balance
                    FROM SupplierTransactions
                    WHERE TransDate >= @StartDate AND TransDate <= @EndDate
                )
                SELECT
                    TransDate,
                    Description,
                    Debit,
                    Credit,
                    SUM(Balance + Debit - Credit) OVER (ORDER BY TransDate ROWS UNBOUNDED PRECEDING) AS Balance
                FROM Statement
                ORDER BY TransDate;
					";

                var result = await _context.Database
                    .SqlQueryRaw<RepSupplierStatementDto>(sql,
                        new SqlParameter("@StartDate", startDate),
                        new SqlParameter("@EndDate", endDate),
                        new SqlParameter("@SupplierID", supplierID),
                        new SqlParameter("@TenantId", TenantId))
                    .ToListAsync();

                return ServiceResult<List<RepSupplierStatementDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report: {ex}", ex);
                return ServiceResult<List<RepSupplierStatementDto>>.Failure(new ServerErrorException("Error while generating report."));
            }
        }

        #endregion

        #region Method for Getting Database bakup files in a folder
        public async Task<ServiceResult<List<string>>> GetNumberOfFilesInBackupFolder(string backUpFolderPath)
        {
            try
            {
                List<string> listOfBackupNames = new List<string>();
                string sql = @"USE master; 
                            -- To allow advanced options to be changed.
                            EXECUTE sp_configure 'show advanced options', 1; 
                            
                            -- To update the currently configured value for advanced options. 
                            RECONFIGURE;                             
                            
                            -- To enable the feature. 
                            EXEC sp_configure 'xp_cmdshell', 1; 
                            
                            -- To update the currently configured value for this feature. 
                            RECONFIGURE;
                            EXEC xp_cmdshell 'dir /b *.bak """ + backUpFolderPath + @""" '";

                var result = await _context.Database
                    .SqlQueryRaw<object>(sql).ToListAsync();


                for (int i = 0; i < result.Count; i++)
                {
                    if (!(result[0].ToString() == "File Not Found") || string.IsNullOrEmpty(result[0].ToString()))
                    {
                        listOfBackupNames.Add(result[i].ToString());
                    }
                }


                return ServiceResult<List<string>>.Success(listOfBackupNames);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report : {ex}", ex);
                return ServiceResult<List<string>>.Failure(new ServerErrorException("Error while generating report."));
            }



        }
        #endregion

        #region Method for deleting file from server folder using a full path on the server
        public async Task<ServiceResult<bool>> DeleteAfileFromServerPC(string fullFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(fullFilePath)) return ServiceResult<bool>.Failure(new BadRequestException("File path can not be null"));
                if (!fullFilePath.EndsWith(".bak") || !fullFilePath.ToLower().Contains("mowt")) return ServiceResult<bool>.Failure(new BadRequestException("Invalid file Path"));

                string sql = @" USE master; 
                                EXEC xp_cmdshell 'del """ + fullFilePath + @""" '";

                var result = await _context.Database
                    .ExecuteSqlRawAsync(sql);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running report : {ex}", ex);
                return ServiceResult<bool>.Failure(new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Get top selling products
        public async Task<ServiceResult<List<ProductSalesDto>>> GetTopSellingProducts(
            DateTime startDate, DateTime endDate, int topN)
        {
            try
            {
                if (startDate > endDate)
                    return ServiceResult<List<ProductSalesDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date."));

                string sql = @"
				    WITH ProductSales AS (
				        SELECT 
				            p.productName,
				            SUM(td.totalPriceInc) AS TotalPriceInc,
				            (SUM(td.totalPriceInc) * 100.0 / 
				                SUM(SUM(td.totalPriceInc)) OVER ()) AS Percentage
				        FROM tbl_transactionDetail td
				        INNER JOIN tbl_transaction t ON td.transactionID = t.Id
				        INNER JOIN tbl_products p ON td.productID = p.Id
				        WHERE t.transactionDate BETWEEN @StartDate AND @EndDate
				            AND t.transactionStatus IN (10, 11, 13)
				            AND t.TenantId = @TenantId
				            AND p.TenantId = @TenantId
				            AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				            AND (td.IsDeleted = 0 OR td.IsDeleted IS NULL)
				        GROUP BY p.productName
				    )
				    SELECT TOP (@TopN) 
				        productName AS ProductName,
				        TotalPriceInc,
				        Percentage
				    FROM ProductSales
				    ORDER BY TotalPriceInc DESC";

                var parameters = new[]
                {
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@TopN", topN),
                    new SqlParameter("@TenantId", _tenantProvider.GetTenantId())
                };

                var result = await _context.Database
                    .SqlQueryRaw<ProductSalesDto>(sql, parameters)
                    .ToListAsync();

                return ServiceResult<List<ProductSalesDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching top selling products: {ex}", ex);
                return ServiceResult<List<ProductSalesDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

        #region Get Sales Per Day 
        public async Task<ServiceResult<List<SalesPerDayDto>>> GetUserSalesPerDayReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ServiceResult<List<SalesPerDayDto>>.Failure(
                        new BadRequestException("Start date cannot be greater than end date.")
                    );
                }

                string sql = @"
				    WITH DailyTotals AS (
				        SELECT
				            CAST(t.transactionDate AS DATE) AS [Date],
				            SUM(td.priceInc) AS DailyTotal
				        FROM tbl_transactionDetail td
				        INNER JOIN tbl_transaction t ON td.transactionID = t.Id
				        WHERE t.transactionDate BETWEEN @StartDate AND @EndDate
				            AND t.transactionStatus IN (10, 11, 13)
				            AND t.TenantId = @TenantId
				            AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				            AND td.TenantId = @TenantId
				            AND (td.IsDeleted = 0 OR td.IsDeleted IS NULL)
				        GROUP BY CAST(t.transactionDate AS DATE)
				    ),
				    UserSales AS (
				        SELECT
				            CAST(t.transactionDate AS DATE) AS [Date],
				            u.UserName,
				            COUNT(t.Id) AS NoOfTransactions,
				            SUM(td.priceInc) AS TotalPriceInc,
				            SUM(td.totalPriceExc - (td.costExc * td.qty)) AS Profit
				        FROM tbl_transactionDetail td
				        INNER JOIN tbl_transaction t ON td.transactionID = t.Id
				        INNER JOIN Users u ON t.soldBy = u.Id
				        WHERE t.transactionDate BETWEEN @StartDate AND @EndDate
				            AND t.transactionStatus IN (10, 11, 13)
				            AND t.TenantId = @TenantId
				            AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)
				            AND td.TenantId = @TenantId
				            AND (td.IsDeleted = 0 OR td.IsDeleted IS NULL)
				        GROUP BY CAST(t.transactionDate AS DATE), u.UserName
				    )
				    SELECT 
				        us.[Date],
				        us.UserName,
				        us.NoOfTransactions,
				        us.TotalPriceInc,
				        us.Profit,
				        (us.TotalPriceInc * 100.0) / dt.DailyTotal AS Percentage
				    FROM UserSales us
				    INNER JOIN DailyTotals dt ON us.[Date] = dt.[Date]
				    ORDER BY us.[Date], us.UserName;";

                var parameters = new[]
                {
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@TenantId", TenantId),
                };

                var result = await _context.Database
                    .SqlQueryRaw<SalesPerDayDto>(sql, parameters)
                    .ToListAsync();

                return ServiceResult<List<SalesPerDayDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while running user sales per day report: {ex}", ex);
                return ServiceResult<List<SalesPerDayDto>>.Failure(
                    new ServerErrorException("Error while generating report."));
            }
        }
        #endregion

    }
}
