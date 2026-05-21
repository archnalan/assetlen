using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.Users;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface IAuthorizationApi
    {
        [Get("/api/Authorization/GetUsersForComboBoxes")]
        Task<IApiResponse<List<ComboBoxDto>>> GetUsersForComboBoxes();

        [Get("/api/Authorization/SearchUsersFromComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchUsersFromComboBoxes(string? keywords, int? offSet, int? limit, string? sortByColumn, bool? sortAscending, CancellationToken cancellation);

        [Post("/api/Authorization/Login")]
        Task<IApiResponse<LoginResponseDto>> Login([Body] UserLogin login);

        [Post("/api/Authorization/RefreshToken")]
        Task<IApiResponse<LoginResponseDto>> RefreshToken([Body] RefreshTokenRequestDto request);

        [Get("/api/Authorization/GetUserProfile")]
        Task<IApiResponse<UpdateUserprofileOutDto>> GetUserProfile([Query]string? userId = null, [Query]string? userName = null);

        [Post("/api/Authorization/CreateUser")]
        Task<IApiResponse<CreateUserResponseDto>> CreateUser([Body] CreateUserDto user);

        [Post("/api/Authorization/RegisterUser")]
        Task<IApiResponse<CreateUserResponseDto>> RegisterUser([Body] RegisterUserDto user);

        [Put("/api/Authorization/UpdateUser")]
        Task<IApiResponse<CreateUserResponseDto>> UpdateUser([Body] CreateUserDto user);

        [Get("/api/Authorization/GetRolesForUserByUserId")]
        Task<IApiResponse<UserRolesDto>> GetRolesForUserByUserId([Query] string userId);

        [Get("/api/Authorization/GetIsEmployeeByUserId")]
        Task<IApiResponse<bool>> GetIsEmployeeByUserId([Query] string userId);

        [Delete("/api/Authorization/DeleteUser")]
        Task<IApiResponse<bool>> DeleteUser([Query] string userId);

        [Get("/api/Authorization/UserNameTaken")]
        Task<IApiResponse<bool>> UserNameTaken([Query] string userName);

        [Get("/api/Authorization/EmailTaken")]
        Task<IApiResponse<bool>> EmailTaken([Query] string email);

        [Post("/api/Authorization/ExternalLoginCallback")]
        Task<IApiResponse<LoginResponseDto>> ExternalLoginCallback([Body] GoogleAuthRequestDto codeDto);

        [Post("/api/Authorization/SendVerificationCode")]
        Task<IApiResponse<string>> SendVerificationCode([Body] SendVerificationCodeDto sendVerificationCodeDto);

        [Post("/api/Authorization/VerifyCode")]
        Task<IApiResponse<string>> VerifyCode([Body] VerifyCodeDto verifyCodeDto);

        [Post("/api/Authorization/ResendVerificationCode")]
        Task<IApiResponse<string>> ResendVerificationCode([Query] string identifier);

        [Get("/api/Authorization/InitiatePasswordReset")]
        Task<IApiResponse<string>> InitiatePasswordReset([Query] string identifier, [Query] string? originDomain = null);

        [Post("/api/Authorization/ResetPassword")]
        Task<IApiResponse<string>> ResetPassword([Body] ResetPasswordDto resetPasswordDto);

        [Post("/api/Authorization/VerifyPasswordResetOtp")]
        Task<IApiResponse<string>> VerifyPasswordResetOtp([Query] string phoneNumber, [Query] string otp);

        [Post("/api/Authorization/AdminInitiatePasswordReset")]
        Task<IApiResponse<string>> AdminInitiatePasswordReset([Body] AdminPasswordResetDto dto);

        [Post("/api/Authorization/SetUserAccountStatus")]
        Task<IApiResponse<string>> SetUserAccountStatus([Body] UserAccountStatusDto dto);

        [Put("/api/Authorization/UpdateUserProfile")]
        Task<IApiResponse<CreateUserResponseDto>> UpdateUserProfile([Body] UpdateUserProfileDto dto);

        [Post("/api/Authorization/InitiateContactChange")]
        Task<IApiResponse<string>> InitiateContactChange([Body] SendVerificationCodeDto dto);

        [Post("/api/Authorization/VerifyContactChange")]
        Task<IApiResponse<CreateUserResponseDto>> VerifyContactChange([Body] VerifyContactChangeDto dto);
    }
}

