using assetlen.ServiceHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Services
{
    public interface IPrintService
    {
        Task<ServiceResult<List<string>>> GetPrintersWindowsAsync();
        Task<ServiceResult<bool>> PrintDocumentAsync(MemoryStream pdfDocument, string printerName = "");
        Task<ServiceResult<bool>> PrintImageAsync(MemoryStream imageStream, string printerName = "");
    }
}
