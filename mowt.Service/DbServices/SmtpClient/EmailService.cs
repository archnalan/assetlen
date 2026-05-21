using mowt.Shared.Models.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace mowt.Web.Controllers.SmtpClient
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void SendEmail(EmailDto emailDto)
        {
            var email = new MimeMessage();
            // Use FromEmail (sender email) not emailSenderAccount (SMTP username)
            var senderEmail = !string.IsNullOrEmpty(emailDto.FromEmail) ? emailDto.FromEmail : emailDto.emailSenderAccount;
            email.From.Add(new MailboxAddress(emailDto.CompanyName, senderEmail));
            email.To.Add(MailboxAddress.Parse(emailDto.ToEmail));
            email.ReplyTo.Add(MailboxAddress.Parse(emailDto.ReplyToEmail));


            email.Subject = emailDto.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = emailDto.Body
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            var smtpHost = emailDto.SmtpHost ?? "smtp-relay.brevo.com";
            var smtpPort = emailDto.SmtpPort > 0 ? emailDto.SmtpPort : 587;
            smtp.Connect(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(emailDto.emailSenderAccount, emailDto.emailSenderSecret);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
