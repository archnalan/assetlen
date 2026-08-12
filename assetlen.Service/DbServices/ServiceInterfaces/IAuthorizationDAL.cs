using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface IAuthorizationDAL
    {
        Task<ServiceResult<string>> AddUserToRoleAsync(string userId, string roleName);
        Task<ServiceResult<CreateUserResponseDto>> CreateUser([Required] CreateUserDto createUserDto, string tenantId);
        Task<ServiceResult<CreateUserResponseDto>> RegisterUser([Required] RegisterUserDto registerUserDto);
        Task<ServiceResult<bool>> DeleteUser([NotNull, Required] string userId);
        Task<ServiceResult<LoginResponseDto>> ExternalLoginCallback(GoogleAuthRequestDto googleAuthRequestDto);
        Task<ServiceResult<UserRolesDto>> GetRolesForUserByUserId(string userId);
        Task<ServiceResult<bool>> GetIsEmployeeByUserId(string userId);
        Task<ServiceResult<UpdateUserprofileOutDto>> GetUserProfile(string id, string userName);
        Task<ServiceResult<List<ComboBoxDto>>> GetUsersForComboBoxes();
        Task<ServiceResult<string>> InitiatePasswordReset(string identifier, string? originDomain = null);
        Task<ServiceResult<LoginResponseDto>> Login(UserLogin userLogin);
        Task<ServiceResult<LoginResponseDto>> LoginUserNameOrPhone(LoginUserNameOrPhoneDto userLogin, string tenantId);
        Task<ServiceResult<string>> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<ServiceResult<string>> ResetPassword(ResetPasswordDto resetPasswordDto);
        Task<ServiceResult<string>> VerifyPasswordResetOtp(string phoneNumber, string otp);
        Task<ServiceResult<CreateUserResponseDto>> UpdateUser(CreateUserDto updateUserprofile);
        Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchUsersFromComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
        Task<ServiceResult<string>> ValidateApiKeyAsync(string apiKey, string tenantId, string userId);
        Task<ServiceResult<string>> IssueSyncKeyAsync(InitialSeedDataDto seedData);
        Task<LoginResponseDto> GenerateToken(AppUser user, string tenantId, string ipAddress = null, string deviceType = null, string browserType = null);
        string GetUserIdFromToken(string token);
        Task<ServiceResult<bool>> UserNameTaken(string userName);
        Task<ServiceResult<bool>> EmailTaken(string email);
        Task<ServiceResult<LoginResponseDto>> RefreshToken(RefreshTokenRequestDto request, string ipAddress = null, string deviceType = null, string browserType = null);

        /// <summary>Every account this person may act in (assetlen.md §10.2).</summary>
        Task<ServiceResult<List<TenantMembershipDto>>> GetMyAccounts(string userId, string? activeTenantId);

        /// <summary>Re-issue the token against another of the caller's accounts.</summary>
        Task<ServiceResult<LoginResponseDto>> SwitchTenant(string userId, string tenantId);
        Task<ServiceResult<string>> SendVerificationCode(SendVerificationCodeDto sendVerificationCodeDto);
        Task<ServiceResult<string>> VerifyCode(VerifyCodeDto verifyCodeDto);
        Task<ServiceResult<string>> ResendVerificationCode(string identifier);
        /// <summary>
        /// Admin-initiated password reset that bypasses exponential backoff.
        /// </summary>
        Task<ServiceResult<string>> AdminInitiatePasswordReset(string userId, string resetMethod = "email", string? originDomain = null);
        /// <summary>
        /// Disable or enable a user account via lockout.
        /// </summary>
        Task<ServiceResult<string>> SetUserAccountStatus(string userId, string action, string? reason = null);
        /// <summary>
        /// Update user profile - allows users to update their own profile information.
        /// </summary>
        Task<ServiceResult<CreateUserResponseDto>> UpdateUserProfile(UpdateUserProfileDto dto);
        /// <summary>
        /// Verify contact change with OTP and update user's email or phone.
        /// </summary>
        Task<ServiceResult<CreateUserResponseDto>> VerifyContactChange(VerifyContactChangeDto dto);
    }
}