using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.SetUserAccount}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly IUsersDAL _usersDAL;
        private readonly IConfiguration _config;
        private readonly ITenantProvider _tenantProvider;
        public UsersController(IUsersDAL usersDAL, IConfiguration config, ITenantProvider tenantProvider)
        {
            _usersDAL = usersDAL;
            _config = config;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult GetEmployerDomain()
        {
            var domain = _config["OfficialEmployerEmailDomain"] ?? string.Empty;
            return Ok(domain);
        }
        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        // REMOVED: UserRoles.RefundItem was removed from this method's allowed roles.
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SearchUsersForComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet = 0, [FromQuery] int? limit = 12, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;

            var result = await _usersDAL.SearchUsersForComboBoxes(keywords1, offset1, limit1, sortByColumn, sortAscending, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<CreateUserResponseDto>), 200)]
        [Authorize(Roles = $"{UserRoles.SetUserAccount}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SearchUserByKeywords([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellationToken = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _usersDAL.SearchUserByKeywords(keywords1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<AppUserDto>), 200)]
        [Authorize(Roles = $"{UserRoles.SetUserAccount}")]
        public async Task<ActionResult> GetAllEmployees([FromQuery] int? offSet = 0, [FromQuery] int? limit = 12, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            var result = await _usersDAL.GetAllUsers(offset1, limit1, sortByColumn, sortAscending, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<AppUserDto>), 200)]
        [Authorize(Roles = $"{UserRoles.SetUserAccount}")]
        public async Task<ActionResult> SearchForEmployees([FromQuery] string? keywords, [FromQuery] int? offSet = 0, [FromQuery] int? limit = 12, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _usersDAL.SearchForUsers(keywords1, offset1, limit1, sortByColumn, sortAscending, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<CreateUserResponseDto>), 200)]
        [Authorize(Roles = $"{UserRoles.SetUserAccount}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SearchEmployeeUsers([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = true, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _usersDAL.SearchEmployees(keywords1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<CreateUserResponseDto>), 200)]
        [Authorize(Roles = $"{UserRoles.SetUserAccount}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SearchGeneralUsers([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = true, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _usersDAL.SearchGeneralUsers(keywords1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<CreateUserResponseDto>), 200)]
        [Authorize(Roles = $"{UserRoles.SetUserAccount}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SearchDisabledUsers([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = true, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _usersDAL.SearchDisabledUsers(keywords1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        // ==================== Employee Promotion Endpoints ====================

        [HttpPost]
        [ProducesResponseType(typeof(EmployeeApprovalStatusDto), 200)]
        [Authorize(Roles = $"{UserRoles.EmployeeApproval}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SubmitEmployeeApproval([FromBody] SubmitEmployeeApprovalDto dto)
        {
            var approverUserId = _tenantProvider.GetUserId(); ;
            var approverUserName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "";

            if (string.IsNullOrEmpty(approverUserId))
                return Unauthorized("Could not identify the approving user.");

            if (string.IsNullOrEmpty(dto.TargetUserId))
                return BadRequest("TargetUserId is required.");

            // Prevent self-approval
            if (approverUserId == dto.TargetUserId)
                return BadRequest("You cannot approve your own promotion.");

            var result = await _usersDAL.SubmitEmployeeApproval(
                dto.TargetUserId, approverUserId, approverUserName,
                dto.IsApproved, dto.Comment);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(EmployeeApprovalStatusDto), 200)]
        [Authorize(Roles = $"{UserRoles.EmployeeApproval}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetEmployeeApprovalStatus([FromQuery] string targetUserId)
        {
            var callerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _usersDAL.GetEmployeeApprovalStatus(targetUserId, callerUserId);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<EmployeeApprovalStatusDto>), 200)]
        [Authorize(Roles = $"{UserRoles.SetUserAccount}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetPendingPromotions([FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            var result = await _usersDAL.GetPendingPromotions(offset1, limit1, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }
    }
}
