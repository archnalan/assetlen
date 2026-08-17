using System.Text.RegularExpressions;
using assetlen.Service.Extensions;
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
using Azure.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using assetlen.ServiceHandler;
using assetlen.Service.DbServices.ServiceInterfaces;
using System.Data;
using Microsoft.Extensions.Hosting;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Http.HttpResults;
using assetlen.Service.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using assetlen.API;
using System.Security.Cryptography;

namespace assetlen.Service.DbServices
{

    public class AuthorizationDAL : IAuthorizationDAL
    {
        private IConfiguration _config;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AssetlenDbContext _context;
        private readonly SmtpSenderService _emailSmtpService;
        private readonly ILogger<AuthorizationDAL> _logger;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPasswordHasher<TenantInstance> _hasher;
        private readonly PasswordHasher<AppUser> _otpHasher;
        private readonly IPandoraSmsService _pandoraSmsService;



        public AuthorizationDAL(IConfiguration config, AssetlenDbContext context, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment, SmtpSenderService emailSmtpService, ILogger<AuthorizationDAL> logger, ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor, RoleManager<IdentityRole> roleManager, IPasswordHasher<TenantInstance> hasher, IPandoraSmsService pandoraSmsService)
        {
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _emailSmtpService = emailSmtpService;
            _logger = logger;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
            _hasher = hasher;
            _otpHasher = new PasswordHasher<AppUser>();
            _pandoraSmsService = pandoraSmsService;
        }


        public async Task<ServiceResult<string>> InitiatePasswordReset(string identifier, string? originDomain = null)
        {
            try
            {
                var allowedDomains = _config.GetSection("AllowedOrigins").Get<string[]>();

                // Use provided origin domain or get from request
                var requestUrl = !string.IsNullOrEmpty(originDomain)
                    ? originDomain
                    : _httpContextAccessor.HttpContext?.Request.HttpContext.Request.Headers.Origin.ToString();

                var isProd = _webHostEnvironment.IsProduction();
                if ((isProd && string.IsNullOrEmpty(requestUrl)) || (allowedDomains != null && allowedDomains.Where(x => x.Contains(requestUrl?.ToString()?.Trim() ?? "", StringComparison.OrdinalIgnoreCase)).Count() == 0))
                {
                    //not authorized domain
                    return ServiceResult<string>.Failure(new ForbiddenException("Not authorised Origin"));
                }

                // Determine if identifier is email or phone
                AppUser? user = null;
                bool isPhone = false;

                // Check if it's a phone number
                var phoneCheck = CheckIsMtnOrAirtel(identifier);
                if (phoneCheck.isValid)
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneCheck.cleanedupNumber);
                    isPhone = true;
                }
                else
                {
                    // Assume it's an email
                    user = await _userManager.FindByEmailAsync(identifier);
                }

                if (user == null) return ServiceResult<string>.Failure(new NotFoundException("User does not exist"));

                // === EXPONENTIAL BACKOFF: Check for rate limiting ===
                // Get the most recent password reset request for this user (used or not) to check throttling
                var mostRecentResetCode = await _context.VerificationCodes
                    .Where(vc => vc.UserId == user.Id && !string.IsNullOrEmpty(vc.ResetToken))
                    .OrderByDescending(vc => vc.CreatedAt)
                    .FirstOrDefaultAsync();

                if (mostRecentResetCode != null)
                {
                    var timeSinceLastRequest = mostRecentResetCode.LastResentAt.HasValue
                        ? DateTime.UtcNow - mostRecentResetCode.LastResentAt.Value
                        : DateTime.UtcNow - mostRecentResetCode.CreatedAt;

                    // Calculate required wait time based on resend count
                    // Pattern: 2, 4 (2²), 16 (4²), 256 (16²), etc. minutes
                    var requiredWaitMinutes = CalculateExponentialBackoff(mostRecentResetCode.ResendCount);

                    if (timeSinceLastRequest.TotalMinutes < requiredWaitMinutes)
                    {
                        var remainingMinutes = Math.Ceiling(requiredWaitMinutes - timeSinceLastRequest.TotalMinutes);
                        var timeMessage = remainingMinutes >= 60
                            ? $"{Math.Ceiling(remainingMinutes / 60)} hour(s)"
                            : $"{remainingMinutes} minute(s)";

                        return ServiceResult<string>.Failure(
                            new BadRequestException($"Too many password reset requests. Please wait {timeMessage} before trying again."));
                    }
                }

                // Invalidate any existing unused password reset codes for this user
                var existingResetCodes = await _context.VerificationCodes
                    .Where(vc => vc.UserId == user.Id && !string.IsNullOrEmpty(vc.ResetToken) && !vc.IsUsed)
                    .ToListAsync();

                foreach (var code in existingResetCodes)
                {
                    code.IsUsed = true;
                }
                await _context.SaveChangesAsync();

                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                // Calculate the ResendCount for the new request (carry forward from previous)
                var newResendCount = mostRecentResetCode != null ? mostRecentResetCode.ResendCount + 1 : 0;

                if (isPhone)
                {
                    // For phone: Generate 6-digit OTP and send via SMS
                    var otp = new Random().Next(100000, 999999).ToString();
                    var hashedOtp = _otpHasher.HashPassword(user, otp);

                    // Store the hashed OTP and encoded token together
                    var verificationCode = new VerificationCode
                    {
                        UserId = user.Id,
                        Code = hashedOtp,
                        ResetToken = encodedToken,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                        IsUsed = false,
                        Type = VerificationType.Phone,
                        Contact = phoneCheck.cleanedupNumber,
                        ResendCount = newResendCount,
                        LastResentAt = newResendCount > 0 ? DateTime.UtcNow : null
                    };

                    _context.VerificationCodes.Add(verificationCode);
                    await _context.SaveChangesAsync();

                    // Send SMS
                    var smsResponse = await _pandoraSmsService.SendSmsAsync(user.PhoneNumber!, $"Your password reset code is: {otp}. Valid for 10 minutes.");
                    if (!smsResponse.Success)
                    {
                        return ServiceResult<string>.Failure(new ServerErrorException("Failed to send SMS. Please try again."));
                    }

                    return ServiceResult<string>.Success("Reset code sent to your phone");
                }
                else
                {
                    // For email: Store verification code with reset token for tracking
                    var verificationCode = new VerificationCode
                    {
                        UserId = user.Id,
                        Code = string.Empty, // Not needed for email reset links
                        ResetToken = encodedToken,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(1), // Email links valid for 1 hour
                        IsUsed = false,
                        Type = VerificationType.Email,
                        Contact = user.Email!,
                        ResendCount = newResendCount,
                        LastResentAt = newResendCount > 0 ? DateTime.UtcNow : null
                    };

                    _context.VerificationCodes.Add(verificationCode);
                    await _context.SaveChangesAsync();

                    // Send reset link
                    var resetLink = $"{requestUrl}/login?token={encodedToken}&email={user.Email}";
                    _emailSmtpService.SendPasswordResetCodeEmailAsync(user.Email!, user.FirstName ?? "User", resetLink);
                    return ServiceResult<string>.Success("Reset link sent to your email");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while initiating password reset. {ex}", ex);
                return ServiceResult<string>.Failure(new ServerErrorException("Error while initiating password reset. Please try again later"));
            }
        }

