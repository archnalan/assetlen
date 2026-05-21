using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SubscriptionRequestController : ControllerBase
    {
        private readonly ISubscriptionRequestDAL _dal;

        public SubscriptionRequestController(ISubscriptionRequestDAL dal)
        {
            _dal = dal;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        private string GetUserName() => User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "";
        private string? GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);

        private bool IsAdmin() =>
            User.IsInRole(UserRoles.mowtSuperAdmin) ||
            User.IsInRole(UserRoles.AdminModuleLogin) ||
            User.IsInRole(UserRoles.SetUserAccount);

        // ─────────────────────────────────────────────────────
        // Public – Submit an enterprise subscription request
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// Submit a new enterprise subscription application.
        /// Can be called by any authenticated user (or extended to allow anonymous).
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SubscriptionRequestDto), 200)]
        public async Task<ActionResult> Submit(
            [FromBody] SubscriptionRequestCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
            var userName = User.Identity?.IsAuthenticated == true ? GetUserName() : null;
            var userEmail = User.Identity?.IsAuthenticated == true ? GetUserEmail() : null;

            var result = await _dal.SubmitRequest(dto, userId, userName, userEmail, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        // ─────────────────────────────────────────────────────
        // Read (admin-only)
        // ─────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(SubscriptionRequestDto), 200)]
        public async Task<ActionResult> GetById(
            [FromQuery][Required] string id,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.GetById(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(List<SubscriptionRequestDto>), 200)]
        public async Task<ActionResult> GetAll(
            [FromBody] SubscriptionRequestQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.GetAll(query, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(SubscriptionRequestStatsDto), 200)]
        public async Task<ActionResult> GetStats(CancellationToken cancellationToken = default)
        {
            var result = await _dal.GetStats(cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        // ─────────────────────────────────────────────────────
        // Admin Workflow
        // ─────────────────────────────────────────────────────

        [HttpPut]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(SubscriptionRequestDto), 200)]
        public async Task<ActionResult> IssueQuote(
            [FromBody] SubscriptionRequestQuoteDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.IssueQuote(dto, GetUserId(), GetUserName(), cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(SubscriptionRequestDto), 200)]
        public async Task<ActionResult> ConfirmPayment(
            [FromBody] SubscriptionRequestPaymentDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.ConfirmPayment(dto, GetUserId(), cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(SubscriptionRequestDto), 200)]
        public async Task<ActionResult> UpdateStatus(
            [FromBody] SubscriptionRequestStatusUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.UpdateStatus(dto, GetUserId(), cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        // ─────────────────────────────────────────────────────
        // Seat Management
        // ─────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(SubscriptionSeatDto), 200)]
        public async Task<ActionResult> AddSeat(
            [FromBody] SubscriptionSeatCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.AddSeat(dto, GetUserId(), cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(List<SubscriptionSeatDto>), 200)]
        public async Task<ActionResult> AddSeatsBulk(
            [FromBody] SubscriptionSeatBulkCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.AddSeatsBulk(dto, GetUserId(), cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> RemoveSeat(
            [FromQuery][Required] string seatId,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.RemoveSeat(seatId, GetUserId(), cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.mowtSuperAdmin}")]
        [ProducesResponseType(typeof(List<SubscriptionSeatDto>), 200)]
        public async Task<ActionResult> GetSeats(
            [FromQuery][Required] string requestId,
            CancellationToken cancellationToken = default)
        {
            var result = await _dal.GetSeatsByRequestId(requestId, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
