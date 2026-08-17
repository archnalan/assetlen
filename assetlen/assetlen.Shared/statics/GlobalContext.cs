using assetlen.Shared.Apicalls;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.statics
{
    public class GlobalContext
    {
        public string? ActiveToastError { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? CurrentLoginPage { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyPhone { get; set; }
        public string? DefaultFont { get; set; } = "montserrat";
        public List<ConfigurationDto>? ConfigSettings { get; set; } = new();

        /// <summary>
        /// Set once at start-up from the host environment. Gates the persona
        /// quick-sign-in on the login page.
        /// <para>
        /// This is a convenience flag only. The seed endpoint behind those
        /// buttons refuses to run outside a Development <em>server</em>, so a
        /// tampered client gets a 404 rather than a demo tenant.
        /// </para>
        /// </summary>
        public bool IsDevelopment { get; set; }

        private readonly IConfigurationsApi _configApi;

        public GlobalContext(IConfigurationsApi configApi)
        {
            _configApi = configApi;
        }

        public void SetDefaultLoginTab(string? page)
        {
            if (!string.IsNullOrEmpty(page))
            {
                CurrentLoginPage = page;
            }
            else
            {
                CurrentLoginPage = "tab-billing";
            }
        }

        public string GetDefaultLoginTab() => CurrentLoginPage ?? "tab-billing";

        public async Task UpdateConfigSettings()
        {
            var configs = await _configApi.GetAllSettingsFromDB();
            if (configs != null && configs.Content != null)
            {
                var settings = configs.Content.ToDictionary(s => s.ConfigId ?? 0, s => s.StringValue ?? string.Empty);
                assetlen.Shared.Models.statics.statics.allSettings = settings;
                CompanyName = settings.GetValueOrDefault((int)assetlen.Shared.Models.statics.statics.Configurations.MyShopNameString, string.Empty);
                CompanyLocation = settings.GetValueOrDefault((int)assetlen.Shared.Models.statics.statics.Configurations.MyShopAddressLine, string.Empty);
                CompanyPhone = settings.GetValueOrDefault((int)assetlen.Shared.Models.statics.statics.Configurations.MyShopTelContact, string.Empty);
            }
        }
    }
}
