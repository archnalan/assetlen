using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.statics;
using Microsoft.FluentUI.AspNetCore.Components;
using static mowt.Shared.Models.statics.statics;

namespace mowt.Shared.Services
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
        void ResetPayments();

        ShiftsDto CurrentShift { get; set; }
        Module CurrentModule { get; set; }
        List<PaymentModeDto> paymentModeDtos { get; set; }
        bool isExpensePayment { get; set; }
        bool isCustomerDeposit { get; set; }
        ExpenseDto? ExpenseRequest { get; set; }
        CustomerDepositDto? CustomerDepositRequest { get; set; }
    }
}