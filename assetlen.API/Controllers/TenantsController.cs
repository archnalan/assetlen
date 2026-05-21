using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace assetlen.API.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.mowtSuperAdmin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TenantController : ControllerBase
    {
        private readonly ITenantServiceDAL _tenantService;

        public TenantController(ITenantServiceDAL tenantService)
        {
            _tenantService = tenantService;
        }

        // GET all tenants
        [HttpGet]
        [ProducesResponseType(typeof(List<TenantDto>), 200)]
        public async Task<IActionResult> GetAllTenants([FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellationToken = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;

            var result = await _tenantService.GetAllTenants(offset1, limit1, cancellationToken, sortByColumn, sortAscending);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        // GET tenant by ID
        [HttpGet("{tenantId}")]
        [ProducesResponseType(typeof(TenantDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTenantById(string tenantId)
        {
            var result = await _tenantService.GetTenantById(tenantId);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        // POST a new tenant
        [HttpPost]
        [ProducesResponseType(typeof(TenantDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateTenant([FromBody] TenantCreateDto tenantDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _tenantService.CreateTenant(tenantDto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return CreatedAtAction(nameof(GetTenantById), new { tenantId = result.Data.Id }, result.Data);
        }

        // PUT (update) a tenant
        [HttpPut("{tenantId}")]
        [ProducesResponseType(typeof(TenantDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTenant(string tenantId, [FromBody] TenantDto tenantDto)
        {
            var result = await _tenantService.UpdateTenant(tenantId, tenantDto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        // DELETE a tenant
        [HttpDelete("{tenantId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTenant(string tenantId)
        {
            var result = await _tenantService.DeleteTenant(tenantId);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return NoContent();
        }
    }
}
