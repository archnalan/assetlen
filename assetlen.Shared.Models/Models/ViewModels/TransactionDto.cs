using assetlen.Shared.Models.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class TransactionDto : BaseDto
    {
        //public int Id { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string? SoldBy { get; set; }

        public decimal? SaleTotal { get; set; }

        public decimal? Change { get; set; }

        public string? ShiftId { get; set; }

        public string? CustomerId { get; set; }

        public int? TransactionStatus { get; set; }

        public string? SaleAgentId { get; set; }

        public string? QuotationId { get; set; }

        public string? OrderStatus { get; set; }

        public string? TransactionComment { get; set; }

        public CustomerDto? Customer { get; set; }
        public AppUserDto? Seller { get; set; }
        public ICollection<TransactionDetailDto>? TransactionDetails { get; set; }
        public ICollection<OrderStatusDto>? orderStatusDtos { get; set; }
    }
    public class TransactionStatusUpdateDto : BaseDto
    {
        //public int Id { get; set; }

        public int? TransactionStatus { get; set; }

        public string? OrderStatus { get; set; }

        public string? TransactionComment { get; set; }

        public string? SellerId { get; set; }

        public string? ShiftId { get; set; }

        public string? CustomerId { get; set; }

        public string? CustomerName { get; set; }
    }
}
