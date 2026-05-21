using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Shared.Models.Models;

namespace assetlen.Service.DbServices
{
    public class UserFavoritesDAL : IUserFavoritesDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ILogger<UserFavoritesDAL> _logger;
        private readonly ITenantProvider _tenantProvider;

        public UserFavoritesDAL(ILogger<UserFavoritesDAL> logger, AssetlenDbContext context, ITenantProvider tenantProvider)
        {
            _logger = logger;
            _context = context;
            _tenantProvider = tenantProvider;
        }

        #region Get Favorites by UserId
        public async Task<ServiceResult<List<UserFavoriteDto>>> GetFavoritesByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return ServiceResult<List<UserFavoriteDto>>.Failure(new ArgumentException("UserId is required."));

                var favorites = await _context.tbl_UserFavorites
                    .Where(f => f.UserId == userId && f.IsDeleted != true)
                    .Include(f => f.Product)
                    .OrderByDescending(f => f.DateTimeCreated)
                    .ToListAsync();

                var dtos = favorites
                    .Select(f => new UserFavoriteDto
                    {
                        Id = f.Id,
                        UserId = f.UserId,
                        ProductId = f.ProductId,
                        IsFavorited = true,
                        DateTimeCreated = f.DateTimeCreated,
                        Product = f.Product?.Adapt<ProductsDto>()
                    })
                    .ToList();

                return ServiceResult<List<UserFavoriteDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching favorites for user {UserId}", userId);
                return ServiceResult<List<UserFavoriteDto>>.Failure(new ServerErrorException("Could not fetch favorites."));
            }
        }
        #endregion

        #region Check if a product is favorited
        public async Task<ServiceResult<bool>> IsFavorited(string userId, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
                    return ServiceResult<bool>.Failure(new ArgumentException("UserId and ProductId are required."));

                var exists = await _context.tbl_UserFavorites
                    .AnyAsync(f => f.UserId == userId && f.ProductId == productId && f.IsDeleted != true);

                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking favorite for user {UserId} product {ProductId}", userId, productId);
                return ServiceResult<bool>.Failure(new ServerErrorException("Could not check favorite status."));
            }
        }
        #endregion

        #region Toggle Favorite
        public async Task<ServiceResult<UserFavoriteDto>> ToggleFavorite(string userId, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
                    return ServiceResult<UserFavoriteDto>.Failure(new ArgumentException("UserId and ProductId are required."));

                var existing = await _context.tbl_UserFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (existing != null)
                {
                    // Toggle soft-delete (un-favorite / re-favorite)
                    existing.IsDeleted = !(existing.IsDeleted ?? false);
                    existing.DateTimeModified = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return ServiceResult<UserFavoriteDto>.Success(new UserFavoriteDto
                    {
                        Id = existing.Id,
                        UserId = existing.UserId,
                        ProductId = existing.ProductId,
                        IsFavorited = existing.IsDeleted != true
                    });
                }

                // New favorite
                var tenantId = _tenantProvider.GetTenantId();
                var favorite = new tbl_UserFavorite
                {
                    UserId = userId,
                    ProductId = productId,
                    TenantId = tenantId,
                    DateTimeCreated = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _context.tbl_UserFavorites.AddAsync(favorite);
                await _context.SaveChangesAsync();

                return ServiceResult<UserFavoriteDto>.Success(new UserFavoriteDto
                {
                    Id = favorite.Id,
                    UserId = favorite.UserId,
                    ProductId = favorite.ProductId,
                    IsFavorited = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite for user {UserId} product {ProductId}", userId, productId);
                return ServiceResult<UserFavoriteDto>.Failure(new ServerErrorException("Could not toggle favorite."));
            }
        }
        #endregion
    }
}
