using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.SupplierMgt}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class SuppliersController : ControllerBase
	{
		private readonly ISupplierDAL _supplierDAL;

		public SuppliersController(ISupplierDAL supplierDAL)
		{
			_supplierDAL = supplierDAL;
		}

		[HttpPost]
		[ProducesResponseType(typeof(SupplierDto), 200)]
		public async Task<ActionResult> AddSupplierToDB([FromBody] SupplierDto supplierDto)
		{

			var result = await _supplierDAL.AddSupplierToDB(supplierDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpDelete]
		public async Task<ActionResult> deleteSuppierUsingSupplierID(string supplierId)
		{

			var result = await _supplierDAL.deleteSuppierUsingSupplierID(supplierId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<SupplierDto>), 200)]
		[Authorize(Roles = $"{UserRoles.SupplierMgt},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetSUpplierFromDB([FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
		{
			int offset1 = offset ?? 0;
			int limit1 = limit ?? 30;

			var result = await _supplierDAL.GetSUpplierFromDB(offset1, limit1, cancellation, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
		[Authorize(Roles = $"{UserRoles.SupplierMgt},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> SearchSupplierFromDbForComboBoxes([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
		{
			int offset1 = offset ?? 0;
			int limit1 = limit ?? 30;

			string keywords1 = keywords ?? string.Empty;
			var result = await _supplierDAL.SearchSupplierFromDbForComboBoxes(keywords1, offset1, limit1, cancellation, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(int), 200)]
		[Authorize(Roles = $"{UserRoles.SupplierMgt},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetSupplierIDFromDBbasedOnSuplierName(string supplierName)
		{

			var result = await _supplierDAL.GetSupplierIDFromDBbasedOnSuplierName(supplierName);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(SupplierDto), 200)]
		[Authorize(Roles = $"{UserRoles.SupplierMgt},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetSuppliersFromDbBasedOnSupplierID(string supplierId)
		{

			var result = await _supplierDAL.GetSuppliersFromDBbasedOnSuplierID(supplierId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<SupplierDto>), 200)]
		[Authorize(Roles = $"{UserRoles.SupplierMgt},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> SearchSupplierUsingKeywords([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
		{
			int offset1 = offset ?? 0;
			int limit1 = limit ?? 30;
			string keywords1 = keywords ?? "";
			var result = await _supplierDAL.SearchSupplierUsingKeywords(keywords1, offset1, limit1, cancellation, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(SupplierDto), 200)]
		public async Task<ActionResult> UpdateSupplierUsingSupplierID(SupplierDto supplierDto)
		{

			var result = await _supplierDAL.UpdateSupplierUsingSupplierID(supplierDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}


		[HttpPost]
		[ProducesResponseType(typeof(FileResult), 200)]
		public async Task<ActionResult> GetSuppliersForCSVExportBySelectedFields([Required][FromBody] List<string> selectedColumnNames)
		{
			var result = await _supplierDAL.GetSuppliersForCSVExportBySelectedFields(selectedColumnNames);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CategoriesExport.xlsx");
		}

		[HttpPost]
		[ProducesResponseType(typeof(ImportResultSummary), 200)]
		public async Task<ActionResult> ImportSuppliersFromExcel([FromBody] ImportDataDto p, CancellationToken token)
		{
			// Extend the timeout to 300 seconds for this action
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
			cts.CancelAfter(TimeSpan.FromSeconds(300));

			try
			{
				var result = await _supplierDAL.ImportSuppliersFromExcel(p);

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
