using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IUserFavoritesDAL
    {
        Task<ServiceResult<List<UserFavoriteDto>>> GetFavoritesByUserId(string userId);
        Task<ServiceResult<bool>> IsFavorited(string userId, string productId);
        Task<ServiceResult<UserFavoriteDto>> ToggleFavorite(string userId, string productId);
    }
}
