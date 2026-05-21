using assetlen.Shared.Models.Models.ViewModels;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ITaxDataAPI
    {

        [Get("/api/taxes/GetAllTaxFromDB")]
        Task<IApiResponse<List<taxDto>>> GetAllTaxFromDB();

        //[Get("/api/taxes/SearchCategoryUsingKeywords")]
        //Task<IApiResponse<PaginationDetails<taxDto>>> SearchCategoryUsingKeywords([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Post("/api/taxes/GetTaxFromDBbasedOnTaxID")]
        Task<IApiResponse<taxDto>> GetTaxFromDBbasedOnTaxID(int taxId);

        [Post("/api/taxes/GetTaxIDFromDBbasedOnTaxDescription")]
        Task<IApiResponse<taxDto>> GetTaxIDFromDBbasedOnTaxDescription(string taxDescription);

        [Post("/api/taxes/CreateNewTax")]
        Task<IApiResponse<taxDto>> CreateNewTax([Body] taxDto taxDto);

        [Put("/api/taxes/UpdateTaxinDBbasedOnTaxID")]
        Task<IApiResponse<taxDto>> UpdateTaxinDBbasedOnTaxID([Body] taxDto taxDto);

        [Delete("/api/taxes/DeleteTaxinDBbasedOnTaxID")]
        Task<IApiResponse<taxDto>> DeleteTaxinDBbasedOnTaxID([Query] string taxId);

        [Get("/api/taxes/SearchTaxFromDB")]
        Task<IApiResponse<List<taxDto>>> SearchTaxFromDB([Query] string? searchText);
        [Get("/api/taxes/SearchTaxesForComboBoxes")]
        Task<IApiResponse<List<ComboBoxDto>>> SearchTaxesForComboBoxes([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);
    }
}
