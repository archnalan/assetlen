using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;

namespace assetlen.Service.FileProcessingServices
{
    public interface IFileProcessing
    {
        Task<ServiceResult<List<Dictionary<string, object>>>> ProcessExcelFile(ImportMedia p);
    }
}