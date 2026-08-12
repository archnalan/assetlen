using assetlen.Service.DataAccess;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static assetlen.Shared.Models.statics.statics;

namespace assetlen.Shared.Models.Models.ViewModels
{
    /// <summary>
    /// What a brand-new tenant needs to exist: the tenant row, its first user,
    /// that user's roles, and the handful of settings the platform itself reads.
    /// </summary>
    /// <remarks>
    /// P1 removed the POS reference data this used to carry (categories,
    /// segments, suppliers, taxes, payment modes, cash denominations, order
    /// statuses) along with ~40 till-behaviour config rows. ASSETLEN seeds no
    /// domain data — a tenant starts with nothing but its own projects.
    /// </remarks>
    public class InitialSeedDataDto
    {
        public string tenantId { get; set; }
        public List<tbl_Configuration> configSeedData { get; set; }
        public AppUser AppUser { get; set; }
        public tbl_Tenant tenantData { get; set; }
        public List<string>? UserRoleNames { get; set; }

        public InitialSeedDataDto()
        {

        }

        public InitialSeedDataDto(string tenantId)
        {
            this.tenantId = tenantId;

            configSeedData = new List<tbl_Configuration>
            {
                // Contractor organisation identity — shown on exports and headers.
                new() { ConfigId = (int)Configurations.MyShopNameString, StringValue = "My organisation name", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.MyShopAddressLine, StringValue = "P.O Box 0001 Kampala", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.MyShopTelContact, StringValue = "+256 414 000 001", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.MyshopTINnumber, StringValue = "", TenantId = tenantId },

                // Session + platform.
                new() { ConfigId = (int)Configurations.AutoLoggOutUsersAfterMinutes, StringValue = "30", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.NoOfDecimalPlaces, StringValue = "0", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AppVersion, StringValue = "2219", TenantId = tenantId },

                // Backup + sync — read by ConfigDAL and SyncDAL.
                new() { ConfigId = (int)Configurations.BackUpDatabaseDirectory, StringValue = @"C:\assetlen backups", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AutoBackupDatabase, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.NumberOfDatabaseFilesToKeep, StringValue = "10", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.LastdbBackupDateTime, StringValue = "08/09/2024 10:07:53", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.OnlineSyncEnabled, StringValue = "false", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.OnlineSyncToken, StringValue = "", TenantId = tenantId },
            };
        }
    }
}
