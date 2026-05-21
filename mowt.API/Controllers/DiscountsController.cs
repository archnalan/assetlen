using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using mowt.Shared.Models.statics;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountsDAL _discountsDAL;
        public DiscountsController(IDiscountsDAL discountsDAL)
        {
            _discountsDAL = discountsDAL;
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(PaginationDetails<DiscountDto>), 200)]
        public async Task<ActionResult> GetDiscountsFromDB([FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = 30;

            var result = await _discountsDAL.GetDiscountsFromDB(offset1, limit1, cancellation, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        public async Task<ActionResult> GetDiscountsFromComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _discountsDAL.GetDiscountsFromComboBoxes(keywords1, offset1, limit1, cancellation, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        public async Task<ActionResult> SearchDiscountsFromComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true, [FromQuery] bool? isActive = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = 30;
            string keywords1 = keywords ?? string.Empty;
            bool isActive1 = isActive ?? true;
            var result = await _discountsDAL.SearchDiscountsFromComboBoxes(keywords1, offset1, limit1, cancellation, sortByColumn, sortAscending, isActive1);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DiscountDto>), 200)]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetDiscountById([FromQuery] string id)
        {
            var result = await _discountsDAL.GetDiscountById(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DiscountDto>), 200)]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetDiscountByValue(decimal? value)
        {
            decimal value1 = value ?? 0;
            var result = await _discountsDAL.GetDiscountByValue(value1);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DiscountDto), 200)]
        public async Task<ActionResult> AddDiscount(DiscountCreateDto discount)
        {
            var result = await _discountsDAL.AddDiscount(discount);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(DiscountDto), 200)]
        public async Task<ActionResult> UpdateDiscount([FromQuery] string id, DiscountDto d)
        {
            var result = await _discountsDAL.UpdateDiscount(id, d);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteCategory([FromQuery] string id)
        {
            var result = await _discountsDAL.DeleteDiscountById(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

    }
}
