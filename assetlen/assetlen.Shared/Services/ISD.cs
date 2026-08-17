using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.statics;
using static assetlen.Shared.Models.statics.statics;

namespace assetlen.Shared.Services
{
    public interface ISD
    {
        bool activeMenu { get; set; }
        bool SelectMultiple { get; set; }
        decimal? screenSize { get; set; }
        string currentMode { get; }
        string currentColor { get; }
        initialState isClicked { get; set; }
        bool themeSettings { get; set; }
        UserClaimsDto CurrentUser { get; }
        event Action? OnUserUpdated;

        void SetUser(UserClaimsDto user);
        void RemoveUser();
        void setColor(string colorHarshCode);
        void setMode(string themeMode);

        Module CurrentModule { get; set; }
    }
}