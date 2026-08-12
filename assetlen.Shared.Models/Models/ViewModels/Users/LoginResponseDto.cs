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

        /// <summary>The account this token is scoped to — the active tenant, not the default.</summary>
        public string TenantId { get; set; }

        public DateTime exp { get; set; }

        /// <summary>
        /// Every account this person may act in. More than one means the picker
        /// is offered; the token above is only ever scoped to one of them.
        /// </summary>
        public List<TenantMembershipDto> Accounts { get; set; } = new();
    }
}
