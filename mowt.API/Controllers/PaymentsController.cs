using mowt.Service.DbServices;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.LibraryModuleLogin},{UserRoles.AdminModuleLogin}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class PaymentsController : ControllerBase
	{
		private readonly IPaymentsDAL _paymentsDAL;

		public PaymentsController(IPaymentsDAL paymentsDAL)
		{
			_paymentsDAL = paymentsDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentsDto>), 200)]
		public async Task<ActionResult> GetPaymentsFromDB()
		{
			var result = await _paymentsDAL.GetPaymentsFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentsDto>), 200)]
		public async Task<ActionResult> GetPaymentsBasedOnSaleID([FromQuery] string saleId)
		{
			var result = await _paymentsDAL.GetPaymentsBasedOnSaleID(saleId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(decimal), 200)]
		public async Task<ActionResult> GetSumOfPaymentsBasedOnSaleID([FromQuery] string saleId)
		{
			var result = await _paymentsDAL.GetSumOfPaymentsBasedOnSaleID(saleId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(PaymentsDto), 200)]
		public async Task<ActionResult> AddPayements([FromBody] PaymentsDto pay)
		{

			var result = await _paymentsDAL.AddPayments(pay);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpPost]
		[ProducesResponseType(typeof(TransactionDto), 200)]
		public async Task<ActionResult> AddPaymentsAndCloseSale([FromBody] List<PaymentsDto> payments)
		{

			var result = await _paymentsDAL.AddPaymentsAndCloseSale(payments);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		public async Task<ActionResult> DeleteCashItem([FromQuery] string saleId)
		{
			var result = await _paymentsDAL.DeletePayment(saleId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentsDto>), 200)]
		public async Task<ActionResult> GetPaymentModeNameUsingID(string payId)
		{
			var result = await _paymentsDAL.GetPaymentModeNameUsingID(payId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentModeDto>), 200)]
		public async Task<ActionResult> GetAllPaymentModes()
		{
			var result = await _paymentsDAL.GetAllPaymentModes();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentsDto>), 200)]
		public async Task<ActionResult> GetCARDPaymentAccountFromDB()
		{
			var result = await _paymentsDAL.GetCARDPaymentAccountFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentsDto>), 200)]
		public async Task<ActionResult> SearchCARDPaymentAccountFromDBUsingKeyword(string keywords)
		{
			var result = await _paymentsDAL.SearchCARDPaymentAccountFromDBUsingKeyword(keywords);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentsDto>), 200)]
		public async Task<ActionResult> GetBANKPaymentAccountFromDB()
		{
			var result = await _paymentsDAL.GetBANKPaymentAccountFromDB();

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PaymentsDto>), 200)]
		public async Task<ActionResult> SearchBANKAccountFromDBUsingKeyword(string keywords)
		{
			var result = await _paymentsDAL.SearchBANKAccountFromDBUsingKeyword(keywords);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}


	}
}
