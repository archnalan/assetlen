using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Services
{
    public interface IFolderPickerService
    {
        Task<string> PickFolderAsync();
    }
}
