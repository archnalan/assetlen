using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.DocumentModels;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.Contractor}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductDetailController : ControllerBase
    {
        private readonly IProductDetailDAL _productDetailDAL;

        public ProductDetailController(IProductDetailDAL productDetailDAL)
        {
            _productDetailDAL = productDetailDAL;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ProductDetailDto>), 200)]
        public async Task<ActionResult> GetPreviewSectionByProductId([FromQuery][Required] string productId, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.GetPreviewSectionByProductId(productId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ProductDetailDto>), 200)]
        public async Task<ActionResult> GetSectionsByProductId([FromQuery][Required] string productId, CancellationToken cancellationToken = default)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var result = await _productDetailDAL.GetSectionsByProductId(productId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProductDetailDto), 200)]
        public async Task<ActionResult> GetSectionById([FromQuery][Required] string id, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.GetSectionById(id, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDetailDto), 200)]
        public async Task<ActionResult> AddSection([FromBody] ProductDetailCreateDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.AddSection(dto, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> AddSectionsBulk([FromQuery][Required] string productId, [FromBody] List<ProductDetailCreateDto> sections, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.AddSectionsBulk(productId, sections, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ProductDetailDto), 200)]
        public async Task<ActionResult> UpdateSection([FromBody] ProductDetailUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.UpdateSection(dto, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> UpdateSectionContent([FromQuery][Required] string id, [FromBody][Required] string content, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.UpdateSectionContent(id, content, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> UpdateSectionTitle([FromQuery][Required] string id, [FromQuery][Required] string title, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.UpdateSectionTitle(id, title, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> ReorderSections([FromQuery][Required] string productId, [FromBody] List<SectionOrderChangeDto> newOrder, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.ReorderSections(productId, newOrder, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteSection([FromQuery][Required] string id, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.DeleteSection(id, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteAllSectionsForProduct([FromQuery][Required] string productId, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.DeleteAllSectionsForProduct(productId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDetailDto), 200)]
        public async Task<ActionResult> UpsertSection([FromBody] ProductDetailUpsertDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.UpsertSection(dto, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDetailDto), 200)]
        public async Task<ActionResult> DuplicateSection([FromQuery][Required] string id, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.DuplicateSection(id, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> NormalizeSortOrder([FromQuery][Required] string productId, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.NormalizeSortOrder(productId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDetailDto>), 200)]
        public async Task<ActionResult> SearchSections([FromQuery][Required] string productId, [FromQuery] string keyword, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.SearchSections(productId, keyword ?? string.Empty, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
        public async Task<ActionResult> GetDocumentSnapshot([FromQuery][Required] string productId, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.GetDocumentSnapshot(productId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> SaveDocument([FromQuery][Required] string productId, [FromBody] List<ProductDetailPersistDto> sections, CancellationToken cancellationToken = default)
        {
            var result = await _productDetailDAL.SaveDocument(productId, sections, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}