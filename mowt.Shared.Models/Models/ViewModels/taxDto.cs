using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class taxDto : BaseDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "Tax must be a positive value.")]
        public decimal? TaxValue { get; set; }

        public string? TaxDescription { get; set; }
    }
}
