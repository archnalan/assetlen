using assetlen.Maui.Services;
using assetlen.Shared.Apicalls;
using assetlen.Shared.Services;
using assetlen.Shared.statics;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Refit;

namespace assetlen.Maui;

public static class MauiProgram
{
    // The one address the whole app talks to. Loopback only resolves for the
    // Windows and Mac Catalyst heads; an Android emulator reaches the host at
    // 10.0.2.2, which is why this is resolved per platform rather than pasted in.
    private static Uri BaseAddressApi =>
        DeviceInfo.Current.Platform == DevicePlatform.Android
            ? new Uri("https://10.0.2.2:7264")
            : new Uri("https://localhost:7264");

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ── Platform seams ──
        // Everything below this line is registered exactly as assetlen.Client
        // registers it. The four services that differ are the four that touch
        // the platform: storage, files, connectivity and closing the app.
        builder.Services.AddSingleton<IFormFactor, Services.FormFactor>();
        builder.Services.AddScoped<IStorageService, StorageServiceMaui>();
        builder.Services.AddScoped<ICustomFileSaver, FileSaverMaui>();
        builder.Services.AddScoped<IPrintService, PrintServiceMaui>();
        builder.Services.AddSingleton<IFolderPickerService, Services.FolderPickerService>();
        builder.Services.AddSingleton<IConnectivityService, Services.ConnectivityService>();
        builder.Services.AddSingleton<IAppCloser, AppCloser>();
        builder.Services.AddTransient<IAppLifecycleHandler, AppLifecycleHandler>();

        // ── Auth ──
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        // ONE instance, two service keys — the same bug the client's comment
        // records: two registrations build two unrelated providers, and the
        // cascading state stays anonymous after a successful sign-in.
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(
            sp => sp.GetRequiredService<CustomAuthStateProvider>());

        builder.Services.AddTransient<AuthHeaderHandler>();

        // Refresh runs on its own client so AuthHeaderHandler cannot depend on itself.
        builder.Services.AddHttpClient("RefreshTokenClient", client =>
        {
            client.BaseAddress = BaseAddressApi;
        });

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = BaseAddressApi });

        // ── Shared app services ──
        builder.Services.AddSingleton<BlazorNavigationService>();
        builder.Services.AddSingleton<NavigatorService>();
        builder.Services.AddSingleton<IApiResponseHandler, ApiResponseHandler>();
        builder.Services.AddSingleton<ISD, SD>();
        builder.Services.AddScoped<IUserSessionService, UserSessionService>();

        // ── ASSETLEN chrome ──
        builder.Services.AddScoped<IToastService, ToastService>();
        builder.Services.AddScoped<ShellState>();
        builder.Services.AddScoped<AttentionState>();
        builder.Services.AddScoped<ProjectMenuState>();

        builder.Services.AddScoped<IArtifactDownloadService, ArtifactDownloadService>();
        builder.Services.AddScoped<IArtifactImageService, ArtifactImageService>();

        builder.Services.AddSingleton<GlobalContext>();

        // ── API clients ──
        void AddApi<T>() where T : class => builder.Services
            .AddRefitClient<T>()
            .ConfigureHttpClient(c => c.BaseAddress = BaseAddressApi)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        // Platform
        AddApi<IAuthorizationApi>();
        AddApi<IUsersAPI>();
        AddApi<IConfigurationsApi>();
        AddApi<ISubscriptionRequestApi>();

        // Projects + Site Diary
        AddApi<IProjectsRSApi>();
        AddApi<IStagesApi>();
        AddApi<IFundingApi>();
        AddApi<IProgressApi>();
        AddApi<IProjectMembersApi>();
        AddApi<IFlagsApi>();
        AddApi<IBudgetApi>();
        AddApi<IArtifactsApi>();
        AddApi<IIngestApi>();
        AddApi<IDevApi>();

        // Scoped for the same reason as in the client: the hub needs the scoped
        // IStorageService for its access token, and a singleton factory would be
        // resolving it from the root provider.
        builder.Services.AddScoped<IStreamHubService>(sp => new StreamHubService(
            sp.GetRequiredService<IStorageService>(),
            sp.GetRequiredService<ILogger<StreamHubService>>(),
            BaseAddressApi));

        var app = builder.Build();

#if DEBUG
        // The persona quick-sign-in renders off this. A Debug native build is a
        // developer's machine by definition; the seed endpoint behind those
        // buttons still 404s on any host that is not a Development server.
        app.Services.GetRequiredService<GlobalContext>().IsDevelopment = true;
#endif

        return app;
    }
}
