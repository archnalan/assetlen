using mowt.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.ViewModels
{
    public class ConfigurationDto : BaseDto
    {
        public string? StringValue { get; set; }

        public int? ConfigId { get; set; }
        public bool Selected { get; set; } = false;
        DateTime? DateTimeCreated { get; set; }
        DateTime? DateTimeModified { get; set; }
    }
}
