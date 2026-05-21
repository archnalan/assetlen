using System.ComponentModel.DataAnnotations;
using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.statics;
using assetlen.Shared.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace assetlen.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class TransactionsController : ControllerBase
	{
		private readonly ITransactionDAL _transactDAL;

		public TransactionsController(ITransactionDAL customerDAL)
		{
			_transactDAL = customerDAL;
		}
		[HttpPost]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> CreateNewTransaction([FromBody] TransactionDto transact)
		{
			var result = await _transactDAL.CreateNewTransaction(transact);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> UpdateTransactionUsingTransactionID([FromQuery] string id, [FromBody] TransactionDto transact)
		{
			var result = await _transactDAL.UpdateTransactionUsingTransactionID(id, transact);

			if (result.IsSuccess) return Ok(result.Data);
			return StatusCode(result.StatusCode, result.Error);
		}

		[HttpPut]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> UpdateTransactionOrderStatusAndComment([FromBody] TransactionStatusUpdateDto transact)
		{
			var result = await _transactDAL.UpdateTransactionOrderStatusAndComment(transact);

			if (result.IsSuccess) return Ok(result.Data);
			return StatusCode(result.StatusCode, result.Error);
		}

		[HttpPut]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> UpdateTransactionStatusAndCreateNewTransaction([FromBody] TransactionStatusUpdateDto transact)
		{
			var result = await _transactDAL.UpdateTransactionStatusAndCreateNewTransaction(transact);

			if (result.IsSuccess) return Ok(result.Data);

			return StatusCode(result.StatusCode, result.Error);
		}

		[HttpPut]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> AddCustomerToTransaction([Required][FromQuery] string transactionId, [Required][FromQuery] string customerId)
		{
			var result = await _transactDAL.AddCustomerToTransaction(transactionId, customerId);

			if (result.IsSuccess) return Ok(result.Data);

			return StatusCode(result.StatusCode, result.Error);
		}

		[HttpGet]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> GetTransactionById([FromQuery] string id)
		{
			var result = await _transactDAL.GetTransactionFromDB(id);

			if (result.IsSuccess) return Ok(result.Data);

			return StatusCode(result.StatusCode, result.Error);
		}

		[HttpGet]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> GetTransactionWithDetailsFromDB([FromQuery] string id)
		{
			var result = await _transactDAL.GetTransactionWithDetailsFromDB(id);

			if (result.IsSuccess) return Ok(result.Data);
			return StatusCode(result.StatusCode, result.Error);
		}
		[HttpGet]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> RefundTransaction([FromQuery] string id)
		{
			var result = await _transactDAL.RefundTransaction(id);

			if (result.IsSuccess) return Ok(result.Data);
			return StatusCode(result.StatusCode, result.Error);
		}

		[HttpGet]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> GetCompletedTransactionFromDBUsingID(string saleId)
		{
			var result = await _transactDAL.GetCompletedTransactionFromDBUsingID(saleId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<TransactionDto>), 200)]
		public async Task<ActionResult> GetTransactionsFromDB([FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] bool? completed, [FromQuery] int? saleStatus, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true, [FromQuery] CancellationToken cancellation = default)
		{
			int offset1 = offSet ?? 0;
			int limit1 = limit ?? 30;

			var result = await _transactDAL.GetTransactionsFromDB(offset1, limit1, completed, saleStatus, sortByColumn, sortAscending, cancellation);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<TransactionDto>), 200)]
		public async Task<ActionResult> SearchTransactionsFromDB([FromQuery] string? keywords, [FromQuery] string? userId, [FromQuery] string? shiftId, [FromQuery] string? customerId, [FromQuery] string? orderStatus, [FromQuery] bool? completed, [FromQuery] int? saleStatus, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] string sortByColumn = "", [FromQuery] bool? sortAscending = true, [FromQuery] CancellationToken cancellation = default)
		{
			int offset1 = offSet ?? 0;
			int limit1 = limit ?? 30;
			string keywords1 = keywords ?? "";
			string userId1 = userId ?? "";
			string shiftId1 = shiftId ?? "";
			string customerId1 = customerId ?? "";
			string orderStatus1 = orderStatus ?? "";
			//completed = null; for all transactions
			DateTime start1 = startDate ?? DateTime.MinValue;
			DateTime end1 = endDate ?? DateTime.UtcNow;

			var result = await _transactDAL.SearchTransactionsFromDB(keywords1, userId1, shiftId1, customerId1, orderStatus1, completed, saleStatus, start1, end1, offset1, limit1, sortByColumn, sortAscending ?? true, cancellation);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<TransactionDto>), 200)]
		public async Task<ActionResult> GetCompletedTransactionsFromDBUsingDateRange(DateTime startDate, DateTime endDate, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
		{
			int offset1 = offSet ?? 0;
			int limit1 = limit ?? 30;

			var result = await _transactDAL.GetCompletedTransactionsFromDBUsingDateRange(startDate, endDate, offset1, limit1, cancellation, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<TransactionDto>), 200)]
		public async Task<ActionResult> GetPendingTransactionFromDBUsingUserID(string UserID)
		{
			var result = await _transactDAL.GetPendingTransactionFromDBUsingUserID(UserID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<TransactionDto>), 200)]
		public async Task<ActionResult> SearchPendingTransactions(string keywords, string UserID, int transactionStatus, int OrderStatus)
		{
			var result = await _transactDAL.SearchPendingTransactions(keywords, UserID, transactionStatus, OrderStatus);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<TransactionDto>), 200)]
		public async Task<ActionResult> GetLastTransactionIDFromDB(string userID)
		{
			var result = await _transactDAL.GetLastTransactionIDFromDB(userID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetCustomerDebitsLowerThanEndDate(TransactionDto t, DateTime EndDate)
		{
			var result = await _transactDAL.GetCustomerDebitsLowerThanEndDate(t, EndDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetTotalRevenueFromDBUsingDateRange(DateTime startDate, DateTime endDate)
		{
			var result = await _transactDAL.GetTotalRevenueFromDBUsingDateRange(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(int), 200)]
		public async Task<ActionResult> GetTotalSalesNoFromDBUsingDateRange(DateTime startDate, DateTime endDate)
		{
			var result = await _transactDAL.GetTotalSalesNoFromDBUsingDateRange(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetSumOfRevenueFromDForToday()
		{
			var result = await _transactDAL.GetSumOfRevenueFromDForToday();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(FileResult), 200)]
		public async Task<ActionResult> GetTransactionsForCSVExportBySelectedFields([Required][FromBody] List<string> selectedColumnNames)
		{
			var result = await _transactDAL.GetTransactionsForCSVExportBySelectedFields(selectedColumnNames);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CategoriesExport.xlsx");
		}

		[HttpPost]
		[ProducesResponseType(typeof(ImportResultSummary), 200)]
		public async Task<ActionResult> ImportTransactionsFromExcel([FromBody] ImportDataDto p, CancellationToken token)
		{
			// Extend the timeout to 300 seconds for this action
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
			cts.CancelAfter(TimeSpan.FromSeconds(300));

			try
			{
				var result = await _transactDAL.ImportTransactionsFromExcel(p);

				if (!result.IsSuccess)
					return StatusCode(result.StatusCode, result.Error);

				return Ok(result.Data);
			}
			catch (OperationCanceledException)
			{
				return StatusCode(408, "Request timed out.");
			}
		}
	}
}
