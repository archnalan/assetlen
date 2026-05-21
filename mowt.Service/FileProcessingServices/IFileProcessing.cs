using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;

namespace mowt.Service.FileProcessingServices
{
    public interface IFileProcessing
    {
        Task<ServiceResult<List<Dictionary<string, object>>>> ProcessExcelFile(ImportMedia p);
    }
}