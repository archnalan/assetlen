using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Service.Extensions;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using mowt.Shared.Models.Models.ViewModels.ReportingDto;
using mowt.Shared.Models.Models.ViewModels.Users;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using mowt.Shared.Models.Models;

namespace mowt.Service.DbServices
{
    public class ProductsDAL : IProductsDAL
    {
        private readonly mowtDbContext _context;
        private readonly IPricingCalculations _pricingCalc;
        private readonly ItaxDAL _taxDAL;
        private readonly ILogger<ProductsDAL> _logger;
        private readonly IProductRelationshipsDAL _relationshipsDAL;
        private readonly FileUploadManager _fileUpload;
        private readonly IExcelDomainService _excelDomainService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITenantProvider _tenantProvider;

        public ProductsDAL(ILogger<ProductsDAL> logger, mowtDbContext context, IProductRelationshipsDAL relationshipsDAL, PricingCalculations pricingCalc, IHttpContextAccessor contextAccessor, FileUploadManager fileUpload, ItaxDAL taxDal, IExcelDomainService excelDomainService, ITenantProvider tenantProvider)
        {
            _logger = logger;
            _context = context;
            _relationshipsDAL = relationshipsDAL;
            _pricingCalc = pricingCalc;
            _fileUpload = fileUpload;
            _excelDomainService = excelDomainService;
            _contextAccessor = contextAccessor;
            _taxDAL = taxDal;
            _tenantProvider = tenantProvider;
        }

