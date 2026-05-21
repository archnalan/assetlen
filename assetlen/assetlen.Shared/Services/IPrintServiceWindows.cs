using assetlen.ServiceHandler;

namespace assetlen.Services
{
    public interface IPrintServiceWindows
    {
        Task<ServiceResult<List<string>>> GetAvailablePrintersAsync();
        Task<ServiceResult<bool>> PrintDocumentAsync(MemoryStream pdfDocument, string printerName = "");
    }
}