        public async Task<ServiceResult<List<ComboBoxDto>>> GetUsersForComboBoxes()
        {
            var result = await _context.Users.AsNoTracking().Select(x => new ComboBoxDto
            {
                IdString = x.Id,
                ValueText = $"{x.FirstName ?? ""} {x.LastName ?? ""}"

            }).ToListAsync();
            return ServiceResult<List<ComboBoxDto>>.Success(result);
        }

        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchUsersFromComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
        {
            IQueryable<AppUser> query = _context.Users.AsNoTracking();
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(x => x.FirstName.Contains(keywords) || x.LastName.Contains(keywords) || x.UserName != null && x.UserName.Contains(keywords));
                }
                var result = await query.Select(x => new ComboBoxDto
                {
                    Id = x.Id,
                    IdString = x.Id,
                    ValueText = $"{x.FirstName ?? ""} {x.LastName ?? ""}"
                }).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(result);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching users from comboboxes. {ex}", ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(new ServerErrorException("Error while searching users from combo boxes. Please try again later"));
            }

        }

        //OAuth  
        public async Task<ServiceResult<LoginResponseDto>> ExternalLoginCallback(GoogleAuthRequestDto googleAuthRequestDto)
        {
            var client = new HttpClient();
            var uri = new Uri("https://oauth2.googleapis.com/token");
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", googleAuthRequestDto.Code },
                { "client_id", $"{_config["Authentication:Google:ClientId"]}" },
                { "client_secret", $"{_config["Authentication:Google:ClientSecret"]}" },
                { "redirect_uri", $"{_config["Authentication:Google:RedirectUri"]}" },
                { "grant_type", "authorization_code" },
                { "code_verifier", googleAuthRequestDto.CodeVerifier }
            });

            var response = await client.PostAsync(uri, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var tenantId = _tenantProvider.GetTenantId() ?? "default"; // Get tenant ID from tenant provider, use "default" if not available
            // Check the status code and response content for error handling
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Parse the response content into a .NET object
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                    // Use the access token or refresh token as needed
                    var accessToken = tokenResponse.access_token;
                    var refreshToken = tokenResponse.refresh_token;

                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _config["Authentication:Google:ClientId"] }
                    };
                    var objFromGoogle = await GoogleJsonWebSignature.ValidateAsync(tokenResponse.id_token, settings);
                    //check user exists
                    var userFromDb = await _userManager.FindByLoginAsync("google", objFromGoogle.Subject);
                    IdentityResult result;
                    if (userFromDb == null)
                    {
                        //check user exists
                        userFromDb = await _userManager.FindByEmailAsync(objFromGoogle.Email);
                        if (userFromDb != null)
                        {
                            result = await _userManager.AddLoginAsync(userFromDb, new UserLoginInfo("google", objFromGoogle.Subject, objFromGoogle.Name));
                            return await LoginUserNoPassword(userFromDb, tenantId);
                        }
                        //Created user
                        var newUser = new AppUser()
                        {
                            Email = objFromGoogle.Email,
                            UserName = await UserNameAllocator.ResolveUserNameAsync(
                                _userManager, objFromGoogle.Name.ToLower().Replace(" ", "")),
                            FirstName = objFromGoogle.Name.Split(" ").FirstOrDefault(),
                            LastName = objFromGoogle.Name.Split(" ")?[1],
                            EmailConfirmed = objFromGoogle.EmailVerified,
                            ProfilePicUrl = objFromGoogle.Picture,
                            IsEmployee = IsEmployerDomainEmail(objFromGoogle.Email),
                        };
                        var strategy = _context.Database.CreateExecutionStrategy();

                        return await strategy.ExecuteAsync(async () =>
                        {

                            using (var scope = _context.Database.BeginTransaction())
                            {
                                result = await _userManager.CreateAsync(newUser);
                                var userLoginInfo = new UserLoginInfo("google", objFromGoogle.Subject, objFromGoogle.Name);
                                result = await _userManager.AddLoginAsync(newUser, userLoginInfo);
                                if (result.Succeeded)
                                {
                                    // Self-registered users default to Contractor —
                                    // they're starting a new tenant org. Crew/Client
                                    // accounts are created via invitation flows.
                                    var defaultRole = UserRoles.Contractor;
                                    var roleExists = await _roleManager.RoleExistsAsync(defaultRole);
                                    if (roleExists)
                                    {
                                        await _userManager.AddToRoleAsync(newUser, defaultRole);
                                    }

                                    //if all is well
                                    scope.Commit();
                                    return await LoginUserNoPassword(newUser, tenantId);
                                }

                                var outputErrors = new List<string>();
                                foreach (var error in result.Errors)
                                {
                                    outputErrors.Add(error.Description);
                                }
                                return ServiceResult<LoginResponseDto>.Failure(new BadRequestException(string.Join("\n", outputErrors)));

                            }

                        });


                    }
                    else
                    {
                        //User already exisit as external login. sign in
                        return await LoginUserNoPassword(userFromDb, tenantId);
                    }




                }
                catch (Exception ex)
                {
                    _logger.LogError("An error occured while signing in with google Message: {ex.Message} and detail :{JsonConvert.SerializeObject(ex)}", ex.Message, ex);
                    return ServiceResult<LoginResponseDto>.Failure(new BadRequestException("Failed to Autheticate. Please try again later"));
                }
            }
            else
            {
                _logger.LogError($"An error occured while signing in with google. the request returned an error. Responsecontent:  :{JsonConvert.SerializeObject(responseContent)}");
                return ServiceResult<LoginResponseDto>.Failure(new BadRequestException("Failed to Autheticate. Please try again later"));

            }
        }

        public async Task<ServiceResult<LoginResponseDto>> Login(UserLogin userLogin)
        {
            var user = await Authenticate(userLogin);
            if (user != null)
            {

                // Check if user needs to verify email or phone
                if (!user.EmailConfirmed && !string.IsNullOrEmpty(user.Email) && !user.Email.EndsWith($"@{_config["DefaultEmailDomain"]}", StringComparison.OrdinalIgnoreCase))
                {
                    //return ServiceResult<LoginResponseDto>.Failure(
                    //    new UnAuthorizedException("Please verify your email address before logging in. Check your email for the verification code."));
                   
                }

                if (!user.PhoneNumberConfirmed && !string.IsNullOrEmpty(user.PhoneNumber))
                {
                    return ServiceResult<LoginResponseDto>.Failure(
                        new UnAuthorizedException("Please verify your phone number before logging in. Check your messages for the verification code."));
                }

                // Get client info from HTTP context
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
                var deviceType = DetermineDeviceType(userAgent);
                var browserType = DetermineBrowserType(userAgent);

                // Check if this is a new device/browser/IP combination before generating token
                var isNewDevice = await CheckForNewDevice(user.Id, user.TenantId, ipAddress, deviceType, browserType);

                var token = await GenerateToken(user, user.TenantId, ipAddress, deviceType, browserType);

                if (isNewDevice && !string.IsNullOrEmpty(user.Email) && !user.Email.EndsWith($"@{_config["DefaultEmailDomain"]}", StringComparison.OrdinalIgnoreCase))
                {
                    // Send email notification for new device login
                    var emailDto = _emailSmtpService.CreateEmailDto();
                    emailDto.Subject = "New Device Login Detected";
                    emailDto.ToEmail = user.Email;
                    emailDto.Body = GenerateNewDeviceLoginEmail(user, ipAddress, deviceType, browserType, DateTime.UtcNow);

                    // Send email asynchronously without blocking
                    _ = Task.Run(() => _emailSmtpService.SendMail(emailDto));
                }

                return ServiceResult<LoginResponseDto>.Success(token);
            }
            return ServiceResult<LoginResponseDto>.Failure(
                new BadRequestException("Invalid login attempt. Check your credentials and try again."));
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginUserNameOrPhone(LoginUserNameOrPhoneDto userLogin, string tenantId)
        {

            var userResult = await AuthenticateByUserNameOrPhone(userLogin);
            if (!userResult.IsSuccess)
                return ServiceResult<LoginResponseDto>.Failure(userResult.Error);

            var user = userResult.Data;

            var tenantExists = await _context.tbl_Tenants.AnyAsync(x => x.TenantId == tenantId);

            if (!tenantExists)
                return ServiceResult<LoginResponseDto>.Failure(
                    new BadRequestException("Invalid Tenant Id. Contact https://assetlen.com for support."));

            // Check if user needs to verify email or phone
            if (!user.EmailConfirmed && !string.IsNullOrEmpty(user.Email))
            {
                return ServiceResult<LoginResponseDto>.Failure(
                    new UnAuthorizedException("Please verify your email address before logging in. Check your email for the verification code."));
            }

            if (!user.PhoneNumberConfirmed && !string.IsNullOrEmpty(user.PhoneNumber) && string.IsNullOrEmpty(user.Email))
            {
                return ServiceResult<LoginResponseDto>.Failure(
                    new UnAuthorizedException("Please verify your phone number before logging in. Check your messages for the verification code."));
            }

            // Get client info from HTTP context
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            var deviceType = DetermineDeviceType(userAgent);
            var browserType = DetermineBrowserType(userAgent);

            // Check if this is a new device/browser/IP combination before generating token
            var isNewDevice = await CheckForNewDevice(user.Id, tenantId, ipAddress, deviceType, browserType);

            var token = await GenerateToken(user, tenantId, ipAddress, deviceType, browserType);

            //Task.Run(() => _emailSmtpController.SendMail(emailDto));

            return ServiceResult<LoginResponseDto>.Success(token);

        }

        private async Task<ServiceResult<LoginResponseDto>> LoginUserNoPassword(AppUser userFromDb, string tenantId)
        {
            // Get client info from HTTP context
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            var deviceType = DetermineDeviceType(userAgent);
            var browserType = DetermineBrowserType(userAgent);

            var token = await GenerateToken(userFromDb, tenantId, ipAddress, deviceType, browserType);

            // Check if this is a new device/browser/IP combination
            var isNewDevice = await CheckForNewDevice(userFromDb.Id, tenantId, ipAddress, deviceType, browserType);

            if (isNewDevice)
            {
                // Send email notification for new device login
                var emailDto = _emailSmtpService.CreateEmailDto();
                emailDto.Subject = "New Device Login Detected (OAuth)";
                emailDto.ToEmail = userFromDb.Email;
                emailDto.Body = GenerateNewDeviceLoginEmail(userFromDb, ipAddress, deviceType, browserType, DateTime.UtcNow);

                // Send email asynchronously without blocking the login process
                _ = Task.Run(() => _emailSmtpService.SendMail(emailDto));
            }

            return ServiceResult<LoginResponseDto>.Success(token);
        }

        public async Task<ServiceResult<CreateUserResponseDto>> CreateUser([Required] CreateUserDto createUserDto, string tenantId)
        {
            var tenantExists = await _context.tbl_Tenants.AnyAsync(x => x.TenantId == tenantId);

            if (!tenantExists) return ServiceResult<CreateUserResponseDto>.Failure(new NotFoundException($"Tenant with ID:{tenantId} is Invalid"));
            //using  trnsaction scope

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                return await CreateUserWithRoleAsync(createUserDto);
            });

            //submethod to allow breakpoint hitting in an arrow function
            async Task<ServiceResult<CreateUserResponseDto>> CreateUserWithRoleAsync([Required] CreateUserDto createUserDto)
            {
                //TODO: check if user exists on the online api

                var usernameExists = await _context.Users
                                    .AnyAsync(x => x.UserName == createUserDto.UserName);

                if (usernameExists)
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException($"Username: {createUserDto.UserName} already taken."));

                using (var scope = await _context.Database.BeginTransactionAsync())
                {
                    //prevent mapster from replcaing ids with null
                    TypeAdapterConfig<CreateUserDto, AppUser>
                                    .NewConfig()
                                    .IgnoreNullValues(true);
                    AppUser newUser = new AppUser();
                    createUserDto.Adapt(newUser);
                    newUser.TenantId = tenantId;
                    newUser.IsEmployee = IsEmployerDomainEmail(createUserDto.Email);

                    var result = await _userManager.CreateAsync(newUser, createUserDto.Password);
                    if (result.Succeeded)
                    {
                        //add to role
                        foreach (var roleName in createUserDto.UserRolesDto.GetRoleStatuses())
                        {
                            ServiceResult<string> role = default!;
                            if (roleName.Status)
                            {
                                role = await AddUserToRoleAsync(newUser.Id, roleName.Name);

                                if (!role.IsSuccess)
                                {
                                    //failed to add one of the roles
                                    await scope.RollbackAsync();
                                    return ServiceResult<CreateUserResponseDto>.Failure(role.Error);
                                }
                            }


                        }
                        await scope.CommitAsync();


                        var output = newUser.Adapt<CreateUserResponseDto>();


                        return ServiceResult<CreateUserResponseDto>.Success(output);
                    }

                    await scope.RollbackAsync();
                    var outputErrors = new List<string>();
                    foreach (var error in result.Errors)
                    {
                        outputErrors.Add(error.Description);
                    }
                    return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException(string.Join("\n", outputErrors)));

                }


            }

        }

        public async Task<ServiceResult<CreateUserResponseDto>> RegisterUser([Required] RegisterUserDto registerUserDto)
        {
            // Get tenant from provider (claims) or use default from configuration
            var tenantId = _tenantProvider.GetTenantId();

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                return await RegisterUserAsync(registerUserDto, tenantId);
            });

            async Task<ServiceResult<CreateUserResponseDto>> RegisterUserAsync([Required] RegisterUserDto registerUserDto, string tenantId)
            {
                // Generate username if not provided
                string userName = registerUserDto.UserName;
                if (string.IsNullOrEmpty(userName))
                {
                    if (!string.IsNullOrEmpty(registerUserDto.Email))
                    {
                        userName = registerUserDto.Email.Split('@')[0];
                    }
                    else if (!string.IsNullOrEmpty(registerUserDto.PhoneNumber))
                    {
                        var contact = CheckIsMtnOrAirtel(registerUserDto.PhoneNumber);

                        if (contact.isValid)
                        {
                            registerUserDto.PhoneNumber = contact.cleanedupNumber;
                            userName = contact.cleanedupNumber;
                        }
                        else
                        {
                            return ServiceResult<CreateUserResponseDto>.Failure(
                                new BadRequestException("Invalid phone number format. Only Ugandan MTN and Airtel numbers are accepted."));
                        }
                    }
                }

                // Check if username is taken and generate unique one
                var originalUserName = userName;
                int counter = 1;
                while (await _context.Users.AnyAsync(x => x.UserName == userName))
                {
                    userName = $"{originalUserName}{counter}";
                    counter++;
                    if (counter > 100) // Prevent infinite loop
                    {
                        return ServiceResult<CreateUserResponseDto>.Failure(
                            new BadRequestException("Unable to generate unique username"));
                    }
                }
                // Check if phone number is already taken
                if (!string.IsNullOrEmpty(registerUserDto.PhoneNumber))
                {
                    var phoneExists = await _context.Users.AnyAsync(x => x.PhoneNumber == registerUserDto.PhoneNumber);
                    if (phoneExists)
                        return ServiceResult<CreateUserResponseDto>.Failure(
                            new BadRequestException($"Phone number: {registerUserDto.PhoneNumber} is already registered."));
                }

                if (string.IsNullOrEmpty(registerUserDto.Email))
                {
                    //assign dummy email to satisfy identity requirements, this will be ignored for login since we check for email confirmed
                    registerUserDto.Email = $"{userName}@{_config["DefaultEmailDomain"]}";
                }

                // Check if email is already taken
                if (!string.IsNullOrEmpty(registerUserDto.Email))
                {
                    var emailExists = await _context.Users.AnyAsync(x => x.Email == registerUserDto.Email);
                    if (emailExists)
                        return ServiceResult<CreateUserResponseDto>.Failure(
                            new BadRequestException($"Email: {registerUserDto.Email} is already registered."));
                }



                using (var scope = await _context.Database.BeginTransactionAsync())
                {
                    var newUser = new AppUser
                    {
                        UserName = userName,
                        Email = registerUserDto.Email,
                        PhoneNumber = registerUserDto.PhoneNumber,
                        FirstName = registerUserDto.FirstName,
                        LastName = registerUserDto.LastName,
                        TenantId = tenantId,
                        EmailConfirmed = false, // Require email verification
                        PhoneNumberConfirmed = false, // Require phone verification
                        LockoutEnabled = false
                    };

                    var result = await _userManager.CreateAsync(newUser, registerUserDto.Password);
                    if (result.Succeeded)
                    {
                        // Self-registered users default to Contractor (new tenant
                        // org). Crew/Client accounts come from invitations.
                        var defaultRoles = new[] { UserRoles.Contractor };
                        foreach (var defaultRole in defaultRoles)
                        {
                            var roleExists = await _roleManager.RoleExistsAsync(defaultRole);
                            if (roleExists)
                            {
                                await _userManager.AddToRoleAsync(newUser, defaultRole);
                            }
                        }

                        await scope.CommitAsync();

                        // Send verification email/SMS
                        if (!string.IsNullOrEmpty(newUser.Email) && !newUser.Email.EndsWith($"@{_config["DefaultEmailDomain"]}"))
                        {
                            // Send email verification code
                            var sendCodeDto = new SendVerificationCodeDto
                            {
                                UserId = newUser.Id,
                                Email = newUser.Email
                            };
                            await SendVerificationCode(sendCodeDto);
                            _logger.LogInformation($"Verification email sent to {newUser.Email}");
                        }
                        else if (!string.IsNullOrEmpty(newUser.PhoneNumber))
                        {
                            // Send SMS verification code
                            var sendCodeDto = new SendVerificationCodeDto
                            {
                                UserId = newUser.Id,
                                PhoneNumber = newUser.PhoneNumber
                            };
                            await SendVerificationCode(sendCodeDto);
                            _logger.LogInformation($"Verification SMS sent to {newUser.PhoneNumber}");
                        }

                        var output = newUser.Adapt<CreateUserResponseDto>();
                        return ServiceResult<CreateUserResponseDto>.Success(output);
                    }

                    await scope.RollbackAsync();
                    var outputErrors = new List<string>();
                    foreach (var error in result.Errors)
                    {
                        outputErrors.Add(error.Description);
                    }
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException(string.Join("\n", outputErrors)));
                }
            }
        }

        public async Task<ServiceResult<bool>> DeleteUser([Required][NotNull] string userId)
        {
            try
            {
                var userFromDb = _context.Users.FirstOrDefault(x => x.TenantId == userId);

                if (userFromDb == null) return ServiceResult<bool>.Failure(new NotFoundException($"User with Id: {userId} does not exist"));

                userFromDb.IsDeleted = true;
                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting user {ex}", ex);
                return ServiceResult<bool>.Failure(new ServerErrorException("Unknown error while deleting user."));
            }

        }

        public async Task<ServiceResult<string>> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (string.IsNullOrEmpty(resetPasswordDto.OldPassword) && string.IsNullOrEmpty(resetPasswordDto.ResetToken) && string.IsNullOrEmpty(resetPasswordDto.VerificationCode))
            {
                return ServiceResult<string>.Failure(new BadRequestException("Old password, reset token, or verification code is required"));
            }

            // Find user by email or phone
            AppUser? user = null;
            var phoneCheck = CheckIsMtnOrAirtel(resetPasswordDto.Identifier);
            if (phoneCheck.isValid)
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneCheck.cleanedupNumber);
            }
            else
            {
                user = await _userManager.FindByEmailAsync(resetPasswordDto.Identifier);
                // Fallback: also try by EmailAddress if provided for backward compatibility
                if (user == null && !string.IsNullOrEmpty(resetPasswordDto.EmailAddress))
                {
                    user = await _userManager.FindByEmailAsync(resetPasswordDto.EmailAddress);
                }
            }

            if (user == null) return ServiceResult<string>.Failure(new NotFoundException("User does not exist"));

            IdentityResult? result = null;

            // Scenario 1: User knows old password (profile page)
            if (resetPasswordDto?.OldPassword is not null)
            {
                result = await _userManager.ChangePasswordAsync(user, resetPasswordDto.OldPassword, resetPasswordDto.Password);
            }
            // Scenario 2: Phone verification with OTP
            else if (resetPasswordDto != null && !string.IsNullOrEmpty(resetPasswordDto.VerificationCode))
            {
                // Find and validate the verification code
                var verificationRecord = await _context.VerificationCodes
                    .Where(vc => vc.UserId == user.Id && !vc.IsUsed && vc.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(vc => vc.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verificationRecord == null)
                {
                    return ServiceResult<string>.Failure(new BadRequestException("Invalid or expired verification code"));
                }

                // Verify the OTP
                var verifyResult = _otpHasher.VerifyHashedPassword(user, verificationRecord.Code, resetPasswordDto.VerificationCode);
                if (verifyResult != PasswordVerificationResult.Success)
                {
                    return ServiceResult<string>.Failure(new BadRequestException("Invalid verification code"));
                }

                // Use the stored reset token
                if (string.IsNullOrEmpty(verificationRecord.ResetToken))
                {
                    return ServiceResult<string>.Failure(new BadRequestException("Invalid reset token"));
                }
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(verificationRecord.ResetToken));
                result = await _userManager.ResetPasswordAsync(user, code, resetPasswordDto.Password);

                if (result.Succeeded)
                {
                    // Mark verification code as used
                    verificationRecord.IsUsed = true;
                    await _context.SaveChangesAsync();
                }
            }
            // Scenario 3: Email reset with token
            else if (!string.IsNullOrEmpty(resetPasswordDto.ResetToken))
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.ResetToken));
                result = await _userManager.ResetPasswordAsync(user, code, resetPasswordDto.Password);
            }
            else
            {
                return ServiceResult<string>.Failure(new BadRequestException("Invalid reset request"));
            }

            if (result != null && result.Succeeded)
            {
                // Send notification about password change
                if (!string.IsNullOrEmpty(user.Email) && !user.Email.EndsWith($"@{_config["DefaultEmailDomain"]}", StringComparison.OrdinalIgnoreCase))
                {
                    _emailSmtpService.SendPasswordChangedNotificationEmail(user.Email, user.FirstName ?? "User");
                }
                else if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    await _pandoraSmsService.SendSmsAsync(user.PhoneNumber, "Your password has been successfully changed. If you didn't make this change, please contact support immediately.");
                }

                return ServiceResult<string>.Success("Password changed successfully");
            }
            else
            {
                var outputErrors = new List<string>();
                if (result != null)
                {
                    foreach (var error in result.Errors)
                    {
                        outputErrors.Add(error.Description);
                    }
                }
                else
                {
                    outputErrors.Add("Failed to reset password");
                }
                return ServiceResult<string>.Failure(new BadRequestException(string.Join("\n", outputErrors)));
            }

        }

        public async Task<ServiceResult<string>> VerifyPasswordResetOtp(string phoneNumber, string otp)
        {
            try
            {
                // Validate phone number
                var phoneCheck = CheckIsMtnOrAirtel(phoneNumber);
                if (!phoneCheck.isValid)
                {
                    return ServiceResult<string>.Failure(new BadRequestException("Invalid phone number"));
                }

                // Find user by phone
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneCheck.cleanedupNumber);
                if (user == null)
                {
                    return ServiceResult<string>.Failure(new NotFoundException("User not found"));
                }

                // Find and validate the verification code
                var verificationRecord = await _context.VerificationCodes
                    .Where(vc => vc.UserId == user.Id && !vc.IsUsed && vc.ExpiresAt > DateTime.UtcNow && vc.Type == VerificationType.Phone)
                    .OrderByDescending(vc => vc.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verificationRecord == null)
                {
                    return ServiceResult<string>.Failure(new BadRequestException("Invalid or expired verification code"));
                }

                // Verify the OTP
                var verifyResult = _otpHasher.VerifyHashedPassword(user, verificationRecord.Code, otp);
                if (verifyResult != PasswordVerificationResult.Success)
                {
                    // Increment attempt count
                    verificationRecord.AttemptCount++;
                    await _context.SaveChangesAsync();

                    if (verificationRecord.AttemptCount >= 5)
                    {
                        verificationRecord.IsUsed = true;
                        await _context.SaveChangesAsync();
                        return ServiceResult<string>.Failure(new BadRequestException("Too many invalid attempts. Please request a new code."));
                    }

                    return ServiceResult<string>.Failure(new BadRequestException("Invalid verification code"));
                }

                // Return the reset token (don't mark as used yet - that happens on actual password reset)
                if (string.IsNullOrEmpty(verificationRecord.ResetToken))
                {
                    return ServiceResult<string>.Failure(new BadRequestException("Reset token not found. Please request a new code."));
                }
                return ServiceResult<string>.Success(verificationRecord.ResetToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password reset OTP");
                return ServiceResult<string>.Failure(new ServerErrorException("Failed to verify code. Please try again."));
            }
        }

        public async Task<ServiceResult<CreateUserResponseDto>> UpdateUser(CreateUserDto updateUserprofile)
        {
            //using  trnsaction scope

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                return await UpdateUserWithRoleAsync(updateUserprofile);
            });
            //submethod to allow breakpoint hitting in an arrow function
            async Task<ServiceResult<CreateUserResponseDto>> UpdateUserWithRoleAsync(CreateUserDto updateUserprofile)
            {
                using (var scope = await _context.Database.BeginTransactionAsync())
                {
                    //var files = _httpContextAccessor. HttpContext?.Request.Form.Files;

                    //read user from user context
                    //var currentUserId = _tenantProvider.GetCurrentUser()?.Id;

                    var objFromDb = await _userManager.FindByIdAsync(updateUserprofile.Id ?? ""); //TODO: get id from httpcontext

                    if (objFromDb == null) return ServiceResult<CreateUserResponseDto>.Failure(new NotFoundException("User does not exist"));

                    //save image if exists

                    #region Profile pic section
                    //if (files.Count() > 0)
                    //{
                    //    //save dp if exists
                    //    var dp = files.Where(x => x.Name.ToLower() == "profilepic").ToArray();
                    //    if (dp.Any())
                    //    {
                    //        var saveResult = await SaveFile(dp.FirstOrDefault());
                    //        if (saveResult.ReturnCode == "200")
                    //        {
                    //            objFromDb.ProfilePicUrl = saveResult.Link;
                    //        }
                    //        else
                    //        {
                    //            return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException(saveResult.Message));

                    //        }
                    //    }
                    //    //save cover pic if exists
                    //    var cover = files.Where(x => x.Name.ToLower() == "coverpic").ToArray();
                    //    if (cover.Any())
                    //    {
                    //        var saveResult = await SaveFile(cover.FirstOrDefault());
                    //        if (saveResult.ReturnCode == "200")
                    //        {
                    //            objFromDb.CoverPhotoUrl = saveResult.Link;
                    //        }
                    //        else
                    //        {
                    //            return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException(saveResult.Message));

                    //        }
                    //    }

                    //} 
                    #endregion

                    // Check if username is taken

                    if (!string.IsNullOrWhiteSpace(updateUserprofile.UserName) && updateUserprofile.UserName != objFromDb.UserName)
                    {
                        var existingUser = await _userManager.FindByNameAsync(updateUserprofile.UserName);
                        if (existingUser != null)
                            return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException("Username is already taken.")); // Triggers Identity validation error
                        objFromDb.UserName = updateUserprofile.UserName;
                    }

                    // Check if phone number is taken
                    if (!string.IsNullOrWhiteSpace(updateUserprofile.PhoneNumber) && updateUserprofile.PhoneNumber != objFromDb.PhoneNumber)
                    {
                        var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == updateUserprofile.PhoneNumber);
                        if (existingUser != null)
                            return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException("Phone number is already in use.")); // Triggers Identity validation error
                        objFromDb.PhoneNumber = updateUserprofile.PhoneNumber;
                    }
                    //check if email is taken
                    if (!string.IsNullOrWhiteSpace(updateUserprofile.Email) && updateUserprofile.Email != objFromDb.Email)
                    {
                        var existingUser = await _userManager.FindByEmailAsync(updateUserprofile.Email);
                        if (existingUser != null)
                            return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException("Email is already in use.")); // Triggers Identity validation error
                        objFromDb.Email = updateUserprofile.Email;
                    }
                    objFromDb.Aboutme = string.IsNullOrWhiteSpace(updateUserprofile.Aboutme) ? objFromDb.Aboutme : updateUserprofile.Aboutme;
                    objFromDb.Contacts = string.IsNullOrWhiteSpace(updateUserprofile.PhoneNumber) ? objFromDb.Contacts : updateUserprofile.PhoneNumber;
                    objFromDb.Address = string.IsNullOrWhiteSpace(updateUserprofile.Address) ? objFromDb.Address : updateUserprofile.Address;
                    //objFromDb.UserName = string.IsNullOrWhiteSpace(updateUserprofile.UserName) ? objFromDb.UserName : updateUserprofile.UserName;
                    objFromDb.PhoneNumber = string.IsNullOrWhiteSpace(updateUserprofile.PhoneNumber) ? objFromDb.PhoneNumber : updateUserprofile.PhoneNumber;
                    objFromDb.FirstName = string.IsNullOrWhiteSpace(updateUserprofile.FirstName) ? objFromDb.FirstName : updateUserprofile.FirstName;
                    objFromDb.LastName = string.IsNullOrWhiteSpace(updateUserprofile.LastName) ? objFromDb.LastName : updateUserprofile.LastName;

                    //password change
                    if (updateUserprofile.ChangePassword)
                    {
                        if (string.IsNullOrEmpty(updateUserprofile.Password) || updateUserprofile.Password.Length < 4) return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException("Invalid password"));
                        var passwordHasher = _userManager.PasswordHasher;
                        objFromDb.PasswordHash = passwordHasher.HashPassword(objFromDb, updateUserprofile.Password);

                    }
                    //TODO: Updating an Email

                    var result = await _userManager.UpdateAsync(objFromDb);
                    if (result.Succeeded)
                    {
                        //add to role
                        foreach (var roleName in updateUserprofile.UserRolesDto.GetRoleStatuses())
                        {
                            ServiceResult<string> role = default!;
                            if (roleName.Status)
                            {
                                var isInRole = await _userManager.IsInRoleAsync(objFromDb, roleName.Name);

                                if (!isInRole)
                                {

                                    role = await AddUserToRoleAsync(updateUserprofile.Id, roleName.Name);

                                    if (!role.IsSuccess)
                                    {
                                        //failed to add one of the roles
                                        await scope.RollbackAsync();
                                        return ServiceResult<CreateUserResponseDto>.Failure(role.Error);
                                    }
                                }
                            }
                            else
                            {
                                var isInRole = await _userManager.IsInRoleAsync(objFromDb, roleName.Name);
                                if (isInRole)
                                {
                                    role = await RemoveUserFromRoleAsync(objFromDb.Id, roleName.Name);
                                    if (!role.IsSuccess)
                                    {
                                        //failed to add one of the roles
                                        await scope.RollbackAsync();
                                        return ServiceResult<CreateUserResponseDto>.Failure(role.Error);
                                    }
                                }
                            }

                        }
                        await scope.CommitAsync();
                        var output = objFromDb.Adapt<CreateUserResponseDto>();
                        return ServiceResult<CreateUserResponseDto>.Success(output);
                    }
                    var outputErrors = new List<string>();
                    foreach (var error in result.Errors)
                    {
                        outputErrors.Add(error.Description);
                    }
                    return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException(string.Join("\n", outputErrors)));

                }
            }




        }

        public async Task<ServiceResult<UpdateUserprofileOutDto>> GetUserProfile(string id = null, string userName = null)
        {
            if (id == null && userName == null)
            {
                return ServiceResult<UpdateUserprofileOutDto>.Failure(new BadRequestException("Both Username and Id cannot be null for this request"));

            }
            AppUser user;
            if (!string.IsNullOrEmpty(userName) && userName != "null" && userName != "undefined")
            {
                user = await _userManager.FindByNameAsync(userName);
            }
            else
            {
                user = await _userManager.FindByIdAsync(id);
            }

            if (user == null) return ServiceResult<UpdateUserprofileOutDto>.Failure(new NotFoundException("User not found"));

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseLink = request != null ? $"{request?.Scheme}://{request?.Host.Value}/" : null;
            //var list = new List<CreatedPostOutDto>();

            var userProfile = user.Adapt<UpdateUserprofileOutDto>();

            userProfile.ProfilePicUrl = !string.IsNullOrEmpty(user.ProfilePicUrl) ? baseLink + user.ProfilePicUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";
            userProfile.CoverPicUrl = !string.IsNullOrEmpty(user.CoverPhotoUrl) ? baseLink + user.CoverPhotoUrl : "https://via.placeholder.com/728x500.png?text=No+Cover+Image";

            return ServiceResult<UpdateUserprofileOutDto>.Success(userProfile);
        }


        //private async Task<string> GenerateUserName(string oldUserName)
        //{
        //    int random = 1;
        //    string newUsername = oldUserName + random.ToString();

        //    while (await _userManager.FindByNameAsync(newUsername) != null)
        //    {
        //        random++;
        //        newUsername = oldUserName + random.ToString();
        //        //prevent  infinte loop
        //        if (random > 100)
        //        {
        //            break;
        //        }

        //    }
        //    return newUsername;
        //}

        /// <summary>
        /// Returns true if the email address ends with the configured employer domain (OfficialEmployerEmailDomain).
        /// </summary>
        private bool IsEmployerDomainEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var domain = _config["OfficialEmployerEmailDomain"];
            if (string.IsNullOrWhiteSpace(domain)) return false;
            return email.EndsWith($"@{domain.Trim()}", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<ServiceResult<string>> AddUserToRoleAsync(string userId, string roleName)
        {
            try
            {
                // Find the user by their ID
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return ServiceResult<string>.Failure(new NotFoundException("User not found."));
                }
                // Check if the role exists
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return ServiceResult<string>.Failure(new BadRequestException($"Role '{roleName}' does not exist."));
                }
                // Check if the user is already in the role
                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (isInRole)
                {
                    return ServiceResult<string>.Failure(new BadRequestException("User is already in the specified role."));
                }

                // Add the user to the role
                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    return ServiceResult<string>.Success("User successfully added to the role.");
                }

                // If role addition failed, concatenate the errors
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult<string>.Failure(new ServerErrorException($"Failed to add user to role. Errors: {errors}"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while adding user to role. {ex}", ex);
                return ServiceResult<string>.Failure(new ServerErrorException("An error occurred while adding user to role. Please try again later."));
            }
        }

        public async Task<ServiceResult<UserRolesDto>> GetRolesForUserByUserId(string userId)
        {
            try
            {
                // Find the user by their ID
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return ServiceResult<UserRolesDto>.Failure(new NotFoundException("User not found."));
                }

                var roles = await _userManager.GetRolesAsync(user);

                var rolesObject = OtherDomainMethods.GenerateUserRoles(roles.ToList());
                return ServiceResult<UserRolesDto>.Success(rolesObject);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting roles for user. {ex}", ex);
                return ServiceResult<UserRolesDto>.Failure(new ServerErrorException("An error occurred while getting roles for the User. Please try again later."));
            }
        }

        public async Task<ServiceResult<bool>> GetIsEmployeeByUserId(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Failure(new NotFoundException("User not found."));

                return ServiceResult<bool>.Success(user.IsEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while checking IsEmployee for user. {ex}", ex);
                return ServiceResult<bool>.Failure(new ServerErrorException("An error occurred while checking employee status. Please try again later."));
            }
        }

        public async Task<ServiceResult<string>> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            try
            {
                // Find the user by their ID
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return ServiceResult<string>.Failure(new NotFoundException("User not found."));
                }
                // Check if the role exists
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return ServiceResult<string>.Failure(new BadRequestException($"Role '{roleName}' does not exist."));
                }
                // Check if the user is in the role
                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (!isInRole)
                {
                    return ServiceResult<string>.Failure(new BadRequestException("User is not in the specified role."));
                }

                // Remove the user from the role
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    return ServiceResult<string>.Success("User successfully removed from the role.");
                }

                // If role removal failed, concatenate the errors
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult<string>.Failure(new ServerErrorException($"Failed to remove user from role. Errors: {errors}"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while removing user from role. {ex}", ex);
                return ServiceResult<string>.Failure(new ServerErrorException("An error occurred while removing user from role. Please try again later."));
            }
        }
        public async Task<ServiceResult<string>> IssueSyncKeyAsync(InitialSeedDataDto seedData)
        {

            try
            {
                var tenanData = await _context.tbl_Tenants
                    .IgnoreQueryFilters()
                       .FirstOrDefaultAsync(x => x.TenantId == seedData.tenantId);


                if (tenanData == null)
                {
                    var strategy = _context.Database.CreateExecutionStrategy();

                    await strategy.ExecuteAsync(async () =>
                    {
                        using (var scope = await _context.Database.BeginTransactionAsync())
                        {

                            //create tenant first
                            var tenantDto = seedData.tenantData;
                            tenantDto.IsActive = true;

                            var newTenant = await _context.tbl_Tenants.AddAsync(tenantDto);
                            var powerUSer = seedData.AppUser;
                            _context.Add(powerUSer);

                            await _context.SaveChangesAsync();

                            if (seedData.UserRoleNames is not null)
                            {

                                foreach (var roleName in seedData.UserRoleNames)
                                {
                                    var role = await _roleManager.FindByNameAsync(roleName);
                                    if (role == null)
                                    {
                                        //create role
                                        role = new IdentityRole(roleName);
                                        await _roleManager.CreateAsync(role);
                                    }
                                    //add user to role
                                    await _userManager.AddToRoleAsync(powerUSer, roleName);

                                }
                                await _context.SaveChangesAsync();

                            }

                            tenanData = newTenant.Entity;

                            // add sync Data
                            await DatabaseSeeder.SeedTenantSettingsAsync(_context, _logger, seedData);
                            scope.Commit();

                        }
                    });

                }

                if (!tenanData.IsActive)
                {
                    _logger.LogWarning("Tenant with ID:{tenantId} is not active.", seedData.tenantId);
                    return ServiceResult<string>.Failure(new BadRequestException($"Tenant with ID:{seedData.tenantId} has is not active. PLease contact Support."));
                }

                //check if no new key has been issued
                if (!string.IsNullOrEmpty(tenanData.keyHarsh) && !string.IsNullOrEmpty(tenanData.CocurrencyKey))
                {
                    _logger.LogWarning("Key already issued for tenant {tenantId}.", tenanData.TenantId);
                    return ServiceResult<string>.Success("Key already issued. Please contact support.");
                }

                //check if user has paid
                var rawKey = Guid.NewGuid().ToString("N");
                var instance = new TenantInstance
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = tenanData.TenantId,
                    Created = DateTime.UtcNow,
                    IsActive = true,
                };

                tenanData.keyHarsh = _hasher.HashPassword(instance, rawKey);
                tenanData.CocurrencyKey = instance.Id;
                tenanData.LastRenewal = instance.Created;
                tenanData.IsActive = true; // Ensure tenant is active when issuing a new key
                                           //update Tenant
                await _context.SaveChangesAsync();

                return ServiceResult<string>.Success(rawKey);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while issuing sync key. {ex}", ex);
                return ServiceResult<string>.Failure(new ServerErrorException(ex.Message));
            }
        }

        public async Task<ServiceResult<string>> ValidateApiKeyAsync(string apiKey, string tenantId, string userId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(apiKey))
            {
                return ServiceResult<string>.Failure(new UnAuthorizedException("API Key or userId or tenantId cannot be null or empty."));
            }

            //check if tenant exists

            var tenant = await _context.tbl_Tenants
                .IgnoreQueryFilters()
                .Where(i => i.IsActive && i.TenantId == tenantId)
                .FirstOrDefaultAsync();
            if (tenant == null) return ServiceResult<string>.Failure(new UnAuthorizedException($"Tenant with ID: {tenantId} is Invalid or not active"));

            var instance = new TenantInstance
            {
                Id = tenant.CocurrencyKey,
                Name = tenant.TenantId,
                Created = tenant.LastRenewal,
                IsActive = tenant.IsActive,
            };
            var result = _hasher.VerifyHashedPassword(
                    instance,
                    tenant.keyHarsh,
                    apiKey
                );

            if (result == PasswordVerificationResult.Success)
            {

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<string>.Failure(new UnAuthorizedException("User ID not found in JWT token."));
                }
                // Check if the user exists in the tenant
                var userExists = await _context.Users.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && !(u.IsDeleted ?? false));
                if (userExists is null)
                {
                    return ServiceResult<string>.Failure(new UnAuthorizedException("User does not exist in the specified tenant."));
                }
                _logger.LogInformation("Here are the headers before adding a sync token {headers}", _httpContextAccessor.HttpContext?.Request.Headers);

                var token = await GenerateToken(userExists, tenantId);


                return ServiceResult<string>.Success(token.token);
            }


            return ServiceResult<string>.Failure(new UnAuthorizedException("Invalid Api key"));
        }

        public string GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "user")?.Value;

                if (string.IsNullOrEmpty(userClaim))
                {
                    throw new InvalidOperationException("User claim not found in token");
                }

                // Parse the user claim JSON
                using var jsonDocument = JsonDocument.Parse(userClaim);
                var userObject = jsonDocument.RootElement;

                // Extract the Id field
                if (userObject.TryGetProperty("Id", out var idElement))
                {
                    return idElement.GetString();
                }

                throw new InvalidOperationException("User ID not found in user claim");
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse user claim JSON", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading token", ex);
            }
        }

        public async Task<LoginResponseDto> GenerateToken(AppUser user, string tenantId, string ipAddress = null, string deviceType = null, string browserType = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Roles are per account when the membership names them: a person can
            // be a developer in their own and delivery-side in another. Falls
            // back to the global roles, which is what every pre-P2.5 row has.
            var accounts = await GetAccountsAsync(user.Id, tenantId);
            var membershipRoles = accounts.FirstOrDefault(a => a.TenantId == tenantId)?.Roles;

            var roles = string.IsNullOrWhiteSpace(membershipRoles)
                ? await _userManager.GetRolesAsync(user)
                : membershipRoles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            user.Roles = JsonConvert.SerializeObject(roles);

            // The claims DTO carries the ACTIVE account, not AppUser.TenantId,
            // which is only where this person lands at sign-in.
            var claimsUser = user.Adapt<UserClaimsDto>();
            claimsUser.TenantId = tenantId;

            var claims = new List<Claim>()
            {
                new Claim("user", JsonConvert.SerializeObject(claimsUser))
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            claims.Add(new Claim("TenantId", tenantId ?? "default"));

            var expiryTime = DateTime.UtcNow.AddMinutes(10); // Changed from 3 days to 1 hour
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: expiryTime,
                signingCredentials: credentials);

            // Generate refresh token with login history
            var deviceFingerprint = GenerateDeviceFingerprint(ipAddress, deviceType, browserType);

            // Check for existing device
            var existingRefreshToken = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id &&
                           rt.DeviceFingerprint == deviceFingerprint)
                .OrderByDescending(rt => rt.LastLoginAt)
                .FirstOrDefaultAsync();

            var refreshToken = new tbl_RefreshToken
            {
                UserId = user.Id,
                TenantId = tenantId,
                IpAddress = ipAddress,
                DeviceType = deviceType,
                BrowserType = browserType,
                DeviceFingerprint = deviceFingerprint,
                FirstLoginAt = existingRefreshToken?.FirstLoginAt ?? DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                LoginCount = (existingRefreshToken?.LoginCount ?? 0) + 1
            };

            // Deactivate old tokens for this device
            if (existingRefreshToken != null)
            {
                existingRefreshToken.RevokedAt = DateTime.UtcNow;
            }

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var result = new LoginResponseDto
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                exp = expiryTime,
                TenantId = tenantId,
                RefreshToken = refreshToken.Token,
                Accounts = accounts
            };

            return result;
        }

        /// <summary>
        /// Every account this person may act in, newest membership last.
        /// <c>AppUser.TenantId</c> is included even without a membership row so a
        /// user who pre-dates the table is never locked out of their own account.
        /// </summary>
        private async Task<List<TenantMembershipDto>> GetAccountsAsync(string userId, string? activeTenantId)
        {
            // IgnoreQueryFilters is not needed — tbl_TenantMemberships is
            // deliberately unscoped, because the filter would hide every account
            // except the one being left.
            var memberships = await _context.tbl_TenantMemberships
                .Where(m => m.UserId == userId && m.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var user = await _context.Users.IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.TenantId is { Length: > 0 } home && memberships.All(m => m.TenantId != home))
            {
                memberships.Add(new tbl_TenantMembership
                {
                    UserId = userId,
                    TenantId = home,
                    IsDefault = true,
                    IsActive = true
                });
            }

            var ids = memberships.Select(m => m.TenantId).ToList();
            var names = await _context.tbl_Tenants.IgnoreQueryFilters()
                .Where(t => ids.Contains(t.TenantId))
                .Select(t => new { t.TenantId, t.Name })
                .AsNoTracking()
                .ToListAsync();

            return memberships
                .Select(m => new TenantMembershipDto
                {
                    TenantId = m.TenantId,
                    TenantName = names.FirstOrDefault(n => n.TenantId == m.TenantId)?.Name ?? m.TenantId,
                    IsDefault = m.IsDefault,
                    IsCurrent = m.TenantId == activeTenantId,
                    Roles = m.Roles,
                    JoinedAt = m.JoinedAt
                })
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.TenantName)
                .ToList();
        }

        public async Task<ServiceResult<List<TenantMembershipDto>>> GetMyAccounts(string userId, string? activeTenantId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return ServiceResult<List<TenantMembershipDto>>.Failure(
                        new UnAuthorizedException("Not signed in."));

                return ServiceResult<List<TenantMembershipDto>>.Success(
                    await GetAccountsAsync(userId, activeTenantId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing accounts for {UserId}", userId);
                return ServiceResult<List<TenantMembershipDto>>.Failure(new ServerErrorException(ex.Message));
            }
        }

        /// <summary>
        /// Re-issue the token against another account. Membership is checked here
        /// and nowhere else — the client picks, the server decides.
        /// </summary>
        public async Task<ServiceResult<LoginResponseDto>> SwitchTenant(string userId, string tenantId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
                    return ServiceResult<LoginResponseDto>.Failure(
                        new BadRequestException("A user and an account are required."));

                var user = await _context.Users.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user is null)
                    return ServiceResult<LoginResponseDto>.Failure(new NotFoundException("User not found."));

                var accounts = await GetAccountsAsync(userId, tenantId);
                if (accounts.All(a => a.TenantId != tenantId))
                    return ServiceResult<LoginResponseDto>.Failure(
                        new ForbiddenException("You do not belong to that account."));

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

                var token = await GenerateToken(user, tenantId, ipAddress,
                    DetermineDeviceType(userAgent), DetermineBrowserType(userAgent));

                _logger.LogInformation("User {UserId} switched to account {TenantId}", userId, tenantId);
                return ServiceResult<LoginResponseDto>.Success(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching {UserId} to account {TenantId}", userId, tenantId);
                return ServiceResult<LoginResponseDto>.Failure(new ServerErrorException(ex.Message));
            }
        }

        public async Task<ServiceResult<LoginResponseDto>> RefreshToken(RefreshTokenRequestDto request, string ipAddress = null, string deviceType = null, string browserType = null)
        {
            try
            {
                // Validate JWT token structure (even if expired)
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(request.Token))
                {
                    return ServiceResult<LoginResponseDto>.Failure(new BadRequestException("Invalid token format"));
                }

                var jwtToken = handler.ReadJwtToken(request.Token);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<LoginResponseDto>.Failure(new BadRequestException("Invalid token claims"));
                }

                // Parse user ID from JSON claim
                using var jsonDocument = System.Text.Json.JsonDocument.Parse(userId);
                var userObject = jsonDocument.RootElement;
                string userIdValue = userObject.GetProperty("Id").GetString();

                // Find the refresh token
                var storedRefreshToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userIdValue);

                if (storedRefreshToken == null)
                {
                    return ServiceResult<LoginResponseDto>.Failure(new UnAuthorizedException("Invalid refresh token"));
                }

                // Check if token is active or in grace period
                if (!storedRefreshToken.IsActive && !storedRefreshToken.IsInGracePeriod)
                {
                    return ServiceResult<LoginResponseDto>.Failure(new UnAuthorizedException("Refresh token has expired or been revoked"));
                }

                // Get the user
                var user = await _userManager.FindByIdAsync(userIdValue);
                if (user == null)
                {
                    return ServiceResult<LoginResponseDto>.Failure(new NotFoundException("User not found"));
                }

                // Generate new tokens
                var newTokenResponse = await GenerateToken(user, storedRefreshToken.TenantId, ipAddress, deviceType, browserType);

                // Revoke old refresh token
                storedRefreshToken.RevokedAt = DateTime.UtcNow;
                storedRefreshToken.ReplacedByToken = newTokenResponse.RefreshToken;
                await _context.SaveChangesAsync();

                return ServiceResult<LoginResponseDto>.Success(newTokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while refreshing token. {ex}", ex);
                return ServiceResult<LoginResponseDto>.Failure(new ServerErrorException("Error refreshing token. Please try again later"));
            }
        }

        private async Task<ServiceResult<AppUser>> AuthenticateByUserNameOrPhone([Required][OneRequiredProp("UserName", "PhoneNumber")] LoginUserNameOrPhoneDto userLogin)
        {
            var user = new AppUser();

            if (!string.IsNullOrEmpty(userLogin.UserName))
            {
                user = await _userManager.FindByNameAsync(userLogin.UserName);
                if (user == null)
                {
                    return ServiceResult<AppUser>.Failure(
                        new BadRequestException("Invalid credentials. Please try again."));
                }
            }

            if (!string.IsNullOrEmpty(userLogin.PhoneNumber))
            {
                var phoneNumber = cleanPhoneNumber(userLogin.PhoneNumber);

                //First get all users in memory to check their phone numbers correctly
                var users = await _context.Users.ToListAsync();
                user = users.FirstOrDefault(x => cleanPhoneNumber(x.PhoneNumber ?? "") == phoneNumber);

                if (user == null)
                {
                    return ServiceResult<AppUser>.Failure(
                        new BadRequestException("Invalid credentials. Please try again."));
                }
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, false);

            if (!result.Succeeded)
            {
                await Task.Delay(500); // slight delay for timing attack mitigation
                return ServiceResult<AppUser>.Failure(
                    new BadRequestException("Invalid credentials. Please try again."));
            }

            return ServiceResult<AppUser>.Success(user);
        }

        private async Task<AppUser> Authenticate(UserLogin userLogin)
        {
            //var user = await _userManager.FindByEmailAsync(userLogin.Email);
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == userLogin.Email && x.IsDeleted != true);
            if (user == null)
            {
                //user = await _userManager.FindByNameAsync(userLogin.Email);
                user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.UserName == userLogin.Email && x.IsDeleted != true);

                if (user == null && cleanPhoneNumber(userLogin.Email).Length > 5)
                {
                    var contact = CheckIsMtnOrAirtel(userLogin.Email);
                    if (!contact.isValid) contact.cleanedupNumber = cleanPhoneNumber(userLogin.Email);

                    user = _context.Users
                        .FirstOrDefault(x => x.PhoneNumber == contact.cleanedupNumber);
                }
            }


            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, false);

                if (result.Succeeded)
                {
                    return user;
                }

            }
            return null;
        }

        private string cleanPhoneNumber(string numberInput)
        {
            if (string.IsNullOrEmpty(numberInput)) return "";
            try
            {
                //remove leading zero, spaces and non numeric
                var clean = int.Parse(string.Join("", numberInput.Where(x => char.IsDigit(x)).ToArray()));
                return clean.ToString();
            }
            catch (Exception)
            {

                return "";
            }
        }

        public async Task<ServiceResult<bool>> UserNameTaken(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    return ServiceResult<bool>.Failure(new BadRequestException("Username is required"));

                var userNameExists = await _userManager.FindByNameAsync(userName);

                if (userNameExists != null)
                {
                    return ServiceResult<bool>.Success(true);
                }

                return ServiceResult<bool>.Success(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error checking if username is taken: {ex}", ex);
                return ServiceResult<bool>.Failure(new ServerErrorException("An error occurred while checking username availability."));
            }
        }

        public async Task<ServiceResult<bool>> EmailTaken(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return ServiceResult<bool>.Failure(new BadRequestException("Email is required"));

                var emailExists = await _userManager.FindByEmailAsync(email);
                if (emailExists != null)
                {
                    return ServiceResult<bool>.Success(true);
                }
                return ServiceResult<bool>.Success(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error checking if email is taken: {ex}", ex);
                return ServiceResult<bool>.Failure(new ServerErrorException("An error occurred while checking email availability."));
            }
        }

        //Save imageupload file
        private async Task<ResultObject> SaveFile(IFormFile dto)
        {
            //handle image upload
            string webRootPath = _webHostEnvironment.WebRootPath;
            string link = null;
            var obj = new ResultObject();

            var files = dto;
            try
            {
                if (files.Length > 0)
                {
                    string uploadPath = Path.Combine(webRootPath, @"\media\images".TrimStart('\\')); // doesnt work if second path has a trailling slash
                    string extension = Path.GetExtension(files.FileName);
                    if (!(extension.ToLower() == ".jpg" || extension.ToLower() == ".png" || extension.ToLower() == ".jpeg"))
                    {
                        throw new ApplicationException("The image file type must be jpg or png");

                    }

                    string fileNewName = Guid.NewGuid().ToString() + extension;

                    using (var fileStream = new FileStream(Path.Combine(uploadPath, fileNewName), FileMode.Create))
                    {
                        await files.CopyToAsync(fileStream);
                    }
                    link = @"media/images/" + fileNewName;
                }

                string msg = "Upload of Image Successful";
                obj.ReturnCode = "200";
                obj.ReturnDescription = msg;
                obj.Response = "Success";
                obj.Message = msg;
                obj.Link = link;

            }
            catch (Exception ex)
            {
                string msg = "An Error has occurred while attempting to Upload Image: Inner Exception: " + ex.Message;
                obj.ReturnCode = "501";
                obj.ReturnDescription = msg;
                obj.Response = "Failed";
                obj.Message = msg;
            }
            return obj;
        }



        private async Task<bool> CheckForNewDevice(string userId, string tenantId, string ipAddress, string deviceType, string browserType)
        {
            try
            {
                // Create a device fingerprint based on IP, device type, and browser
                var deviceFingerprint = GenerateDeviceFingerprint(ipAddress, deviceType, browserType);

                // Check if this device fingerprint exists for this user in refresh tokens
                var existingDevice = await _context.RefreshTokens
                    .AnyAsync(rt =>
                        rt.UserId == userId &&
                        rt.DeviceFingerprint == deviceFingerprint);

                return !existingDevice; // Return true if this is a new device
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while checking device history. {ex}", ex);
                // Don't fail the login if we can't check device history
                return false;
            }
        }

        private string GenerateDeviceFingerprint(string ipAddress, string deviceType, string browserType)
        {
            // Create a consistent fingerprint from the combination of IP, device, and browser
            var fingerprintData = $"{ipAddress?.ToLowerInvariant() ?? "unknown"}|{deviceType?.ToLowerInvariant() ?? "unknown"}|{browserType?.ToLowerInvariant() ?? "unknown"}";

            // Hash the fingerprint for consistency and storage efficiency
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprintData));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private string GenerateNewDeviceLoginEmail(AppUser user, string ipAddress, string deviceType, string browserType, DateTime loginTime)
        {
            var userName = !string.IsNullOrEmpty(user.FirstName)
                ? $"{user.FirstName} {user.LastName}"
                : user.UserName;

            return $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
                .info-box {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #4CAF50; }}
                .warning {{ color: #d9534f; font-weight: bold; }}
                .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>New Device Login Detected</h2>
                </div>
                <div class='content'>
                    <p>Hello {userName},</p>
                    
                    <p>We detected a new login to your ASSETLEN account from a device we haven't seen before.</p>
                    
                    <div class='info-box'>
                        <strong>Login Details:</strong><br>
                        <strong>Time:</strong> {loginTime:yyyy-MM-dd HH:mm:ss} UTC<br>
                        <strong>IP Address:</strong> {ipAddress ?? "Unknown"}<br>
                        <strong>Device Type:</strong> {deviceType ?? "Unknown"}<br>
                        <strong>Browser:</strong> {browserType ?? "Unknown"}
                    </div>
                    
                    <p><strong>Was this you?</strong></p>
                    <p>If you recognize this activity, you can safely ignore this email.</p>
                    
                    <p class='warning'>If you don't recognize this activity:</p>
                    <ul>
                        <li>Change your password immediately</li>
                        <li>Review your account for any unauthorized changes</li>
                        <li>Contact our support team at support@assetlen.com</li>
                    </ul>
                    
                    <div class='footer'>
                        <p>This is an automated security notification from ASSETLEN.</p>
                        <p>For assistance, contact us at <a href='https://assetlen.com'>https://assetlen.com</a></p>
                    </div>
                </div>
            </div>
        </body>
        </html>";
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

            if (userAgent.Contains("Edg", StringComparison.OrdinalIgnoreCase))
                return "Edge";
            if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase) && !userAgent.Contains("Edg", StringComparison.OrdinalIgnoreCase))
                return "Chrome";
            if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                return "Firefox";
            if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase) && !userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
                return "Safari";
            if (userAgent.Contains("Opera", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("OPR", StringComparison.OrdinalIgnoreCase))
                return "Opera";

            return "Unknown";
        }

        public async Task<ServiceResult<string>> SendVerificationCode(SendVerificationCodeDto sendVerificationCodeDto)
        {
            var issued = await IssueVerificationCode(sendVerificationCodeDto);

            return issued.IsSuccess
                ? ServiceResult<string>.Success(issued.Data.Message)
                : ServiceResult<string>.Failure(issued.Error);
        }

        /// <summary>
        /// Send a code for an email or phone change. <paramref name="revealCode"/>
        /// is the Development-only reveal: there is no inbox locally, so without
        /// it the change flow cannot be walked end to end.
        /// </summary>
        public async Task<ServiceResult<ContactChallengeDto>> InitiateContactChange(
            SendVerificationCodeDto dto, bool revealCode)
        {
            var issued = await IssueVerificationCode(dto);

            if (!issued.IsSuccess)
                return ServiceResult<ContactChallengeDto>.Failure(issued.Error);

            return ServiceResult<ContactChallengeDto>.Success(new ContactChallengeDto
            {
                Message = issued.Data.Message,
                DevCode = revealCode ? issued.Data.Code : null
            });
        }

        private sealed record IssuedCode(string Message, string Code);

        private async Task<ServiceResult<IssuedCode>> IssueVerificationCode(SendVerificationCodeDto sendVerificationCodeDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(sendVerificationCodeDto.UserId);
                if (user == null)
                    return ServiceResult<IssuedCode>.Failure(new NotFoundException("User not found"));

                // Determine what to send to (email or phone)
                var contact = sendVerificationCodeDto.Email ?? sendVerificationCodeDto.PhoneNumber;
                var verificationType = !string.IsNullOrEmpty(sendVerificationCodeDto.Email)
                    ? VerificationType.Email
                    : VerificationType.Phone;

                if (string.IsNullOrEmpty(contact))
                    return ServiceResult<IssuedCode>.Failure(new BadRequestException("Email or phone number is required"));

                // Generate 6-digit code
                var code = new Random().Next(100000, 999999).ToString();

                // Hash the OTP code before storing (security measure to prevent database inspection bypass)
                var hashedCode = _otpHasher.HashPassword(user, code);

                // Store verification code in database
                var verificationCode = new VerificationCode
                {
                    UserId = user.Id,
                    Code = hashedCode,
                    Type = verificationType,
                    Contact = contact,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10) // 10 minutes expiry
                };

                _context.VerificationCodes.Add(verificationCode);
                await _context.SaveChangesAsync();

                // Send code via email or SMS
                if (verificationType == VerificationType.Email)
                {
                    try
                    {
                        _emailSmtpService.SendVerificationCodeEmailAsync(
                            contact,
                            user.FirstName ?? "User",
                            code,
                            10
                        );
                        _logger.LogInformation($"Verification email queued for {contact}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send verification email");
                        return ServiceResult<IssuedCode>.Failure(new ServerErrorException("Failed to send verification email"));
                    }
                }
                else
                {
                    // Send SMS verification code
                    var phoneValidation = CheckIsMtnOrAirtel(contact);

                    if (phoneValidation.isValid && !string.IsNullOrEmpty(phoneValidation.cleanedupNumber))
                    {
                        // Valid Ugandan number - send SMS via PandoraSms
                        try
                        {
                            var smsMessage = $"Your verification code is {code}. Valid for 10 minutes. Do not share this code with anyone.";
                            var smsResult = await _pandoraSmsService.SendSmsAsync(phoneValidation.cleanedupNumber, smsMessage);

                            if (smsResult?.Success == true)
                            {
                                _logger.LogInformation($"SMS verification code sent to {contact}");
                            }
                            else
                            {
                                _logger.LogError($"Failed to send SMS to {contact}: {smsResult.ErrorMessage}");
                                return ServiceResult<IssuedCode>.Failure(new ServerErrorException("Failed to send SMS verification code"));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending SMS to {contact}");
                            return ServiceResult<IssuedCode>.Failure(new ServerErrorException("Failed to send SMS verification code"));
                        }
                    }
                    else
                    {
                        // Non-Ugandan number - log only (cannot send SMS with current gateway)
                        _logger.LogInformation($"Phone number {contact} is not a Ugandan number. SMS verification not sent. Code: {code}");
                        return ServiceResult<IssuedCode>.Success(new IssuedCode(
                            "Verification code generated successfully. Note: SMS sending is only supported for Ugandan phone numbers.",
                            code));
                    }
                }

                return ServiceResult<IssuedCode>.Success(new IssuedCode("Verification code sent successfully", code));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code");
                return ServiceResult<IssuedCode>.Failure(new ServerErrorException("Failed to send verification code"));
            }
        }

        public async Task<ServiceResult<string>> VerifyCode(VerifyCodeDto verifyCodeDto)
        {
            try
            {
                // Try to find user by various identifiers
                AppUser user = null;

                // Try as userId first
                user = await _userManager.FindByIdAsync(verifyCodeDto.Identifier);

                // If not found, try as email
                if (user == null)
                    user = await _userManager.FindByEmailAsync(verifyCodeDto.Identifier);

                // If not found, try as username
                if (user == null)
                    user = await _userManager.FindByNameAsync(verifyCodeDto.Identifier);


                // If not found, try as phone number
                if (user == null)
                {
                    var contactNumber = CheckIsMtnOrAirtel(verifyCodeDto.Identifier);

                    if (!contactNumber.isValid) contactNumber.cleanedupNumber = cleanPhoneNumber(verifyCodeDto.Identifier);
                    user = await _context.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == contactNumber.cleanedupNumber);
                }

                if (user == null)
                    return ServiceResult<string>.Failure(new NotFoundException("User not found"));

                var userId = user.Id;

                // Find the most recent unused verification code for this user
                var verificationCode = await _context.VerificationCodes
                    .Where(vc => vc.UserId == userId
                        && !vc.IsUsed
                        && vc.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(vc => vc.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verificationCode == null)
                    return ServiceResult<string>.Failure(new BadRequestException("No valid verification code found. Please request a new one."));

                // Increment attempt count
                verificationCode.AttemptCount++;

                // Check if too many attempts
                if (verificationCode.AttemptCount > 5)
                {
                    verificationCode.IsUsed = true; // Invalidate the code
                    await _context.SaveChangesAsync();
                    return ServiceResult<string>.Failure(new BadRequestException("Too many attempts. Please request a new code."));
                }

                // Verify the code using password hasher (secure hash comparison)
                var verificationResult = _otpHasher.VerifyHashedPassword(user, verificationCode.Code, verifyCodeDto.Code);

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    await _context.SaveChangesAsync();
                    return ServiceResult<string>.Failure(new BadRequestException($"Invalid verification code. {5 - verificationCode.AttemptCount} attempts remaining."));
                }

                // Mark code as used
                verificationCode.IsUsed = true;
                await _context.SaveChangesAsync();

                // Update user's email or phone confirmation status
                if (verificationCode.Type == VerificationType.Email)
                {
                    user.EmailConfirmed = true;
                }
                else
                {
                    user.PhoneNumberConfirmed = true;
                }

                await _userManager.UpdateAsync(user);

                return ServiceResult<string>.Success("Verification successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying code");
                return ServiceResult<string>.Failure(new ServerErrorException("Failed to verify code"));
            }
        }

        public async Task<ServiceResult<string>> ResendVerificationCode(string identifier)
        {
            try
            {
                // Try to find user by various identifiers
                AppUser user = null;

                // Try as userId first
                user = await _userManager.FindByIdAsync(identifier);

                // If not found, try as email
                if (user == null)
                    user = await _userManager.FindByEmailAsync(identifier);

                // If not found, try as username
                if (user == null)
                    user = await _userManager.FindByNameAsync(identifier);

                // If not found, try as phone number
                if (user == null)
                {
                    var contactNumber = CheckIsMtnOrAirtel(identifier);

                    if (!contactNumber.isValid) contactNumber.cleanedupNumber = cleanPhoneNumber(identifier);
                    user = await _context.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == contactNumber.cleanedupNumber);
                }

                if (user == null)
                    return ServiceResult<string>.Failure(new NotFoundException("User not found"));

                var userId = user.Id;

                // Get the most recent verification code (used or not) to check resend throttling
                var mostRecentCode = await _context.VerificationCodes
                    .Where(vc => vc.UserId == userId && !vc.IsUsed)
                    .OrderByDescending(vc => vc.CreatedAt)
                    .FirstOrDefaultAsync();

                // Check if we need to apply exponential backoff
                if (mostRecentCode != null)
                {
                    var timeSinceLastSent = mostRecentCode.LastResentAt.HasValue
                        ? DateTime.UtcNow - mostRecentCode.LastResentAt.Value
                        : DateTime.UtcNow - mostRecentCode.CreatedAt;

                    // Calculate required wait time based on resend count
                    // Pattern: 2, 4 (2²), 16 (4²), 256 (16²), etc.
                    var requiredWaitMinutes = CalculateExponentialBackoff(mostRecentCode.ResendCount);

                    if (timeSinceLastSent.TotalMinutes < requiredWaitMinutes && false) //TODO: check disabled for testing purposes
                    {
                        var remainingMinutes = Math.Ceiling(requiredWaitMinutes - timeSinceLastSent.TotalMinutes);
                        return ServiceResult<string>.Failure(
                            new BadRequestException($"Please wait {remainingMinutes} more minute(s) before requesting another verification code. Resend limit to prevent abuse."));
                    }
                }

                // Invalidate any existing codes
                var existingCodes = await _context.VerificationCodes
                    .Where(vc => vc.UserId == userId && !vc.IsUsed)
                    .ToListAsync();

                foreach (var code in existingCodes)
                {
                    code.IsUsed = true;
                }
                await _context.SaveChangesAsync();

                // Determine what to send to
                string contact = null;
                VerificationType type;

                if (!user.EmailConfirmed && !string.IsNullOrEmpty(user.Email) && !user.Email.EndsWith($"@{_config["DefaultEmailDomain"]}", StringComparison.OrdinalIgnoreCase))
                {
                    contact = user.Email;
                    type = VerificationType.Email;
                }
                else if (!user.PhoneNumberConfirmed && !string.IsNullOrEmpty(user.PhoneNumber))
                {
                    contact = user.PhoneNumber;
                    type = VerificationType.Phone;
                }
                else
                {
                    return ServiceResult<string>.Failure(new BadRequestException("Account is already verified"));
                }

                // Send new code
                var sendDto = new SendVerificationCodeDto
                {
                    UserId = userId,
                    Email = type == VerificationType.Email ? contact : null,
                    PhoneNumber = type == VerificationType.Phone ? contact : null
                };

                var result = await SendVerificationCode(sendDto);

                // If successful, update the resend tracking on the new code
                if (result.IsSuccess)
                {
                    var newCode = await _context.VerificationCodes
                        .Where(vc => vc.UserId == userId && !vc.IsUsed)
                        .OrderByDescending(vc => vc.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (newCode != null)
                    {
                        // Increment resend count from previous code
                        newCode.ResendCount = mostRecentCode != null ? mostRecentCode.ResendCount + 1 : 0;
                        newCode.LastResentAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code");
                return ServiceResult<string>.Failure(new ServerErrorException("Failed to resend verification code"));
            }
        }

        /// <summary>
        /// Admin-initiated password reset that bypasses exponential backoff.
        /// Sends reset email or OTP directly without rate limiting.
        /// </summary>
        public async Task<ServiceResult<string>> AdminInitiatePasswordReset(string userId, string resetMethod = "email", string? originDomain = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ServiceResult<string>.Failure(new NotFoundException("User not found"));

                var allowedDomains = _config.GetSection("AllowedOrigins").Get<string[]>();
                var requestUrl = !string.IsNullOrEmpty(originDomain)
                    ? originDomain
                    : _httpContextAccessor.HttpContext?.Request.HttpContext.Request.Headers.Origin.ToString();

                // Invalidate existing unused reset codes
                var existingResetCodes = await _context.VerificationCodes
                    .Where(vc => vc.UserId == user.Id && !string.IsNullOrEmpty(vc.ResetToken) && !vc.IsUsed)
                    .ToListAsync();
                foreach (var code in existingResetCodes) { code.IsUsed = true; }
                await _context.SaveChangesAsync();

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                if (resetMethod == "otp" && !string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var otp = new Random().Next(100000, 999999).ToString();
                    var hashedOtp = _otpHasher.HashPassword(user, otp);
                    var verificationCode = new VerificationCode
                    {
                        UserId = user.Id,
                        Code = hashedOtp,
                        ResetToken = encodedToken,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                        IsUsed = false,
                        Type = VerificationType.Phone,
                        Contact = user.PhoneNumber,
                        ResendCount = 0, // Admin reset, no backoff tracking
                    };
                    _context.VerificationCodes.Add(verificationCode);
                    await _context.SaveChangesAsync();

                    var smsResponse = await _pandoraSmsService.SendSmsAsync(user.PhoneNumber!, $"Your password reset code is: {otp}. Valid for 10 minutes. Initiated by admin.");
                    if (!smsResponse.Success)
                        return ServiceResult<string>.Failure(new ServerErrorException("Failed to send SMS. Please try again."));

                    return ServiceResult<string>.Success($"Reset OTP sent to {user.PhoneNumber}");
                }
                else
                {
                    // Email reset
                    if (string.IsNullOrEmpty(user.Email))
                        return ServiceResult<string>.Failure(new BadRequestException("User has no email address configured"));

                    var verificationCode = new VerificationCode
                    {
                        UserId = user.Id,
                        Code = string.Empty,
                        ResetToken = encodedToken,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(1),
                        IsUsed = false,
                        Type = VerificationType.Email,
                        Contact = user.Email,
                        ResendCount = 0,
                    };
                    _context.VerificationCodes.Add(verificationCode);
                    await _context.SaveChangesAsync();

                    var resetLink = $"{requestUrl}/login?token={encodedToken}&email={user.Email}";
                    _emailSmtpService.SendPasswordResetCodeEmailAsync(user.Email!, user.FirstName ?? "User", resetLink);
                    return ServiceResult<string>.Success($"Reset link sent to {user.Email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while admin-initiating password reset. {ex}", ex);
                return ServiceResult<string>.Failure(new ServerErrorException("Error while initiating password reset."));
            }
        }

        /// <summary>
        /// Enable/disable a user account or soft-delete/restore.
        /// </summary>
        public async Task<ServiceResult<string>> SetUserAccountStatus(string userId, string action, string? reason = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ServiceResult<string>.Failure(new NotFoundException("User not found"));

                switch (action.ToLower())
                {
                    case "disable":
                        await _userManager.SetLockoutEnabledAsync(user, true);
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        return ServiceResult<string>.Success($"Account for {user.FirstName} {user.LastName} has been disabled");

                    case "enable":
                        await _userManager.SetLockoutEndDateAsync(user, null);
                        return ServiceResult<string>.Success($"Account for {user.FirstName} {user.LastName} has been enabled");

                    case "softdelete":
                        user.IsDeleted = true;
                        user.DateTimeModified = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                        // Also lock the account
                        await _userManager.SetLockoutEnabledAsync(user, true);
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        return ServiceResult<string>.Success($"Account for {user.FirstName} {user.LastName} has been soft-deleted");

                    case "restore":
                        user.IsDeleted = false;
                        user.DateTimeModified = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);
                        return ServiceResult<string>.Success($"Account for {user.FirstName} {user.LastName} has been restored");

                    default:
                        return ServiceResult<string>.Failure(new BadRequestException($"Unknown action: {action}"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while setting user account status. {ex}", ex);
                return ServiceResult<string>.Failure(new ServerErrorException("Error while updating account status."));
            }
        }

        /// <summary>
        /// Calculate exponential backoff wait time in minutes
        /// Pattern: 2, 4 (2²), 16 (4²), 256 (16²), etc.
        /// </summary>
        private double CalculateExponentialBackoff(int resendCount)
        {
            if (resendCount == 0) return 2; // First resend: 2 minutes

            double waitTime = 2;
            for (int i = 0; i < resendCount; i++)
            {
                waitTime = Math.Pow(waitTime, 2);
            }

            // Cap at 24 hours (1440 minutes) to prevent excessive wait times
            return Math.Min(waitTime, 1440);
        }

        /// <summary>
        /// Validates if a phone number is a valid Ugandan number (MTN or Airtel)
        /// and returns cleaned/formatted number
        /// </summary>
        private PhoneValidationResult CheckIsMtnOrAirtel(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new PhoneValidationResult { isValid = false };
            }

            // Remove all non-numeric characters
            var cleanedNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Remove country code if present (256 for Uganda)
            if (cleanedNumber.StartsWith("256"))
            {
                cleanedNumber = cleanedNumber.Substring(3);
            }
            else if (cleanedNumber.StartsWith("+256"))
            {
                cleanedNumber = cleanedNumber.Substring(4);
            }

            // Remove leading zero if present
            if (cleanedNumber.StartsWith("0"))
            {
                cleanedNumber = cleanedNumber.Substring(1);
            }

            // Check if it's a valid Ugandan mobile number (9 digits after removing country code and leading zero)
            if (cleanedNumber.Length != 9)
            {
                return new PhoneValidationResult { isValid = false, cleanedupNumber = phoneNumber };
            }

            var prefix = cleanedNumber.Substring(0, 2);
            string network = null;

            // MTN Uganda prefixes: 77, 78, 76, 79
            if (prefix == "77" || prefix == "78" || prefix == "76" || prefix == "79")
            {
                network = "mtn";
            }
            // Airtel Uganda prefixes: 70, 75, 74
            else if (prefix == "70" || prefix == "75" || prefix == "74")
            {
                network = "airtel";
            }

            if (network != null)
            {
                // Format as international number with country code
                var formattedNumber = $"256{cleanedNumber}";
                return new PhoneValidationResult
                {
                    isValid = true,
                    MtnOrAirtel = network,
                    cleanedupNumber = formattedNumber
                };
            }

            // Not a recognized Ugandan network
            return new PhoneValidationResult { isValid = false, cleanedupNumber = phoneNumber };
        }

        /// <summary>
        /// Update user profile - allows users to update their own profile information.
        /// Does NOT handle email/phone changes - those require OTP verification via VerifyContactChange.
        /// </summary>
        public async Task<ServiceResult<CreateUserResponseDto>> UpdateUserProfile(UpdateUserProfileDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.Id);
                if (user == null)
                    return ServiceResult<CreateUserResponseDto>.Failure(new NotFoundException("User not found"));

                // Update basic profile fields (no verification needed)
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Address = dto.Address;
                user.Aboutme = dto.Aboutme;
                user.Industry = dto.Industry;

                // Do NOT update email or phone here - those changes must go through OTP verification
                // via the VerifyContactChange endpoint

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var output = user.Adapt<CreateUserResponseDto>();
                    return ServiceResult<CreateUserResponseDto>.Success(output);
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException(errors));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating user profile. {ex}", ex);
                return ServiceResult<CreateUserResponseDto>.Failure(new ServerErrorException("Error updating profile"));
            }
        }

        /// <summary>
        /// Change a user's username. No OTP: unlike an email or a phone, a
        /// username proves nothing and reaches nobody — it is only a label.
        /// </summary>
        public async Task<ServiceResult<CreateUserResponseDto>> UpdateUserName(UpdateUserNameDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                    return ServiceResult<CreateUserResponseDto>.Failure(new NotFoundException("User not found"));

                var requested = dto.UserName?.Trim() ?? string.Empty;

                if (requested.Length < 3)
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException("A username needs at least three characters."));

                if (string.Equals(user.UserName, requested, StringComparison.OrdinalIgnoreCase))
                    return ServiceResult<CreateUserResponseDto>.Success(user.Adapt<CreateUserResponseDto>());

                var taken = await _userManager.FindByNameAsync(requested);
                if (taken != null && taken.Id != user.Id)
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException("That username is already taken."));

                IdentityResult result;
                try
                {
                    result = await _userManager.SetUserNameAsync(user, requested);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    // The lookup above runs through the tenant query filter and
                    // the unique index does not — a name held in another
                    // contractor's org is invisible here but still collides.
                    _logger.LogInformation("Username {Name} is taken outside this tenant", requested);
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException("That username is already taken."));
                }

                if (!result.Succeeded)
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description))));

                return ServiceResult<CreateUserResponseDto>.Success(user.Adapt<CreateUserResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing username for {UserId}", dto.UserId);
                return ServiceResult<CreateUserResponseDto>.Failure(new ServerErrorException("Error changing username"));
            }
        }

        /// <summary>
        /// Is this username free, and if not, what is? Answered against the
        /// whole platform rather than the caller's tenant — usernames are unique
        /// across ASSETLEN, so a per-tenant answer would tell the reader a name
        /// is available and then fail on save.
        /// </summary>
        public async Task<ServiceResult<UserNameAvailabilityDto>> CheckUserName(string userName, string? currentUserId)
        {
            try
            {
                var requested = (userName ?? string.Empty).Trim();
                var result = new UserNameAvailabilityDto { UserName = requested };

                if (requested.Length < 3)
                {
                    result.Reason = "At least three characters.";
                    return ServiceResult<UserNameAvailabilityDto>.Success(result);
                }

                if (requested.Length > 64)
                {
                    result.Reason = "That is longer than 64 characters.";
                    return ServiceResult<UserNameAvailabilityDto>.Success(result);
                }

                if (!UserNameShape.IsMatch(requested))
                {
                    result.Reason = "Letters, numbers, and . _ - only.";
                    return ServiceResult<UserNameAvailabilityDto>.Success(result);
                }

                var taken = await TakenAsync(new[] { requested }, currentUserId);

                if (!taken.Contains(Normalize(requested)))
                {
                    result.Available = true;
                    return ServiceResult<UserNameAvailabilityDto>.Success(result);
                }

                result.Reason = "Someone already has that one.";
                result.Suggestions = await SuggestAsync(requested, currentUserId);

                return ServiceResult<UserNameAvailabilityDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username {UserName}", userName);
                return ServiceResult<UserNameAvailabilityDto>.Failure(new ServerErrorException("Could not check that username"));
            }
        }

        private static readonly Regex UserNameShape = new(@"^[A-Za-z0-9._-]+$", RegexOptions.Compiled);

        private static string Normalize(string s) => s.ToUpperInvariant();

        /// <summary>
        /// Which of these are already claimed. Filters are bypassed because the
        /// unique index on AspNetUsers is global while the tenant filter is not
        /// — checking through it reports a name free that the insert rejects.
        /// </summary>
        private async Task<HashSet<string>> TakenAsync(IEnumerable<string> candidates, string? exceptUserId)
        {
            var normalized = candidates.Select(Normalize).Distinct().ToList();

            var hits = await _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.NormalizedUserName != null && normalized.Contains(u.NormalizedUserName))
                .Where(u => exceptUserId == null || u.Id != exceptUserId)
                .Select(u => u.NormalizedUserName!)
                .ToListAsync();

            return new HashSet<string>(hits, StringComparer.Ordinal);
        }

        /// <summary>Free variations on what the reader typed, so a refusal comes with a way forward.</summary>
        private async Task<List<string>> SuggestAsync(string requested, string? currentUserId)
        {
            var stem = requested.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            if (stem.Length < 3) stem = requested;

            var random = new Random();
            var candidates = new List<string>
            {
                $"{stem}.{random.Next(10, 99)}",
                $"{stem}{random.Next(100, 999)}",
                $"{stem}_{random.Next(1, 9)}",
                $"the{stem}",
                $"{stem}.co"
            };

            var taken = await TakenAsync(candidates, currentUserId);

            return candidates
                .Where(c => c.Length <= 64 && !taken.Contains(Normalize(c)))
                .Take(3)
                .ToList();
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
               || ex.InnerException?.Message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Verify OTP and update user's email or phone number.
        /// </summary>
        public async Task<ServiceResult<CreateUserResponseDto>> VerifyContactChange(VerifyContactChangeDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                    return ServiceResult<CreateUserResponseDto>.Failure(new NotFoundException("User not found"));

                // Find the most recent unused verification code
                var verificationCode = await _context.VerificationCodes
                    .Where(vc => vc.UserId == dto.UserId && !vc.IsUsed && vc.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(vc => vc.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verificationCode == null)
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException("No valid verification code found. Please request a new one."));

                // Increment attempt count
                verificationCode.AttemptCount++;

                if (verificationCode.AttemptCount > 5)
                {
                    verificationCode.IsUsed = true;
                    await _context.SaveChangesAsync();
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException("Too many attempts. Please request a new code."));
                }

                // Verify the code
                var verificationResult = _otpHasher.VerifyHashedPassword(user, verificationCode.Code, dto.VerificationCode);

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    await _context.SaveChangesAsync();
                    return ServiceResult<CreateUserResponseDto>.Failure(
                        new BadRequestException($"Invalid verification code. {5 - verificationCode.AttemptCount} attempts remaining."));
                }

                // Mark code as used
                verificationCode.IsUsed = true;
                await _context.SaveChangesAsync();

                // Update the contact based on type
                if (!string.IsNullOrEmpty(dto.NewEmail))
                {
                    // Check if email is already taken
                    var emailExists = await _userManager.FindByEmailAsync(dto.NewEmail);
                    if (emailExists != null && emailExists.Id != user.Id)
                        return ServiceResult<CreateUserResponseDto>.Failure(
                            new BadRequestException("Email is already in use by another account"));

                    user.Email = dto.NewEmail;
                    user.EmailConfirmed = true; // Mark as confirmed since they verified via OTP
                }

                if (!string.IsNullOrEmpty(dto.NewPhoneNumber))
                {
                    // Validate and format phone number
                    var phoneCheck = CheckIsMtnOrAirtel(dto.NewPhoneNumber);
                    if (!phoneCheck.isValid)
                        return ServiceResult<CreateUserResponseDto>.Failure(
                            new BadRequestException("Invalid phone number format"));

                    // Check if phone is already taken
                    var phoneExists = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneCheck.cleanedupNumber);
                    if (phoneExists != null && phoneExists.Id != user.Id)
                        return ServiceResult<CreateUserResponseDto>.Failure(
                            new BadRequestException("Phone number is already in use by another account"));

                    user.PhoneNumber = phoneCheck.cleanedupNumber;
                    user.PhoneNumberConfirmed = true; // Mark as confirmed since they verified via OTP
                }

                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    var output = user.Adapt<CreateUserResponseDto>();
                    return ServiceResult<CreateUserResponseDto>.Success(output);
                }

                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return ServiceResult<CreateUserResponseDto>.Failure(new BadRequestException(errors));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while verifying contact change. {ex}", ex);
                return ServiceResult<CreateUserResponseDto>.Failure(
                    new ServerErrorException("Error verifying contact change"));
            }
        }

        public class PhoneValidationResult
        {
            public bool isValid { get; set; }
            public string MtnOrAirtel { get; set; }
            public string cleanedupNumber { get; set; }
        }

        public class TenantInstance
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime Created { get; set; }
            public bool IsActive { get; set; }
        }
        public class ResultObject
        {
            public string ReturnCode { get; set; }
            public string ReturnDescription { get; set; }
            public string Response { get; set; }
            public string Message { get; set; }
            public string Link { get; set; }
            public string videoThumbnail { get; set; }
        }
    }
}
