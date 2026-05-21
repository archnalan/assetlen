using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ExportDtos
{
    public class TransactionsExportDto
    {
        public int TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string? SoldBy { get; set; }

        public decimal? SaleTotal { get; set; }

        public decimal? Change { get; set; }

        public int? ShiftId { get; set; }

        public int? CustomerId { get; set; }

        public int? TransactionStatus { get; set; }

        public string? SaleAgentId { get; set; }

        public int? QuotationId { get; set; }

        public int? OrderStatus { get; set; }

        public string? TransactionComment { get; set; }
    }
}
