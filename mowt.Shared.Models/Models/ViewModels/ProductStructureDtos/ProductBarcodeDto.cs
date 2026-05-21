using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels.ProductStructureDtos
{
    public class ProductBarcodeDto
    {
        public string ProductId { get; set; }
        public string? Barcode { get; set; }
        public string? ProductName { get; set; }
        public bool Selected { get; set; }
        public ConfirmAction ConfirmAction { get; set; } = ConfirmAction.None;
    }

    public enum ConfirmAction
    {
        None,
        Confirm,
        Skip,
        SkipAll,
        Cancel
    }
}
