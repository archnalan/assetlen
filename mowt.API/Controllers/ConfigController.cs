using System.ComponentModel.DataAnnotations;
using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using mowt.Shared.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.AdminModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigDAL _configDAL;
        private readonly IConfiguration _configuration;

        public ConfigController(IConfigDAL configDAL, IConfiguration configuration)
        {
            _configDAL = configDAL;
            _configuration = configuration;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ConfigurationDto>), 200)]
        public async Task<ActionResult> GetAllSettingsFromDB()
        {
            var result = await _configDAL.GetAllSettingsFromDB();

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ConfigurationDto>), 200)]
        public async Task<ActionResult> GetSettingsByConfigIds([FromQuery][Required] List<int> configIds)
        {
            var result = await _configDAL.GetSettingsByConfigIds(configIds);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ConfigurationDto), 200)]
        [Authorize(Roles = $"{UserRoles.AdminModuleLogin},{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetSettingFromDBbasedOnID([FromQuery] int id)
        {
            var result = await _configDAL.GetSettingByID(id);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ConfigurationDto), 200)]
        public async Task<ActionResult> CreateSettingInDB([FromBody] ConfigurationDto config)
        {
            var result = await _configDAL.CreateSettingInDB(config);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ConfigurationDto), 200)]
        public async Task<ActionResult> UpdateSettingInDB([FromBody] ConfigurationDto config)
        {
            var result = await _configDAL.UpdateSettingInDB(config);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }


        [HttpPut]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> UpdateDatabaseBackupSettingsInDB([FromBody] DatabaseBackupDto config)
        {
            if (_configuration["AppMode"] != "1") return StatusCode(403, new ForbiddenException("Database restore not supported in the current mode"));
            if (string.IsNullOrEmpty(config.BackupLocation.StringValue)) return StatusCode(400, new BadRequestException("Valid BackupLocation is required"));

            var result = await _configDAL.UpdateDatabaseBackupSettingsInDB(config);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> RestoreDatabase([FromQuery] string backupFilePath, [FromQuery] bool backupDbFirst)
        {
            if (_configuration["AppMode"] != "1") return StatusCode(403, new ForbiddenException("Database restore not supported in the current mode"));

            var result = await _configDAL.RestoreDatabaseAsync(backupFilePath, backupDbFirst);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteSettingFromDB([FromQuery] int id)
        {
            var result = await _configDAL.DeleteSettingFromDB(id);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }



        [HttpDelete]
        public async Task<ActionResult> ResetDataBaseTransactions()
        {
            var result = await _configDAL.ResetDataBaseTransactions();

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateDatabaseSchemaWithScript(string scriptFileName)
        {
            var result = await _configDAL.UpdateDatabaseSchemaWithScript(scriptFileName);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost]
        [ProducesResponseType(typeof(List<ConfigurationDto>), 200)]
        public async Task<ActionResult> CreateConfigSettingsInDB([FromBody] List<ConfigurationDto> configDtos)
        {
            var result = await _configDAL.CreateConfigSettingsInDB(configDtos);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpPut]
        [ProducesResponseType(typeof(List<ConfigurationDto>), 200)]
        public async Task<ActionResult> UpdateConfigSettingsInDB([FromBody][Required] List<ConfigurationDto> configDtos)
        {
            var result = await _configDAL.UpdateConfigSettingsInDB(configDtos);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }
        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> TurnOnSyncingWithOnlineServer([FromHeader][Required] bool shouldtrunOn)
        {
            var result = await _configDAL.TurnOnSyncingWithOnlineServer(shouldtrunOn);

            if (result.IsSuccess) return Ok(result.Data);
            return StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<int>), 200)]
        public async Task<ActionResult> GetExistingSettingIds([FromQuery] List<int> settingIds)
        {
            var result = await _configDAL.GetExistingSettingIdsAsync(settingIds);

            if (result.IsSuccess) return Ok(result.Data);

            return StatusCode(result.StatusCode, result.Error);
        }

    }
}
