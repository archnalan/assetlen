using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IUserDocumentsDAL
    {
        Task<ServiceResult<List<UserDocumentDto>>> GetCollectionByUserId(string userId);
        Task<ServiceResult<bool>> IsInCollection(string userId, string productId);
        Task<ServiceResult<UserDocumentDto>> ToggleDocument(string userId, string productId);
    }
}
