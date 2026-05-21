using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Services
{
    public interface ICustomFileSaver
    {
        Task<ServiceResult<string>> SaveFileAsync(string defaultFileName, string fileExtension, MemoryStream stream, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> OpenFileWithDefaultAppAsync(string fullPath);
        Task<FileResultDto> PickFileFromSystem();
    }
}
