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
    public class InitialSeedDataDto
    {
        public string tenantId { get; set; }
        public tbl_Category categories { get; set; }
        public tbl_Segment segments { get; set; }
        public tbl_Supplier suppliers { get; set; }
        public List<tbl_Tax> taxes { get; set; }
        public List<tbl_PaymentMode>? paymentModes { get; set; }
        public List<tbl_CashItem> cashItems { get; set; }
        public List<tbl_Configuration> configSeedData { get; set; }
        public List<tbl_OrderStatus>? orderStatuses { get; set; }
        public AppUser AppUser { get; set; }
        public tbl_Tenant tenantData { get; set; }
        public List<string>? UserRoleNames { get; set; }
        public InitialSeedDataDto()
        {

        }
        public InitialSeedDataDto(string tenantId)
        {
            this.tenantId = tenantId;
            categories = new tbl_Category
            {
                TenantId = tenantId,
                Category = "Default Category"
            };
            segments = new tbl_Segment
            {
                TenantId = tenantId,
                Segment = "Default Segment"
            };
            taxes = new List<tbl_Tax>
            {
                new tbl_Tax
                {
                    TaxValue = 0.0000m,
                    TaxDescription = "No Sales Tax",
                    TenantId = tenantId
                },
                new tbl_Tax
                {
                    TaxValue = 18.0000m,
                    TaxDescription = "Value Added Tax",
                    TenantId = tenantId
                }
            };
            suppliers = new tbl_Supplier
            {
                TenantId = tenantId,
                FullName = "Default Supplier"
            };
            var paymentModes = new List<tbl_PaymentMode>
                {
                    new tbl_PaymentMode
                    {
                        Id = "1",
                        Description = "Cash",
                        IsDeleted = false,
                        TenantId = tenantId
                    },
                    new tbl_PaymentMode
                    {
                        Id = "2",
                        Description = "Account",
                        IsDeleted = false,
                        TenantId = tenantId
                    },
                    new tbl_PaymentMode
                    {
                        Id = "3",
                        Description = "Card",
                        IsDeleted = false,
                        TenantId = tenantId
                    },
                    new tbl_PaymentMode
                    {
                        Id = "4",
                        Description = "Cheque",
                        IsDeleted = false,
                        TenantId = tenantId
                    },
                    new tbl_PaymentMode
                    {
                        Id = "5",
                        Description = "Bank Deposit",
                        IsDeleted = false,
                        TenantId = tenantId
                    }
                };
            this.paymentModes = paymentModes;

            var orderStatuses = new List<tbl_OrderStatus>
            {
                new tbl_OrderStatus
                {
                    OrderName = "Pending",
                    SortOrder = 1,
                    TenantId = tenantId
                },
                new tbl_OrderStatus
                {
                    OrderName = "Approved",
                    SortOrder = 2,
                    TenantId = tenantId
                },
                new tbl_OrderStatus
                {
                    OrderName = "Processed",
                    SortOrder = 3,
                    TenantId = tenantId
                }
            };
            this.orderStatuses = orderStatuses;

            var cashItems = new List<tbl_CashItem>
                {
                    new tbl_CashItem { Amount = 50000.00m, TenantId = tenantId },
                    new tbl_CashItem { Amount = 2000.00m, TenantId = tenantId },
                    new tbl_CashItem { Amount = 1000.00m, TenantId = tenantId },
                    new tbl_CashItem { Amount = 500.00m, TenantId = tenantId },
                    new tbl_CashItem { Amount = 200.00m, TenantId = tenantId },
                    new tbl_CashItem { Amount = 10000.00m, TenantId = tenantId },
                    new tbl_CashItem { Amount = 20000.00m, TenantId = tenantId },
                    new tbl_CashItem { Amount = 100.00m, TenantId = tenantId },
                    new tbl_CashItem {Amount = 5000.00m,TenantId = tenantId}
                };
            this.cashItems = cashItems;

            var configSeedData = new List<tbl_Configuration>
            {
                new() { ConfigId = (int)Configurations.AutoStartShift, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.MyShopNameString, StringValue = "My Bussiness name", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.MyShopAddressLine, StringValue = "P.O Box 0001 Kampala", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.MyShopTelContact, StringValue = "+256 414 000 001", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.MyshopTINnumber, StringValue = "", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.DefaultPaymentModeSettingCode, StringValue = "1", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.BarcodePrefix, StringValue = "56861", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.DefaultTaxOption, StringValue = "", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.DefaultDiscount1, StringValue = "10", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.DefaultDiscount2, StringValue = "6", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.DefaultDiscount3, StringValue = "4", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AutoPrintSaleReceiptOnSaleCompletion, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.IndicateDisCountsOnReceipts, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.ForceLinkSalesToWaiter, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.PreventSellingOutOfStockItems, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.IncreaseQtyForSimilarItems, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AutoLoggOutUsersAfterMinutes, StringValue = "30", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.NoOfDecimalPlaces, StringValue = "0", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.UseCustomerSpecificPricing, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AllowBarcodeCardReadersSignIn, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.TrackCheckInCheckOutTime, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.UsePriceExclInstead, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.IncludeTaxInCalculatedProfit, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.HidePaymentOptionCash, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.HidePaymentOptionAccount, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.HidePaymentOptionCard, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.HidePaymentOptionCheque, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.HidePaymentOptionBankDeposit, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AutomaticCardProcessing, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.CardProcessingDeviceType, StringValue = "-1", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.ConnectedDeviceId, StringValue = "0102030405060708091011121314151617181920212223242526272829303132", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.CurrencyForCardPayments, StringValue = "143", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.PrintDeviceSlipsForCardProcessing, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.PrintmowtReceiptForCardProcessing, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.ShowTotalExpectedAmountWhenCashingOut, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.CompleteAndPayOrdersFromOrdersModule, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.EnableHandPointSimulator, StringValue = "False", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.BackUpDatabaseDirectory, StringValue = @"C:\assetlen backups", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AutoBackupDatabase, StringValue = "True", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.NumberOfDatabaseFilesToKeep, StringValue = "10", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.LastdbBackupDateTime, StringValue = "08/09/2024 10:07:53", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AppVersion, StringValue = "2219", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.OnlineSyncEnabled, StringValue = "false", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.OnlineSyncToken, StringValue = "", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.EnableBillingItemNotes, StringValue = "false", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.EnableOrdersItemNotes, StringValue = "false", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.AllowUsersResumeAnyTransaction, StringValue = "false", TenantId = tenantId },
                new() { ConfigId = (int)Configurations.LowStockLevelNotification, StringValue = "false", TenantId = tenantId },
            };

            this.configSeedData = configSeedData;

        }
    }
}
