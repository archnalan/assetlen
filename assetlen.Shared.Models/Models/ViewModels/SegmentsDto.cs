using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class SegmentsDto : BaseDto
    {
        //public int? Id { get; set; }

        public string? Segment { get; set; }

        public string? Description { get; set; }

        public bool HideInPos { get; set; }
    }
}
