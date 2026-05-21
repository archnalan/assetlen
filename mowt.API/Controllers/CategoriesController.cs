using mowt.Service.DbServices;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Service.DbServices.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mowt.Shared.Models.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Identity;
using mowt.Shared.Models.Models;
using System.Diagnostics.Tracing;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.ComponentModel.DataAnnotations;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.AdminModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryDAL _categoryDAL;
        public CategoriesController(ICategoryDAL categoryDAL)
        {
            _categoryDAL = categoryDAL;
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(PaginationDetails<CategoryDto>), 200)]
        public async Task<ActionResult> GetCategoriesFromDB([FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {

            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;

            var result = await _categoryDAL.GetCategoriesFromDB(offset1, limit1, cancellation, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        public async Task<ActionResult> SearchCategoriesFromComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _categoryDAL.SearchCategoriesFromComboBoxes(keywords1, offset1, limit1, cancellation, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        [AllowAnonymous]
        public async Task<ActionResult> GetCategoryBasedOnID([FromQuery] string id)
        {
            var result = await _categoryDAL.GetCategoryById(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetCategoryIDBasedCategoryName(string name)
        {
            var result = await _categoryDAL.GetCategoryByName(name);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        public async Task<ActionResult> AddCategory(CategoryDto c)
        {
            var result = await _categoryDAL.AddCategory(c);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        public async Task<ActionResult> UpdateCategory([FromQuery] string id, CategoryDto c)
        {
            var result = await _categoryDAL.UpdateCategory(id, c);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteCategory([FromQuery] string id)
        {
            var result = await _categoryDAL.DeleteCategoryById(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<CategoryDto>), 200)]
        //modify the endpoint to take in Task<IApiResponse<PaginationDetails<CategoryDto>>> SearchCategoriesFromDB([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        public async Task<ActionResult> SearchCategoriesFromDB([FromQuery] string keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            var result = await _categoryDAL.SearchCategoriesFromDB(keywords, offset1, limit1, cancellation, sortByColumn, sortAscending);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<ActionResult> GetCategoriesForCSVExportBySelectedFields([Required][FromBody] List<string> selectedColumnNames)
        {
            var result = await _categoryDAL.GetCategoriesForCSVExportBySelectedFields(selectedColumnNames);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CategoriesExport.xlsx");
        }

        [HttpPost]
        [ProducesResponseType(typeof(ImportResultSummary), 200)]
        public async Task<ActionResult> ImportCategoriesFromExcel([FromBody] ImportDataDto p, CancellationToken token)
        {
            // Extend the timeout to 300 seconds for this action
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(600));

            try
            {
                var result = await _categoryDAL.ImportCategoriesFromExcel(p);

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
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<ActionResult> GetTopCategories([FromQuery] int? limit, [FromQuery] CancellationToken cancellationToken = default)
        {
            int limit1 = limit ?? 10;
            var result = await _categoryDAL.GetTopCategories(limit1, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
