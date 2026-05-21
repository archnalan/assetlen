using mowt.Shared.Models.Models.ViewModels.ReportingDto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
    public class ShiftsDto : BaseDto
    {
        [Required(ErrorMessage = "User for the shift is required")]
        public string? UserId { get; set; }


        public DateTime? DateTimeOpened { get; set; }

        [Required(ErrorMessage = "Please enter the Opening balance")]
        public decimal? OpeningBalance { get; set; } = 0;

        public decimal? CurrentBalance { get; set; }

        public decimal? ShiftEndAmount { get; set; }

        public DateTime? DateTimeClosed { get; set; }

        public string? ActiveId { get; set; }

        public string? SubActiveId { get; set; }

        public decimal? ShiftEndCash { get; set; }

        public decimal? ShiftEndCard { get; set; }

        public decimal? ShiftEndCheque { get; set; }

        public string? Comment { get; set; }

        public bool? DrawerStatus { get; set; }

        public decimal? ShiftEndBank { get; set; }

        public decimal? ShiftEndAcc { get; set; }
        [NotMapped]
        public UserClaimsDto? User { get; set; }
        [NotMapped]
        public List<PaymentModeSummaryDto>? ShiftclosureSummary { get; set; }
    }
}
