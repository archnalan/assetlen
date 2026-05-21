using mowt.Service.DbServices.ServiceInterfaces;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.LibraryModuleLogin},{UserRoles.AdminModuleLogin}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PrinterPreferencesController : ControllerBase
    {
        private readonly IPrinterPreferencesDAL _printerPreferencesDAL;

        public PrinterPreferencesController(IPrinterPreferencesDAL printerPreferencesDAL)
        {
            _printerPreferencesDAL = printerPreferencesDAL;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationDetails<PrinterPreferancesDto>), 200)]
        public async Task<ActionResult> GetPrinterPreferences([FromQuery] string? keywords, [FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] CancellationToken cancellationToken = default, [FromQuery] string sortByColumn = null, [FromQuery] bool sortAscending = true)
        {
            int offset1 = offset ?? 0;
            int limit1 = limit ?? 30;
            string keywords1 = keywords ?? string.Empty;

            var result = await _printerPreferencesDAL.GetPrinterPreferences(keywords1, offset1, limit1, cancellationToken, sortByColumn, sortAscending);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error.Message);

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PrinterPreferancesDto), 200)]
        public async Task<ActionResult> GetPrinterPreferencesById(string id)
        {
            var result = await _printerPreferencesDAL.GetPrinterPreferencesById(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error.Message);

            return Ok(result.Data);
        }
        [HttpGet("{SlipType}")]
        [ProducesResponseType(typeof(PrinterPreferancesDto), 200)]
        public async Task<ActionResult> GetPrinterPreferencesBySlipType(int SlipType)
        {
            var result = await _printerPreferencesDAL.GetPrinterPreferencesBySlipType(SlipType);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error.Message);

            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PrinterPreferancesDto), 200)]
        public async Task<ActionResult> AddOrUpdatePrinterPreferences([FromBody] PrinterPreferancesDto printerPreferences)
        {
            var result = await _printerPreferencesDAL.AddOrUpdatePrinterPreferences(printerPreferences);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error.Message);

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeletePrinterPreferences(string id)
        {
            var result = await _printerPreferencesDAL.DeletePrinterPreferences(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error.Message);

            return Ok(result.Data);
        }
    }
}
