using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface IDiscountsDAL
    {
        Task<ServiceResult<DiscountDto>> AddDiscount(DiscountCreateDto d);
        Task<ServiceResult<bool>> DeleteDiscountById(string id);
        Task<ServiceResult<DiscountDto>> GetDiscountById(string id);
        Task<ServiceResult<DiscountDto>> GetDiscountByValue(decimal value);
        Task<ServiceResult<PaginationDetails<ComboBoxDto>>> GetDiscountsFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchDiscountsFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending, bool isActive);
        Task<ServiceResult<PaginationDetails<DiscountDto>>> GetDiscountsFromDB(int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<DiscountDto>> UpdateDiscount(string id, DiscountDto dDto);
    }
}