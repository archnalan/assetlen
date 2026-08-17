using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using assetlen.Client;
using assetlen.Shared.Apicalls;
using assetlen.Shared.Layout;
using assetlen.Shared.Services;
using assetlen.Shared.statics;
using assetlen.Client.Services;
using Refit;

var baseAddressApi = new Uri("https://localhost:7264");
//var baseAddressApi = new Uri("http://localhost:5140");
//var baseAddressApi = new Uri("https://api.assetlen.com");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

// Register a named HttpClient for refresh token without AuthHeaderHandler to avoid circular dependency
builder.Services.AddHttpClient("RefreshTokenClient", client =>
{
    client.BaseAddress = baseAddressApi;
});

// AuthHeaderHandler must be Transient because DelegatingHandlers cannot be reused
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddTransient<IAppLifecycleHandler, AppLifecycleHandler>();
// ONE instance, two service keys.
//
// This was registered twice — AddScoped<AuthenticationStateProvider,
// CustomAuthStateProvider>() and AddScoped<CustomAuthStateProvider>() — which
// built two unrelated objects. Login called MarkUserAsAuthenticated on the
// concrete one, so NotifyAuthenticationStateChanged fired on an instance
// CascadingAuthenticationState had never subscribed to: the cascading state
// stayed anonymous, every [Authorize] page rendered "Not signed in", and only a
// full reload (which re-reads the token from storage) recovered. The chrome
// looked signed in the whole time because it resolves the concrete type.
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<IStorageService, StorageServiceWeb>();
builder.Services.AddSingleton<BlazorNavigationService>();
builder.Services.AddSingleton<NavigatorService>();
builder.Services.AddSingleton<IApiResponseHandler, ApiResponseHandler>();
builder.Services.AddSingleton<ISD, SD>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddSingleton<IAppCloser, AppCloser>();

// ── ASSETLEN chrome ──
// Toasts and shell state are the product's own. The Fluent UI toast and dialog
// providers they replace pulled a second design system into every page, and the
// two never agreed on a colour, a radius or a motion curve.
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<ShellState>();

// What is waiting on the reader, and where. One scan, read by the rail, the
// bottom bar, Home and the Needs-you page — the last two each ran their own
// copy of the same per-project loop and could disagree about the count.
builder.Services.AddScoped<AttentionState>();

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
AddApi<IArtifactsApi>();
AddApi<IIngestApi>();

// Development demo world. The endpoint behind this answers 404 on any host that
// is not Development, so registering it everywhere costs nothing.
AddApi<IDevApi>();

// Scoped, not singleton. The hub needs IStorageService for the access token,
// and IStorageService is scoped — a singleton factory resolves it from the root
// provider, which the validating provider refuses. The throw landed inside
// ComponentFactory while Blazor was instantiating EntryDetail mid-diff, so the
// visible failure was a NullReferenceException out of RenderTreeDiffBuilder and
// every "what moved" link took the app down. In WebAssembly scoped is one
// instance for the app's lifetime anyway, so nothing is shared less than before.
builder.Services.AddScoped<IStreamHubService>(sp => new StreamHubService(
    sp.GetRequiredService<IStorageService>(),
    sp.GetRequiredService<ILogger<StreamHubService>>(),
    baseAddressApi));

builder.Services.AddSingleton<GlobalContext>();
builder.Services.AddScoped<ICustomFileSaver, FileSaverWeb>();

// Artifact bytes sit behind an authenticated endpoint, so they cannot be
// reached from an href or a src. Everything fetches through Refit and hands the
// browser a blob.
builder.Services.AddScoped<IArtifactDownloadService, ArtifactDownloadService>();
builder.Services.AddScoped<IPrintService, PrintServiceWeb>();
builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

builder.Services.AddSingleton<IFolderPickerService, FolderPickerService>();

var host = builder.Build();

// The persona quick-sign-in renders off this. Read from the host environment
// rather than from a compile-time symbol so a Release build served by a
// Development host still offers it.
host.Services.GetRequiredService<GlobalContext>().IsDevelopment = builder.HostEnvironment.IsDevelopment();

await host.RunAsync();
