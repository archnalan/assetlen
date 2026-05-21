using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class SizeDto : BaseDto
    {
        public int Width { get; set; }

        public int? Height { get; set; }
    }
}
