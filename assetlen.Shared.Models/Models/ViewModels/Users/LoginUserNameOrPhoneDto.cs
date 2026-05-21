using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
    public class LoginUserNameOrPhoneDto
    {
        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        [MinLength(4)]
        public string Password { get; set; }
    }
}
