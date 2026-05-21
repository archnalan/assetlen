using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{

    public class PaginationDetails<T> where T : class
    {
        public List<T> Data { get; set; }
        public int Limit { get; set; }
        public int OffSet { get; set; }
        public long TotalSize { get; set; }
        public bool IsNext { get; set; }

    }
}
