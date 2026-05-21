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
    public class UserDocumentsDAL : IUserDocumentsDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<UserDocumentsDAL> _logger;
        private readonly ITenantProvider _tenantProvider;

        public UserDocumentsDAL(ILogger<UserDocumentsDAL> logger, mowtDbContext context, ITenantProvider tenantProvider)
        {
            _logger = logger;
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<ServiceResult<List<UserDocumentDto>>> GetCollectionByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return ServiceResult<List<UserDocumentDto>>.Failure(new ArgumentException("UserId is required."));

                var docs = await _context.tbl_UserDocuments
                    .Where(d => d.UserId == userId && d.IsDeleted != true)
                    .Include(d => d.Product)
                    .OrderByDescending(d => d.DateTimeCreated)
                    .ToListAsync();

                var dtos = docs.Select(d => new UserDocumentDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    ProductId = d.ProductId,
                    IsInCollection = true,
                    DateTimeCreated = d.DateTimeCreated,
                    Product = d.Product?.Adapt<ProductsDto>()
                }).ToList();

                return ServiceResult<List<UserDocumentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching collection for user {UserId}", userId);
                return ServiceResult<List<UserDocumentDto>>.Failure(new ServerErrorException("Could not fetch collection."));
            }
        }

        public async Task<ServiceResult<bool>> IsInCollection(string userId, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
                    return ServiceResult<bool>.Failure(new ArgumentException("UserId and ProductId are required."));

                var exists = await _context.tbl_UserDocuments
                    .AnyAsync(d => d.UserId == userId && d.ProductId == productId && d.IsDeleted != true);

                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking collection for user {UserId} product {ProductId}", userId, productId);
                return ServiceResult<bool>.Failure(new ServerErrorException("Could not check collection status."));
            }
        }

        public async Task<ServiceResult<UserDocumentDto>> ToggleDocument(string userId, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
                    return ServiceResult<UserDocumentDto>.Failure(new ArgumentException("UserId and ProductId are required."));

                var existing = await _context.tbl_UserDocuments
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.ProductId == productId);

                if (existing != null)
                {
                    existing.IsDeleted = !(existing.IsDeleted ?? false);
                    existing.DateTimeModified = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return ServiceResult<UserDocumentDto>.Success(new UserDocumentDto
                    {
                        Id = existing.Id,
                        UserId = existing.UserId,
                        ProductId = existing.ProductId,
                        IsInCollection = existing.IsDeleted != true
                    });
                }

                var doc = new tbl_UserDocument
                {
                    UserId = userId,
                    ProductId = productId,
                    TenantId = _tenantProvider.GetTenantId(),
                    DateTimeCreated = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _context.tbl_UserDocuments.AddAsync(doc);
                await _context.SaveChangesAsync();

                return ServiceResult<UserDocumentDto>.Success(new UserDocumentDto
                {
                    Id = doc.Id,
                    UserId = doc.UserId,
                    ProductId = doc.ProductId,
                    IsInCollection = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling collection for user {UserId} product {ProductId}", userId, productId);
                return ServiceResult<UserDocumentDto>.Failure(new ServerErrorException("Could not toggle collection."));
            }
        }
    }
}
