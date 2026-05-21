using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
    public class LoginResponseDto
    {
        public string token { get; set; }
        public string RefreshToken { get; set; }
        public string TenantId { get; set; }
        public DateTime exp { get; set; }
    }
}
