using mowt.Shared.Apicalls;
using mowt.Shared.Layout;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using mowt.Shared.Services;
using mowt.Shared.statics;
using mowt.Web.Client.Services;
using mowt.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Refit;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//var baseAddressApi = new Uri("https://localhost:7264");
var baseAddressApi = new Uri("http://localhost:5140");
//var baseAddressApi = new Uri("https://api.mowt.com");


builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();
// Add device-specific services used by the mowt.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddFluentUIComponents();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddBlazoredLocalStorage();

// Register a named HttpClient for refresh token without AuthHeaderHandler to avoid circular dependency
builder.Services.AddHttpClient("RefreshTokenClient", client =>
{
    client.BaseAddress = baseAddressApi;
});

// AuthHeaderHandler must be Transient because DelegatingHandlers cannot be reused
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddTransient<IAppLifecycleHandler, AppLifecycleHandler>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<IStorageService, StorageServiceWeb>();
builder.Services.AddSingleton<BlazorNavigationService>();
builder.Services.AddSingleton<NavigatorService>();
builder.Services.AddSingleton<Microsoft.FluentUI.AspNetCore.Components.DialogService>();
builder.Services.AddSingleton<IApiResponseHandler, ApiResponseHandler>();
builder.Services.AddSingleton<ISD, SD>();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton<IAppCloser, AppCloser>();

builder.Services
    .AddRefitClient<ICustomersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<ISuppliersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
 .AddRefitClient<ISegmentsAPI>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
.AddRefitClient<ICustomerDeposit>()
.ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
.AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
         .AddRefitClient<ICategoriesAPI>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
         .AddRefitClient<IReportsApi>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
         .AddRefitClient<IShiftsApi>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
         .AddRefitClient<ITransactionsApi>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
        .AddRefitClient<IAuthorizationApi>()
.ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
.AddHttpMessageHandler<AuthHeaderHandler>();


builder.Services
    .AddRefitClient<ICustomerDeposit>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
         .AddRefitClient<ICategoriesAPI>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
 .AddRefitClient<IProductsAPI>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
 .AddRefitClient<ITaxDataAPI>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
 .AddRefitClient<IGenerateCodeAPI>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
 .AddRefitClient<ICashItemsApi>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
 .AddRefitClient<IDiscountsAPI>()
 .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
 .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IExpenseApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<ITransactionDetailApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IPaymentsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IRefundsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IUsersAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services
    .AddRefitClient<IProductReceivingApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IExpenseTypeApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IGenerateBarcodeAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
.AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IConfigurationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IOrderStatusesApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
.AddRefitClient<IReceiptsApi>()
.ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
.AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
.AddRefitClient<ISlipLayout>()
.ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
.AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services
    .AddRefitClient<IFileProcessingApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
          .AddRefitClient<IProductRelationshipsApi>()
          .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
          .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
          .AddRefitClient<ICustomerPricingApi>()
          .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
          .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
              .AddRefitClient<IPrinterPreferencesApi>()
              .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
              .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IProductDetailApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IUserFavoritesAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IUserDocumentsAPI>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IProductDetailFeedbackApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<ISubscriptionRequestApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();

// ── Remote Site Refit clients ──
builder.Services
    .AddRefitClient<IProjectsRSApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IStagesApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IFundingApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services
    .AddRefitClient<IProgressApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddSingleton<GlobalContext>();
builder.Services.AddScoped<ICustomFileSaver, FileSaverWeb>();
builder.Services.AddScoped<IPrintService, PrintServiceWeb>();
builder.Services.AddScoped<FormDataPrep>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

builder.Services.AddScoped<IFolderPickerService, FolderPickerService>();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWX5fdXRRQ2JZVUd+VkVWYEs=");


await builder.Build().RunAsync();
