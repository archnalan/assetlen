using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class CategoryDto : BaseDto
    {
        //public int? Id { get; set; }

        public string? Category { get; set; }

        public string? Description { get; set; }

        public bool HideInPos { get; set; }
    }

}
