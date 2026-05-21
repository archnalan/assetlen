
using mowt.Shared.Apicalls;
using mowt.Shared.Layout;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using mowt.Shared.Services;
using mowt.Shared.statics;
using mowt.Web.Components;

using mowt.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Radzen;
using Refit;
using Serilog;
using Syncfusion.Blazor;

//var builder = WebApplication.CreateBuilder(args);
var currentPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

var baseAddressApi = new Uri("https://localhost:7264");
//var baseAddressApi = new Uri("http://localhost:5140");
//var baseAddressApi = new Uri("https://api.mowt.com");


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddRazorComponents()
//    .AddInteractiveWebAssemblyComponents();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.Authority = "mowt"; // Your authority URL
    options.Audience = "mowt"; // Typically your API resource name
                               // Add other token validation parameters if needed

    options.RequireHttpsMetadata = false;

});
builder.Services.AddCascadingAuthenticationState();
// Add device-specific services used by the mowtX.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddFluentUIComponents();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddTransient<IAppLifecycleHandler, AppLifecycleHandler>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<IStorageService, StorageServiceWeb>();
builder.Services.AddSingleton<BlazorNavigationService>();
builder.Services.AddSingleton<NavigatorService>();
builder.Services.AddSingleton<Microsoft.FluentUI.AspNetCore.Components.DialogService>();
builder.Services.AddSingleton<IApiResponseHandler, ApiResponseHandler>();
builder.Services.AddSingleton<ISD, SD>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
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


builder.Services.AddSingleton<GlobalContext>();
builder.Services.AddScoped<ICustomFileSaver, FileSaverWeb>();
builder.Services.AddScoped<IPrintService, PrintServiceWeb>();
builder.Services.AddScoped<FormDataPrep>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();



builder.Services.AddSingleton<IFolderPickerService, FolderPickerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWX5fdXRRQ2JZVUd+VkVWYEs=");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(mowt.Shared._Imports).Assembly,
        typeof(mowt.Web.Client._Imports).Assembly);

app.Run();
