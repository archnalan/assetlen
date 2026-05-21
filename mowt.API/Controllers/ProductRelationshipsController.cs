using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.ProductConfig}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class ProductRelationshipsController : ControllerBase
	{

		private readonly IProductRelationshipsDAL _relationshipsDAL;

		public ProductRelationshipsController(IProductRelationshipsDAL relationshipsDAL)
		{
			_relationshipsDAL = relationshipsDAL;
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ProductRelationshipDto>), 200)]
		public async Task<ActionResult> GetProdRelationshipbasedOnhasSubAndIssubProd(string issubProdID, string hasSubProdID)
		{
			var result = await _relationshipsDAL.GetProdRelationshipbasedOnhasSubAndIssubProd(issubProdID, hasSubProdID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ProductRelationshipDto>), 200)]
		public async Task<ActionResult> GetProdRelationshipBbasedIssubProd(string issubProdID)
		{
			var result = await _relationshipsDAL.GetProdRelationshipBbasedIssubProd(issubProdID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(List<ProductRelationshipDto>), 200)]
		public async Task<ActionResult> GetProdRelationshipBbasedOnhasSubProductID(string hasSubProdID)
		{
			var result = await _relationshipsDAL.GetRelationsByHasSubProdID(hasSubProdID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(ProductRelationshipDto), 200)]
		public async Task<ActionResult> UpdateProductRelationShip(string relationId, ProductRelationshipDto prDto)
		{
			var result = await _relationshipsDAL.UpdateProductRelationShip(relationId, prDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}
		[HttpPut]
		[ProducesResponseType(typeof(ProductRelationshipDto), 200)]
		[Obsolete("Method obsolete. Call UpdateProductRelationShip instead. Provide the relationship Id and the relationship dto.")]
		public async Task<ActionResult> UpdateProductRelationShipBasedonIsSubAndHasSubIDs(string issubProdID, string hasSubProdID)
		{
			var result = await _relationshipsDAL.UpdateProductRelationShipBasedonIsSubAndHasSubIDs(issubProdID, hasSubProdID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(ProductRelationshipDto), 200)]
		[Obsolete("Method obsolete. Call UpdateProductRelationShip instead. Provide the relationship Id and the relationship dto.")]
		public async Task<ActionResult> UpdateSortOrderBasedonIsSubAndHasSubIDs(ProductRelationshipDto prDto)
		{
			var result = await _relationshipsDAL.UpdateSortOrderBasedonIsSubAndHasSubIDs(prDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		public async Task<ActionResult> HardDeleteProduRelationshipBbasedOnRelationShipID([FromQuery] string id)
		{
			var result = await _relationshipsDAL.HardDeleteProduRelationshipBbasedOnRelationShipID(id);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpDelete]
		public async Task<ActionResult> HardDeleteProduRelationshipBbasedOnHasSubProdIDAndIssubProd(string issubProdID, string hasSubProdID)
		{
			var result = await _relationshipsDAL.HardDeleteProduRelationshipBbasedOnHasSubProdIDAndIssubProd(issubProdID, hasSubProdID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpDelete]
		public async Task<ActionResult> HardDeleteProduRelationshipBbasedOnHasSubProductID(string hasSubProdID)
		{
			var result = await _relationshipsDAL.HardDeleteProduRelationshipBbasedOnHasSubProductID(hasSubProdID);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpPost]
		[ProducesResponseType(typeof(ProductRelationshipDto), 200)]
		public async Task<ActionResult> CreateNewProductRelationShipFromDB(ProductRelationshipDto prDto)
		{
			var result = await _relationshipsDAL.AddProductRelationship(prDto);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}



	}
}
