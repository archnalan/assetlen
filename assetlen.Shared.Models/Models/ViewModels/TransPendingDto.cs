using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
	public class TransPendingDto : BaseDto
	{
		//public int Id { get; set; }
		public DateTime? TransactionDate { get; set; }
		public int? SoldBy { get; set; }
		public decimal? SaleTotal { get; set; }
		public decimal? Change { get; set; }
		public int? ShiftId { get; set; }
		public int? CustomerId { get; set; }
		public int? TransactionStatus { get; set; }
		public int? SaleAgentId { get; set; }
		public int? QuotationId { get; set; }
		public int? OrderStatus { get; set; }
		public string? TransactionComment { get; set; }
		public string? FullName { get; set; }
	}

}
