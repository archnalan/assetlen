using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class BankPaymentDto : BaseDto
    {
        public string? Name { get; set; }
        public string? AccountNumber { get; set; }
        public string? PaymentReference { get; set; }
    }
    public class ChequePaymentDto
    {

        public string? NameOnCheque { get; set; }
        public string? BankName { get; set; }
        public string? PaymentReference { get; set; }
    }
}
