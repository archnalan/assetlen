using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class DiscountDto : BaseDto
    {
        // public int Id { get; set; }
        public string? DiscountName { get; set; }
        public decimal? DiscountValue { get; set; }

        public bool? isValuePercentage { get; set; }
        public bool? Active { get; set; }
    }
    public class DiscountCreateDto : DiscountDto
    {
        public new int DiscountId { get; }
    }
}
