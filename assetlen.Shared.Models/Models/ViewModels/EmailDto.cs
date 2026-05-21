using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class EmailDto : BaseDto
    {
        public string? FromEmail { get; set; }
        public string? ReplyToEmail { get; set; }
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? SenderName { get; set; }
        public string? SenderPhonenumber { get; set; }
        public string? Category { get; set; }
        public string? websiteLink { get; set; }
        public string? CompanyName { get; set; }
        public string? emailSenderAccount { get; set; }
        public string? emailSenderSecret { get; set; }
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }

        public EmailDto()
        {

        }
    }
}