        #region Read all products from Database
        public async Task<ServiceResult<PaginationDetails<ProductsDto>>> GetProductsFromDB(
     int offSet, int limit, CancellationToken cancellationToken,
     string sortByColumn, bool sortAscending)
        {
            try
            {
                IQueryable<tbl_Product> query = _context.tbl_Products;
                query = ExcludeHiddenProducts(query);

                var products = await query
                    .OrderBy(c => c.ProductName)
                    .ToPaginatedResultAsync(offSet, limit, cancellationToken,
                                           sortByColumn, sortAscending);

                var productsDto = products.Adapt<PaginationDetails<ProductsDto>>();
                foreach (var productDto in productsDto.Data)
                {
                    if (!string.IsNullOrEmpty(productDto.ProductImage))
                    {
                        string baseUrl = $"{_contextAccessor.HttpContext?.Request.Scheme}://{_contextAccessor.HttpContext?.Request.Host}";
                        string imagePath = Path.Combine("images/products", productDto.ProductImage).Replace("\\", "/");
                        productDto.ProductImageUrl = $"{baseUrl}/{imagePath}";
                    }
                }
                await PopulateUserFavourites(productsDto.Data);
                return ServiceResult<PaginationDetails<ProductsDto>>.Success(productsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return ServiceResult<PaginationDetails<ProductsDto>>.Failure(
                    new ServerErrorException("Could not fetch products"));
            }
        }
        #endregion

        #region Read products from Database based on productID
        public async Task<ServiceResult<ProductsDto>> GetProductsBasedOnID(string productId)
        {
            try
            {
                var result = await _context.tbl_Products.Include(x => x.Tax).FirstOrDefaultAsync(x => x.Id == productId);
                if (result == null && (productId == "-100" || productId == "-101" || productId == "-102" || productId == "-103" || productId == "-104"))
                {
                    if (productId == "-100")
                    {

                        result = new()
                        {
                            ProductName = "Trial test item 1"
                        };
                    }
                    if (productId == "-101")
                    {

                        result = new()
                        {
                            ProductName = "Trial test item 2"
                        };
                    }
                    if (productId == "-102")
                    {

                        result = new()
                        {
                            ProductName = "Trial test item 3 with a long Product Name"
                        };
                    }
                    if (productId == "-103")
                    {

                        result = new()
                        {
                            ProductName = "Trial test item 4"
                        };
                    }
                    if (productId == "-104")
                    {

                        result = new()
                        {
                            ProductName = "Trial test item 5"
                        };
                    }
                }

                if (result == null)
                {
                    return ServiceResult<ProductsDto>.Failure(new NotFoundException($"Product with id {productId} not found"));
                }
                var productDto = result.Adapt<ProductsDto>();
                if (!string.IsNullOrEmpty(productDto.ProductImage))
                {
                    string baseUrl = $"{_contextAccessor.HttpContext?.Request.Scheme}://{_contextAccessor.HttpContext?.Request.Host}";
                    string imagePath = Path.Combine("images/products", productDto.ProductImage).Replace("\\", "/");
                    productDto.ProductImageUrl = $"{baseUrl}/{imagePath}";
                }
                await PopulateUserFavourites(new[] { productDto });
                return ServiceResult<ProductsDto>.Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching product by ID: {ProductId}", productId);
                return ServiceResult<ProductsDto>.Failure(new ServerErrorException("Could not fetch product."));
            }
        }
        #endregion

        #region Read products with Quantity based on productID
        public async Task<ServiceResult<List<ProductWithQtyDto>>> GetSubProductContentByParentID(string parentProductId)
        {
            if (string.IsNullOrEmpty(parentProductId))
                return ServiceResult<List<ProductWithQtyDto>>.Failure(
                    new BadRequestException("Parent Product ID is required"));

            try
            {
                var subProducts = await _context.tbl_ProductRelationships
                    .Where(x => x.HasAsubProductId == parentProductId)
                    .Join(
                        _context.tbl_Products,
                        relation => relation.IsAsubProductId,
                        product => product.Id,
                        (relation, product) => new ProductWithQtyDto
                        {
                            ProductId = relation.IsAsubProductId,
                            ProductName = product.ProductName,
                            CostInclusive = product.CostInclusive,
                            CostExclusive = product.CostExclusive,
                            PriceInclusive = product.PriceInclusive,
                            Qty = relation.Qty
                        })
                    .ToListAsync();

                if (!subProducts.Any())
                {
                    return ServiceResult<List<ProductWithQtyDto>>.Failure(
                        new NotFoundException($"No sub-products found for parent product ID: {parentProductId}"));
                }

                return ServiceResult<List<ProductWithQtyDto>>.Success(subProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching sub-products for parent product ID: {ParentProductId}", parentProductId);
                return ServiceResult<List<ProductWithQtyDto>>.Failure(
                    new ServerErrorException("Could not fetch sub-products."));
            }
        }
        #endregion

        #region Read products from Database based on productcode 
        public async Task<ServiceResult<ProductsDto>> GetProductsBasedOnProdCode(string productCode)
        {

            try
            {
                var result = await _context.tbl_Products.FirstOrDefaultAsync(x => x.ProductCode == productCode);

                if (result == null)
                {
                    return ServiceResult<ProductsDto>.Failure(new NotFoundException($"Product with code {productCode} not found"));
                }


                return ServiceResult<ProductsDto>.Success(result.Adapt<ProductsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching product by code: {ProductCode}", productCode);
                return ServiceResult<ProductsDto>.Failure(new ServerErrorException("Could not fetch product."));
            }



        }
        #endregion

        #region Read products from Database based on barcode 
        public async Task<ServiceResult<ProductsDto>> GetProductsBasedOnBarcode(string barCode)
        {
            try
            {
                var result = await _context.tbl_Products.FirstOrDefaultAsync(x => x.BarCode == barCode);

                if (result == null)
                {
                    return ServiceResult<ProductsDto>.Failure(new NotFoundException($"Product with barCode {barCode} not found"));
                }


                return ServiceResult<ProductsDto>.Success(result.Adapt<ProductsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching product by barcode: {BarCode}", barCode);
                return ServiceResult<ProductsDto>.Failure(new ServerErrorException("Could not fetch product."));
            }


        }
        public async Task<ServiceResult<MemoryStream>> GetProductsForCSVExportBasedOnSelectedFields(string segmentId, string categoryId, string supplierId, List<string> availableColumnNames)
        {
            try
            {
                IQueryable<tbl_Product> query = _context.tbl_Products;

                // Filter by segment, category, and supplier  
                if (!string.IsNullOrEmpty(categoryId))
                    query = query.Where(x => x.CategoryId == categoryId);
                if (!string.IsNullOrEmpty(segmentId))
                    query = query.Where(x => x.SegmentId == segmentId);
                if (!string.IsNullOrEmpty(supplierId))
                    query = query.Where(x => x.SupplierId == supplierId);

                // Fetch related data for names  
                var productsWithDetails = await query
                    .Select(x => new
                    {
                        x.Id,
                        x.ProductCode,
                        x.ProductName,
                        x.BarCode,
                        x.PriceExclusive,
                        x.PriceInclusive,
                        x.InStock,
                        x.ReOrderLevel,
                        x.ReOrderQty,
                        x.TrackInventory,
                        x.CompoundCostPricing,
                        x.CostExclusive,
                        x.CostInclusive,
                        x.CostIncStatus,
                        x.HasSubProduct,
                        x.IsAsubProduct,
                        x.Location,
                        x.ProductImage,
                        x.TaxId,
                        x.CategoryId,
                        x.SegmentId,
                        x.SupplierId,
                        TaxName = x.TaxId != null ? _context.tbl_Taxes.FirstOrDefault(c => c.Id == x.TaxId).TaxDescription : null,
                        CategoryName = x.CategoryId != null ? _context.tbl_Categories.FirstOrDefault(c => c.Id == x.CategoryId).Category : null,
                        SegmentName = x.SegmentId != null ? _context.tbl_Segments.FirstOrDefault(s => s.Id == x.SegmentId).Segment : null,
                        SupplierName = x.SupplierId != null ? _context.tbl_Suppliers.FirstOrDefault(s => s.Id == x.SupplierId).FullName : null
                    })
                    .ToListAsync();

                // Map to export DTO and filter selected fields  
                var exportObject = productsWithDetails.Select(product =>
                {
                    var exportDto = new ProductsExportDto();
                    foreach (var field in availableColumnNames)
                    {
                        switch (field)
                        {
                            case "ProductCode":
                                exportDto.ProductCode = product.ProductCode;
                                break;
                            case "ProductName":
                                exportDto.ProductName = product.ProductName;
                                break;
                            case "BarCode":
                                exportDto.BarCode = product.BarCode;
                                break;
                            case "PriceExclusive":
                                exportDto.PriceExclusive = product.PriceExclusive;
                                break;
                            case "PriceInclusive":
                                exportDto.PriceInclusive = product.PriceInclusive;
                                break;
                            case "InStock":
                                exportDto.InStock = product.InStock;
                                break;
                            case "ReOrderLevel":
                                exportDto.ReOrderLevel = product.ReOrderLevel;
                                break;
                            case "ReOrderQty":
                                exportDto.ReOrderQty = product.ReOrderQty;
                                break;
                            case "TrackInventory":
                                exportDto.TrackInventory = product.TrackInventory;
                                break;
                            case "CompoundCostPricing":
                                exportDto.CompoundCostPricing = product.CompoundCostPricing;
                                break;
                            case "CostExclusive":
                                exportDto.CostExclusive = product.CostExclusive;
                                break;
                            case "CostInclusive":
                                exportDto.CostInclusive = product.CostInclusive;
                                break;

                                break;

                            case "HasSubProduct":
                                exportDto.HasSubProduct = product.HasSubProduct;
                                break;
                            //case "IsAsubProduct":
                            //    exportDto.IsAsubProduct = product.IsAsubProduct;
                            //    break;
                            case "Location":
                                exportDto.Location = product.Location;
                                break;
                            //case "ProductImage":
                            //    exportDto.ProductImage = product.ProductImage;
                            //    break;
                            case "TaxName":
                                exportDto.TaxName = product.TaxName;
                                break;
                            case "CategoryName":
                                exportDto.CategoryName = product.CategoryName;
                                break;
                            case "SegmentName":
                                exportDto.SegmentName = product.SegmentName;
                                break;
                            case "SupplierName":
                                exportDto.SupplierName = product.SupplierName;
                                break;
                        }
                    }
                    return exportDto;
                }).ToList();

                // Create Excel file and return it  
                var memoryStream = await _excelDomainService.ExportExcelRecords(exportObject, availableColumnNames, "Products");
                return ServiceResult<MemoryStream>.Success(memoryStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting products.");
                return ServiceResult<MemoryStream>.Failure(new ServerErrorException("Could not export products."));
            }
        }
        #endregion

        #region Add Product to DB

        public async Task<ServiceResult<ProductsDto>> AddProduct([Required] ProductCreateDto p)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var calculationsDto = p.Adapt<ProductPricing>();
                        if (!string.IsNullOrEmpty(p.TaxId))
                        {
                            var taxResult = await _taxDAL.GetTaxFromDBbasedOnTaxID(p.TaxId);
                            if (taxResult.IsSuccess)
                            {
                                calculationsDto.Tax = taxResult.Data;
                            }
                            else
                            {
                                return ServiceResult<ProductsDto>.Failure(taxResult.Error);
                            }
                        }

                        var resultCheck = _pricingCalc.ProductCalculationChecks(calculationsDto, false, true);

                        if (!resultCheck.IsSuccess)
                            return ServiceResult<ProductsDto>.Failure(resultCheck.Error);

                        var productDto = resultCheck.Data.Adapt(p);

                        string saveLocation = "images/products";
                        string defaultImage = "NoImagePlaceholder.jpg";

                        if (!string.IsNullOrEmpty(productDto.Base64Image))
                        {
                            var bytes = Convert.FromBase64String(productDto.Base64Image);
                            var uploadedResult = await _fileUpload.HandleByteArrayUploadAsync(bytes, productDto.ProductImageName ?? "my-image.jpg", "image/jpeg", saveLocation);

                            if (!uploadedResult.IsSuccess)
                                return ServiceResult<ProductsDto>.Failure(uploadedResult.Error);

                            var createdImage = uploadedResult.Data;
                            productDto.ProductImage = createdImage.ImageUniqueName;
                        }
                        else
                        {
                            productDto.ProductImage = defaultImage;
                        }

                        var product = productDto.Adapt<tbl_Product>();
                        product.Tax = null;

                        product.CreatedBy = _tenantProvider.GetUserId();
                        await _context.AddAsync(product);
                        await _context.SaveChangesAsync();

                        if (p.ProductRelationships != null && p.ProductRelationships.Count > 0)
                        {
                            p.ProductRelationships.ForEach(r => r.HasAsubProductId = product.Id);
                            var relationResult = await _relationshipsDAL.AddProductRelationships(p.ProductRelationships);
                            if (!relationResult.IsSuccess)
                            {
                                return ServiceResult<ProductsDto>.Failure(relationResult.Error);
                            }
                        }

                        var productResult = product.Adapt<ProductsDto>();

                        await transaction.CommitAsync();
                        return ServiceResult<ProductsDto>.Success(productResult);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
                        {
                            string ErrorMessage = "The Product you are trying to create already exists in this system. Please choose another name";
                            return ServiceResult<ProductsDto>.Failure(new ServerErrorException(ErrorMessage));
                        }
                        _logger.LogError(ex, "Error while creating product.");
                        return ServiceResult<ProductsDto>.Failure(new ServerErrorException("Could not create product."));
                    }
                }
            });
        }
        #endregion

