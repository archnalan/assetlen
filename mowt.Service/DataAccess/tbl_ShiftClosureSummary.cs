using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DataAccess
{
    public class tbl_ShiftClosureSummary : BaseEntity
    {


        public string Description { get; set; }
        public string ShiftId { get; set; }
        public string PaymentModeID { get; set; }
        public decimal? SaleTotal { get; set; }

        public decimal? TotalCounted { get; set; } = 0;

        public decimal? ShiftExpense { get; set; } = 0;
        public decimal? TotalExpected { get; set; }
        public decimal? TotalShortage { get; set; }
        [ForeignKey(nameof(ShiftId))] public virtual tbl_Shift? Shift { get; set; }
    }
}
