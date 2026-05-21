using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels.ReportingDto
{

    public class PaymentModeSummaryDto
    {
        public string Description { get; set; }
        public string ShiftId { get; set; }
        public string PaymentModeId { get; set; }
        public decimal? SaleTotal { get; set; }
        [NotMapped]
        public decimal? TotalCounted { get; set; } = 0;
        [NotMapped]
        public decimal? ShiftExpense { get; set; } = 0;
        [NotMapped]
        public decimal? TotalExpected { get; set; }
        [NotMapped]
        public decimal? TotalShortage { get; set; }
    }

    public class ShiftAmountCollectedDto
    {

        public string userId { get; set; }
        public string userName { get; set; }
        public decimal ShiftTotal { get; set; }
        public DateTime ShiftOpened { get; set; }


    }

    public class ChangeSummaryDto
    {
        public string Description { get; set; }
        public string ShiftId { get; set; }
        public string PaymentModeID { get; set; }
        public decimal Change { get; set; }
    }

}
