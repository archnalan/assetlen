using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.DocumentModels
{
    public class SectionOrderChangeDto
    {
        public string Id { get; set; } = string.Empty;
        public int NewSortOrder { get; set; }
    }
}
