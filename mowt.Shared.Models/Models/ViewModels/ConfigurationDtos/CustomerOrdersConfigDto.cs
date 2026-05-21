using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ConfigurationDtos
{
    public class CustomerOrdersConfigDto
    {
        public bool AllowOrderCompletionWhenInOrders { get; set; } = false;
        public bool AllowAnyUserResumeTransaction { get; set; } = false;

    }
}
