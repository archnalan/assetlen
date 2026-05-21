using mowt.Service.DbServices;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	//[Authorize(Roles = $"{UserRoles.ProductConfig}",
	//    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class GenerateBarCodeController : ControllerBase
	{
		private readonly IGenerateBarcodeDAL _barcodeDAL;

		public GenerateBarCodeController(IGenerateBarcodeDAL barcodeDAL)
		{
			_barcodeDAL = barcodeDAL;
		}

		[HttpPost]
		[ProducesResponseType(typeof(UniqueFieldDto), 200)]
		public async Task<ActionResult> CreateBarcodeNumberInDB(UniqueFieldDto UniqueBarcodeNumber)
		{
			var result = await _barcodeDAL.CreateBarcodeNumberInDB(UniqueBarcodeNumber);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<UniqueFieldDto>), 200)]
		public async Task<ActionResult> GetUniqueBarcodeNumberFromDB()
		{
			var result = await _barcodeDAL.GetUniqueBarcodeNumberFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(string), 200)]
		public async Task<ActionResult> GenerateNextBarcode([MinLength(5)] string companyCode = "")
		{
			var result = await _barcodeDAL.GenerateNextBarcode(companyCode);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<string>), 200)]
		public async Task<ActionResult> GenerateBarcodes(int n, string companyCode = "")
		{
			var result = await _barcodeDAL.GenerateBarcodes(n, companyCode);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(UniqueFieldDto), 200)]
		public async Task<ActionResult> UpdateBarcodeNumberInDB(UniqueFieldDto uniqueFieldDto)
		{
			var result = await _barcodeDAL.UpdateBarcodeNumberInDB(uniqueFieldDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
	}
}
