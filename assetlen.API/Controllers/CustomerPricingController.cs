using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
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
	public class CustomerPricingController : ControllerBase
	{
		private readonly ICustomerPricingDAL _pricingDAL;

		public CustomerPricingController(ICustomerPricingDAL pricingDAL)
		{
			_pricingDAL = pricingDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<PricingsDto>), 200)]
		public async Task<ActionResult> GetAllCustomerBasedPricingFromDB([FromQuery] int offset = 0, [FromQuery] int limit = 100, [FromQuery] string sortByColumn = "Id", [FromQuery] bool sortAscending = true, CancellationToken cancellationToken = default)
		{
			var result = await _pricingDAL.GetAllCustomerBasedPricingFromDB(offset, limit, cancellationToken, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PricingsDto>), 200)]
		public async Task<ActionResult> GetPricingListByCustomerIdAndProductId(string prodId, string custId)
		{
			var result = await _pricingDAL.GetPricingListByCustomerIdAndProductId(prodId, custId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PricingsDto), 200)]
		[Authorize(Roles = $"{UserRoles.Crew}, {UserRoles.Contractor}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetCustomerPricingByPricingId([FromQuery] string id)
		{
			var result = await _pricingDAL.GetCustomerPricingByID(id);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(PricingsDto), 200)]
		public async Task<ActionResult> CreateNewCustomerPricing([FromBody] PricingsDto pDto)
		{
			var result = await _pricingDAL.AddCustomerPricing(pDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(PricingsDto), 200)]
		public async Task<ActionResult> UpdateCustomerPricing([FromBody] PricingsDto c)
		{
			var result = await _pricingDAL.UpdateCustomerPricing(c);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		public async Task<ActionResult> RemoveCustomerPricingOnProduct([FromQuery] string productId)
		{
			var result = await _pricingDAL.DeleteCustomerPricingListByProductId(productId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpDelete]
		public async Task<ActionResult> RemoveCustomerPricingOnCustomer([FromQuery] string customerId)
		{
			var result = await _pricingDAL.DeleteCustomerPricingListByCustomerId(customerId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpDelete]
		public async Task<ActionResult> RemoveCustomerPricingById([FromQuery] string id)
		{
			var result = await _pricingDAL.DeleteCustomerPricingById(id);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<PricingsDto>), 200)]
		[Authorize(Roles = $"{UserRoles.Crew}, {UserRoles.Contractor}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetCustomerPricingByCustomerId([FromQuery] string customerId)
		{
			var result = await _pricingDAL.GetCustomerPricingListByCustomerID(customerId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[Authorize(Roles = $"{UserRoles.Crew}, {UserRoles.Contractor}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		[HttpGet]
		[ProducesResponseType(typeof(List<PricingsDto>), 200)]
		public async Task<ActionResult> GetCustomerPricingListByProductID([FromQuery] string productId)
		{
			var result = await _pricingDAL.GetCustomerPricingListByProductID(productId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<PricingsDto>), 200)]
		[Authorize(Roles = $"{UserRoles.Crew}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		// REMOVED: UserRoles.PriceChange was removed from this method's allowed roles.
		public async Task<ActionResult> SearchCustomerBasedPricingInDb([FromQuery] string keywords, [FromQuery] int offset = 0, [FromQuery] int limit = 100, [FromQuery] string sortByColumn = "Id", [FromQuery] bool sortAscending = true, CancellationToken cancellationToken = default)
		{
			var result = await _pricingDAL.SearchCustomerBasedPricingInDb(keywords, offset, limit, cancellationToken, sortByColumn, sortAscending);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPost]
		[ProducesResponseType(typeof(PricingsDto), 200)]
		[Authorize(Roles = $"{UserRoles.Crew}, {UserRoles.Contractor}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> CreateUpdateCustomerPricing([FromBody] PricingsDto pricingDto)
		{
			var result = await _pricingDAL.CreateUpdateCustomerPricing(pricingDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
	}
}
