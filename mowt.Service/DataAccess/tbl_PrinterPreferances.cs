using mowt.Shared.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DataAccess
{
    public class tbl_PrinterPreferances : BaseEntity
    {
        [Key]
        public string Id { get; set; }
        public int ReceiptSlipType { get; set; }
        public string PrinterName { get; set; }
    }
}
