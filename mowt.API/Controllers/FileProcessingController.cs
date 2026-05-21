using mowt.Service.FileProcessingServices;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileProcessingController : ControllerBase
    {
        private readonly IFileProcessing _fileProcessing;
        public FileProcessingController(IFileProcessing fileProcessing)
        {
            _fileProcessing = fileProcessing;
        }

        [HttpPost]
        [ProducesResponseType(typeof(List<Dictionary<string, object>>), 200)]
        public async Task<IActionResult> ProcessExcelFile([FromForm] ImportMedia p)
        {
            var result = await _fileProcessing.ProcessExcelFile(p);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
