using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserFavoritesController : ControllerBase
    {
        private readonly IUserFavoritesDAL _favoritesDAL;
        private readonly ITenantProvider _tenantProvider;

        public UserFavoritesController(IUserFavoritesDAL favoritesDAL, ITenantProvider tenantProvider)
        {
            _favoritesDAL = favoritesDAL;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserFavoriteDto>), 200)]
        public async Task<ActionResult> GetMyFavorites()
        {
            var userId = _tenantProvider.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _favoritesDAL.GetFavoritesByUserId(userId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> IsFavorited([FromQuery][Required] string productId)
        {
            var userId = _tenantProvider.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _favoritesDAL.IsFavorited(userId, productId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserFavoriteDto), 200)]
        public async Task<ActionResult> ToggleFavorite([FromQuery][Required] string productId)
        {
            var userId = _tenantProvider.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var result = await _favoritesDAL.ToggleFavorite(userId, productId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }
    }
}
