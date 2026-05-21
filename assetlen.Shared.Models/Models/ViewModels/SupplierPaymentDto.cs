using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class SupplierPaymentDto : BaseDto
    {
        //public int Id { get; set; }

        public int? UserId { get; set; }

        public int? SupplierId { get; set; }

        public decimal? Amount { get; set; }

        public DateTime? DateTimePayed { get; set; }

        public int? PaymentId { get; set; }
    }
}