        #region ProcessProducts fr

        public async Task<ServiceResult<List<Dictionary<string, object>>>> ProcessExcelFile(ProductMedia p)
        {
            if (p == null)
                return ServiceResult<List<Dictionary<string, object>>>.Failure(
                    new BadRequestException($"Product Data is required and cannot be null "));

            try
            {
                if (p.ProductFile != null)
                {
                    var uploadedResult = await _fileUpload.HandleExcelUploadAsync(p.ProductFile);

                    return uploadedResult;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing product Excel file.");
            }
            return ServiceResult<List<Dictionary<string, object>>>.Failure(new ServerErrorException("Server error while processing Excel file."));

        }
        #endregion

        #region update Product in the  DB
        public async Task<ServiceResult<ProductsDto>> UpdateProduct(ProductsDto p)
        {
            if (p == null)
                return ServiceResult<ProductsDto>.Failure(new BadRequestException($"Product Data is required and cannot be null "));

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        try
                        {
                            var calculationsDto = p.Adapt<ProductPricing>();
                            if (!string.IsNullOrEmpty(p.TaxId))
                            {
                                var taxResult = await _taxDAL.GetTaxFromDBbasedOnTaxID(p.TaxId);
                                if (taxResult.IsSuccess)
                                {
                                    calculationsDto.Tax = taxResult.Data;
                                }
                                else
                                {
                                    return ServiceResult<ProductsDto>.Failure(taxResult.Error);
                                }
                            }
                            var resultCheck = _pricingCalc.ProductCalculationChecks(calculationsDto, false, true);

                            if (!resultCheck.IsSuccess)
                                return ServiceResult<ProductsDto>.Failure(resultCheck.Error);

                            var objFromDb = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == p.Id);
                            if (objFromDb == null) return ServiceResult<ProductsDto>.Failure(new NotFoundException($"Product with Id of {p.Id} not found"));

                            objFromDb.BarCode = p.BarCode ?? objFromDb.BarCode;
                            objFromDb.Description = p.Description ?? objFromDb.Description;
                            objFromDb.CategoryId = p.CategoryId ?? objFromDb.CategoryId;
                            objFromDb.CompoundCostPricing = p.CompoundCostPricing ?? objFromDb.CompoundCostPricing;
                            objFromDb.CostExclusive = p.CostExclusive ?? objFromDb.CostExclusive;
                            objFromDb.CostInclusive = p.CostInclusive ?? objFromDb.CostInclusive;
                            objFromDb.CostIncStatus = p.CostIncStatus ?? objFromDb.CostIncStatus;
                            objFromDb.ProductName = p.ProductName ?? objFromDb.ProductName;
                            objFromDb.HasSubProduct = p.HasSubProduct ?? objFromDb.HasSubProduct;
                            objFromDb.IsAsubProduct = p.IsAsubProduct ?? objFromDb.IsAsubProduct;
                            objFromDb.ProductImage = p.ProductImage ?? objFromDb.ProductImage;
                            objFromDb.ProductCode = p.ProductCode ?? objFromDb.ProductCode;
                            objFromDb.InStock = p.InStock ?? objFromDb.InStock;
                            objFromDb.Location = p.Location ?? objFromDb.Location;
                            objFromDb.PriceExclusive = p.PriceExclusive ?? objFromDb.PriceExclusive;
                            objFromDb.PriceExclusive2 = p.PriceExclusive2 ?? objFromDb.PriceExclusive2;
                            objFromDb.PriceInclusive = p.PriceInclusive ?? objFromDb.PriceInclusive;
                            objFromDb.PriceInclusive2 = p.PriceInclusive2 ?? objFromDb.PriceInclusive2;
                            objFromDb.ReOrderLevel = p.ReOrderLevel ?? objFromDb.ReOrderLevel;
                            objFromDb.ReOrderQty = p.ReOrderQty ?? objFromDb.ReOrderQty;
                            objFromDb.SupplierId = p.SupplierId ?? objFromDb.SupplierId;
                            objFromDb.TaxId = p.TaxId ?? objFromDb.TaxId;
                            objFromDb.TrackInventory = p.TrackInventory ?? objFromDb.TrackInventory;
                            objFromDb.SegmentId = p.SegmentId ?? objFromDb.SegmentId;
                            objFromDb.AccessLevel = p.AccessLevel ?? objFromDb.AccessLevel;

                            string saveLocation = "images/products";

                            if (!string.IsNullOrEmpty(p.Base64Image))
                            {
                                var bytes = Convert.FromBase64String(p.Base64Image);
                                var uploadedResult = await _fileUpload.HandleByteArrayUploadAsync(bytes, p.ProductImageName ?? "my-image.jpg", "image/jpeg", saveLocation);

                                if (!uploadedResult.IsSuccess)
                                    return ServiceResult<ProductsDto>.Failure(uploadedResult.Error);

                                var createdImage = uploadedResult.Data;
                                objFromDb.ProductImage = createdImage.ImageUniqueName;
                            }
                            else
                            {
                                objFromDb.ProductImage = objFromDb.ProductImage;
                            }


                            if (p.ProductRelationships != null && p.ProductRelationships.Count > 0)
                            {
                                p.ProductRelationships.ForEach(r => r.HasAsubProductId = objFromDb.Id);
                                var relationResult = await _relationshipsDAL.CreateUpdateRelationshipsByParentId(objFromDb.Id, p.ProductRelationships);
                                if (!relationResult.IsSuccess)
                                {
                                    return ServiceResult<ProductsDto>.Failure(relationResult.Error);
                                }
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return ServiceResult<ProductsDto>.Success(objFromDb.Adapt<ProductsDto>());
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Error while updating product with ID: {ProductId}", p.Id);
                            return ServiceResult<ProductsDto>.Failure(new ServerErrorException("Could not update product."));
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while updating product with ID: {ProductId}", p.Id);
                        return ServiceResult<ProductsDto>.Failure(new ServerErrorException("Could not update product."));
                    }
                }

            });
        }
        #endregion

        #region update Product Stock in the  DB
        public async Task<ServiceResult<bool>> UpdateProductStock(string productId, decimal inStock)
        {
            try
            {
                var objFromDb = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == productId);
                if (objFromDb == null) return ServiceResult<bool>.Failure(new NotFoundException($"Product with Id of {productId} not found"));
                objFromDb.InStock = inStock;
                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product stock for product ID: {ProductId}", productId);
                return ServiceResult<bool>.Failure(new ServerErrorException("Could not update product stock."));
            }
        }
        #endregion

        #region update Product UpdateProductStockList
        public async Task<ServiceResult<bool>> UpdateProductStockList(List<StockParam> stockParamsList)
        {
            if (stockParamsList == null || !stockParamsList.Any())
            {
                return ServiceResult<bool>.Failure(new BadRequestException("Stock parameters list cannot be null or empty."));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var stockParams in stockParamsList)
                    {
                        var product = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == stockParams.ProductId);
                        if (product == null)
                        {
                            return ServiceResult<bool>.Failure(new NotFoundException($"Product with Id {stockParams.ProductId} not found."));
                        }

                        product.InStock = stockParams.InStockAmount;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ServiceResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error while updating product stocks.");
                    return ServiceResult<bool>.Failure(new ServerErrorException("Could not update product stocks."));
                }
            }
        }

        #endregion

        #region update Product Stock  From Product Receiving
        public async Task<ServiceResult<bool>> UpdateStockFromProductReceiving(List<StockParam> stockParamsList)
        {
            if (stockParamsList == null || !stockParamsList.Any())
            {
                return ServiceResult<bool>.Failure(new BadRequestException("Stock parameters list cannot be null or empty."));
            }

            try
            {
                foreach (var stockParams in stockParamsList)
                {
                    var product = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == stockParams.ProductId);
                    if (product == null)
                    {
                        return ServiceResult<bool>.Failure(new NotFoundException($"Product with Id {stockParams.ProductId} not found."));
                    }

                    product.InStock = stockParams.InStockAmount;
                }

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product stocks from product receiving.");
                return ServiceResult<bool>.Failure(new ServerErrorException("Could not update product stocks."));
            }
        }

        #endregion

        #region update Product Barcode in the  DB
        public async Task<ServiceResult<List<ProductBarcodeDto>>> UpdateProductBarcodes(List<ProductBarcodeDto> barcodes)
        {
            if (barcodes == null || !barcodes.Any())
            {
                return ServiceResult<List<ProductBarcodeDto>>.Failure(new BadRequestException("Barcode list cannot be null or empty."));
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var barcodeDto in barcodes)
                        {
                            var product = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == barcodeDto.ProductId);
                            if (product == null)
                            {
                                return ServiceResult<List<ProductBarcodeDto>>.Failure(new NotFoundException($"Product with Id {barcodeDto.ProductId} not found."));
                            }

                            product.BarCode = barcodeDto.Barcode;
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return ServiceResult<List<ProductBarcodeDto>>.Success(barcodes);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error while updating product barcodes.");
                        return ServiceResult<List<ProductBarcodeDto>>.Failure(new ServerErrorException("Could not update product barcodes."));
                    }
                }
            });
        }

        #endregion

        #region update Product costPrices in the  DB
        public async Task<ServiceResult<ProductsDto>> UpdateProductPrices(string productId, ProductPricing prices)
        {
            try
            {
                if (prices == null)
                    return ServiceResult<ProductsDto>.Failure(new BadRequestException($"Product Pricing Data is required and cannot be null "));
                if (!string.IsNullOrEmpty(prices.TaxId))
                {
                    var taxResult = await _taxDAL.GetTaxFromDBbasedOnTaxID(prices.TaxId);
                    if (taxResult.IsSuccess)
                    {
                        prices.Tax = taxResult.Data;
                    }
                    else
                    {
                        return ServiceResult<ProductsDto>.Failure(taxResult.Error);
                    }
                }

                var resultCheck = _pricingCalc.ProductCalculationChecks(prices, false, true);
                if (!resultCheck.IsSuccess)
                    return ServiceResult<ProductsDto>.Failure(resultCheck.Error);

                var objFromDb = await _context.tbl_Products
                    .FirstOrDefaultAsync(x => x.Id == productId);

                if (objFromDb == null) return ServiceResult<ProductsDto>.Failure(
                    new NotFoundException($"Product with Id of {productId} not found"));

                objFromDb.CostInclusive = prices.CostInclusive;
                objFromDb.CostExclusive = prices.CostExclusive;
                objFromDb.PriceExclusive = prices.PriceExclusive;
                objFromDb.PriceInclusive = prices.PriceInclusive;
                objFromDb.PriceExclusive2 = prices.PriceExclusive2;
                objFromDb.PriceInclusive2 = prices.PriceInclusive2;

                await _context.SaveChangesAsync();

                return ServiceResult<ProductsDto>.Success(objFromDb.Adapt<ProductsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product prices for product ID: {ProductId}", productId);
                return ServiceResult<ProductsDto>.Failure(new ServerErrorException("Could not update product prices."));
            }

        }
        #endregion

        #region update Product costPrices in the  DB
        public async Task<ServiceResult<bool>> UpdateProductCostPrices(List<CostPriceChange> costChange)
        {
            if (costChange == null || !costChange.Any())
            {
                return ServiceResult<bool>.Failure(new BadRequestException("Cost price change list cannot be null or empty."));
            }
            try
            {
                foreach (var cost in costChange)
                {
                    var product = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == cost.productId);
                    if (product == null)
                    {
                        return ServiceResult<bool>.Failure(new NotFoundException($"Product with Id {cost.productId} not found."));
                    }
                    product.CostExclusive = cost.costExc;
                    product.CostInclusive = cost.costInc;
                }
                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product cost prices.");
                return ServiceResult<bool>.Failure(new ServerErrorException("Could not update product cost prices."));

            }

        }
        #endregion

        #region update Product onImport in the  DB
        public async Task<ServiceResult<ProductsDto>> UpdateProductOnImportUsingProductCode(ProductsDto p)
        {
            if (p == null)
                return ServiceResult<ProductsDto>.Failure(new BadRequestException($"Product Data is required and cannot be null "));

            var objFromDb = await _context.tbl_Products.AsNoTracking().FirstOrDefaultAsync(x => x.ProductCode == p.ProductCode);
            if (objFromDb == null) return ServiceResult<ProductsDto>.Failure(new NotFoundException($"Product with code of {p.ProductCode} not found"));

            p.Id = objFromDb.Id;
            return await UpdateProduct(p);

        }
        #endregion

        #region update Product onImport in the DB
        public async Task<ServiceResult<ProductImportResultSummaryDto>> UpdateProductOnImportFromExcel(ProductImportDataFinalDto p)
        {
            if (p == null)
                return ServiceResult<ProductImportResultSummaryDto>.Failure(new BadRequestException($"Product Data is required and cannot be null"));

            // Counters for summary
            int totalProducts = 0;
            int updatedCount = 0;
            int createdCount = 0;
            int failedCount = 0;
            List<string> messages = new List<string>();

            foreach (var prodInList in p.UploadedExcelContent)
            {
                totalProducts++;
                try
                {
                    tbl_Product productEntity = null;
                    bool isUpdate = false;

                    // Check if product ID is provided via mapping.
                    var productIdColumnName = p.ColumnMappingsList
                                .FirstOrDefault(x => x.SystemColumn.Equals(nameof(ProductsDto.Id), StringComparison.OrdinalIgnoreCase))
                                ?.SelectedFileColumn;

                    string prodIdStr = !string.IsNullOrEmpty(productIdColumnName)
                                ? GetValue(prodInList, productIdColumnName)?.ToString()
                                : "";



                    if (!string.IsNullOrEmpty(prodIdStr))
                    {
                        string prodId = prodIdStr;
                        // If product ID is provided, try to locate the product in the DB.
                        productEntity = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == prodId);
                        if (productEntity == null)
                        {
                            // Use helper to build a description of the product.
                            string productDescription = BuildProductDescription(prodInList, p);
                            messages.Add($"&#x1F4CC; {productDescription} could not be updated due to invalid product ID.");
                            failedCount++;
                            continue; // Skip this record.
                        }
                        isUpdate = true;
                    }
                    else
                    {
                        // No valid product ID provided.
                        // Try to identify product by barcode and/or product code.
                        string barcode = GetValue(prodInList, GetKey("barcode", p.ColumnMappingsList))?.ToString();
                        string productCode = GetValue(prodInList, GetKey("productcode", p.ColumnMappingsList))?.ToString();

                        // Build the query based on available identifiers.
                        var query = _context.tbl_Products.AsQueryable();
                        bool hasIdentifier = false;
                        if (!string.IsNullOrEmpty(productCode))
                        {
                            query = query.Where(x => x.ProductCode == productCode);
                            hasIdentifier = true;
                        }
                        if (!string.IsNullOrEmpty(barcode))
                        {
                            query = query.Where(x => x.BarCode == barcode);
                            hasIdentifier = true;
                        }

                        if (!hasIdentifier)
                        {
                            string productDescription = BuildProductDescription(prodInList, p);
                            messages.Add($"&#x1F4CC; {productDescription} could not be processed due to missing product ID, barcode, and product code.");
                            failedCount++;
                            continue;
                        }

                        var matchedProducts = await query.ToListAsync();
                        if (matchedProducts.Count == 1)
                        {
                            productEntity = matchedProducts.First();
                            isUpdate = true;
                        }
                        else if (matchedProducts.Count > 1)
                        {
                            string reason = !string.IsNullOrEmpty(productCode)
                                ? $"non unique product code '{productCode}'"
                                : $"non unique barcode '{barcode}'";
                            string productDescription = BuildProductDescription(prodInList, p);
                            messages.Add($"&#x1F4CC; {productDescription} update not performed because multiple products were found with {reason}.");
                            failedCount++;
                            continue;
                        }
                        else
                        {
                            // No match found; proceed to create a new product.
                            productEntity = new tbl_Product();
                            _context.tbl_Products.Add(productEntity);
                            isUpdate = false;
                        }
                    }


                    //taxes
                    var taxKey = GetKey("TaxName", p.ColumnMappingsList);
                    //var taxValueKey = GetKey("TaxPercent", p.ColumnMappingsList);
                    string taxStr = !string.IsNullOrEmpty(taxKey) ? GetValue(prodInList, taxKey)?.ToString()! : string.Empty;
                    //string taxValueStr = !string.IsNullOrEmpty(taxValueKey) ? GetValue(prodInList, taxValueKey)?.ToString()! : string.Empty;
                    var tax = _context.tbl_Taxes.FirstOrDefault(x => x.TaxDescription == taxStr);

                    if (tax == null && !isUpdate)
                    {
                        string productDescription = BuildProductDescription(prodInList, p);
                        messages.Add($"&#x1F4CC; {productDescription} could not be processed due to Invalid Tax scheme.");
                        failedCount++;
                        continue;
                    }


                    //categories
                    var catKey = GetKey("CategoryName", p.ColumnMappingsList);
                    string catStr = !string.IsNullOrEmpty(catKey) ? GetValue(prodInList, catKey)?.ToString()! : string.Empty;
                    var category = _context.tbl_Categories.FirstOrDefault(x => x.Category == catStr);

                    if (category == null && !isUpdate)
                    {
                        string productDescription = BuildProductDescription(prodInList, p);
                        messages.Add($"&#x1F4CC; {productDescription} could not be processed due to Invalid Category Name.");
                        failedCount++;
                        continue;
                    }
                    //segment
                    var segmentKey = GetKey("SegmentName", p.ColumnMappingsList);
                    string segmentStr = !string.IsNullOrEmpty(segmentKey) ? GetValue(prodInList, segmentKey)?.ToString()! : string.Empty;
                    var segment = _context.tbl_Segments.FirstOrDefault(x => x.Segment == segmentStr);

                    if (segment == null && !isUpdate)
                    {
                        string productDescription = BuildProductDescription(prodInList, p);
                        messages.Add($"&#x1F4CC; {productDescription} could not be processed due to Invalid Segment Name.");
                        failedCount++;
                        continue;
                    }
                    //supplier
                    var supplierKey = GetKey("SupplierName", p.ColumnMappingsList);
                    string supplierStr = !string.IsNullOrEmpty(supplierKey) ? GetValue(prodInList, supplierKey)?.ToString()! : string.Empty;
                    var supplier = _context.tbl_Suppliers.FirstOrDefault(x => x.FullName == supplierStr);

                    if (supplier == null && !isUpdate)
                    {
                        string productDescription = BuildProductDescription(prodInList, p);
                        messages.Add($"&#x1F4CC; {productDescription} could not be processed due to Invalid Supplier Name.");
                        failedCount++;
                        continue;
                    }


                    // Map properties dynamically based on ColumnMappingsList.
                    foreach (var mapping in p.ColumnMappingsList)
                    {
                        var columnName = mapping.SystemColumn.ToLower();
                        var fileColumn = mapping.SelectedFileColumn;
                        if (string.IsNullOrEmpty(fileColumn))
                            continue;
                        var value = GetValue(prodInList, fileColumn);

                        switch (columnName)
                        {

                            case "barcode":
                                productEntity.BarCode = value.ToString();
                                break;
                            case "categoryname":
                                if (!string.IsNullOrEmpty(value?.ToString()))
                                    productEntity.CategoryId = category.Id;
                                break;
                            case "compoundcostpricing":
                                if (int.TryParse(value?.ToString(), out var compoundCost))
                                    productEntity.CompoundCostPricing = compoundCost;
                                break;
                            case "costexclusive":
                                if (decimal.TryParse(value?.ToString(), out var costExclusive))
                                    productEntity.CostExclusive = costExclusive;
                                break;
                            case "costinclusive":
                                if (decimal.TryParse(value?.ToString(), out var costInclusive))
                                    productEntity.CostInclusive = costInclusive;
                                break;
                            case "costincstatus":
                                if (bool.TryParse(value?.ToString(), out var costIncStatus))
                                    productEntity.CostIncStatus = costIncStatus;
                                break;

                            case "productname":
                                productEntity.ProductName = value.ToString();
                                break;
                            case "hassubproduct":
                                if (bool.TryParse(value?.ToString(), out var hasSubProduct))
                                    productEntity.HasSubProduct = hasSubProduct;
                                break;
                            case "isasubproduct":
                                if (!string.IsNullOrEmpty(value?.ToString()))
                                    productEntity.IsAsubProduct = value?.ToString();
                                break;
                            case "productimage":
                                productEntity.ProductImage = value.ToString();
                                break;
                            case "productcode":
                                productEntity.ProductCode = value.ToString();
                                break;
                            case "instock":
                                if (decimal.TryParse(value?.ToString(), out var inStock))
                                    productEntity.InStock = inStock;
                                break;
                            case "location":
                                productEntity.Location = value.ToString();
                                break;
                            case "priceexclusive":
                                if (decimal.TryParse(value?.ToString(), out var priceExclusive))
                                    productEntity.PriceExclusive = priceExclusive;
                                break;
                            case "priceexclusive2":
                                if (decimal.TryParse(value?.ToString(), out var priceExclusive2))
                                    productEntity.PriceExclusive2 = priceExclusive2;
                                break;
                            case "priceinclusive":
                                if (decimal.TryParse(value?.ToString(), out var priceInclusive))
                                    productEntity.PriceInclusive = priceInclusive;
                                break;
                            case "priceinclusive2":
                                if (decimal.TryParse(value?.ToString(), out var priceInclusive2))
                                    productEntity.PriceInclusive2 = priceInclusive2;
                                break;
                            case "reorderlevel":
                                if (decimal.TryParse(value?.ToString(), out var reOrderLevel))
                                    productEntity.ReOrderLevel = reOrderLevel;
                                break;
                            case "reorderqty":
                                if (decimal.TryParse(value?.ToString(), out var reOrderQty))
                                    productEntity.ReOrderQty = reOrderQty;
                                break;
                            case "suppliername":
                                if (!string.IsNullOrEmpty(value?.ToString()))
                                    productEntity.SupplierId = supplier.Id;
                                break;
                            case "taxname":
                                if (!string.IsNullOrEmpty(value?.ToString()))
                                    productEntity.TaxId = tax.Id;
                                break;
                            case "trackinventory":
                                if (bool.TryParse(value?.ToString(), out var trackInventory))
                                    productEntity.TrackInventory = trackInventory;
                                break;
                            case "segmentname":
                                if (!string.IsNullOrEmpty(value?.ToString()))
                                    productEntity.SegmentId = segment.Id;
                                break;
                        }



                    }

                    if (isUpdate)
                    {
                        await _context.SaveChangesAsync();
                        updatedCount++;

                    }
                    else
                    {
                        productEntity.CreatedBy = _tenantProvider.GetUserId();
                        productEntity.Tax = tax;
                        var calculationsDto = productEntity.Adapt<ProductPricing>();

                        var resultCheck = _pricingCalc.ProductCalculationChecks(calculationsDto, false, true);

                        if (!resultCheck.IsSuccess)
                        {
                            failedCount++;
                            messages.Add($"&#x1F4CC; Product with Barcode: {productEntity.BarCode},Code: {productEntity.ProductCode},  name: '{productEntity.ProductName}' could not be added due to {resultCheck.Error.Message}.");

                        }
                        else
                        {
                            resultCheck.Data.Adapt(productEntity);
                            await _context.SaveChangesAsync();
                            createdCount++;

                        }

                    }


                }
                catch (Exception ex)
                {
                    string productDescription = BuildProductDescription(prodInList, p);
                    messages.Add($"&#x1F4CC; {productDescription} could not be imported due to {ex.Message}");
                    failedCount++;
                }
            }

            // Build summary message.
            string summary = $"Total Products Processed: {totalProducts}\nCreated: {createdCount}\nUpdated: {updatedCount}\nFailed: {failedCount}";



            // Format result message: each message on a new line.
            string resultMessage = string.Join("\n", messages);
            var output = new ProductImportResultSummaryDto
            {
                Summary = summary,
                Errors = resultMessage
            };
            return ServiceResult<ProductImportResultSummaryDto>.Success(output);
        }
        #endregion

        #region update Product onImport in the  DB
        public async Task<ServiceResult<ProductsDto>> UpdateProductOnImportUsingBarCode(ProductsDto p)
        {

            if (p == null)
                return ServiceResult<ProductsDto>.Failure(new BadRequestException($"Product Data is required and cannot be null "));

            var objFromDb = await _context.tbl_Products.AsNoTracking().FirstOrDefaultAsync(x => x.BarCode == p.BarCode);
            if (objFromDb == null) return ServiceResult<ProductsDto>.Failure(new NotFoundException($"Product with Barcode of {p.BarCode} not found"));

            p.Id = objFromDb.Id;
            return await UpdateProduct(p);


        }

        private string BuildProductDescription(Dictionary<string, object> prod, ProductImportDataFinalDto p)
        {
            List<string> parts = new List<string>();

            var productIdKey = GetKey("productid", p.ColumnMappingsList);
            var barcodeKey = GetKey("barcode", p.ColumnMappingsList);
            var productCodeKey = GetKey("productcode", p.ColumnMappingsList);
            var productNameKey = GetKey("productname", p.ColumnMappingsList);

            var productIdVal = !string.IsNullOrEmpty(productIdKey) ? GetValue(prod, productIdKey)?.ToString() : "";
            var barcodeVal = !string.IsNullOrEmpty(barcodeKey) ? GetValue(prod, barcodeKey)?.ToString() : "";
            var productCodeVal = !string.IsNullOrEmpty(productCodeKey) ? GetValue(prod, productCodeKey)?.ToString() : "";
            var productNameVal = !string.IsNullOrEmpty(productNameKey) ? GetValue(prod, productNameKey)?.ToString() : "";

            if (!string.IsNullOrEmpty(productIdVal))
                parts.Add($"ID: {productIdVal}");
            if (!string.IsNullOrEmpty(barcodeVal))
                parts.Add($"Barcode: {barcodeVal}");
            if (!string.IsNullOrEmpty(productCodeVal))
                parts.Add($"Product Code: {productCodeVal}");
            if (!string.IsNullOrEmpty(productNameVal))
                parts.Add($"Name: {productNameVal}");

            return "Product [" + string.Join(", ", parts) + "]";
        }
        private object GetValue(Dictionary<string, object> item, string key)
        {
            return item.TryGetValue(key, out object? value) ? (value == null ? "" : value) : "";
        }
        private string GetKey(string columnName, List<ColumnMapping> mappings)
        {
            return mappings.FirstOrDefault(x => x.SystemColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))?.SelectedFileColumn;
        }
        #endregion

        #region Delete Product softdelete
        public async Task<ServiceResult<bool>> DeleteProducts(string productId)
        {
            try
            {
                var objFromDb = await _context.tbl_Products.FirstOrDefaultAsync(x => x.Id == productId);
                if (objFromDb == null) return ServiceResult<bool>.Failure(new NotFoundException($"Product with id {productId}  was not found"));

                objFromDb.IsDeleted = true;
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting product with ID: {ProductId}", productId);
                return ServiceResult<bool>.Failure(new ServerErrorException("Could not delete product."));
            }
        }
        #endregion

        #region Search products
        public async Task<ServiceResult<PaginationDetails<ProductsDto>>> SearchProducts(string keywords, string categoryId, string segmentId, string supplierID, bool? hasSubProduct, decimal inStock, int offSet, int limit, CancellationToken cancellationToken, string? sortByColumn, bool sortAscending)
        {

            IQueryable<tbl_Product> query = _context.tbl_Products;
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.
                             Where(x => x.Id.ToString() == keywords ||
                             x.ProductCode != null && x.ProductCode.Contains(keywords) ||
                             x.BarCode != null && x.BarCode.Contains(keywords) ||
                             x.ProductName != null && x.ProductName.Contains(keywords)
                             );
                }

                if (!string.IsNullOrEmpty(categoryId))
                {

                    query = query.Where(x => x.CategoryId == categoryId);

                }

                if (!string.IsNullOrEmpty(segmentId))
                {

                    query = query.Where(x => x.SegmentId == segmentId);
                }
                if (!string.IsNullOrEmpty(supplierID))
                {

                    query = query.Where(x => x.SupplierId == supplierID);
                }
                if (hasSubProduct != null && hasSubProduct == true)
                {
                    query = query.Where(x => x.HasSubProduct == true);
                }
                //if (inStock > 0)
                //{
                //    query = query.Where(x => x.InStock >= inStock);
                //}


                var result = await query.AsNoTracking().Include(x => x.Tax)
                    .OrderBy(x => x.ProductName)
                    .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                return ServiceResult<PaginationDetails<ProductsDto>>.Success(result.Adapt<PaginationDetails<ProductsDto>>());

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching Products: {ex}", ex);
                return ServiceResult<PaginationDetails<ProductsDto>>.Failure(new ServerErrorException(ex.Message));
            }


        }
        #endregion

        #region Search products for combo boxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchProductsForComboBoxes(string keywords, string categoryId, string segmentId, string supplierID, bool hasSubProduct, decimal inStock, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {

            IQueryable<tbl_Product> query = _context.tbl_Products;
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.
                             Where(x => x.Id.ToString() == keywords ||
                             x.ProductCode != null && x.ProductCode.Contains(keywords) ||
                             x.BarCode != null && x.BarCode.Contains(keywords) ||
                             x.ProductName != null && x.ProductName.Contains(keywords)
                             );
                }
                if (!string.IsNullOrEmpty(categoryId))
                {

                    query = query.Where(x => x.CategoryId == categoryId);

                }

                if (!string.IsNullOrEmpty(segmentId))
                {

                    query = query.Where(x => x.SegmentId == segmentId);
                }
                if (!string.IsNullOrEmpty(supplierID))
                {
                    query = query.Where(x => x.SupplierId == supplierID);
                }
                if (hasSubProduct)
                {
                    query = query.Where(x => x.HasSubProduct == true);
                }
                if (inStock > 0)
                {
                    query = query.Where(x => x.InStock >= inStock);//value in db should be more than 0
                }

                query = ExcludeHiddenProducts(query);

                var result = await query.AsNoTracking().Include(x => x.Tax).Select(x => new ComboBoxDto
                {
                    Id = x.Id,
                    IdString = x.Id.ToString(),
                    ValueText = $"{x.ProductName} ({(x.PriceExclusive ?? 0.00M).ToString("N2")})"
                }).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(result);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching Products: {ex}", ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(new ServerErrorException(ex.Message));
            }
        }
        #endregion

        #region Search products top 15
        public async Task<ServiceResult<List<ProductsDto>>> SearchTOP15Products(string keywords, string categoryId, string segmentId, string supplierID)
        {
            IQueryable<tbl_Product> query = _context.tbl_Products;

            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {

                    query = query.
                             Where(x => x.Id.ToString() == keywords ||
                             x.ProductCode.Contains(keywords) ||
                             x.BarCode.Contains(keywords) ||
                             x.ProductName.Contains(keywords)
                             );
                }
                if (!string.IsNullOrEmpty(categoryId))
                {

                    query = query.Where(x => x.CategoryId == categoryId);

                }

                if (!string.IsNullOrEmpty(segmentId))
                {

                    query = query.Where(x => x.SegmentId == segmentId);
                }
                if (!string.IsNullOrEmpty(supplierID))
                {

                    query = query.Where(x => x.SupplierId == supplierID);
                }

                query = ExcludeHiddenProducts(query);

                var result = await query.AsNoTracking().Take(30).ToListAsync();
                return ServiceResult<List<ProductsDto>>.Success(result.Adapt<List<ProductsDto>>());

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching Products: {ex}", ex);
                return ServiceResult<List<ProductsDto>>.Failure(new ServerErrorException(ex.Message));
            }


        }
        #endregion

        #region Search TOP 15 products
        public async Task<ServiceResult<List<ProductsDto>>> GetTrendingProducts(int trendingCount)
        {
            try
            {
                // Query the top-selling products directly in the database
                //.Where(td => td.DateTimeModified > DateTime.UtcNow.AddDays(-31))
                var topSellingProductsQuery = _context.tbl_TransactionDetails
                    .GroupBy(td => td.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        RequestCount = g.Select(td => td.TransactionId).Distinct().Count() // Count distinct transactions
                    })
                    .OrderByDescending(p => p.RequestCount)
                    .Take(trendingCount);

                // Query for the products that match the top selling ProductIds
                var result = await _context.tbl_Products
                .Include(x => x.Tax)
                .Join(
                    topSellingProductsQuery,
                    product => product.Id,
                    topProduct => topProduct.ProductId,
                    (product, topProduct) => new
                    {
                        Product = product,
                        topProduct.RequestCount
                    }
                )
                .Where(p =>
                    (string.IsNullOrEmpty(p.Product.CategoryId) ||
                        !_context.tbl_Categories.Any(c => c.Id == p.Product.CategoryId
                        && c.HideInPos == true)) &&
                    (string.IsNullOrEmpty(p.Product.SegmentId) ||
                        !_context.tbl_Segments.Any(s => s.Id == p.Product.SegmentId
                        && s.HideInPos == true))
                )
                .OrderByDescending(p => p.RequestCount)
                .Select(p => p.Product)
                .ToListAsync();

                // Return the top selling products list as a DTO
                return ServiceResult<List<ProductsDto>>.Success(result.Adapt<List<ProductsDto>>());
            }
            catch (Exception ex)
            {
                // Handle exceptions (log or return error response)
                return ServiceResult<List<ProductsDto>>.Failure(new ServerErrorException(ex.Message));
            }
        }
        #endregion

        #region QueryModifier for Hiding Products by segment or category
        private IQueryable<tbl_Product> ExcludeHiddenProducts(IQueryable<tbl_Product> query)
        {
            // Filter logic: Show products only when:
            // - They have no category OR their category is not hidden
            // - AND they have no segment OR their segment is not hidden

            return query.Where(p =>
                (string.IsNullOrEmpty(p.CategoryId) ||
                !_context.tbl_Categories.Any(
                    c => c.Id == p.CategoryId && c.HideInPos == true))
                    &&
                (string.IsNullOrEmpty(p.SegmentId) ||
                !_context.tbl_Segments.Any(
                    s => s.Id == p.SegmentId && s.HideInPos == true))
            );
        }
        #endregion


        #region Get Product Stock Level
        public async Task<ServiceResult<StockNotifyDto>> GetProductStockLevel(string productId)
        {
            try
            {
                var product = await _context.tbl_Products
                    .Where(p => p.Id == productId)
                    .Select(p => new StockNotifyDto
                    {
                        ProductId = p.Id,
                        ProductName = p.ProductName ?? "",
                        CurrentStock = p.InStock ?? 0,
                        ReOrderLevel = p.ReOrderLevel ?? 0
                    })
                    .FirstOrDefaultAsync();
                if (product == null)
                {
                    return ServiceResult<StockNotifyDto>.Failure(
                        new NotFoundException($"Product with ID {productId} not found."));
                }

                return ServiceResult<StockNotifyDto>.Success(product);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching stock level notifications: {ex}", ex);
                return ServiceResult<StockNotifyDto>.Failure(new ServerErrorException("Could not fetch stock level notifications."));
            }
        }
        #endregion

        #region Stock Level Notification
        public async Task<ServiceResult<List<StockNotifyDto>>> GetStockLevelNotification()
        {
            try
            {
                var notifications = await _context.tbl_Products
                    .Where(p => p.ReOrderLevel.HasValue && p.InStock.HasValue && p.InStock < p.ReOrderLevel)
                    .Select(p => new StockNotifyDto
                    {
                        ProductId = p.Id,
                        ProductName = p.ProductName ?? "",
                        CurrentStock = p.InStock ?? 0,
                        ReOrderLevel = p.ReOrderLevel ?? 0
                    })
                    .ToListAsync();
                return ServiceResult<List<StockNotifyDto>>.Success(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching stock level notifications: {ex}", ex);
                return ServiceResult<List<StockNotifyDto>>.Failure(new ServerErrorException("Could not fetch stock level notifications."));
            }
        }
        #endregion

        #region Get Books By Category ID
        public async Task<ServiceResult<PaginationDetails<ProductsDto>>> GetBooksByCategoryId(
            string categoryId,
            int offSet,
            int limit,
            CancellationToken cancellationToken,
            string sortByColumn,
            bool sortAscending)
        {
            try
            {
                var query = _context.tbl_Products
                    .AsNoTracking()
                    .Where(p => p.CategoryId == categoryId);

                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting
                if (!string.IsNullOrEmpty(sortByColumn))
                {
                    var sortOrder = sortAscending ? "ascending" : "descending";
                    query = query.OrderBy($"{sortByColumn} {sortOrder}");
                }
                else
                {
                    query = query.OrderBy(p => p.ProductName);
                }

                var products = await query
                    .Skip(offSet)
                    .Take(limit)
                    .ToListAsync(cancellationToken);

                var productDtos = products.Adapt<List<ProductsDto>>();
                await PopulateUserFavourites(productDtos);
                var paginationDetails = new PaginationDetails<ProductsDto>
                {
                    Data = productDtos,
                    TotalSize = totalCount
                };

                foreach (var productDto in paginationDetails.Data)
                {
                    if (!string.IsNullOrEmpty(productDto.ProductImage))
                    {
                        string baseUrl = $"{_contextAccessor.HttpContext?.Request.Scheme}://{_contextAccessor.HttpContext?.Request.Host}";
                        string imagePath = Path.Combine("images/products", productDto.ProductImage).Replace("\\", "/");
                        productDto.ProductImageUrl = $"{baseUrl}/{imagePath}";
                    }
                }

                return ServiceResult<PaginationDetails<ProductsDto>>.Success(
                    paginationDetails
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books by category ID: {CategoryId}", categoryId);
                return ServiceResult<PaginationDetails<ProductsDto>>.Failure(
                    new ServerErrorException("Error getting books by category"));
            }
        }
        #endregion

        #region Get Free Books
        public async Task<ServiceResult<PaginationDetails<ProductsDto>>> GetFreeBooks(
            int offSet,
            int limit,
            CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.tbl_Products
                    .AsNoTracking()
                    .Where(p => p.PriceInclusive == 0 || p.PriceInclusive == null);

                var totalCount = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderBy(p => p.ProductName)
                    .Skip(offSet)
                    .Take(limit)
                    .ToListAsync(cancellationToken);

                var productDtos = products.Adapt<List<ProductsDto>>();
                var paginationDetails = new PaginationDetails<ProductsDto>
                {
                    Data = productDtos,
                    TotalSize = totalCount
                };

                foreach (var productDto in paginationDetails.Data)
                {
                    if (!string.IsNullOrEmpty(productDto.ProductImage))
                    {
                        string baseUrl = $"{_contextAccessor.HttpContext?.Request.Scheme}://{_contextAccessor.HttpContext?.Request.Host}";
                        string imagePath = Path.Combine("images/products", productDto.ProductImage).Replace("\\", "/");
                        productDto.ProductImageUrl = $"{baseUrl}/{imagePath}";
                    }
                }

                return ServiceResult<PaginationDetails<ProductsDto>>.Success(
                    paginationDetails
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting free books");
                return ServiceResult<PaginationDetails<ProductsDto>>.Failure(
                    new ServerErrorException("Error getting free books"));
            }
        }
        #endregion

        private async Task PopulateUserFavourites(IEnumerable<ProductsDto> dtos)
        {
            var userId = _tenantProvider.GetUserId();
            if (string.IsNullOrEmpty(userId)) return;

            var productIds = dtos.Select(d => d.Id).Where(id => id != null).ToList();
            if (!productIds.Any()) return;

            var favIds = (await _context.tbl_UserFavorites
                .Where(f => f.UserId == userId && f.IsDeleted != true && productIds.Contains(f.ProductId))
                .Select(f => f.ProductId)
                .ToListAsync())
                .ToHashSet();

            foreach (var dto in dtos)
                dto.Favourite = favIds.Contains(dto.Id!);
        }

        #region Get Books By CreatedBy (user's own books)
        public async Task<ServiceResult<PaginationDetails<ProductsDto>>> GetBooksByCreatedBy(
            string userId,
            int offSet,
            int limit,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return ServiceResult<PaginationDetails<ProductsDto>>.Failure(
                        new BadRequestException("UserId is required."));

                var query = _context.tbl_Products
                    .AsNoTracking()
                    .Where(p => p.CreatedBy == userId && p.Deleted != true);

                var totalCount = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderByDescending(p => p.DateTimeCreated)
                    .Skip(offSet)
                    .Take(limit)
                    .ToListAsync(cancellationToken);

                var productDtos = products.Adapt<List<ProductsDto>>();
                var paginationDetails = new PaginationDetails<ProductsDto>
                {
                    Data = productDtos,
                    TotalSize = totalCount
                };

                foreach (var productDto in paginationDetails.Data)
                {
                    if (!string.IsNullOrEmpty(productDto.ProductImage))
                    {
                        string baseUrl = $"{_contextAccessor.HttpContext?.Request.Scheme}://{_contextAccessor.HttpContext?.Request.Host}";
                        string imagePath = Path.Combine("images/products", productDto.ProductImage).Replace("\\\\", "/");
                        productDto.ProductImageUrl = $"{baseUrl}/{imagePath}";
                    }
                }
                await PopulateUserFavourites(paginationDetails.Data);
                return ServiceResult<PaginationDetails<ProductsDto>>.Success(paginationDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books by creator {UserId}", userId);
                return ServiceResult<PaginationDetails<ProductsDto>>.Failure(
                    new ServerErrorException("Error getting created books"));
            }
        }
        #endregion

    }
}
