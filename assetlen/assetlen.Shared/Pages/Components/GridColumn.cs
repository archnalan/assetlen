using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Pages.Components
{
    public class GridColumn<T>
    {
        public Expression<Func<T, object>> Property { get; set; } = default!;
        public string? Format { get; set; }
        public bool Sortable { get; set; } = false;
        public bool isDisplayValue { get; set; } = false;
    }
}
