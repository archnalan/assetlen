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
    public class UserDocumentsController : ControllerBase
    {
        private readonly IUserDocumentsDAL _documentsDAL;
        private readonly ITenantProvider _tenantProvider;

        public UserDocumentsController(IUserDocumentsDAL documentsDAL, ITenantProvider tenantProvider)
        {
            _documentsDAL = documentsDAL;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserDocumentDto>), 200)]
        public async Task<ActionResult> GetMyCollection()
        {
            var userId = _tenantProvider.GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _documentsDAL.GetCollectionByUserId(userId);
            if (!result.IsSuccess) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> IsInCollection([FromQuery][Required] string productId)
        {
            var userId = _tenantProvider.GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _documentsDAL.IsInCollection(userId, productId);
            if (!result.IsSuccess) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserDocumentDto), 200)]
        public async Task<ActionResult> ToggleDocument([FromQuery][Required] string productId)
        {
            var userId = _tenantProvider.GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _documentsDAL.ToggleDocument(userId, productId);
            if (!result.IsSuccess) return BadRequest(result.Error);
            return Ok(result.Data);
        }
    }
}
