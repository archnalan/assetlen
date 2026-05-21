using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IPrinterPreferencesDAL
    {
        Task<ServiceResult<PaginationDetails<ComboBoxDto>>> GetPrinterPreferences(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<PrinterPreferancesDto>> AddOrUpdatePrinterPreferences(PrinterPreferancesDto printerPreferences);
        Task<ServiceResult<bool>> DeletePrinterPreferences(string id);
        Task<ServiceResult<PrinterPreferancesDto>> GetPrinterPreferencesById(string id);
        Task<ServiceResult<PrinterPreferancesDto>> GetPrinterPreferencesBySlipType(int slipTypeId);
    }
}
