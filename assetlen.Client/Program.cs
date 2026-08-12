using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using assetlen.Client;
using assetlen.Shared.Apicalls;
using assetlen.Shared.Layout;
using assetlen.Shared.Services;
using assetlen.Shared.statics;
using assetlen.Client.Services;
using assetlen.Client.Services;
using Refit;
using Syncfusion.Blazor;


var currentPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

var baseAddressApi = new Uri("https://localhost:7264");
//var baseAddressApi = new Uri("http://localhost:5140");
//var baseAddressApi = new Uri("https://api.assetlen.com");


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


builder.Services.AddCascadingAuthenticationState();
// Add device-specific services used by the assetlen.Shared project
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
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton<IAppCloser, AppCloser>();

// One registration helper — every ASSETLEN API client is registered the same way.
void AddApi<T>() where T : class => builder.Services
    .AddRefitClient<T>()
    .ConfigureHttpClient(c => c.BaseAddress = baseAddressApi)
    .AddHttpMessageHandler<AuthHeaderHandler>();

// Platform
AddApi<IAuthorizationApi>();
AddApi<IUsersAPI>();
AddApi<IConfigurationsApi>();
AddApi<ISubscriptionRequestApi>();

// Projects + Site Log
AddApi<IProjectsRSApi>();
AddApi<IStagesApi>();
AddApi<IFundingApi>();
AddApi<IProgressApi>();
AddApi<IProjectMembersApi>();
AddApi<IFlagsApi>();
AddApi<IBudgetApi>();

builder.Services.AddSingleton<IStreamHubService>(sp => new StreamHubService(
    sp.GetRequiredService<IStorageService>(),
    sp.GetRequiredService<ILogger<StreamHubService>>(),
    baseAddressApi));

builder.Services.AddSingleton<GlobalContext>();
builder.Services.AddScoped<ICustomFileSaver, FileSaverWeb>();
builder.Services.AddScoped<IPrintService, PrintServiceWeb>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();


builder.Services.AddSingleton<IFolderPickerService, FolderPickerService>();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWX5fdXRRQ2JZVUd+VkVWYEs=");

await builder.Build().RunAsync();
