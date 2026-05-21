using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ReportingDto
{
    public class RepStockMovementDto
    {
        public string Description { get; set; }
        public DateTime TransDate { get; set; }
        public string? ProductCode { get; set; }
        public string? Barcode { get; set; }
        public string EventType { get; set; }
        public string EventTypeId { get; set; }
        public decimal OldQty { get; set; }
        public decimal ChangeQty { get; set; }
        public decimal NewQty { get; set; }
        public string? User { get; set; }
        public string? ProductId { get; set; }
    }
}
