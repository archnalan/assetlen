using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class PrinterPreferancesDto : BaseDto
    {
        public int ReceiptSlipType { get; set; }
        public string PrinterName { get; set; }
        public string Id { get; set; }
    }
}
