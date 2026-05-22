using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System;

namespace assetlen.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize(Roles = $"{UserRoles.Contractor}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly ISyncDAL _syncDAL;

        public SyncController(ISyncDAL syncDAL)
        {
            _syncDAL = syncDAL;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SizeDto), 200)]
        public async Task<IActionResult> GetChanges([FromQuery] DateTime lastSync, [FromQuery] int Offset = 0, [FromQuery] int batchSize = 200)
        {
            var result = await _syncDAL.GetChangesFromOnlineApi(lastSync, Offset, batchSize);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

    }

}
