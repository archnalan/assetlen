using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Services;

namespace assetlen.Client.Services
{
    public class FileSaverWeb : ICustomFileSaver
    {
        public Task<ServiceResult<bool>> OpenFileWithDefaultAppAsync(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Task<FileResultDto> PickFileFromSystem()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<string>> SaveFileAsync(string defaultFileName, string fileExtension, MemoryStream stream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
    public class PrintServiceWeb : IPrintService
    {
        public Task<ServiceResult<List<string>>> GetPrintersWindowsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<bool>> PrintDocumentAsync(MemoryStream pdfDocument, string printerName = "")
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<bool>> PrintImageAsync(MemoryStream imageStream, string printerName = "")
        {
            throw new NotImplementedException();
        }
    }
    public class FolderPickerService : IFolderPickerService
    {
        public Task<string> PickFolderAsync()
        {
            throw new NotImplementedException();
        }
    }
}
