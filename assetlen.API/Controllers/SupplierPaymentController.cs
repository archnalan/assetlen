using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
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
	public class SupplierPaymentController : ControllerBase
	{
		private readonly ISupplierPaymentDAL _supplierPayDAL;

		public SupplierPaymentController(ISupplierPaymentDAL supplierPayDAL)
		{
			_supplierPayDAL = supplierPayDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<SupplierPaymentDto>), 200)]
		public async Task<ActionResult> GetSupplierPaymentSUMLowerThanEndDate(string SupplierID, DateTime EndDate)
		{
			var result = await _supplierPayDAL.GetSupplierPaymentSUMLowerThanEndDate(SupplierID, EndDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<SupplierPaymentDto>), 200)]
		public async Task<ActionResult> GetSupplierInvoiceSUMUsingSupplierIDAndEndDate(string SupplierID, DateTime EndDate)
		{
			var result = await _supplierPayDAL.GetSupplierInvoiceSUMUsingSupplierIDAndEndDate(SupplierID, EndDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(SupplierPaymentDto), 200)]
		public async Task<ActionResult> AddSupplierPaymentToDB([Required] SupplierPaymentDto spDto)
		{
			var result = await _supplierPayDAL.AddSupplierPaymentToDB(spDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

	}
}
