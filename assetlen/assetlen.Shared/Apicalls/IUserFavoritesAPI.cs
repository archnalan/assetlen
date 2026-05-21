using assetlen.Shared.Models.Models.ViewModels;
using Refit;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Apicalls
{
    public interface IUserFavoritesAPI
    {
        [Get("/api/UserFavorites/GetMyFavorites")]
        Task<IApiResponse<List<UserFavoriteDto>>> GetMyFavorites();

        [Get("/api/UserFavorites/IsFavorited")]
        Task<IApiResponse<bool>> IsFavorited([Query][Required] string productId);

        [Post("/api/UserFavorites/ToggleFavorite")]
        Task<IApiResponse<UserFavoriteDto>> ToggleFavorite([Query][Required] string productId);
    }
}
