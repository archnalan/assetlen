using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.statics
{
	public static class statics
	{

		public static T DeepCopy<T>(this T source)
		{
			if (source == null)
				return default;

			var serialized = JsonSerializer.Serialize(source);
			return JsonSerializer.Deserialize<T>(serialized);
		}

		public static async Task ReadExactlyHelperAsync(this Stream stream, byte[] buffer, int offset, int count)
		{
			int totalRead = 0;
			while (totalRead < count)
			{
				int read = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead);
				if (read == 0) throw new EndOfStreamException();
				totalRead += read;
			}
		}

		//public static Form frmUserDashboard;
		//public static Form frmBarcodeGeneratorBulk;
		//public static Form frmStockInventoryMgt;
		//public static Form frmStockReceivingGRN;
		//public static Form frmManagerModuleMAIN;
		//public static Form frmDGVCustomersExtended;
		//public static Form frmProductImportNExport;
		//public static Form frmCategoriesAndSegmentsDGV;
		//public static Form frmPay;
		//public static Form frmProductsENTRY;
		//public static Form frmExpenses;
		//public static Form frmSpecialCustomerPricing;
		public static int ShiftID;
		public static int ActiveSaleID;
		public static int ActiveUserIDBillingModule;
		public static int ActiveUserIDAdminModule;
		public static decimal ActiveSaleTotal;
		public static string CurrencySymbol = System.Globalization.RegionInfo.CurrentRegion.CurrencySymbol;
		public static int noOfDecimals;
		public static string searchKeyWordBarcodeForm;
		public static bool isApplicationExitCall = false;
		public static bool alreadyBackedUpDbToday = true;
		public static string selectedTab;
        public const string sessionStateKey = "sessionState";

        #region ASSETLEN App Modules
        public enum Module
		{
			AdminModule = 1,
			BillingModule = 2,
			OrdersModule = 3,
			RestaurantModule = 4,
		}
		#endregion

		#region List of System Roles
		// Module-level capabilities for each role are defined in RolePermissions.
		public enum UserRoles
		{
			// Platform
			AssetlenSuperAdmin = 1,
			ViewSystemlog,
			// Tenant
			Contractor,
			// Project leadership
			ProjectLead,
			// Crew
			Foreman,
			Inspector,
			Cameraman,
			Crew,
			// External
			Subcontractor,
			Client,
			ClientObserver
		}
		#endregion

		#region Payment Form

		public static decimal ActiveBalance;
		public static decimal ActiveCash;
		public static decimal ActiveAccount;
		public static decimal ActiveCard;
		public static decimal ActiveCheque;
		public static decimal ActiveBankDeposit;
		public static int ActiveCustomerID = -1;
		public static int productIDReporting = -1;
		public static string ActiveCardRef;
		public static string ActiveChequeNo;
		public static string ActiveNameOnCheque;
		public static int ActiveBankID = -1;
		public static DateTime BankingDate = DateTime.Now;
		public static int SelectedCardType = -1;
		public static int SelectedBank = -1;

		public static int DefaultPaymentMode;

		public static Dictionary<int, int> RegistrationStatus;


		#region PaymentOPtions
		public static int Cash;
		public static int Account;
		public static int Card;
		public static int Bankdeposit;
		public static int Cheque;



		#endregion


		// public static decimal ActiveChange;
		public static int ActiveSaleAgentID = -1;


		public static decimal PaymentTotal()
		{
			return statics.ActiveCash + statics.ActiveAccount + statics.ActiveCard + statics.ActiveCheque + statics.ActiveBankDeposit;

		}
		public enum SaleStatus
		{
			opened = 4,
			OnHold = 1,
			ActiveIDonHold = 2,
			Quotation = 3,
			SaleCompletedMark = 5,
			Order = 6,
			Refunded = 7,

			ClosedInstantly = 0 + 10,
			ClosedFromHold = 1 + 10,
			ClosedFromOrder = 2 + 10,
			ClosedFromQuote = 3 + 10,

		}
		#endregion


		public static string startUpErrorMessage;
		public static string RegistrationMessage;


		#region AdminDashboard
		//public static Form frmCustomerPricingENTRY;
		#endregion

		#region Change Discounts form
		public static decimal DiscountAmount;
		//public static Form frmDiscount;
		#endregion

		#region dgvCustomerList Form
		//public static Form frmDGVCustomers;
		//public static Form frmDGVCustomerSelect;
		//public static Form frmCustomersENTRY;
		#endregion

		#region Receipt Printing
		public enum PrintSlipID
		{
			BillingCash = 1,
			BillingAccount = 2,
			SaleOrder = 3,
			RestaurantOrder = 4,
			KitchenOrder = 5,
			AccountPayment = 6,
			QuotationSlip = 7,
			CashUpSlip = 8,
			PurchaseOrder = 9,
			GRNslip = 10,
			OrderDeliverlySlip = 11,
			RefundSlip = 12,
			CustomerStatement = 13,
			SupplierStatement = 14,
		}
		public enum PrintItemType
		{
			Text = 1,
			Image = 2,
			TransactionDateAndTime = 3,
			TransactionDate = 4,
			CashierName = 5,
			ItemsAndPrices = 6,
			ItemsAndPricesSepareteQuantity = 7,
			SlipID = 8,
			TotalincludingTax = 9,
			TotalExcludingTax = 10,
			TaxDetail = 11,
			TotalTax = 12,
			Change = 13,
			PaymentTypeAndAmount = 14,
			SalesPerson = 15,
			TableStockCode = 16,
			TableDescription = 17,
			TablePriceInc = 18,
			TableQty = 19,
			TableTax = 20,
			TableSubTotalExc = 21,
			TableSubTotalInc = 22,
			DrawerID = 23,
			ReportDetail = 24,
			AmountPaid = 25,
			CustomerAccount = 26,
			CustomerName = 27,
			CustomerTel = 28,
			RefundReason = 29,
			RefundedAmount = 30,
			ProductCode = 31,
			ProductDescription = 32,
			ProductsAndPrices = 33,
			CustomerEmail = 34,
			CompanyName = 35,
			IDNumber = 36,
			VATNumber = 37,
			CustAddress = 38,
			CustomerStatementDetail = 39,
			AccountBalance = 40,
			CreditLimit = 41,
			ToDate = 42,
			CustomerStatementFromDate = 43,
			SupplierName = 44,
			SupplierAddress = 45,
			SupplierVATNumber = 46,
			SupplierStatementDetail = 47,
			SupplierEmail = 48,
			SupplierAccountBalance = 49,
			SupplierTel = 50,
			SupplierOrderDetailWithStockcode = 51,
			SupplierOrderDetailWithBarCode = 52,
			SupplierTotalCost = 53,
			SupplierTotalQuantity = 54,
			CustomerOrderComment = 55,
			SupplierAccountNumber = 56,
			MyShopName = 57,
			MyshopAddressLine1 = 58,
			MyshopTelContact = 59,
			MyshopTIN = 60,
		}


		#endregion

		#region TempStorage for System settings                                         
		public static Dictionary<int, string> allSettings = new();

		#endregion

		#region Configuration settings and SettingIDs
		public enum Configurations
		{
			AutoStartShift = 1,
			MyShopNameString = 2,
			MyShopAddressLine = 3,
			MyShopTelContact = 4,
			MyshopTINnumber = 5,

			BarcodePrefix = 7,
			DefaultTaxOption = 8,
			UsePriceExclInstead = 26,
			IncludeTaxInCalculatedProfit = 27,

			DefaultDiscount1 = 9,
			DefaultDiscount2 = 10,
			DefaultDiscount3 = 11,

			AutoPrintSaleReceiptOnSaleCompletion = 16,
			IndicateDisCountsOnReceipts = 17,
			ForceLinkSalesToWaiter = 18,
			PreventSellingOutOfStockItems = 19,
			IncreaseQtyForSimilarItems = 20,
			AutoLoggOutUsersAfterMinutes = 21,
			NoOfDecimalPlaces = 22,
			UseCustomerSpecificPricing = 23,
			AllowBarcodeCardReadersSignIn = 24,
			TrackCheckInCheckOutTime = 25,
			DefaultPaymentModeSettingCode = 6,
			HidePaymentOptionCash = 28,
			HidePaymentOptionAccount = 29,
			HidePaymentOptionCard = 30,
			HidePaymentOptionCheque = 31,
			HidePaymentOptionBankDeposit = 32,
			AutomaticCardProcessing = 33,
			CardProcessingDeviceType = 34,
			ConnectedDeviceId = 35,
			CurrencyForCardPayments = 36,
			PrintDeviceSlipsForCardProcessing = 37,
			PrintmowtReceiptForCardProcessing = 38,
			ShowTotalExpectedAmountWhenCashingOut = 39,
			CompleteAndPayOrdersFromOrdersModule = 40,
			EnableHandPointSimulator = 41,
			BackUpDatabaseDirectory = 42,
			AutoBackupDatabase = 43,
			NumberOfDatabaseFilesToKeep = 44,
			LastdbBackupDateTime = 45,
			NotActivated = 46,
			RegistrationExpired = 47,
			AdminOnlyactivation = 48,
			AppVersion = 49,
			OnlineSyncEnabled = 50,
			OnlineSyncToken = 51,
			EnableBillingItemNotes = 52,
			EnableOrdersItemNotes = 53,
			AllowUsersResumeAnyTransaction = 54,
			LowStockLevelNotification = 55,
            RequireSubscriptionForBooks=56
        }
		#endregion

		#region RefundForm
		public static int SaleIDforRefund;
		//public static Form frmRefundsForm;
		#endregion

		#region Expenses form
		//public static ExpenseBLL expenseBLL;
		#endregion

		#region BarCodeGeneratorForm
		public static string justGeneratedBarcode;
		//public static Form frmBarcodeGenerator;
		#endregion
		#region App Registration Status
		//initialize dictionary

		static Dictionary<int, string> registrationStatusCodes = new Dictionary<int, string>();
		static bool initializeRegCodesLog = true;
		public static string AppRegisrationStatusCodes(int statusCode)
		{

			if (initializeRegCodesLog)
			{
				registrationStatusCodes.Add(5, "Un Registered");
				registrationStatusCodes.Add(3, "Un Registered - Invalid Key");
				registrationStatusCodes.Add(6, "Un Registered - Enter your Key");
				registrationStatusCodes.Add(19, "Registered - Back Office");
				registrationStatusCodes.Add(29, "Registered - Back office");
				registrationStatusCodes.Add(39, "Registered");
				registrationStatusCodes.Add(49, "Registered");
				registrationStatusCodes.Add(1, "Un registered - Key not for this computer");
				registrationStatusCodes.Add(4, "un Registered - Key expired");
				registrationStatusCodes.Add(2, "Un Registered - Invalid Key");
				registrationStatusCodes.Add(7, "Registeration Error - System date time has been rolled - Back");
			}
			initializeRegCodesLog = false;
			try
			{
				return registrationStatusCodes[statusCode];
			}
			catch (Exception)
			{
			}
			return "";
		}


		#endregion
		public static void InititaiteBackendPort()
		{

		}
	}
	public static class UserRoles
	{
		// Platform
		public const string AssetlenSuperAdmin = nameof(statics.UserRoles.AssetlenSuperAdmin);
		public const string ViewSystemlog = nameof(statics.UserRoles.ViewSystemlog);

		// Tenant administration
		public const string Contractor = nameof(statics.UserRoles.Contractor);

		// Project leadership
		public const string ProjectLead = nameof(statics.UserRoles.ProjectLead);

		// Crew specializations
		public const string Foreman = nameof(statics.UserRoles.Foreman);
		public const string Inspector = nameof(statics.UserRoles.Inspector);
		public const string Cameraman = nameof(statics.UserRoles.Cameraman);
		public const string Crew = nameof(statics.UserRoles.Crew);

		// External
		public const string Subcontractor = nameof(statics.UserRoles.Subcontractor);
		public const string Client = nameof(statics.UserRoles.Client);
		public const string ClientObserver = nameof(statics.UserRoles.ClientObserver);
	}

	public enum PrintItemType
	{
		Text = 1,
		Image = 2,
		TransactionDateAndTime = 3,
		TransactionDate = 4,
		CashierName = 5,
		//ItemsAndPrices = 6,
		ItemsAndPricesSepareteQuantity = 7,
		SlipID = 8,
		TotalincludingTax = 9,
		TotalExcludingTax = 10,
		TaxDetail = 11,
		TotalTax = 12,
		Change = 13,
		PaymentTypeAndAmount = 14,
		SalesPerson = 15,
		TableStockCode = 16,
		TableDescription = 17,
		TablePriceInc = 18,
		TableQty = 19,
		TableTax = 20,
		TableSubTotalExc = 21,
		TableSubTotalInc = 22,
		DrawerID = 23,
		ReportDetail = 24,
		AmountPaid = 25,
		CustomerAccount = 26,
		CustomerName = 27,
		CustomerTel = 28,
		RefundReason = 29,
		RefundedAmount = 30,
		ProductCode = 31,
		//ProductDescription = 32,
		ProductsAndPrices = 33,
		CustomerEmail = 34,
		CompanyName = 35,
		IDNumber = 36,
		//VATNumber = 37,
		CustAddress = 38,
		CustomerStatementDetail = 39,
		AccountBalance = 40,
		CreditLimit = 41,
		ToDate = 42,
		CustomerStatementFromDate = 43,
		SupplierName = 44,
		SupplierAddress = 45,
		SupplierVATNumber = 46,
		SupplierStatementDetail = 47,
		SupplierEmail = 48,
		SupplierAccountBalance = 49,
		SupplierTel = 50,
		SupplierOrderDetailWithStockcode = 51,
		SupplierOrderDetailWithBarCode = 52,
		SupplierTotalCost = 53,
		SupplierTotalQuantity = 54,
		CustomerOrderComment = 55,
		SupplierAccountNumber = 56,
		MyShopName = 57,
		MyshopAddressLine1 = 58,
		MyshopTelContact = 59,
		MyshopTIN = 60,
	}



}
