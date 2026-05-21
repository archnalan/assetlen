using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using assetlen.Shared.Models.statics;

namespace assetlen.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.AdminModuleLogin}", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BanksController : ControllerBase
    {
        private readonly IBankDAL _bankDAL;
        public BanksController(IBankDAL bankDAL)
        {
            _bankDAL = bankDAL;
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(PaginationDetails<BankDto>), 200)]
        public async Task<ActionResult> GetBanksFromDB([FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            var result = await _bankDAL.GetBanksFromDB(offset1, limit1, cancellation, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
        public async Task<ActionResult> SearchBanksFromComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;
            var result = await _bankDAL.SearchBanksFromComboBoxes(keywords1, offset1, limit1, cancellation, sortByColumn, sortAscending);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(BankDto), 200)]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetBankById([FromQuery] string id)
        {
            var result = await _bankDAL.GetBankById(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(BankDto), 200)]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetBankByName([FromQuery] string name)
        {
            var result = await _bankDAL.GetBankByName(name);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetBankIDBasedOnBankName([FromQuery] string bankName)
        {
            var result = await _bankDAL.GetBankIDBasedOnBankName(bankName);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BankDto), 200)]
        public async Task<ActionResult> AddBank([FromBody] BankDto bank)
        {
            var result = await _bankDAL.AddBank(bank);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BankDto), 200)]
        public async Task<ActionResult> UpdateBank([FromBody] BankDto bank)
        {
            var result = await _bankDAL.UpdateBank(bank);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteBank([FromQuery] string id)
        {
            var result = await _bankDAL.DeleteBankById(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<BankDto>), 200)]
        public async Task<ActionResult> SearchBanksFromDB([FromQuery] string keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] CancellationToken cancellation = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offSet ?? 0;
            int limit1 = limit ?? 30;
            var result = await _bankDAL.SearchBanksFromDB(keywords, offset1, limit1, cancellation, sortByColumn, sortAscending);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(FileResult), 200)]
        public async Task<ActionResult> GetBanksForCSVExportBySelectedFields([Required][FromBody] List<string> selectedColumnNames)
        {
            var result = await _bankDAL.GetBanksForCSVExportBySelectedFields(selectedColumnNames);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BanksExport.xlsx");
        }

        [HttpPost]
        [ProducesResponseType(typeof(ImportResultSummary), 200)]
        public async Task<ActionResult> ImportBanksFromExcel([FromBody] ImportDataDto p, CancellationToken token)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(300));

            try
            {
                var result = await _bankDAL.ImportBanksFromExcel(p);

                if (!result.IsSuccess)
                    return StatusCode(result.StatusCode, result.Error);

                return Ok(result.Data);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, "Request timed out.");
            }
        }
    }
}