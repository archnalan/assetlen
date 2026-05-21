using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.ViewModels;
using Refit;

namespace assetlen.Shared.Apicalls
{
    public interface IConfigurationsApi
    {
        [Post("/api/Config/CreateConfigSettingsInDB")]
        Task<IApiResponse<List<ConfigurationDto>>> CreateConfigSettingsInDB([Body] List<ConfigurationDto> configDtos);

        [Put("/api/Config/UpdateConfigSettingsInDB")]
        Task<IApiResponse<List<ConfigurationDto>>> UpdateConfigSettingsInDB([Body] List<ConfigurationDto> configDtos);

        [Put("/api/Config/UpdateDatabaseBackupSettingsInDB")]
        Task<IApiResponse<bool>> UpdateDatabaseBackupSettingsInDB([Body] DatabaseBackupDto backupDto);

        [Get("/api/Config/RestoreDatabase")]
        Task<IApiResponse<bool>> RestoreDatabase([Query] string backupFilePath, [Query] bool backupDbFirst);

        [Get("/api/Config/GetExistingSettingIds")]
        Task<IApiResponse<List<int>>> GetExistingSettingIds([Query(CollectionFormat.Multi)] List<int?> settingIds);

        [Get("/api/Config/GetSettingFromDBbasedOnID")]
        Task<IApiResponse<ConfigurationDto>> GetSettingFromDBbasedOnID(int id);

        [Get("/api/Config/GetAllSettingsFromDB")]
        Task<IApiResponse<List<ConfigurationDto>>> GetAllSettingsFromDB();

        [Get("/api/Config/GetSettingsByConfigIds")]
        Task<IApiResponse<List<ConfigurationDto>>> GetSettingsByConfigIds([Query(CollectionFormat.Multi)] List<int> configIds);
    }
}
