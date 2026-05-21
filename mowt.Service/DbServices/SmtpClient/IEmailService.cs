using mowt.Shared.Models.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace mowt.Web.Controllers.SmtpClient
{
    public interface IEmailService
    {
        void SendEmail(EmailDto emailDto);
    }
}
