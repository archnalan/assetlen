using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using assetlen.Shared.Models.ViewModels;
using Hangfire;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using static assetlen.Shared.Models.statics.statics;

namespace assetlen.Service.DbServices
{
    public class ConfigDAL : IConfigDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ILogger<ConfigDAL> _logger;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ConfigDAL(AssetlenDbContext context, ILogger<ConfigDAL> logger, ITenantProvider tenantProvider, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _tenantProvider = tenantProvider;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        #region Check if Setting IDs exist in the DB and return existing IDs
        public async Task<ServiceResult<List<int>>> GetExistingSettingIdsAsync(List<int> settingIds)
        {
            try
            {
                if (settingIds == null || !settingIds.Any())
                {
                    return ServiceResult<List<int>>.Failure(
                        new BadRequestException("Ids data in required"));
                }

                var existingIds = await _context.tbl_Configurations
                .Where(c => settingIds.Contains(c.ConfigId))
                .Select(c => c.ConfigId)
                .ToListAsync();

                return ServiceResult<List<int>>.Success(existingIds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching existing setting IDs: {Error}", ex);
                return ServiceResult<List<int>>.Failure(
                    new ServerErrorException("Could not fetch setting IDs."));
            }
        }
        #endregion

        #region Create Settings in the DB
        public async Task<ServiceResult<ConfigurationDto>> CreateSettingInDB(ConfigurationDto configDto)
        {
            if (configDto == null) return ServiceResult<ConfigurationDto>.Failure(
                new BadRequestException("Configuration data is required."));

            try
            {
                var config = configDto.Adapt<tbl_Configuration>();

                await _context.tbl_Configurations.AddAsync(config);

                await _context.SaveChangesAsync();

                var createdConfigDto = config.Adapt<ConfigurationDto>();

                return ServiceResult<ConfigurationDto>.Success(createdConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating configuration {ConfigId}: {Error}", configDto.ConfigId, ex);
                if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    string errorMessage = "The Configuration you are trying to add already exists in this system. Please choose another number.";
                    return ServiceResult<ConfigurationDto>.Failure(new BadRequestException(errorMessage));
                }
                return ServiceResult<ConfigurationDto>.Failure(
                    new ServerErrorException("Could not create configuration."));
            }

        }
        #endregion

        #region Create Multiple Settings in the DB
        public async Task<ServiceResult<List<ConfigurationDto>>> CreateConfigSettingsInDB(List<ConfigurationDto> configDtos)
        {
            if (configDtos == null || configDtos.Count == 0) return ServiceResult<List<ConfigurationDto>>.Failure(
                new BadRequestException("Configuration data is required."));

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var configs = configDtos.Adapt<List<tbl_Configuration>>();
                        await _context.AddRangeAsync(configs);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        var createdConfigDtos = configs.Adapt<List<ConfigurationDto>>();
                        return ServiceResult<List<ConfigurationDto>>.Success(createdConfigDtos);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Error while creating configurations: {Error}", ex);
                        return ServiceResult<List<ConfigurationDto>>.Failure(
                            new ServerErrorException("Could not create configurations."));
                    }
                }
            });
        }
        #endregion

        #region Update Settings in the DB (String types)
        public async Task<ServiceResult<ConfigurationDto>> UpdateSettingInDB(ConfigurationDto configDto)
        {
            if (configDto == null) return ServiceResult<ConfigurationDto>.Failure(
                new BadRequestException("Configuration data is required."));

            var configInDb = await _context.tbl_Configurations.FirstOrDefaultAsync(c => c.Id == configDto.Id)
               ?? await _context.tbl_Configurations.FirstOrDefaultAsync(c => c.ConfigId == configDto.ConfigId);

            if (configInDb == null)
                return ServiceResult<ConfigurationDto>.Failure(
                new NotFoundException($"Configuration with ID: {configDto.Id} and or CofigId {configDto.ConfigId} not found."));

            try
            {
                // Updating the fields
                configInDb.ConfigId = configDto.ConfigId ?? 0;
                configInDb.StringValue = configDto.StringValue ?? configInDb.StringValue;

                await _context.SaveChangesAsync();

                // Update configDto with the values from configInDb
                configDto.ConfigId = configInDb.ConfigId;
                configDto.StringValue = configInDb.StringValue;

                return ServiceResult<ConfigurationDto>.Success(configDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating configuration {ConfigId}: {Error}", configDto.ConfigId, ex);
                return ServiceResult<ConfigurationDto>.Failure(
                    new ServerErrorException("Could not update configuration."));
            }
        }
        #endregion

        #region Update Multiple Settings in the DB (String types)
        public async Task<ServiceResult<List<ConfigurationDto>>> UpdateConfigSettingsInDB(List<ConfigurationDto> configDtos)
        {
            if (configDtos == null || configDtos.Count == 0) return ServiceResult<List<ConfigurationDto>>.Failure(
                new BadRequestException("Configuration data is required."));

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var configsInDb = await _context.tbl_Configurations.ToListAsync();
                        foreach (var configDto in configDtos)
                        {
                            var configInDb = configsInDb.FirstOrDefault(x => x.ConfigId == configDto.ConfigId);
                            if (configInDb == null)
                            {
                                var newIdExists = Enum.IsDefined(typeof(Configurations), configDto.ConfigId);
                                if (!newIdExists) continue;

                                _context.tbl_Configurations.Add(new tbl_Configuration
                                {
                                    ConfigId = configDto.ConfigId ?? 0,
                                    StringValue = configDto.StringValue ?? string.Empty
                                });
                            }
                            else
                            {
                                if (configDto.ConfigId == (int)Configurations.OnlineSyncEnabled)
                                {
                                    var turnOnSyncing = configDto.StringValue.ToLower() == "true" ? true : false;
                                    var resultSync = await TurnOnSyncingWithOnlineServer(turnOnSyncing);
                                    if (!resultSync.IsSuccess)
                                    {
                                        _logger.LogError("Failed to turn on syncing with online server: {Error}", resultSync.Error);
                                        return ServiceResult<List<ConfigurationDto>>.Failure(resultSync.Error);
                                    }
                                }
                                else configInDb.StringValue = configDto.StringValue;
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        var updatedConfigDtos = configsInDb.Adapt<List<ConfigurationDto>>();
                        return ServiceResult<List<ConfigurationDto>>.Success(updatedConfigDtos);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Error while updating configurations: {Error}", ex);
                        return ServiceResult<List<ConfigurationDto>>.Failure(
                            new ServerErrorException("Could not update configurations."));
                    }
                }
            });
        }
        #endregion


        #region Update Multiple Database backup Settings in the DB
        public async Task<ServiceResult<List<ConfigurationDto>>> UpdateDatabaseBackupSettingsInDB(DatabaseBackupDto dbBackupSettings)
        {
            if (dbBackupSettings == null) return ServiceResult<List<ConfigurationDto>>.Failure(
                new BadRequestException("Configuration data is required."));

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var configDtos = GetConfigurationDtos(dbBackupSettings);

                        var configsInDb = await _context.tbl_Configurations.ToListAsync();
                        foreach (var configDto in configDtos)
                        {
                            var configInDb = configsInDb.FirstOrDefault(x => x.ConfigId == configDto.ConfigId);
                            if (configInDb == null)
                            {
                                var newIdExists = Enum.IsDefined(typeof(Configurations), configDto.ConfigId);
                                if (!newIdExists) continue;

                                _context.tbl_Configurations.Add(new tbl_Configuration
                                {
                                    ConfigId = configDto.ConfigId ?? 0,
                                    StringValue = configDto.StringValue ?? string.Empty
                                });
                            }
                            else
                            {

                                configInDb.StringValue = configDto.StringValue;
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        if (dbBackupSettings.SaveAndBackupNow)
                        {
                            var resultOut = await BackUpDataBaseNow();
                            if (!resultOut.IsSuccess)
                            {
                                return ServiceResult<List<ConfigurationDto>>.Failure(resultOut.Error);
                            }
                        }

                        var updatedConfigDtos = configsInDb.Adapt<List<ConfigurationDto>>();
                        return ServiceResult<List<ConfigurationDto>>.Success(updatedConfigDtos);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Error while updating database backup settings: {Error}", ex);
                        return ServiceResult<List<ConfigurationDto>>.Failure(
                            new ServerErrorException("Could not update database backup settings."));
                    }
                }
            });
        }


        #endregion

        #region Backup and restore
        /// <summary>
        /// BACKUP DATABASE and single-user RESTORE were SQL Server's. The database is
        /// PostgreSQL (CLAUDE.md §5.1.1) and is backed up outside the app with pg_dump.
        /// </summary>
        private static ServiceResult<bool> NotFromInsideTheApp(string what) =>
            ServiceResult<bool>.Failure(new NotImplementedException(
                $"{what} is not available from inside the app. Use pg_dump / pg_restore on the server."));

        public Task<ServiceResult<bool>> BackUpDataBaseNow() =>
            Task.FromResult(NotFromInsideTheApp("Database backup"));

        public Task<ServiceResult<bool>> RestoreDatabaseAsync(string backupFilePath, bool backupDbFirst) =>
            Task.FromResult(NotFromInsideTheApp("Database restore"));
        #endregion

        #region Read Setting from Database based on SettingID
        public async Task<ServiceResult<ConfigurationDto>> GetSettingByID(int settingId, bool hideError = false)
        {
            try
            {
                var setting = await _context.tbl_Configurations.FirstOrDefaultAsync(x => x.ConfigId == settingId);

                if (setting == null)
                {
                    _logger.LogError("Configuration with ID: {SettingId} not found.", settingId);
                    return ServiceResult<ConfigurationDto>.Failure(
                        new NotFoundException($"Configuration with ID: {settingId} not found."));
                }

                return ServiceResult<ConfigurationDto>.Success(setting.Adapt<ConfigurationDto>());
            }
            catch (Exception ex)
            {
                if (!hideError)
                {
                    _logger.LogError("Error while fetching setting with ID {SettingId}: {Error}", settingId, ex);
                }
                return ServiceResult<ConfigurationDto>.Failure(
                    new ServerErrorException("Could not fetch setting."));
            }
        }
        #endregion

        #region Read ALL Settings from Database
        public async Task<ServiceResult<List<ConfigurationDto>>> GetAllSettingsFromDB()
        {
            try
            {
                var configs = await _context.tbl_Configurations.ToListAsync();
                return ServiceResult<List<ConfigurationDto>>.Success(configs.Adapt<List<ConfigurationDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching all configurations: {Error}", ex);
                return ServiceResult<List<ConfigurationDto>>.Failure(
                    new ServerErrorException("Could not fetch configurations."));
            }
        }
        #endregion

        #region Read Settings from Database based on ConfigIds
        public async Task<ServiceResult<List<ConfigurationDto>>> GetSettingsByConfigIds(List<int> configIds)
        {
            try
            {
                var configs = await _context.tbl_Configurations
                    .Where(c => configIds.Contains(c.ConfigId))
                    .ToListAsync();
                return ServiceResult<List<ConfigurationDto>>.Success(configs.Adapt<List<ConfigurationDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching configurations by Config IDs: {Error}", ex);
                return ServiceResult<List<ConfigurationDto>>.Failure(
                    new ServerErrorException("Could not fetch configurations."));
            }
        }
        #endregion

        #region Delete  Setting from Database
        public async Task<ServiceResult<bool>> DeleteSettingFromDB(int id)
        {
            var configInDb = await _context.tbl_Configurations.FindAsync(id);

            if (configInDb == null) return ServiceResult<bool>
                    .Failure(new NotFoundException($"Configuration with ID: {id} not found."));

            try
            {
                configInDb.IsDeleted = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting configuration with ID {ConfigId}: {Error}", id, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete configuration."));
            }

        }
        #endregion

        #region Update database Schema
        // The scripts were T-SQL; the schema now moves only by migration (assetlen.Postgres).
        public Task<ServiceResult<bool>> UpdateDatabaseSchemaWithScript(string scriptFileName) =>
            Task.FromResult(NotFromInsideTheApp("Running a schema script"));
        #endregion

        #region Turn on syncing with online server
        public async Task<ServiceResult<bool>> TurnOnSyncingWithOnlineServer(bool shouldTurnOn)
        {


            try
            {

                if (shouldTurnOn)
                {
                    //TODO: ensure user has an active subscription.
                    var configEnabled = await _context.tbl_Configurations.FirstOrDefaultAsync(c => c.ConfigId == (int)Configurations.OnlineSyncEnabled);
                    var configToken = await _context.tbl_Configurations.FirstOrDefaultAsync(c => c.ConfigId == (int)Configurations.OnlineSyncToken);


                    if (configEnabled.StringValue != "true" && string.IsNullOrEmpty(configToken.StringValue))
                    {
                        var syncData = new InitialSeedDataDto();
                        syncData.tenantId = _tenantProvider.GetTenantId();

                        syncData.configSeedData = await _context.tbl_Configurations.ToListAsync();
                        syncData.AppUser = await _context.Users.OrderBy(x => x.DateTimeCreated).FirstAsync();
                        syncData.tenantData = await _context.tbl_Tenants.FirstOrDefaultAsync(x => x.TenantId == syncData.tenantId);
                        //get role names
                        var roleids = await _context.UserRoles
                            .Where(x => x.UserId == syncData.AppUser.Id)
                            .Select(x => x.RoleId)
                            .ToListAsync();
                        syncData.UserRoleNames = await _context.Roles
                            .Where(x => roleids.Contains(x.Id))
                            .Select(x => x.Name)
                            .ToListAsync();


                        using var client = _httpClientFactory.CreateClient("SyncClient");
                        var onlineBaseUrl = _configuration["OnlineApi:BaseUrl"];
                        client.BaseAddress = new Uri(onlineBaseUrl);

                        // Recreate original request
                        var request = new HttpRequestMessage(new HttpMethod("POST"), "/api/Authorization/IssueSyncKey");

                        // Properly set content and content type
                        request.Content = new StringContent(JsonConvert.SerializeObject(syncData), Encoding.UTF8, "application/json");

                        // Apply headers, excluding content-type from headers as it's already set above

                        //request.Headers.TryAddWithoutValidation("IsOnlineSync", "true");
                        request.Headers.TryAddWithoutValidation("TenantId", syncData.tenantId);

                        var response = await client.SendAsync(request);
                        var responseContenttest = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {

                            var syncResponse = responseContenttest;
                            if (!string.IsNullOrEmpty(syncResponse))
                            {
                                _logger.LogInformation("Syncing with online server is turned on successfully. API Key: {key}", syncResponse);
                                // Save the API key to the database or configuration
                                configToken.StringValue = syncResponse;

                                configEnabled.StringValue = "true";
                                await _context.SaveChangesAsync();
                                _configuration["OnlineApi:IsEnabled"] = "true";

                                return ServiceResult<bool>.Success(true);
                            }
                            else
                            {
                                _logger.LogError("Failed to turn on syncing with online server: {message}", responseContenttest);
                                return ServiceResult<bool>.Failure(new ServerErrorException($"Failed to turn on syncing with online server: {syncResponse}"));
                            }
                        }
                        else
                        {
                            _logger.LogError("Failed to turn on syncing with online server. Status code: {statusCode}, Message {message } Detail, {Detail}", response.StatusCode, responseContenttest, response);
                            return ServiceResult<bool>.Failure(new ServerErrorException($"Failed to turn on syncing with online server. Status code: {response.StatusCode}"));
                        }
                    }
                    if (!string.IsNullOrEmpty(configToken.StringValue))
                    {
                        configEnabled.StringValue = "true";
                        await _context.SaveChangesAsync();

                        return ServiceResult<bool>.Success(true);
                    }
                }

                else
                {
                    var configEnabled = await _context.tbl_Configurations.FirstOrDefaultAsync(c => c.ConfigId == (int)Configurations.OnlineSyncEnabled);
                    configEnabled.StringValue = "false";
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Syncing with online server is turned off successfully.");
                    return ServiceResult<bool>.Success(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating configuration for online sync: {Error}", ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not update configuration for online sync."));
            }
            return ServiceResult<bool>.Failure(
                new ServerErrorException("An error occurred while turning on syncing."));
        }
        #endregion

        private List<ConfigurationDto> GetConfigurationDtos(DatabaseBackupDto Content)
        {
            return new List<ConfigurationDto>
        {
            new ConfigurationDto
            {
                StringValue = Content.AutoBackupDb.StringValue,
                ConfigId = Content.AutoBackupDb.ConfigId,
                Id = Content.AutoBackupDb.Id,
            },
            new ConfigurationDto
            {
                StringValue = Content.BackupLocation.StringValue,
                ConfigId = Content.BackupLocation.ConfigId,
                Id = Content.BackupLocation.Id,
            },
            new ConfigurationDto
            {
                StringValue = Content.NumberOfFileToKeep.StringValue,
                ConfigId = Content.NumberOfFileToKeep.ConfigId,
                Id = Content.NumberOfFileToKeep.Id,
            }
        };
        }
    }
}
