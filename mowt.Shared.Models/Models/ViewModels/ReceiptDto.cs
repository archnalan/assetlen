using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{
	public class ReceiptDto : BaseDto
	{
		//public int Id { get; set; }

		public int? SlipId { get; set; }

		public int? PrintItemType { get; set; }

		public string? ItemText { get; set; }

		public int? X { get; set; }

		public int? Y { get; set; }

		public int? Height { get; set; }

		public int? Width { get; set; }

		public int? Align { get; set; }

		public int? Width2 { get; set; }

		public int? Align2 { get; set; }

		public int? Width3 { get; set; }

		public int? Align3 { get; set; }

		public string? FontName { get; set; }

		public double? FontSize { get; set; }

		public int? Width4 { get; set; }

		public int? Align4 { get; set; }

		public int? Width5 { get; set; }

		public int? Align5 { get; set; }
	}
}
