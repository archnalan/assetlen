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

        // ── The four auth emails ────────────────────────────────────────────
        //
        // Each one supplies only its own words; the sheet around them comes from
        // EmailTheme.Shell so they cannot drift apart again. companyName and
        // websiteLink are still accepted so the call sites are unchanged, but
        // they are deliberately unused: the identity on an authentication email
        // is the product's, not a configurable string that was still reading
        // "Ministry of Works & Transport" when this was written.

        private string GenerateVerificationEmailHtml(string firstName, string code, int expiryMinutes, string companyName, string websiteLink)
        {
            var name = EmailTheme.Escape(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName);

            var body =
                EmailTheme.Paragraph($"Hello {name}, use this code to confirm your email address.")
              + EmailTheme.CodePanel(code, expiryMinutes)
              + EmailTheme.Paragraph("Type it into the screen that asked for it. It works once.")
              + EmailTheme.Notice(
                    "<strong>Nobody at ASSETLEN will ever ask you for this code.</strong> "
                  + "If you did not try to confirm this address, you can ignore this message; "
                  + "nothing has changed on the account.");

            // The preheader is set, so the inbox preview does not put a live code
            // on the lock screen of whoever is holding the phone.
            return EmailTheme.Shell(
                preheader: "Confirm your email address on ASSETLEN.",
                eyebrow: "Verification",
                title: "Confirm your email address",
                bodyHtml: body);
        }

        private string GeneratePasswordResetEmailHtml(string firstName, string code, int expiryMinutes, string companyName, string websiteLink)
        {
            var name = EmailTheme.Escape(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName);
            var isLink = code.StartsWith("http://") || code.StartsWith("https://");

            var body =
                EmailTheme.Paragraph($"Hello {name}, someone asked to reset the password on your ASSETLEN account.")
              + (isLink
                    ? EmailTheme.Button(code, "Choose a new password") + EmailTheme.FallbackLink(code)
                    : EmailTheme.CodePanel(code, expiryMinutes))
              + EmailTheme.Notice(
                    "<strong>If that was not you, ignore this message.</strong> "
                  + "Your password has not changed, and it cannot change until this "
                  + (isLink ? "link" : "code") + " is used.");

            return EmailTheme.Shell(
                preheader: "Reset the password on your ASSETLEN account.",
                eyebrow: "Security",
                title: "Reset your password",
                bodyHtml: body);
        }

        private string GeneratePasswordChangedEmailHtml(string firstName, string companyName, string websiteLink)
        {
            var name = EmailTheme.Escape(string.IsNullOrWhiteSpace(firstName) ? "there" : firstName);

            // No code and no link. This one only confirms a fact, and a button on
            // it would be the thing an attacker imitates.
            var body =
                EmailTheme.Paragraph($"Hello {name}, the password on your ASSETLEN account has been changed.")
              + EmailTheme.Paragraph("You can sign in with the new one now. Any other device stays signed in until its session ends.")
              + EmailTheme.Notice(
                    "<strong>If you did not do this, act now.</strong> "
                  + "Reset your password from the sign-in screen, and tell whoever administers your account.");

            return EmailTheme.Shell(
                preheader: "Your ASSETLEN password was changed.",
                eyebrow: "Security",
                title: "Your password was changed",
                bodyHtml: body);
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
            emailDto.Subject = "Reset your ASSETLEN password";
            emailDto.ToEmail = appuser.Email;

            // Same sheet as the code-based reset — the two are the same event
            // reaching the reader by two routes, and looking different would make
            // one of them look forged.
            emailDto.Body = GeneratePasswordResetEmailHtml(
                appuser.FirstName, callbackUrl, 0, emailDto.CompanyName, emailDto.websiteLink);

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
