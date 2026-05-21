using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.statics;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.ProductConfig}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsDAL _productsDAL;
        private readonly ITenantProvider _tenantProvider;

        public ProductsController(IProductsDAL productsDAL, ITenantProvider tenantProvider)
        {
            _productsDAL = productsDAL;
            _tenantProvider = tenantProvider;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> AddProduct([FromBody] ProductCreateDto p)
        {
            var result = await _productsDAL.AddProduct(p);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(List<ProductsExportDto>), 200)]
        public async Task<IActionResult> ProcessExcelFile([FromForm] ProductMedia p)
        {
            var result = await _productsDAL.ProcessExcelFile(p);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }


        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteProducts([FromQuery][Required] string productId)
        {
            var result = await _productsDAL.DeleteProducts(productId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> GetProductsBasedOnBarcode([FromQuery][Required] string barCode)
        {
            var result = await _productsDAL.GetProductsBasedOnBarcode(barCode);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        [AllowAnonymous]
        public async Task<ActionResult> GetProductsBasedOnID([FromQuery][Required] string productId)
        {
            var result = await _productsDAL.GetProductsBasedOnID(productId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductWithQtyDto>), 200)]
        [Authorize(Roles = $"{UserRoles.ProductConfig},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetSubProductContentByParentID([FromQuery][Required] string parentProductId)
        {
            var result = await _productsDAL.GetSubProductContentByParentID(parentProductId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        [Authorize(Roles = $"{UserRoles.ProductConfig},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetProductsBasedOnProdCode([FromQuery][Required] string productCode)
        {
            var result = await _productsDAL.GetProductsBasedOnProdCode(productCode);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<ActionResult> GetProductsForCSVExportBasedOnSelectedFields([FromQuery] string? segmentId, [FromQuery] string? categoryId, [FromQuery] string? supplierId, [Required][FromBody] List<string> availableColumnNames)
        {
            var result = await _productsDAL.GetProductsForCSVExportBasedOnSelectedFields(segmentId, categoryId, supplierId, availableColumnNames);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductsExport.xlsx");
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> UpdateProductOnImportFromExcel([FromBody] ProductImportDataFinalDto p, CancellationToken token)
        {

            // Extend the timeout to 300 seconds for this action
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(300));

            try
            {
                var result = await _productsDAL.UpdateProductOnImportFromExcel(p);

                if (!result.IsSuccess)
                    return StatusCode(result.StatusCode, result.Error);

                return Ok(result.Data);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, "Request timed out.");
            }


        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<ProductsDto>), 200)]
        [Authorize(Roles = $"{UserRoles.ProductConfig},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetProductsFromDB([FromQuery] int? offSet = 0, [FromQuery] int? limit = 12, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;

            var result = await _productsDAL.GetProductsFromDB(offset1, limit1, cancellationToken, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductsDto>), 200)]
        [Authorize(Roles = $"{UserRoles.ProductConfig},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetTrendingProducts([FromQuery] int? trendingCount)
        {
            int trendingCount1 = trendingCount ?? 30;
            var result = await _productsDAL.GetTrendingProducts(trendingCount1);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<ProductsDto>), 200)]
        [AllowAnonymous]
        public async Task<ActionResult> SearchProducts([FromQuery] string? keywords, [FromQuery] string? categoryId, [FromQuery] string? segmentId, [FromQuery] string? supplierID, [FromQuery] bool? hasSubProduct = null, [FromQuery] decimal? inStock = 1, [FromQuery] int? offSet = 0, [FromQuery] int? limit = 12, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
        {
            string keywords1 = keywords ?? "";
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            decimal inStock1 = inStock ?? 0;

            var result = await _productsDAL.SearchProducts(keywords1, categoryId ?? "" ?? "", segmentId ?? "", supplierID, hasSubProduct, inStock1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        [Authorize(Roles = $"{UserRoles.ProductConfig},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SearchProductsForComboBoxes([FromQuery] string? keywords, [FromQuery] string? categoryId, [FromQuery] string? segmentId, [FromQuery] string? supplierID, [FromQuery] bool? hasSubProduct = false, [FromQuery] decimal? inStock = 1, [FromQuery] int? offSet = 0, [FromQuery] int? limit = 12, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            bool hasSub1 = hasSubProduct ?? false;
            decimal inStock1 = inStock ?? 1;

            var result = await _productsDAL.SearchProductsForComboBoxes(keywords1, categoryId ?? "", segmentId ?? "", supplierID ?? "", hasSub1, inStock1, offset1, limit1, cancellationToken, sortByColumn ?? "", sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<ProductsDto>), 200)]
        [Authorize(Roles = $"{UserRoles.ProductConfig},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SearchTOP15Products([FromQuery] string? keywords, [FromQuery] string? categoryId, [FromQuery] string? segmentId, [FromQuery] string? supplierID)
        {
            var result = await _productsDAL.SearchTOP15Products(keywords, categoryId, segmentId, supplierID);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> UpdateProduct([FromBody] ProductsDto p)
        {
            var result = await _productsDAL.UpdateProduct(p);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }


        [HttpPut]
        [ProducesResponseType(typeof(List<ProductBarcodeDto>), 200)]
        public async Task<ActionResult> UpdateProductBarcodes([FromBody] List<ProductBarcodeDto> products)
        {
            var result = await _productsDAL.UpdateProductBarcodes(products);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> UpdateProductPrices([FromQuery][Required] string productId, [FromBody][Required] ProductPricing pricing)
        {
            var result = await _productsDAL.UpdateProductPrices(productId, pricing);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> UpdateProductOnImportUsingBarCode([FromBody][Required] ProductsDto p)
        {
            var result = await _productsDAL.UpdateProductOnImportUsingBarCode(p);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
        [HttpPost]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> UpdateProductOnImportUsingProductID([FromBody] ProductsDto p)
        {
            var result = await _productsDAL.UpdateProduct(p);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }



        [HttpPost]
        [ProducesResponseType(typeof(ProductsDto), 200)]
        public async Task<ActionResult> UpdateProductOnImportUsingProductCode(ProductsDto p)
        {
            var result = await _productsDAL.UpdateProductOnImportUsingProductCode(p);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> UpdateProductStock([FromQuery][Required] string productId, [FromQuery][Required] decimal inStockAmount)
        {
            var result = await _productsDAL.UpdateProductStock(productId, inStockAmount);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> UpdateProductStockList([FromBody] List<StockParam> stockParamsList)
        {
            var result = await _productsDAL.UpdateProductStockList(stockParamsList);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> UpdateProductCostPrices([FromBody] List<CostPriceChange> costChange)
        {
            var result = await _productsDAL.UpdateProductCostPrices(costChange);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(StockNotifyDto), 200)]
        public async Task<ActionResult> GetProductStockLevel([FromQuery][Required] string productId)
        {
            var result = await _productsDAL.GetProductStockLevel(productId);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<StockNotifyDto>), 200)]
        public async Task<ActionResult> GetStockLevelNotification()
        {
            var result = await _productsDAL.GetStockLevelNotification();

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginationDetails<ProductsDto>), 200)]
        public async Task<ActionResult> GetBooksByCategoryId(
            [FromQuery][Required] string categoryId,
            [FromQuery] int? offSet = 0,
            [FromQuery] int? limit = 12,
            [FromQuery] string? sortByColumn = null,
            [FromQuery] bool sortAscending = false,
            [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 12;

            var result = await _productsDAL.GetBooksByCategoryId(
                categoryId,
                offset1,
                limit1,
                cancellationToken,
                sortByColumn,
                sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginationDetails<ProductsDto>), 200)]
        public async Task<ActionResult> GetFreeBooks(
            [FromQuery] int? offSet = 0,
            [FromQuery] int? limit = 12,
            [FromQuery] CancellationToken cancellationToken = default)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 12;

            var result = await _productsDAL.GetFreeBooks(
                offset1,
                limit1,
                cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<ProductsDto>), 200)]
        [Authorize(Roles = $"{UserRoles.LibraryModuleLogin},{UserRoles.ProductConfig}",
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetMyCreatedBooks(
            [FromQuery] int? offSet = 0,
            [FromQuery] int? limit = 200,
            [FromQuery] CancellationToken cancellationToken = default)
        {
            var userId = HttpContext.User.FindFirst("sub")?.Value
                      ?? HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? _tenantProvider.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 200;

            var result = await _productsDAL.GetBooksByCreatedBy(userId, offset1, limit1, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
