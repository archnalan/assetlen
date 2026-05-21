using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{

    public class ReceiptPrintDto
    {
        //public int Id { get; set; }
        public int CustomerId { get; set; }
        public int SupplierId { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public int ReceiptSlipType { get; set; }
    }
}
