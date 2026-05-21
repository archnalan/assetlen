using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class PaymentModeDto : BaseDto
    {
        public string? Description { get; set; }

        public bool IsModeEnabled { get; set; }
    }
}
