using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.DocumentModels;
using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Service.DbServices
{
    public class ProductDetailDAL : IProductDetailDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ILogger<ProductDetailDAL> _logger;
        private readonly ITenantProvider _tenantProvider;

        private const int MaxTitleLength = 250;
        private const int MinLevel = 1;
        private const int MaxLevel = 3;

        public ProductDetailDAL(
            ILogger<ProductDetailDAL> logger,
            AssetlenDbContext context,
            ITenantProvider tenantProvider)
        {
            _logger = logger;
            _context = context;
            _tenantProvider = tenantProvider;
        }

        #region Get Sections By ProductId
        public async Task<ServiceResult<List<ProductDetailDto>>> GetPreviewSectionByProductId(
            string productId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<List<ProductDetailDto>>.Failure(
                    new BadRequestException("ProductId is required"));

            try
            {
                var section = await _context.tbl_ProductDetails
                    .Where(x => x.ProductId == productId && x.IsDeleted != true)
                    .OrderBy(x => x.SortOrder)
                    .FirstOrDefaultAsync(cancellationToken);

                var result = section.Adapt<List<ProductDetailDto>>();
                return ServiceResult<List<ProductDetailDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sections for ProductId: {ProductId}", productId);
                return ServiceResult<List<ProductDetailDto>>.Failure(
                    new ServerErrorException("Could not fetch product sections"));
            }
        }
        #endregion

        #region Get Sections By ProductId
        public async Task<ServiceResult<List<ProductDetailDto>>> GetSectionsByProductId(
            string productId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<List<ProductDetailDto>>.Failure(
                    new BadRequestException("ProductId is required"));

            try
            {
                var sections = await _context.tbl_ProductDetails
                    .Where(x => x.ProductId == productId && x.IsDeleted != true)
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync(cancellationToken);

                var result = sections.Adapt<List<ProductDetailDto>>();
                return ServiceResult<List<ProductDetailDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sections for ProductId: {ProductId}", productId);
                return ServiceResult<List<ProductDetailDto>>.Failure(
                    new ServerErrorException("Could not fetch product sections"));
            }
        }
        #endregion

        #region Get Section By Id
        public async Task<ServiceResult<ProductDetailDto>> GetSectionById(
            string id,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                return ServiceResult<ProductDetailDto>.Failure(
                    new BadRequestException("Section Id is required"));

            try
            {
                var section = await _context.tbl_ProductDetails
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true, cancellationToken);

                if (section == null)
                    return ServiceResult<ProductDetailDto>.Failure(
                        new NotFoundException($"Section with Id {id} not found"));

                return ServiceResult<ProductDetailDto>.Success(section.Adapt<ProductDetailDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching section by Id: {SectionId}", id);
                return ServiceResult<ProductDetailDto>.Failure(
                    new ServerErrorException("Could not fetch section"));
            }
        }
        #endregion

        #region Add Section
        public async Task<ServiceResult<ProductDetailDto>> AddSection(
            ProductDetailCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateSectionData(dto.ProductId, dto.Level, dto.Title);
            if (!validationResult.IsSuccess)
                return ServiceResult<ProductDetailDto>.Failure(validationResult.Error);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Auto-assign SortOrder if not provided
                    int sortOrder = dto.SortOrder ?? await GetNextSortOrder(dto.ProductId, cancellationToken);

                    var section = new tbl_ProductDetail
                    {
                        ProductId = dto.ProductId,
                        Level = dto.Level,
                        SortOrder = sortOrder,
                        Title = dto.Title,
                        Content = dto.Content ?? string.Empty,
                    };

                    await _context.tbl_ProductDetails.AddAsync(section, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return ServiceResult<ProductDetailDto>.Success(section.Adapt<ProductDetailDto>());
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error adding section for ProductId: {ProductId}", dto.ProductId);
                    return ServiceResult<ProductDetailDto>.Failure(
                        new ServerErrorException("Could not add section"));
                }
            });
        }
        #endregion

        #region Add Sections Bulk
        public async Task<ServiceResult<bool>> AddSectionsBulk(
            string productId,
            List<ProductDetailCreateDto> sections,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("ProductId is required"));

            if (sections == null || !sections.Any())
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Sections list cannot be empty"));

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Soft delete existing sections
                    var existingSections = await _context.tbl_ProductDetails
                        .Where(x => x.ProductId == productId)
                        .ToListAsync(cancellationToken);

                    foreach (var existing in existingSections)
                    {
                        existing.IsDeleted = true;
                    }

                    // Add new sections with sequential SortOrder
                    int sortOrder = 1;
                    foreach (var dto in sections)
                    {
                        var validationResult = ValidateSectionData(productId, dto.Level, dto.Title);
                        if (!validationResult.IsSuccess)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return ServiceResult<bool>.Failure(validationResult.Error);
                        }

                        var section = new tbl_ProductDetail
                        {
                            ProductId = productId,
                            Level = dto.Level,
                            SortOrder = sortOrder++,
                            Title = dto.Title,
                            Content = dto.Content ?? string.Empty
                        };

                        await _context.tbl_ProductDetails.AddAsync(section, cancellationToken);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return ServiceResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error bulk adding sections for ProductId: {ProductId}", productId);
                    return ServiceResult<bool>.Failure(
                        new ServerErrorException("Could not bulk add sections"));
                }
            });
        }
        #endregion

        #region Update Section
        public async Task<ServiceResult<ProductDetailDto>> UpdateSection(
            ProductDetailUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(dto.Id))
                return ServiceResult<ProductDetailDto>.Failure(
                    new BadRequestException("Section Id is required"));

            try
            {
                var section = await _context.tbl_ProductDetails
                    .FirstOrDefaultAsync(x => x.Id == dto.Id && x.IsDeleted != true, cancellationToken);

                if (section == null)
                    return ServiceResult<ProductDetailDto>.Failure(
                        new NotFoundException($"Section with Id {dto.Id} not found"));

                if (dto.Level.HasValue)
                {
                    if (dto.Level < MinLevel || dto.Level > MaxLevel)
                        return ServiceResult<ProductDetailDto>.Failure(
                            new BadRequestException($"Level must be between {MinLevel} and {MaxLevel}"));
                    section.Level = dto.Level.Value;
                }

                if (dto.Title != null)
                {
                    if (dto.Title.Length > MaxTitleLength)
                        return ServiceResult<ProductDetailDto>.Failure(
                            new BadRequestException($"Title cannot exceed {MaxTitleLength} characters"));
                    section.Title = dto.Title;
                }

                if (dto.Content != null)
                    section.Content = dto.Content;

                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<ProductDetailDto>.Success(section.Adapt<ProductDetailDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section with Id: {SectionId}", dto.Id);
                return ServiceResult<ProductDetailDto>.Failure(
                    new ServerErrorException("Could not update section"));
            }
        }
        #endregion

        #region Update Section Content
        public async Task<ServiceResult<bool>> UpdateSectionContent(
            string id,
            string content,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Section Id is required"));

            try
            {
                var section = await _context.tbl_ProductDetails
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true, cancellationToken);

                if (section == null)
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Section with Id {id} not found"));

                section.Content = content ?? string.Empty;
                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating content for section Id: {SectionId}", id);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not update section content"));
            }
        }
        #endregion

        #region Update Section Title
        public async Task<ServiceResult<bool>> UpdateSectionTitle(
            string id,
            string title,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Section Id is required"));

            if (string.IsNullOrEmpty(title))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Title is required"));

            if (title.Length > MaxTitleLength)
                return ServiceResult<bool>.Failure(
                    new BadRequestException($"Title cannot exceed {MaxTitleLength} characters"));

            try
            {
                var section = await _context.tbl_ProductDetails
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true, cancellationToken);

                if (section == null)
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Section with Id {id} not found"));

                section.Title = title;
                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating title for section Id: {SectionId}", id);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not update section title"));
            }
        }
        #endregion

        #region Reorder Sections
        public async Task<ServiceResult<bool>> ReorderSections(
            string productId,
            List<SectionOrderChangeDto> newOrder,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("ProductId is required"));

            if (newOrder == null || !newOrder.Any())
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Order list cannot be empty"));

            // Validate uniqueness and contiguous sequence
            var sortOrders = newOrder.Select(x => x.NewSortOrder).ToList();
            if (sortOrders.Distinct().Count() != sortOrders.Count)
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Duplicate SortOrder values detected"));

            var expectedSequence = Enumerable.Range(1, newOrder.Count).ToList();
            if (!sortOrders.OrderBy(x => x).SequenceEqual(expectedSequence))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("SortOrder must be a contiguous sequence starting at 1"));

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    foreach (var orderChange in newOrder)
                    {
                        var section = await _context.tbl_ProductDetails
                            .FirstOrDefaultAsync(x => x.Id == orderChange.Id && x.IsDeleted != true, cancellationToken);

                        if (section == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return ServiceResult<bool>.Failure(
                                new NotFoundException($"Section with Id {orderChange.Id} not found"));
                        }

                        section.SortOrder = orderChange.NewSortOrder;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return ServiceResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error reordering sections for ProductId: {ProductId}", productId);
                    return ServiceResult<bool>.Failure(
                        new ServerErrorException("Could not reorder sections"));
                }
            });
        }
        #endregion

        #region Delete Section
        public async Task<ServiceResult<bool>> DeleteSection(
            string id,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Section Id is required"));

            try
            {
                var section = await _context.tbl_ProductDetails
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true, cancellationToken);

                if (section == null)
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Section with Id {id} not found"));

                // Soft delete
                section.IsDeleted = true;
                await _context.SaveChangesAsync(cancellationToken);

                // Normalize sort order after deletion
                await NormalizeSortOrderInternal(section.ProductId, cancellationToken);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting section with Id: {SectionId}", id);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete section"));
            }
        }
        #endregion

        #region Delete All Sections For Product
        public async Task<ServiceResult<bool>> DeleteAllSectionsForProduct(
            string productId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("ProductId is required"));

            try
            {
                var sections = await _context.tbl_ProductDetails
                    .Where(x => x.ProductId == productId && x.IsDeleted != true)
                    .ToListAsync(cancellationToken);

                foreach (var section in sections)
                {
                    section.IsDeleted = true;
                }

                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all sections for ProductId: {ProductId}", productId);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete sections"));
            }
        }
        #endregion

        #region Upsert Section
        public async Task<ServiceResult<ProductDetailDto>> UpsertSection(
            ProductDetailUpsertDto dto,
            CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateSectionData(dto.ProductId, dto.Level, dto.Title);
            if (!validationResult.IsSuccess)
                return ServiceResult<ProductDetailDto>.Failure(validationResult.Error);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    tbl_ProductDetail section;

                    if (!string.IsNullOrEmpty(dto.Id))
                    {
                        // Update existing
                        section = await _context.tbl_ProductDetails
                            .FirstOrDefaultAsync(x => x.Id == dto.Id && x.IsDeleted != true, cancellationToken);

                        if (section == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return ServiceResult<ProductDetailDto>.Failure(
                                new NotFoundException($"Section with Id {dto.Id} not found"));
                        }

                        section.Level = dto.Level;
                        section.Title = dto.Title;
                        section.Content = dto.Content ?? string.Empty;

                        if (dto.SortOrder.HasValue)
                            section.SortOrder = dto.SortOrder.Value;
                    }
                    else
                    {
                        // Create new
                        int sortOrder = dto.SortOrder ?? await GetNextSortOrder(dto.ProductId, cancellationToken);

                        section = new tbl_ProductDetail
                        {
                            ProductId = dto.ProductId,
                            Level = dto.Level,
                            SortOrder = sortOrder,
                            Title = dto.Title,
                            Content = dto.Content ?? string.Empty,
                        };

                        await _context.tbl_ProductDetails.AddAsync(section, cancellationToken);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return ServiceResult<ProductDetailDto>.Success(section.Adapt<ProductDetailDto>());
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error upserting section for ProductId: {ProductId}", dto.ProductId);
                    return ServiceResult<ProductDetailDto>.Failure(
                        new ServerErrorException("Could not upsert section"));
                }
            });
        }
        #endregion

        #region Duplicate Section
        public async Task<ServiceResult<ProductDetailDto>> DuplicateSection(
            string id,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                return ServiceResult<ProductDetailDto>.Failure(
                    new BadRequestException("Section Id is required"));

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var original = await _context.tbl_ProductDetails
                        .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted != true, cancellationToken);

                    if (original == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return ServiceResult<ProductDetailDto>.Failure(
                            new NotFoundException($"Section with Id {id} not found"));
                    }

                    // Shift following sections
                    await ShiftSortOrders(original.ProductId, original.SortOrder + 1, cancellationToken);

                    // Create duplicate
                    var duplicate = new tbl_ProductDetail
                    {
                        ProductId = original.ProductId,
                        Level = original.Level,
                        SortOrder = original.SortOrder + 1,
                        Title = $"{original.Title} (Copy)",
                        Content = original.Content,
                    };

                    await _context.tbl_ProductDetails.AddAsync(duplicate, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return ServiceResult<ProductDetailDto>.Success(duplicate.Adapt<ProductDetailDto>());
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error duplicating section with Id: {SectionId}", id);
                    return ServiceResult<ProductDetailDto>.Failure(
                        new ServerErrorException("Could not duplicate section"));
                }
            });
        }
        #endregion

        #region Normalize Sort Order
        public async Task<ServiceResult<bool>> NormalizeSortOrder(
            string productId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("ProductId is required"));

            try
            {
                await NormalizeSortOrderInternal(productId, cancellationToken);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error normalizing sort order for ProductId: {ProductId}", productId);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not normalize sort order"));
            }
        }
        #endregion

        #region Search Sections
        public async Task<ServiceResult<List<ProductDetailDto>>> SearchSections(
            string productId,
            string keyword,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<List<ProductDetailDto>>.Failure(
                    new BadRequestException("ProductId is required"));

            if (string.IsNullOrEmpty(keyword))
                return await GetSectionsByProductId(productId, cancellationToken);

            try
            {
                var sections = await _context.tbl_ProductDetails
                    .Where(x => x.ProductId == productId
                        && x.IsDeleted != true
                        && (x.Title.Contains(keyword) || x.Content.Contains(keyword)))
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync(cancellationToken);

                var result = sections.Adapt<List<ProductDetailDto>>();
                return ServiceResult<List<ProductDetailDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching sections for ProductId: {ProductId}, Keyword: {Keyword}",
                    productId, keyword);
                return ServiceResult<List<ProductDetailDto>>.Failure(
                    new ServerErrorException("Could not search sections"));
            }
        }
        #endregion

        #region Get Document Snapshot
        public async Task<ServiceResult<Dictionary<string, string>>> GetDocumentSnapshot(
            string productId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<Dictionary<string, string>>.Failure(
                    new BadRequestException("ProductId is required"));

            try
            {
                var snapshot = await _context.tbl_ProductDetails
                    .Where(x => x.ProductId == productId && x.IsDeleted != true)
                    .OrderBy(x => x.SortOrder)
                    .ToDictionaryAsync(x => x.Id, x => x.Content, cancellationToken);

                return ServiceResult<Dictionary<string, string>>.Success(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document snapshot for ProductId: {ProductId}", productId);
                return ServiceResult<Dictionary<string, string>>.Failure(
                    new ServerErrorException("Could not get document snapshot"));
            }
        }
        #endregion

        #region Save Document
        public async Task<ServiceResult<bool>> SaveDocument(
            string productId,
            List<ProductDetailPersistDto> sections,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("ProductId is required"));

            if (sections == null || !sections.Any())
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Sections list cannot be empty"));

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Get existing sections
                    var existingSections = await _context.tbl_ProductDetails
                        .Where(x => x.ProductId == productId && x.IsDeleted != true)
                        .ToListAsync(cancellationToken);

                    var existingDict = existingSections.ToDictionary(x => x.Id);

                    int sortOrder = 1;
                    foreach (var dto in sections)
                    {
                        var validationResult = ValidateSectionData(productId, dto.Level, dto.Title);
                        if (!validationResult.IsSuccess)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return ServiceResult<bool>.Failure(validationResult.Error);
                        }

                        tbl_ProductDetail section;

                        if (!string.IsNullOrEmpty(dto.ExistingId) && existingDict.ContainsKey(dto.ExistingId))
                        {
                            // Update existing section
                            section = existingDict[dto.ExistingId];
                            section.Title = dto.Title;
                            section.Content = dto.Content ?? string.Empty;
                            section.Level = dto.Level;
                            section.SortOrder = sortOrder++;
                            section.ParentId = dto.ParentId;
                            existingDict.Remove(dto.ExistingId);
                        }
                        else
                        {
                            // Create new section
                            section = new tbl_ProductDetail
                            {
                                ProductId = productId,
                                Level = dto.Level,
                                SortOrder = sortOrder++,
                                Title = dto.Title,
                                Content = dto.Content ?? string.Empty,
                                ParentId = dto.ParentId
                            };

                            await _context.tbl_ProductDetails.AddAsync(section, cancellationToken);
                        }
                    }

                    // Soft delete remaining sections not in the new list
                    foreach (var remaining in existingDict.Values)
                    {
                        remaining.IsDeleted = true;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return ServiceResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error saving document for ProductId: {ProductId}", productId);
                    return ServiceResult<bool>.Failure(
                        new ServerErrorException("Could not save document"));
                }
            });
        }
        #endregion

        #region Private Helper Methods

        private async Task<int> GetNextSortOrder(string productId, CancellationToken cancellationToken)
        {
            var maxSortOrder = await _context.tbl_ProductDetails
                .Where(x => x.ProductId == productId && x.IsDeleted != true)
                .MaxAsync(x => (int?)x.SortOrder, cancellationToken);

            return (maxSortOrder ?? 0) + 1;
        }

        private async Task ShiftSortOrders(
            string productId,
            int startingFromSortOrder,
            CancellationToken cancellationToken)
        {
            var sectionsToShift = await _context.tbl_ProductDetails
                .Where(x => x.ProductId == productId
                    && x.SortOrder >= startingFromSortOrder
                    && x.IsDeleted != true)
                .ToListAsync(cancellationToken);

            foreach (var section in sectionsToShift)
            {
                section.SortOrder++;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task NormalizeSortOrderInternal(string productId, CancellationToken cancellationToken)
        {
            var sections = await _context.tbl_ProductDetails
                .Where(x => x.ProductId == productId && x.IsDeleted != true)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);

            int newSortOrder = 1;
            foreach (var section in sections)
            {
                section.SortOrder = newSortOrder++;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private ServiceResult<bool> ValidateSectionData(string productId, int level, string title)
        {
            if (string.IsNullOrEmpty(productId))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("ProductId is required"));

            if (level < MinLevel || level > MaxLevel)
                return ServiceResult<bool>.Failure(
                    new BadRequestException($"Level must be between {MinLevel} and {MaxLevel}"));

            if (string.IsNullOrEmpty(title))
                return ServiceResult<bool>.Failure(
                    new BadRequestException("Title is required"));

            if (title.Length > MaxTitleLength)
                return ServiceResult<bool>.Failure(
                    new BadRequestException($"Title cannot exceed {MaxTitleLength} characters"));

            return ServiceResult<bool>.Success(true);
        }

        #endregion
    }
}