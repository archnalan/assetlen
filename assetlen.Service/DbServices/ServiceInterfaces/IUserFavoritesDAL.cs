using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface IUserFavoritesDAL
    {
        Task<ServiceResult<List<UserFavoriteDto>>> GetFavoritesByUserId(string userId);
        Task<ServiceResult<bool>> IsFavorited(string userId, string productId);
        Task<ServiceResult<UserFavoriteDto>> ToggleFavorite(string userId, string productId);
    }
}
