using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Velopack;

namespace assetlen.Shared.Models.UpdateManger
{
    public static class AssetlenUpdateManager
    {

        public static UpdateManager mgr = null;
        private static UpdateInfo? newVersion = null;
        public static bool IsUpdateReadyForInstall = false;


        public static async Task CheckAndDownloadUpdate(ILogger logger)
        {
            try
            {
                mgr = new UpdateManager(@"https://clientsapi.assetlen.com/api/Download/updates");



                if (!mgr.IsInstalled) return;
                // check for new version
                newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                    return; // no update available

                // download new version
                logger.LogInformation($"New version available: {newVersion.BaseRelease.FileName} - {newVersion.BaseRelease.Version}");

                await mgr.DownloadUpdatesAsync(newVersion);
                IsUpdateReadyForInstall = true;

            }
            catch (Exception ex)
            {
                logger.LogError("Error checking for updates: {ex}", ex);
            }
        }
        public static void ApplyUpdatesAndRestart(ILogger logger)
        {
            if (newVersion == null)
            {
                logger.LogInformation("No new version available to apply.");
                return;
            }
            try
            {
                if (mgr == null)
                {
                    logger.LogError("Update manager is not initialized.");
                    return;
                }
                logger.LogInformation($"Applying updates for version: {newVersion.BaseRelease.Version}");
                IsUpdateReadyForInstall = false;
                mgr.ApplyUpdatesAndRestart(newVersion);
            }
            catch (Exception ex)
            {
                logger.LogError("Error applying updates: {ex}", ex);
            }
        }

    }

}


