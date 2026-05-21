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
    public interface IShiftsApi
    {

        [Get("/api/Shifts/GetShiftsFromDB")]
        Task<IApiResponse<PaginationDetails<ShiftsDto>>> GetShiftsFromDB([Query] DateTime startDate, [Query] DateTime endDate, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Shifts/SearchShifts")]
        Task<IApiResponse<PaginationDetails<ShiftsDto>>> SearchShifts([Query] DateTime startDate, [Query] DateTime endDate, [Query] int? offset, [Query] int? limit, [Query] string sortByColumn = null, [Query] bool sortAscending = true, string keywords = "", string UserId = "", bool shiftStatus = false, [Query] CancellationToken cancellation = default);

        [Get("/api/Shifts/SearchShiftsForComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchShiftsForComboBoxes([Query] string? keywords, [Query] int? offset, [Query] int? limit, [Query] string sortByColumn = null, [Query] bool sortAscending = true, [Query] CancellationToken cancellation = default);

        [Post("/api/Shifts/CreateNewShift")]
        Task<IApiResponse<ShiftsDto>> CreateNewShift([Body] ShiftsDto shiftsDto);

        [Get("/api/Shifts/GetShiftAmountCollectedPerPaymentModeUsingShiftID")]
        Task<IApiResponse<List<PaymentModeSummaryDto>>> GetShiftAmountCollectedPerPaymentModeUsingShiftID([Query] string shiftId);

        [Get("/api/Shifts/GetShiftPerformanceReport")]
        Task<IApiResponse<List<ShiftPerformanceDto>>> GetShiftPerformanceReport([Query] DateTime reportDate, [Query] string? UserId = null);

        [Get("/api/Shifts/GetLastTransactionfromDB")]
        Task<IApiResponse<TransactionDto>> GetLastTransactionfromDB([Query] string shiftId);

        [Post("/api/Shifts/CloseShiftUsingShiftId")]
        Task<IApiResponse<ShiftsDto>> CloseShiftUsingShiftId([Body] ShiftsDto shiftsDto);

        [Get("/api/Shifts/CheckforOpenShift")]
        Task<IApiResponse<ShiftsDto>> CheckforOpenShift([Query] string userId);

        [Put("/api/Shifts/UpdateActiveTransactionInShift")]
        Task<IApiResponse<ShiftsDto>> UpdateActiveTransactionInShift([Query] string shiftId, [Query] string activateSaleId);

        [Get("/api/Shifts/CanUserResumeTransactionFromShift")]
        Task<IApiResponse<bool>> CanUserResumeTransactionFromShift([Query] string userId, [Query] string transactionId);
    }
}
