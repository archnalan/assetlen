using System.ComponentModel.DataAnnotations;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.statics;
using assetlen.Shared.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace assetlen.API.Controllers
{
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Crew}",
       AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerDAL _customerDAL;

        public CustomerController(ICustomerDAL customerDAL)
        {
            _customerDAL = customerDAL;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CustomerDto>), 200)]
        public async Task<ActionResult> GetCustomersFromDb([FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;

            var result = await _customerDAL.GetCustomersFromDB(offset1, limit1, cancellation, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<CustomerDto>), 200)]
        public async Task<ActionResult> GetCustomerById([FromQuery] string id)
        {
            var result = await _customerDAL.GetCustomerById(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<CustomerDto>), 200)]
        public async Task<ActionResult> SearchCustomerByKeywords([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellationToken = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _customerDAL.SearchCustomerByKeywords(keywords1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        public async Task<ActionResult> SearchCustomerByKeywordsForComboBoxes([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellationToken = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _customerDAL.SearchCustomerByKeywordsForComboBoxes(keywords1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ImportResultSummary), 200)]
        public async Task<ActionResult> ImportCustomersFromExcel([FromBody] ImportDataDto p, CancellationToken token)
        {
            // Extend the timeout to 300 seconds for this action
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(300));

            try
            {
                var result = await _customerDAL.ImportCustomersFromExcel(p);

                if (!result.IsSuccess)
                    return StatusCode(result.StatusCode, result.Error);

                return Ok(result.Data);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, "Request timed out.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<ActionResult> GetCustomersForCSVExportBySelectedFields([Required][FromBody] List<string> selectedColumnNames)
        {
            var result = await _customerDAL.GetCustomersForCSVExportBySelectedFields(selectedColumnNames);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CategoriesExport.xlsx");
        }

    }
}
