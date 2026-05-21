using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;
using System.Linq.Dynamic.Core;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace assetlen.Service.DbServices
{
    public class CategoryDAL : ICategoryDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<CategoryDAL> _logger;
        private readonly ITenantProvider _tenantProvider;
        private readonly IExcelDomainService _excelDomainService;
        public CategoryDAL(ILogger<CategoryDAL> logger, mowtDbContext context, ITenantProvider tenantProvider, IExcelDomainService excelDomainService)
        {
            _logger = logger;
            _context = context;
            _tenantProvider = tenantProvider;
            _excelDomainService = excelDomainService;
        }

        #region Read Categories from Database
        public async Task<ServiceResult<PaginationDetails<CategoryDto>>> GetCategoriesFromDB(int offSet, int limit, CancellationToken cancellationToken, string? sortByColumn, bool sortAscending)
        {
            try
            {
                var categories = await _context.tbl_Categories.AsNoTracking().OrderBy(c => c.Category).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var categoriesDto = categories.Adapt<PaginationDetails<CategoryDto>>();

                return ServiceResult<PaginationDetails<CategoryDto>>.Success(categoriesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching categories from database: {Error}", ex);
                return ServiceResult<PaginationDetails<CategoryDto>>.Failure(
                    new ServerErrorException("Could not fetch categories."));
            }
        }
        #endregion

        #region Read CategoriesID from Database based on CategoryName
        public async Task<ServiceResult<string>> GetCategoryIDBasedOnCategoryName(string categoryName)
        {
            try
            {
                string sql = "select categoryId from tbl_category where deleted=0 and category = @category";

                var result = await _context.tbl_Categories.FromSqlRaw(sql, new SqlParameter("@category", categoryName))
                                                            .Select(c => c.Id).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(result))
                {
                    _logger.LogError("Category with name: {CategoryName} not found.", categoryName);
                    return ServiceResult<string>.Failure(new NotFoundException($"Category with name {categoryName} not found."));
                }

                return ServiceResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching category with name {CategoryName}: {Error}", categoryName, ex);
                return ServiceResult<string>.Failure(
                    new ServerErrorException("Could not fetch category."));
            }
        }
        #endregion

        #region Read Categories from Database for ComboBoxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchCategoriesFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            var query = _context.tbl_Categories.AsNoTracking();
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.Id.ToString().Contains(keywords)
                    || c.Category != null && c.Category.ToLower().Contains(keywords.ToLower())
                    || c.Description != null && c.Description.ToLower().Contains(keywords.ToLower()));
                }
                var categories = await query.AsNoTracking()
                                       .Select(x => new ComboBoxDto
                                       {
                                           Id = x.Id,
                                           IdString = x.Id.ToString(),
                                           ValueText = x.Category ?? string.Empty
                                       })
                                       .ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching categories for combobox: {Error}", ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException("Could not search categories."));
            }
        }
        #endregion

        #region Add Category to DB
        public async Task<ServiceResult<CategoryDto>> AddCategory(CategoryDto c)
        {
            if (c == null) return ServiceResult<CategoryDto>.Failure(
                                new BadRequestException("Category data is required."));

            var categoryExists = await _context.tbl_Categories.AnyAsync(x => x.Category == c.Category);

            if (categoryExists) return ServiceResult<CategoryDto>.Failure(
                                    new ConflictException($"Category {c.Category} already exists."));

            try
            {
                var cat = c.Adapt<tbl_Category>();

                await _context.AddAsync(cat);

                await _context.SaveChangesAsync();

                var createdCat = cat.Adapt<CategoryDto>();

                return ServiceResult<CategoryDto>.Success(createdCat);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating category: {Error}", ex);
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
                {
                    string errorMessage = "The Category you are trying to create already exists in this system. Please choose another name.";
                    return ServiceResult<CategoryDto>.Failure(new BadRequestException(errorMessage));
                }

                return ServiceResult<CategoryDto>.Failure(
                    new ServerErrorException("Could not create category."));
            }


        }
        #endregion

        #region Get Category from Database based on CategoryID
        public async Task<ServiceResult<CategoryDto>> GetCategoryById(string id)
        {
            try
            {
                var category = await _context.tbl_Categories.FindAsync(id);

                if (category == null)
                {
                    _logger.LogError("Category with ID: {CategoryId} not found.", id);
                    return ServiceResult<CategoryDto>.Failure(
                        new NotFoundException($"Category with ID: {id} not found."));
                }

                return ServiceResult<CategoryDto>.Success(category.Adapt<CategoryDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching category with ID {CategoryId}: {Error}", id, ex);
                return ServiceResult<CategoryDto>.Failure(
                    new ServerErrorException("Could not fetch category."));
            }

        }
        #endregion

        #region Get CategoriesID from Database based on CategoryName
        public async Task<ServiceResult<CategoryDto>> GetCategoryByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return ServiceResult<CategoryDto>.Failure(
                                                new BadRequestException("Category name is required."));

            try
            {
                var category = await _context.tbl_Categories.FirstOrDefaultAsync(c => c.Category == name);

                if (category == null)
                {
                    _logger.LogError("Category with name: {CategoryName} not found.", name);
                    return ServiceResult<CategoryDto>.Failure(
                        new NotFoundException($"Category with name: {name} not found."));
                }

                return ServiceResult<CategoryDto>.Success(category.Adapt<CategoryDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching category with name {CategoryName}: {Error}", name, ex);
                return ServiceResult<CategoryDto>.Failure(
                    new ServerErrorException("Could not fetch category."));
            }

        }
        #endregion

        #region update category in the  DB
        public async Task<ServiceResult<CategoryDto>> UpdateCategory(string id, CategoryDto cDto)
        {
            if (cDto == null) return ServiceResult<CategoryDto>.Failure(
                                new BadRequestException("Category data is required."));

            if (cDto.Id != id) return ServiceResult<CategoryDto>.Failure(
                    new BadRequestException($"Category with ID: {id} is not the same as category with ID: {cDto.Id}"));


            var categoryInDb = await _context.tbl_Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (categoryInDb == null) return ServiceResult<CategoryDto>.Failure(
                                    new NotFoundException($"Category with ID {id} not found."));

            try
            {
                //Map the incoming data excluding unchanged properties
                categoryInDb.Category = cDto.Category ?? categoryInDb.Category;
                categoryInDb.Description = cDto.Description ?? categoryInDb.Description;
                categoryInDb.HideInPos = cDto.HideInPos;

                await _context.SaveChangesAsync();

                return ServiceResult<CategoryDto>.Success(categoryInDb.Adapt<CategoryDto>());

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating category with ID {CategoryId}: {Error}", id, ex);
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
                {
                    string errorMessage = "The Category you are trying to update already exists in this system. Please choose another name.";
                    return ServiceResult<CategoryDto>.Failure(new BadRequestException(errorMessage));
                }

                return ServiceResult<CategoryDto>.Failure(
                    new ServerErrorException("Could not update category."));
            }

        }
        #endregion

        #region Delete category softdelete
        public async Task<ServiceResult<bool>> DeleteCategoryById(string id)
        {
            try
            {
                var categoryInDb = await _context.tbl_Categories.FindAsync(id);

                if (categoryInDb == null)
                {
                    _logger.LogError("Category with ID: {CategoryId} not found for deletion.", id);
                    return ServiceResult<bool>
                        .Failure(new NotFoundException($"Category with ID: {id} not found."));
                }

                //soft delete
                categoryInDb.IsDeleted = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("category with ID {id} could not be deleted.: {ex}", id, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete category."));
            }
        }
        #endregion

        #region Search Categories from Database
        public async Task<ServiceResult<PaginationDetails<CategoryDto>>> SearchCategoriesFromDB(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            var query = _context.tbl_Categories.AsNoTracking();
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.Id.ToString().Contains(keywords)
                    || c.Category != null && c.Category.ToLower().Contains(keywords.ToLower())
                    || c.Description != null && c.Description.ToLower().Contains(keywords.ToLower()));
                }
                var categories = await query.ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

                var categoriesDto = categories.Adapt<PaginationDetails<CategoryDto>>();

                return ServiceResult<PaginationDetails<CategoryDto>>.Success(categoriesDto);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching categories: {Error}", ex);
                return ServiceResult<PaginationDetails<CategoryDto>>.Failure(
                    new ServerErrorException("Could not search categories."));
            }
        }
        #endregion

        #region Get Categories ForCSVExport BasedOn SelectedFields

        public async Task<ServiceResult<MemoryStream>> GetCategoriesForCSVExportBySelectedFields(List<string> selectedColumnNames)
        {
            try
            {
                IQueryable<tbl_Category> query = _context.tbl_Categories;

                // Build the dynamic SELECT clause
                var selectFields = new List<string>();
                var properties = typeof(tbl_Category).GetProperties();
                foreach (var prop in properties)
                {
                    if (selectedColumnNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                        selectFields.Add(prop.Name);
                }

                // Project only the selected columns dynamically
                var dynamicQuery = query.Select($"new ({string.Join(", ", selectFields)})");

                var exportObject = dynamicQuery.Adapt<List<CategoriesExportDto>>();
                //create excel file and return it
                var memorystream = await _excelDomainService.ExportExcelRecords(exportObject, selectedColumnNames, "Categories");

                return ServiceResult<MemoryStream>.Success(memorystream);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while exporting categories: {Error}", ex);
                return ServiceResult<MemoryStream>.Failure(
                    new ServerErrorException("Could not export categories."));
            }
        }
        #endregion

        #region Import Categories from Excel
        public async Task<ServiceResult<ImportResultSummary>> ImportCategoriesFromExcel(ImportDataDto p)
        {
            if (p == null)
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("Import data is required."));

            int totalCategories = 0;
            int updatedCount = 0;
            int createdCount = 0;
            int failedCount = 0;
            List<string> messages = new List<string>();

            if (p.UploadedExcelContent == null || p.UploadedExcelContent.Count == 0)
            {
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No data found in the uploaded file."));
            }

            foreach (var catInList in p.UploadedExcelContent)
            {
                totalCategories++;
                try
                {
                    tbl_Category? categoryEntity = null;
                    bool isUpdate = false;

                    if (p.ColumnMappingsList == null || p.ColumnMappingsList.Count == 0)
                    {
                        return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No column mappings provided."));
                    }
                    var categoryNameKey = GetKey("Category", p.ColumnMappingsList);
                    string categoryName = !string.IsNullOrEmpty(categoryNameKey) ? GetValue(catInList, categoryNameKey)?.ToString()! : string.Empty;
                    if (string.IsNullOrEmpty(categoryName))
                    {
                        string description = BuildCategoryDescription(catInList, p);
                        messages.Add($"&#x1F4CC; {description} could not be processed due to missing Category name.");
                        failedCount++;
                        continue;
                    }
                    categoryEntity = await _context.tbl_Categories.FirstOrDefaultAsync(c => c.Category == categoryName);
                    if (categoryEntity != null)
                    {
                        isUpdate = true;
                    }
                    else
                    {
                        // Check if name is unique (for creation)
                        bool exists = await _context.tbl_Categories.AnyAsync(c => c.Category == categoryName);
                        if (exists)
                        {
                            string description = BuildCategoryDescription(catInList, p);
                            messages.Add($"&#x1F4CC; {description} could not be added as the category name '{categoryName}' already exists.");
                            failedCount++;
                            continue;
                        }
                        // Create new category
                        categoryEntity = new tbl_Category();
                        _context.tbl_Categories.Add(categoryEntity);
                        isUpdate = false;
                    }

                    // Map fields from Excel data to category entity
                    foreach (var mapping in p.ColumnMappingsList)
                    {
                        string systemColumn = mapping.SystemColumn.ToLower();
                        string fileColumn = mapping.SelectedFileColumn;
                        if (string.IsNullOrEmpty(fileColumn))
                            continue;

                        object value = GetValue(catInList, fileColumn);

                        switch (systemColumn)
                        {
                            case "category":
                                categoryEntity.Category = value?.ToString();
                                break;
                            case "description":
                                categoryEntity.Description = value?.ToString();
                                break;
                            case "hideinpos":
                                if (value != null && bool.TryParse(value.ToString(), out bool hideInPos))
                                    categoryEntity.HideInPos = hideInPos;
                                break;
                            case "deleted":
                                if (value != null && bool.TryParse(value.ToString(), out bool deleted))
                                    categoryEntity.Deleted = deleted;
                                break;
                                // CategoryId is handled above and not mapped here
                        }
                    }

                    await _context.SaveChangesAsync();

                    if (isUpdate)
                        updatedCount++;
                    else
                        createdCount++;
                }
                catch (Exception ex)
                {
                    string description = BuildCategoryDescription(catInList, p);
                    _logger.LogError("Error while importing category: {Description}. Error: {Error}", description, ex);
                    messages.Add($"&#x1F4CC; {description} could not be imported due to an error.");
                    failedCount++;
                }
            }

            string summary = $"Total Categories Processed: {totalCategories}\n\nCreated: {createdCount}\nUpdated: {updatedCount}\nFailed: {failedCount}";
            string resultMessage = string.Join("\n", messages);

            var output = new ImportResultSummary
            {
                Summary = summary,
                Errors = resultMessage
            };

            return ServiceResult<ImportResultSummary>.Success(output);
        }
        private string BuildCategoryDescription(Dictionary<string, object> catData, ImportDataDto p)
        {
            List<string> parts = new List<string>();

            var categoryIdKey = GetKey("CategoryId", p.ColumnMappingsList);
            var categoryNameKey = GetKey("Category", p.ColumnMappingsList);
            var descriptionKey = GetKey("Description", p.ColumnMappingsList);

            var categoryIdVal = !string.IsNullOrEmpty(categoryIdKey) ? GetValue(catData, categoryIdKey)?.ToString() : "";
            var categoryNameVal = !string.IsNullOrEmpty(categoryNameKey) ? GetValue(catData, categoryNameKey)?.ToString() : "";
            var descriptionVal = !string.IsNullOrEmpty(descriptionKey) ? GetValue(catData, descriptionKey)?.ToString() : "";

            if (!string.IsNullOrEmpty(categoryIdVal))
                parts.Add($"ID: {categoryIdVal}");
            if (!string.IsNullOrEmpty(categoryNameVal))
                parts.Add($"Name: {categoryNameVal}");
            if (!string.IsNullOrEmpty(descriptionVal))
                parts.Add($"Description: {descriptionVal}");

            return "Category [" + string.Join(", ", parts) + "]";
        }
        private object GetValue(Dictionary<string, object> item, string key)
        {
            return item.TryGetValue(key, out object? value) ? (value == null ? "" : value) : "";
        }
        private string GetKey(string columnName, List<ColumnMapping> mappings)
        {
            return mappings.FirstOrDefault(x => x.SystemColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))!.SelectedFileColumn;
        }
        public static string NormalizeString(string? input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim().ToLowerInvariant();
        }

        public static bool CompareNormalizedStrings(string? str1, string? str2)
        {
            return NormalizeString(str1) == NormalizeString(str2);
        }

        #endregion

        #region Get Top Categories
        public async Task<ServiceResult<List<CategoryDto>>> GetTopCategories(int limit, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _context.tbl_Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Category)
                    .Take(limit)
                    .ToListAsync(cancellationToken);

                var categoryDtos = categories.Adapt<List<CategoryDto>>();
                return ServiceResult<List<CategoryDto>>.Success(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top categories");
                return ServiceResult<List<CategoryDto>>.Failure(new ServerErrorException("Error getting top categories"));
            }
        }
        #endregion
    }

}
