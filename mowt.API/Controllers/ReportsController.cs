using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ReportingDto;
using mowt.Shared.Models.statics;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	// REMOVED: UserRoles.CustomerManagement was removed; access now restricted to GenerateReports only.
	[Authorize(Roles = $"{UserRoles.GenerateReports}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class ReportsController : ControllerBase
	{
		private readonly IReportsDAL _reportsDAL;

		public ReportsController(IReportsDAL reportsDAL)
		{
			_reportsDAL = reportsDAL;
		}


		[HttpDelete]
		public async Task<ActionResult> DeleteAfileFromServerPC(string fullFilePath)
		{

			var result = await _reportsDAL.DeleteAfileFromServerPC(fullFilePath);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetCustomerBalance([FromQuery] DateTime endDate, [FromQuery] string customerID)
		{
			var result = await _reportsDAL.GetCustomerBalanceAsync(customerID, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepCustomerStatementDto>), 200)]
		public async Task<ActionResult> GetCustomerStatement([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string customerID)
		{

			var result = await _reportsDAL.GetCustomerStatementAsync(startDate, endDate, customerID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepExpensesDto>), 200)]
		public async Task<ActionResult> GetExpensesPerUser([FromQuery] string UserID, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{

			var result = await _reportsDAL.GetExpensesPerUserAsync(UserID, startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepExpensesDto>), 200)]
		public async Task<ActionResult> GetExpenses([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? expenseTypeId, [FromQuery] string? userId)
		{
			var result = await _reportsDAL.GetExpensesAsync(startDate, endDate, expenseTypeId, userId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<string>), 200)]
		public async Task<ActionResult> GetNumberOfFilesIntheBackupFolder(string backUpFolderPath)
		{

			var result = await _reportsDAL.GetNumberOfFilesInBackupFolder(backUpFolderPath);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepProductPurchasesDto>), 200)]
		public async Task<ActionResult> GetProductPurchases([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? SupplierID, [FromQuery] string? CategoryID, [FromQuery] string? SegmentID, [FromQuery] string? keyWords)
		{
			var result = await _reportsDAL.GetProductPurchasesAsyc(startDate, endDate, SupplierID ?? "", CategoryID ?? "", SegmentID ?? "", keyWords);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepProductInStockDto>), 200)]
		public async Task<ActionResult> GetProductsInStockReport([FromQuery] string? supplierId, [FromQuery] string? categoryId, [FromQuery] string? segmentId)
		{

			var result = await _reportsDAL.GetProductsInStockReport(supplierId ?? "", categoryId ?? "", segmentId ?? "");

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepProductSalesDto>), 200)]
		public async Task<ActionResult> GetProductsSoldReport(DateTime startDate, DateTime endDate)
		{

			var result = await _reportsDAL.GetProductsSoldReport(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepSalesPerCustomerDto>), 200)]
		public async Task<ActionResult> GetSalesPerCustomer([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string CustomerID)
		{
			var result = await _reportsDAL.GetSalesPerCustomerAsync(startDate, endDate, CustomerID);

			if (!result.IsSuccess)
			{
				return StatusCode(result.StatusCode, result.Error);
			}

			return Ok(result.Data);
		}


		[HttpGet]
		[ProducesResponseType(typeof(List<RepProductInStockDto>), 200)]
		public async Task<ActionResult> GetSaleDetail([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? userId)
		{
			var result = await _reportsDAL.GetSaleDetail(startDate, endDate, userId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepSalesPerCategoryAndSegmentDto>), 200)]
		public async Task<ActionResult> GetSalesPerCategoryAndSegment([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? CategoryID, [FromQuery] string? SegmentID)
		{

			var result = await _reportsDAL.GetSalesPerCategoryAndSegmentAsync(startDate, endDate, CategoryID ?? "", SegmentID ?? "");

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(SalesPerDayReportResponse), 200)]
		public async Task<ActionResult> GetSalesPerDayReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{

			var result = await _reportsDAL.GetSalesPerDayReport(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepSalesPerDayDashboardDto>), 200)]
		public async Task<ActionResult> GetSalesPerDayDashboard([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{
			var result = await _reportsDAL.GetSalesPerDayDashboard(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<MonthlySalesDto>), 200)]
		public async Task<ActionResult> GetSalesPerMonth([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{
			var result = await _reportsDAL.GetSalesPerMonth(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepStockMovementDto>), 200)]
		public async Task<ActionResult> GetStockMovementReport([FromQuery] string productID, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{

			var result = await _reportsDAL.GetStockMovementReport(productID, startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<RepSupplierStatementDto>), 200)]
		public async Task<ActionResult> GetSupplierStatement([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string supplierID)
		{
			var result = await _reportsDAL.GetSupplierStatementAsync(startDate, endDate, supplierID);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);
			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<YearlySalesDto>), 200)]
		public async Task<ActionResult> GetSalesPerYear([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{
			var result = await _reportsDAL.GetSalesPerYear(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ProductSalesDto>), 200)]
		public async Task<ActionResult> GetTopSellingProducts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int topN)
		{
			var result = await _reportsDAL.GetTopSellingProducts(startDate, endDate, topN);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<SalesPerDayDto>), 200)]
		public async Task<ActionResult> GetUserSalesPerDayReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{
			var result = await _reportsDAL.GetUserSalesPerDayReport(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
	}
}
