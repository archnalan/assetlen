using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ReportingDto;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Apicalls
{
    public interface IPrinterPreferencesApi
    {

        [Get("/api/PrinterPreferences/GetPrinterPreferences")]
        Task<IApiResponse<PaginationDetails<PrinterPreferancesDto>>> GetPrinterPreferences([Query] string? keywords, [Query] int? offset, [Query] int? limit, [Query] CancellationToken cancellationToken = default, [Query] string sortByColumn = null, [Query] bool sortAscending = true);

        [Delete("/api/PrinterPreferences/DeletePrinterPreferences/{id}")]
        Task<IApiResponse<List<PrinterPreferancesDto>>> DeletePrinterPreferences(string id);
        [Get("/api/PrinterPreferences/GetPrinterPreferencesById/{id}")]
        Task<IApiResponse<PrinterPreferancesDto>> GetPrinterPreferencesById(string id);
        [Get("/api/PrinterPreferences/GetPrinterPreferencesBySlipType/{slipType}")]
        Task<IApiResponse<PrinterPreferancesDto>> GetPrinterPreferencesBySlipType(int slipType);

        [Post("/api/PrinterPreferences/AddOrUpdatePrinterPreferences")]
        Task<IApiResponse<PrinterPreferancesDto>> AddOrUpdatePrinterPreferences([Body] PrinterPreferancesDto printerPreferancesDto);
    }
}
