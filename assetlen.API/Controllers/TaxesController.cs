using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace assetlen.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	//[Authorize(Roles = $"{UserRoles.SetSystemConfig}",
	//    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class TaxesController : ControllerBase
	{
		private readonly ItaxDAL _taxDAL;

		public TaxesController(ItaxDAL taxDAL)
		{
			_taxDAL = taxDAL;
		}

		[HttpPost]
		[ProducesResponseType(typeof(CustomerDto), 200)]
		public async Task<ActionResult> CreateNewTax(taxDto taxDto)
		{

			var result = await _taxDAL.CreateNewTax(taxDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}


		[HttpGet]
		[ProducesResponseType(typeof(taxDto), 200)]
		[Authorize(Roles = $"{UserRoles.SetSystemConfig},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetTaxFromDBbasedOnTaxID(string taxId)
		{
			var result = await _taxDAL.GetTaxFromDBbasedOnTaxID(taxId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(int), 200)]
		[Authorize(Roles = $"{UserRoles.SetSystemConfig},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetTaxIDFromDBbasedOnTaxDescription(string taxDescription)
		{
			var result = await _taxDAL.GetTaxIDFromDBbasedOnTaxDescription(taxDescription);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<taxDto>), 200)]
		[Authorize(Roles = $"{UserRoles.SetSystemConfig},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetAllTaxFromDB()
		{
			var result = await _taxDAL.GetAllTaxFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(List<taxDto>), 200)]
		[Authorize(Roles = $"{UserRoles.SetSystemConfig},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> SearchTaxesForComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
		{
			int offset1 = offSet ?? 0;
			int limit1 = limit ?? 30;
			string keywords1 = keywords ?? string.Empty;

			var result = await _taxDAL.SearchTaxesForComboBoxes(keywords1, offset1, limit1, sortByColumn, sortAscending, cancellationToken);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(bool), 200)]
		[Authorize(Roles = $"{UserRoles.SetSystemConfig},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetTop1TaxFromSalesDBUsingTaxID(string taxId)
		{
			var result = await _taxDAL.GetTop1TaxFromSalesDBUsingTaxID(taxId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(bool), 200)]
		[Authorize(Roles = $"{UserRoles.SetSystemConfig},{UserRoles.LibraryModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetTop1TaxFromProductsDBUsingTaxID(string taxId)
		{
			var result = await _taxDAL.GetTop1TaxFromProductsDBUsingTaxID(taxId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(taxDto), 200)]
		public async Task<ActionResult> UpdateTaxinDBbasedOnTaxID(taxDto taxDto)
		{
			var result = await _taxDAL.UpdateTaxinDBbasedOnTaxID(taxDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		[ProducesResponseType(typeof(bool), 200)]
		public async Task<ActionResult> DeleteTaxinDBbasedOnTaxID(string taxId)
		{
			var result = await _taxDAL.DeleteTaxinDBbasedOnTaxID(taxId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpDelete]
		[ProducesResponseType(typeof(bool), 200)]
		public async Task<ActionResult> HardDeleteTaxinDBbasedOnID(string taxId)
		{
			var result = await _taxDAL.HardDeleteTaxinDBbasedOnID(taxId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<taxDto>), 200)]
		public async Task<ActionResult> SearchTaxFromDB([FromQuery] string? searchText)
		{
			string search = searchText ?? string.Empty;
			var result = await _taxDAL.SearchTaxFromDB(search);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);
			return Ok(result.Data);
		}
	}
}
