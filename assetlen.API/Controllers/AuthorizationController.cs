using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using Google.Apis.Auth;
using System.ComponentModel.DataAnnotations;
//using NuGet.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using assetlen.Service.DataAccess;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Service.DbServices.SmtpClient;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.DbServices;
using assetlen.Shared.Models.statics;

using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.ServiceHandler;

namespace assetlen.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = UserRoles.SetUserAccount,
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthorizationController : ControllerBase
    {
        private IAuthorizationDAL _authorizationDAL;
        private readonly IConfiguration _configuration;
        private readonly ITenantProvider _tenantProvider;

        public AuthorizationController(IAuthorizationDAL authorizationDAL, IConfiguration configuration, ITenantProvider tenantProvider)
        {
            _authorizationDAL = authorizationDAL;
            _configuration = configuration;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> InitiatePasswordReset([FromQuery][MinLength(3)] string identifier, [FromQuery] string? originDomain = null)
        {
            var initiatedResult = await _authorizationDAL.InitiatePasswordReset(identifier, originDomain);

            if (!initiatedResult.IsSuccess)
                return StatusCode(initiatedResult.StatusCode, initiatedResult.Error);

            return Ok(initiatedResult);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ComboBoxDto>), 200)]
        public async Task<ActionResult> GetUsersForComboBoxes([FromHeader] string TenantId)
        {
            var usersResult = await _authorizationDAL.GetUsersForComboBoxes();

            if (!usersResult.IsSuccess)
                return StatusCode(usersResult.StatusCode, usersResult.Error);

            return Ok(usersResult.Data);
        }
        [HttpGet]
        [ProducesResponseType(typeof(List<UserRolesDto>), 200)]
        [AllowAnonymous]
        public async Task<ActionResult> GetRolesForUserByUserId([FromQuery] string userId)
        {
            var userIdOrig = _tenantProvider.GetUserId();
            if(string.IsNullOrEmpty(userIdOrig))
                return StatusCode(401, new { Message = "Un Authorized" });
            var isAccountManager = User?.IsInRole(UserRoles.SetUserAccount);
            if ((!isAccountManager??false) && !(userId.Equals(userIdOrig, StringComparison.OrdinalIgnoreCase)))
                return StatusCode(403, new { Message = "Invalid Userid provided" });

            var usersResult = await _authorizationDAL.GetRolesForUserByUserId(userId);

            if (!usersResult.IsSuccess)
                return StatusCode(usersResult.StatusCode, usersResult.Error);

            return Ok(usersResult.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> GetIsEmployeeByUserId([FromQuery] string userId)
        {
            var result = await _authorizationDAL.GetIsEmployeeByUserId(userId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        public async Task<ActionResult> SearchUsersFromComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] string? sortByColumn, [FromQuery] bool? sortAscending, [FromQuery] CancellationToken cancellation)
        {
            string keywords1 = keywords ?? string.Empty;
            string sort = sortByColumn ?? string.Empty;
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            bool ascend = sortAscending ?? true;
            var usersResult = await _authorizationDAL.SearchUsersFromComboBoxes(keywords1, offset1, limit1, sort, ascend, cancellation);

            if (!usersResult.IsSuccess)
                return StatusCode(usersResult.StatusCode, usersResult.Error);

            return Ok(usersResult.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        public async Task<IActionResult> ExternalLoginCallback([FromBody] GoogleAuthRequestDto googleAuthRequestDto)
        {
            var callbackResult = await _authorizationDAL.ExternalLoginCallback(googleAuthRequestDto);

            if (!callbackResult.IsSuccess)
                return StatusCode(callbackResult.StatusCode, callbackResult.Error);

            return Ok(callbackResult.Data);
        }


        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        public async Task<IActionResult> Login([FromBody][Required] UserLogin userLogin)
        {
            var loginResponse = await _authorizationDAL.Login(userLogin);

            if (!loginResponse.IsSuccess)
                return StatusCode(loginResponse.StatusCode, loginResponse.Error);

            return Ok(loginResponse.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> IssueSyncKey([FromBody] InitialSeedDataDto seedData)
        {
            var loginResponse = await _authorizationDAL.IssueSyncKeyAsync(seedData);

            if (!loginResponse.IsSuccess)
                return StatusCode(loginResponse.StatusCode, loginResponse.Error);

            return Ok(loginResponse.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        public async Task<IActionResult> LoginUserNameOrPhone([FromBody][Required][OneRequiredProp("UserName", "PhoneNumber")] LoginUserNameOrPhoneDto userLogin, [Required] string tenantId)
        {
            var loginResponse = await _authorizationDAL.LoginUserNameOrPhone(userLogin, tenantId);

            if (!loginResponse.IsSuccess)
                return StatusCode(loginResponse.StatusCode, loginResponse.Error);

            return Ok(loginResponse.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateUserResponseDto), 200)]
        public async Task<IActionResult> CreateUser([FromBody][Required] CreateUserDto createUserDto)
        {
            var tenantIdFromToken = User.FindFirst("TenantId");
            if (tenantIdFromToken?.Value == null) return BadRequest("Invalid TenantId");
            var userResult = await _authorizationDAL.CreateUser(createUserDto, tenantIdFromToken?.Value);
            if (!userResult.IsSuccess)
                return StatusCode(userResult.StatusCode, userResult.Error);

            return Ok(userResult.Data);
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CreateUserResponseDto), 200)]
        public async Task<IActionResult> RegisterUser([FromBody][Required] RegisterUserDto registerUserDto)
        {
            var userResult = await _authorizationDAL.RegisterUser(registerUserDto);

            if (!userResult.IsSuccess)
                return StatusCode(userResult.StatusCode, userResult.Error);

            return Ok(userResult.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> ResetPassword([FromBody][Required] ResetPasswordDto resetPasswordDto)
        {
            var passReset = await _authorizationDAL.ResetPassword(resetPasswordDto);

            if (!passReset.IsSuccess)
                return StatusCode(passReset.StatusCode, passReset.Error);

            return Ok(passReset.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> VerifyPasswordResetOtp([FromQuery][Required] string phoneNumber, [FromQuery][Required] string otp)
        {
            var verifyResult = await _authorizationDAL.VerifyPasswordResetOtp(phoneNumber, otp);

            if (!verifyResult.IsSuccess)
                return StatusCode(verifyResult.StatusCode, verifyResult.Error);

            return Ok(verifyResult.Data);
        }



        [HttpPut]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> UpdateUser([FromBody] CreateUserDto updateUserprofile)
        {
            var userProfileResult = await _authorizationDAL.UpdateUser(updateUserprofile);

            if (!userProfileResult.IsSuccess)
                return StatusCode(userProfileResult.StatusCode, userProfileResult.Error);

            return Ok(userProfileResult.Data);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> DeleteUser([FromQuery][Required] string userId)
        {
            var userProfileResult = await _authorizationDAL.DeleteUser(userId);

            if (!userProfileResult.IsSuccess)
                return StatusCode(userProfileResult.StatusCode, userProfileResult.Error);

            return Ok(userProfileResult.Data);
        }
        // GET: api/Posts/5
        [HttpGet]
        [ProducesResponseType(typeof(UpdateUserprofileOutDto), 200)]
        public async Task<IActionResult> GetUserProfile(string? id = null, string? userName = null)
        {
            var userProfileResult = await _authorizationDAL.GetUserProfile(id, userName);

            if (!userProfileResult.IsSuccess)
                return StatusCode(userProfileResult.StatusCode, userProfileResult.Error);

            return Ok(userProfileResult);
        }

        //EmailTaken(string email)
        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> EmailTaken([FromQuery][Required] string email)
        {
            var emailCheckResult = await _authorizationDAL.EmailTaken(email);
            if (!emailCheckResult.IsSuccess)
                return StatusCode(emailCheckResult.StatusCode, emailCheckResult.Error);
            return Ok(emailCheckResult.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> UserNameTaken([FromQuery][Required] string userName)
        {
            var userNameCheckResult = await _authorizationDAL.UserNameTaken(userName);
            if (!userNameCheckResult.IsSuccess)
                return StatusCode(userNameCheckResult.StatusCode, userNameCheckResult.Error);
            return Ok(userNameCheckResult.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        public async Task<IActionResult> RefreshToken([FromBody][Required] RefreshTokenRequestDto request)
        {
            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var deviceType = DetermineDeviceType(userAgent);
            var browserType = DetermineBrowserType(userAgent);

            var refreshResult = await _authorizationDAL.RefreshToken(request, ipAddress, deviceType, browserType);

            if (!refreshResult.IsSuccess)
                return StatusCode(refreshResult.StatusCode, refreshResult.Error);

            return Ok(refreshResult.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> SendVerificationCode([FromBody][Required] SendVerificationCodeDto sendVerificationCodeDto)
        {
            var result = await _authorizationDAL.SendVerificationCode(sendVerificationCodeDto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> VerifyCode([FromBody][Required] VerifyCodeDto verifyCodeDto)
        {
            var result = await _authorizationDAL.VerifyCode(verifyCodeDto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> ResendVerificationCode([FromQuery][Required] string identifier)
        {
            var result = await _authorizationDAL.ResendVerificationCode(identifier);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Admin-initiated password reset that bypasses exponential backoff rate limiting.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> AdminInitiatePasswordReset([FromBody][Required] AdminPasswordResetDto dto)
        {
            var originDomain = HttpContext.Request.Headers.Origin.ToString();
            var result = await _authorizationDAL.AdminInitiatePasswordReset(dto.UserId, dto.ResetMethod, originDomain);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Disable/enable a user account or soft-delete/restore.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> SetUserAccountStatus([FromBody][Required] UserAccountStatusDto dto)
        {
            var result = await _authorizationDAL.SetUserAccountStatus(dto.UserId, dto.Action, dto.Reason);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Update user profile - allows users to update their own profile information.
        /// Email and phone changes require OTP verification via separate endpoints.
        /// </summary>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(CreateUserResponseDto), 200)]
        public async Task<IActionResult> UpdateUserProfile([FromBody][Required] UpdateUserProfileDto dto)
        {
            var result = await _authorizationDAL.UpdateUserProfile(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Initiate contact change (email or phone) - sends OTP for verification.
        /// </summary>
        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> InitiateContactChange([FromBody][Required] SendVerificationCodeDto dto)
        {
            var result = await _authorizationDAL.SendVerificationCode(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Verify and complete contact change (email or phone) with OTP.
        /// </summary>
        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CreateUserResponseDto), 200)]
        public async Task<IActionResult> VerifyContactChange([FromBody][Required] VerifyContactChangeDto dto)
        {
            var result = await _authorizationDAL.VerifyContactChange(dto);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        private string DetermineDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
                return "Mobile";
            if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
                return "Tablet";

            return "Desktop";
        }

        private string DetermineBrowserType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Edge", StringComparison.OrdinalIgnoreCase))
                return "Edge";
            if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
                return "Chrome";
            if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                return "Firefox";
            if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase))
                return "Safari";

            return "Unknown";
        }
    }
}
