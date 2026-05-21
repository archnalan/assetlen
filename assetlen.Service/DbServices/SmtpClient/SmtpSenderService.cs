using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Service.DbServices.SmtpClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace assetlen.Service.DbServices.SmtpClient
{

    public class SmtpSenderService
    {
        private readonly IEmailService _emailService;
        private IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;

        public SmtpSenderService(IEmailService emailService, IConfiguration configuration, UserManager<AppUser> userManager)
        {
            _emailService = emailService;
            _configuration = configuration;
            _userManager = userManager;
        }

        public EmailDto CreateEmailDto()
        {
            var emailDto = new EmailDto
            {
                SmtpHost = _configuration["SmtpSettings:Host"],
                SmtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587"),
                FromEmail = _configuration["SmtpSettings:SenderEmail"],
                SenderName = _configuration["SmtpSettings:SenderName"],
                emailSenderAccount = _configuration["SmtpSettings:Username"],
                emailSenderSecret = _configuration["SmtpSettings:Password"],
                ReplyToEmail = _configuration["SmtpSettings:ReplyToEmail"],
                CompanyName = _configuration["SmtpSettings:CompanyName"],
                websiteLink = _configuration["SmtpSettings:WebsiteLink"]
            };
            return emailDto;
        }

        //public IActionResult SubmitForm(string content)
        //{
        //    var allowedDomains = _configuration.GetSection("AllowedOrigins").Get<string[]>();

        //    var requestUrl = HttpContext.Request.HttpContext.Request.Headers.Origin;
        //    if (allowedDomains.Where(x => x.Contains(requestUrl.ToString().Trim(), StringComparison.OrdinalIgnoreCase)).Count() == 0)
        //    {
        //        //not authorized domain
        //        return Unauthorized("Not authorised");
        //    }

        //    //base 64 decode info

        //    var clientJson = Base64Decode(content);
        //    if (string.IsNullOrEmpty(clientJson)) return BadRequest("Invalid input data");


        //    //send email

        //    var emailDto = JsonConvert.DeserializeObject<EmailDto>(clientJson);

        //    var emailSenderCredentials = _configuration.GetSection($"SmtpCredentials:{requestUrl}");
        //    emailDto.emailSenderAccount = emailSenderCredentials?["emailSenderAccount"].ToString();
        //    emailDto.emailSenderSecret = emailSenderCredentials?["emailSenderSecret"].ToString();
        //    emailDto.CompanyName = emailSenderCredentials?["CompanyName"].ToString();
        //    //emailDto.ReplyToEmail = emailSenderCredentials?["ReplyToEmail"].ToString();
        //    emailDto.websiteLink = requestUrl.ToString();
        //    emailDto.ToEmail = emailSenderCredentials?["ToEmail"].ToString();

        //    //this should be updated last 
        //    emailDto.Body = EmailTemplates.EmailTemplates.WebsiteForm(emailDto);

        //    SendMail(emailDto);
        //    return Ok();
        //}

        public void SendMail(EmailDto emailDto)
        {
            try
            {
                emailDto.emailSenderSecret = emailDto.emailSenderSecret ?? _configuration["SmtpSettings:Password"];
                emailDto.emailSenderAccount = emailDto.emailSenderAccount ?? _configuration["SmtpSettings:Username"];
                emailDto.SmtpHost = emailDto.SmtpHost ?? _configuration["SmtpSettings:Host"];
                emailDto.SmtpPort = emailDto.SmtpPort != 0 ? emailDto.SmtpPort : int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                _emailService.SendEmail(emailDto);
            }
            catch (Exception ex)
            {
                //log error
            }
        }

        public void SendVerificationCodeEmailAsync(string toEmail, string firstName, string code, int expiryMinutes = 10)
        {
            // Fire and forget - don't await
            Task.Run(async () =>
            {
                try
                {
                    var emailDto = CreateEmailDto();
                    emailDto.ToEmail = toEmail;
                    emailDto.Subject = "Verify Your Email Address";
                    emailDto.Body = GenerateVerificationEmailHtml(firstName, code, expiryMinutes, emailDto.CompanyName, emailDto.websiteLink);

                    SendMail(emailDto);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw
                    Console.WriteLine($"Error sending verification email: {ex.Message}");
                }
            });
        }

        public void SendPasswordResetCodeEmailAsync(string toEmail, string firstName, string code, int expiryMinutes = 10)
        {
            // Fire and forget - don't await
            Task.Run(async () =>
            {
                try
                {
                    var emailDto = CreateEmailDto();
                    emailDto.ToEmail = toEmail;
                    emailDto.Subject = "Reset Your Password";
                    emailDto.Body = GeneratePasswordResetEmailHtml(firstName, code, expiryMinutes, emailDto.CompanyName, emailDto.websiteLink);

                    SendMail(emailDto);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw
                    Console.WriteLine($"Error sending password reset email: {ex.Message}");
                }
            });
        }

        private string GenerateVerificationEmailHtml(string firstName, string code, int expiryMinutes, string companyName, string websiteLink)
        {
            var company = companyName ?? "ASSETLEN";
            var name = firstName ?? "there";
            var website = websiteLink ?? "https://assetlen.com";

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f7; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 28px; font-weight: 600; }}
        .content {{ padding: 40px 30px; }}
        .greeting {{ font-size: 18px; color: #333333; margin-bottom: 20px; }}
        .message {{ font-size: 16px; color: #555555; line-height: 1.6; margin-bottom: 30px; }}
        .code-container {{ background-color: #f8f9fa; border-radius: 8px; padding: 30px; text-align: center; margin: 30px 0; border: 2px dashed #667eea; }}
        .code {{ font-size: 36px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: 'Courier New', monospace; }}
        .expiry {{ font-size: 14px; color: #888888; margin-top: 15px; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .warning-text {{ font-size: 14px; color: #856404; margin: 0; }}
        .footer {{ background-color: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef; }}
        .footer-text {{ font-size: 14px; color: #6c757d; margin: 5px 0; }}
        .link {{ color: #667eea; text-decoration: none; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{company}</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {name},</div>
            <div class='message'>
                Thank you for creating an account! To complete your registration and verify your email address, 
                please use the verification code below:
            </div>
            <div class='code-container'>
                <div class='code'>{code}</div>
                <div class='expiry'>This code expires in {expiryMinutes} minutes</div>
            </div>
            <div class='warning'>
                <p class='warning-text'>
                    <strong>⚠️ Security Notice:</strong> If you didn't request this verification code, 
                    please ignore this email. Your account is safe and no changes have been made.
                </p>
            </div>
            <div class='message'>
                Once verified, you'll have full access to all features and resources.
            </div>
        </div>
        <div class='footer'>
            <p class='footer-text'>This is an automated message, please do not reply.</p>
            <p class='footer-text'>
                <a href='{website}' class='link'>Visit our website</a>
            </p>
            <p class='footer-text'>© 2026 {company}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordResetEmailHtml(string firstName, string code, int expiryMinutes, string companyName, string websiteLink)
        {
            var company = companyName ?? "ASSETLEN";
            var name = firstName ?? "there";
            var website = websiteLink ?? "https://assetlen.com";

            // Detect if code is a link
            bool isLink = code.StartsWith("http://") || code.StartsWith("https://");

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f7; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 40px 20px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 28px; font-weight: 600; }}
        .content {{ padding: 40px 30px; }}
        .greeting {{ font-size: 18px; color: #333333; margin-bottom: 20px; }}
        .message {{ font-size: 16px; color: #555555; line-height: 1.6; margin-bottom: 30px; }}
        .code-container {{ background-color: #fff5f5; border-radius: 8px; padding: 30px; text-align: center; margin: 30px 0; border: 2px dashed #f5576c; }}
        .code {{ font-size: 36px; font-weight: bold; color: #f5576c; letter-spacing: 8px; font-family: 'Courier New', monospace; }}
        .btn {{ display: inline-block; padding: 14px 30px; background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: #ffffff !important; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; margin: 20px 0; }}
        .link-text {{ font-size: 14px; color: #6c757d; word-break: break-all; margin-top: 20px; }}
        .expiry {{ font-size: 14px; color: #888888; margin-top: 15px; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .warning-text {{ font-size: 14px; color: #856404; margin: 0; }}
        .footer {{ background-color: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef; }}
        .footer-text {{ font-size: 14px; color: #6c757d; margin: 5px 0; }}
        .link {{ color: #f5576c; text-decoration: none; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{company}</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {name},</div>
            <div class='message'>
                We received a request to reset your password. {(isLink ? "Click the button below to reset your password:" : "Use the code below to reset your password:")}
            </div>
            {(isLink ? $@"
            <div style='text-align: center;'>
                <a href='{code}' class='btn'>Reset Password</a>
            </div>
            <p class='link-text'>
                If the button doesn't work, copy and paste this link into your browser:<br>
                <a href='{code}' style='color: #f5576c; font-size: 14px;'>{code}</a>
            </p>
            " : $@"
            <div class='code-container'>
                <div class='code'>{code}</div>
                <div class='expiry'>This code expires in {expiryMinutes} minutes</div>
            </div>
            ")}
            <div class='warning'>
                <p class='warning-text'>
                    <strong>⚠️ Security Alert:</strong> If you didn't request a password reset, 
                    please ignore this email and ensure your account is secure. Consider changing your password 
                    if you suspect unauthorized access.
                </p>
            </div>
        </div>
        <div class='footer'>
            <p class='footer-text'>This is an automated message, please do not reply.</p>
            <p class='footer-text'>
                <a href='{website}' class='link'>Visit our website</a>
            </p>
            <p class='footer-text'>© 2026 {company}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordChangedEmailHtml(string firstName, string companyName, string websiteLink)
        {
            var company = companyName ?? "ASSETLEN";
            var name = firstName ?? "there";
            var website = websiteLink ?? "https://assetlen.com";

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f7; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #00b09b 0%, #96c93d 100%); padding: 40px 20px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 28px; font-weight: 600; }}
        .content {{ padding: 40px 30px; }}
        .greeting {{ font-size: 18px; color: #333333; margin-bottom: 20px; }}
        .message {{ font-size: 16px; color: #555555; line-height: 1.6; margin-bottom: 30px; }}
        .success-icon {{ text-align: center; margin: 30px 0; }}
        .success-icon svg {{ width: 80px; height: 80px; }}
        .info-box {{ background-color: #e8f5e9; border-left: 4px solid #4caf50; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .info-text {{ font-size: 14px; color: #2e7d32; margin: 0; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .warning-text {{ font-size: 14px; color: #856404; margin: 0; }}
        .footer {{ background-color: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef; }}
        .footer-text {{ font-size: 14px; color: #6c757d; margin: 5px 0; }}
        .link {{ color: #00b09b; text-decoration: none; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{company}</h1>
        </div>
        <div class='content'>
            <div class='success-icon'>
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <circle cx='12' cy='12' r='10' fill='#4caf50'/>
                    <path d='M8 12l2 2 4-4' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                </svg>
            </div>
            <div class='greeting'>Hello {name},</div>
            <div class='message'>
                Your password has been successfully changed. Your account is now secured with your new password.
            </div>
            <div class='info-box'>
                <p class='info-text'>
                    <strong>✓ Password Changed:</strong> {DateTime.UtcNow.ToString("MMM dd, yyyy")} at {DateTime.UtcNow.ToString("HH:mm")} UTC
                </p>
            </div>
            <div class='warning'>
                <p class='warning-text'>
                    <strong>⚠️ Security Alert:</strong> If you didn't make this change, please contact our support team immediately. 
                    Your account may be compromised.
                </p>
            </div>
            <div class='message'>
                For your security, we recommend:
                <ul style='margin: 10px 0; padding-left: 20px; color: #555555;'>
                    <li>Using a strong, unique password</li>
                    <li>Not sharing your password with anyone</li>
                    <li>Regularly updating your password</li>
                    <li>Logging out from devices you don't use</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p class='footer-text'>This is an automated message, please do not reply.</p>
            <p class='footer-text'>
                <a href='{website}' class='link'>Visit our website</a>
            </p>
            <p class='footer-text'>© 2026 {company}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public void SendPasswordChangedNotificationEmail(string toEmail, string firstName)
        {
            // Fire and forget - don't await
            Task.Run(async () =>
            {
                try
                {
                    var emailDto = CreateEmailDto();
                    emailDto.ToEmail = toEmail;
                    emailDto.Subject = "Password Changed Successfully";
                    emailDto.Body = GeneratePasswordChangedEmailHtml(firstName, emailDto.CompanyName, emailDto.websiteLink);

                    SendMail(emailDto);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw
                    Console.WriteLine($"Error sending password changed notification: {ex.Message}");
                }
            });
        }

        public void SendDeveloperNotificationEmail(string message)
        {
            try
            {
                var emailDto = CreateEmailDto();
                emailDto.ToEmail = "seanjems@gmail.com";
                emailDto.Body = message;
                emailDto.Subject = "Error your app. Please check it out";
                _emailService.SendEmail(emailDto);
            }
            catch (Exception ex)
            {
                //log error
            }
        }

        public async Task sendPasswordResetEmail(AppUser appuser, string requestorUri)
        {
            //send confirmation email for the new email
            var code = await _userManager.GeneratePasswordResetTokenAsync(appuser);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            //CHANGE THIS TO PRODUCTION LINK
            var callbackUrl = $"{requestorUri ?? "https://app.assetlen.com"}/resetpassword/{code}/{"isLocal"}";

            var emailDto = CreateEmailDto();
            emailDto.Subject = "Reset Your Password";
            emailDto.ToEmail = appuser.Email;
            var company = emailDto.CompanyName ?? "ASSETLEN";

            emailDto.Body = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background-color: #f4f4f7; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 28px; }}
        .content {{ padding: 40px 30px; }}
        .btn {{ display: inline-block; padding: 14px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 30px; text-align: center; }}
        .footer-text {{ font-size: 14px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{company}</h1>
        </div>
        <div class='content'>
            <h2>Password Reset Request</h2>
            <p>You have requested to reset your password for your ASSETLEN account.</p>
            <p>Click the button below to reset your password:</p>
            <div style='text-align: center;'>
                <a href='{callbackUrl}' class='btn'>Reset Password</a>
            </div>
            <p style='margin-top: 30px; font-size: 14px; color: #6c757d;'>
                If the button doesn't work, copy and paste this link into your browser:<br>
                <a href='{callbackUrl}' style='color: #667eea;'>{callbackUrl}</a>
            </p>
            <p style='margin-top: 30px; padding: 15px; background: #fff3cd; border-left: 4px solid #ffc107; font-size: 14px;'>
                <strong>Security Notice:</strong> If you didn't request this, please ignore this email.
            </p>
        </div>
        <div class='footer'>
            <p class='footer-text'>Thank you for being part of ASSETLEN.</p>
            <p class='footer-text'>© 2026 {company}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            // Send asynchronously in background
            _ = Task.Run(() => SendMail(emailDto));
        }
        //[HttpGet]
        //public async Task<IActionResult> sendPasswordResetEmail(string email)
        //{
        //    AppUser appuser = await _userManager.FindByEmailAsync(email);
        //    if (appuser == null)
        //    {
        //        return Ok("Email does not exist");
        //     }
        //    //send confrmation email for the new email
        //    var code = await _userManager.GeneratePasswordResetTokenAsync(appuser);
        //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));


        //    //CHANGE THIS TO PRODUCTION LINK
        //    var callbackUrl = $"{ "https://social.kampalacentraladventist.org"}/resetpassword/{code}";

        //    var emailDto = new EmailDto(true);
        //    emailDto.Subject = "Reset Your Password";
        //    emailDto.ToEmail = appuser.Email;
        //    emailDto.Body = $"You have requested to reset your password with SDA Kampala Central church.  <br/><br/>If you want to rest your password, please click this link.<br/><br/> {callbackUrl}<br/><br/> Thank you for being part of SDA Kampala Central church. ";

        //    SendMail(emailDto);
        //    return Ok("Email confirmation sent");
        //}



        private static string Base64Decode(string base64EncodedData)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
