using mowt.Shared.Models.Models.ViewModels.ExportDtos;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IExcelDomainService
    {
        Task<List<Dictionary<string, object>>> ImportExcelRecords(MemoryStream file);
        Task<MemoryStream> ExportExcelRecords<T>(List<T> records, List<string> selectedColumns, string sheetName);
    }
}