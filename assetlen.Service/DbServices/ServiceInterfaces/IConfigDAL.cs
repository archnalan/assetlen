using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.ViewModels;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
	public interface IConfigDAL
	{
		Task<ServiceResult<ConfigurationDto>> CreateSettingInDB(ConfigurationDto configDto);
		Task<ServiceResult<List<ConfigurationDto>>> GetAllSettingsFromDB();
		Task<ServiceResult<ConfigurationDto>> GetSettingByID(int settingId, bool hideError = false);
		Task<ServiceResult<ConfigurationDto>> UpdateSettingInDB(ConfigurationDto configDto);
		Task<ServiceResult<bool>> DeleteSettingFromDB(int id);
		Task<ServiceResult<bool>> UpdateDatabaseSchemaWithScript(string scriptFileName);
		Task<ServiceResult<List<ConfigurationDto>>> CreateConfigSettingsInDB(List<ConfigurationDto> configDtos);
		Task<ServiceResult<List<ConfigurationDto>>> UpdateConfigSettingsInDB(List<ConfigurationDto> configDtos);
		Task<ServiceResult<List<int>>> GetExistingSettingIdsAsync(List<int> settingIds);
		Task<ServiceResult<bool>> TurnOnSyncingWithOnlineServer(bool shouldTurnOn);
		Task<ServiceResult<List<ConfigurationDto>>> GetSettingsByConfigIds(List<int> configIds);
		Task<ServiceResult<List<ConfigurationDto>>> UpdateDatabaseBackupSettingsInDB(DatabaseBackupDto dbBackupSettings);
		Task<ServiceResult<bool>> RestoreDatabaseAsync(string backupFilePath, bool backupDbFirst);
	}
}