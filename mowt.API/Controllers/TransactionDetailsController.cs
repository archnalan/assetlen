using mowt.Service.DbServices;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using mowt.Shared.Models.statics;
using mowt.Shared.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.LibraryModuleLogin}, {UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class TransactionDetailsController : ControllerBase
	{
		private readonly ITransactionDetailDAL _tDetailsDAL;

		public TransactionDetailsController(ITransactionDetailDAL tDetailsDAL)
		{
			_tDetailsDAL = tDetailsDAL;
		}

		[HttpPost]
		[ProducesResponseType(typeof(List<TransactionDetailDto>), 200)]
		public async Task<ActionResult> CreateOrSyncNewTransactionDetails(List<TransactionDetailDto> tdDto)
		{
			var result = await _tDetailsDAL.CreateOrSyncNewTransactionDetails(tdDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(TransactionDetailDto), 200)]
		public async Task<ActionResult> UpdateTransactionDetail(TransactionDetailDto tdDto)
		{
			var result = await _tDetailsDAL.UpdateTransactionDetail(tdDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<TransactionDetailDto>), 200)]
		public async Task<ActionResult> GetTransactionDetailBasedOnTransactionID([Required][FromQuery] string transId, [FromQuery] bool? completed, [FromQuery] string? statusOrder, [FromQuery] int? saleStatus)
		{
			var result = await _tDetailsDAL.GetTransactionDetailBasedOnTransactionID(transId, completed, statusOrder, saleStatus);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<TransactionDetailDto>), 200)]
		public async Task<ActionResult> GetTransactionDetailBasedOnTransactionIDandSpecialPricing(string transId, bool spPricing)
		{
			var result = await _tDetailsDAL.GetTransactionDetailBasedOnTransactionIDandSpecialPricing(transId, spPricing);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(List<TransactionDetailDto>), 200)]
		public async Task<ActionResult> GetTransactionDetailBasedOnTransactionIDandProdID(string transId, string prodId)
		{
			var result = await _tDetailsDAL.GetTransactionDetailBasedOnTransactionIDandProdID(transId, prodId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(TransactionDetailDto), 200)]
		public async Task<ActionResult> GetTransactionDetailWithRelatedDataFromDB(string detailId)
		{
			var result = await _tDetailsDAL.GetTransactionDetailWithRelatedDataFromDB(detailId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetTransactionTotalInc(string transId)
		{
			var result = await _tDetailsDAL.GetTransactionTotalInc(transId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetTransactionTotalExc(string transId)
		{
			var result = await _tDetailsDAL.GetTransactionTotalExc(transId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		public async Task<ActionResult> DeleteTransactionDetailBasedOnTransactionID(string transId)
		{
			var result = await _tDetailsDAL.DeleteTransactionDetailBasedOnTransactionID(transId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpDelete]
		public async Task<ActionResult> DeleteTransactionDetailPerDetailID(string detailID)
		{
			var result = await _tDetailsDAL.DeleteTransactionDetailPerDetailID(detailID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(TransactionDetailDto), 200)]
		public async Task<ActionResult> GetTransactionDetailBasedOnDetailID(string detailID)
		{
			var result = await _tDetailsDAL.GetTransactionDetailBasedOnDetailID(detailID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(FileResult), 200)]
		public async Task<ActionResult> GetTransactionDetailsForCSVExportBySelectedFields([Required][FromBody] List<string> selectedColumnNames)
		{
			var result = await _tDetailsDAL.GetTransactionDetailsForCSVExportBySelectedFields(selectedColumnNames);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CategoriesExport.xlsx");
		}

		[HttpPost]
		[ProducesResponseType(typeof(ImportResultSummary), 200)]
		public async Task<ActionResult> ImportTransactionDetailsFromExcel([FromBody] ImportDataDto p, CancellationToken token)
		{
			// Extend the timeout to 300 seconds for this action
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
			cts.CancelAfter(TimeSpan.FromSeconds(300));

			try
			{
				var result = await _tDetailsDAL.ImportTransactionDetailsFromExcel(p);

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
