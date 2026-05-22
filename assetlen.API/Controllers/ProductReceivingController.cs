using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace assetlen.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.Contractor}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class ProductReceivingController : ControllerBase
	{
		private readonly IProductReceivingDAL _receivingDAL;

		public ProductReceivingController(IProductReceivingDAL paymentsDAL)
		{
			_receivingDAL = paymentsDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ProductReceivingDto>), 200)]
		public async Task<ActionResult> GetProductReceivingDetailFromDBPerGRNumber(string GRNumber)
		{
			var result = await _receivingDAL.GetProductReceivingDetailFromDBPerGRNnumber(GRNumber);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ProductReceivingDto>), 200)]
		public async Task<ActionResult> GetProductsReceivedFromDBUsingDateRange(DateTime startDate, DateTime endDate)
		{
			var result = await _receivingDAL.GetProductsReceivedFromDBUsingDateRange(startDate, endDate);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpPost]
		[ProducesResponseType(typeof(ProductReceivingDto), 200)]
		public async Task<ActionResult> AddProductReceivingDetailToDB(ProductReceivingDto prDto)
		{
			var result = await _receivingDAL.AddOProductReceivingDetailToDB(prDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(List<ProductReceivingDto>), 200)]
		public async Task<ActionResult> ReceiveMultipleProducts([FromBody] ReceivingStockData recData)
		{
			var result = await _receivingDAL.ReceiveMultipleProducts(recData.productsReceiving, recData.stockParams, recData.costChanges);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);
			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ProductReceivingDto>), 200)]
		public async Task<ActionResult> GetProductsReceivedFromDBUsingDateRangeAndGRNSupplierNumber(DateTime startDate, DateTime endDate, string GRNumber)
		{
			var result = await _receivingDAL.GetProductsReceivedFromDBUsingDateRangeAndGRNSupplierNumber(startDate, endDate, GRNumber);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<ProductReceivingDto>), 200)]
		public async Task<ActionResult> SearchProductReceivingDetailFromDB(string? receiveStockId, string? supplierAccount, string? keywords = "", string? barCode = "", int? offset = 0, int? limit = 10, CancellationToken token = default)
		{
			int offset1 = offset ?? 0;
			int limit2 = limit ?? 10;
			var result = await _receivingDAL.SearchProductReceivingDetailFromDB(receiveStockId, supplierAccount, keywords, barCode, offset1, limit2, token);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}


	}
}
