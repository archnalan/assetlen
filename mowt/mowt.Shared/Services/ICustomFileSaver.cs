using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Services
{
    public interface ICustomFileSaver
    {
        Task<ServiceResult<string>> SaveFileAsync(string defaultFileName, string fileExtension, MemoryStream stream, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> OpenFileWithDefaultAppAsync(string fullPath);
        Task<FileResultDto> PickFileFromSystem();
    }
}
