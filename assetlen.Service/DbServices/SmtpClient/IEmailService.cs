using assetlen.Shared.Models.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace assetlen.Service.DbServices.SmtpClient
{
    public interface IEmailService
    {
        void SendEmail(EmailDto emailDto);
    }
}
