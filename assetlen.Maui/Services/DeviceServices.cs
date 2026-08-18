using assetlen.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace assetlen.Maui.Services;

public class AppCloser : IAppCloser
{
    public void CloseApp() => Application.Current?.Quit();
}

public class AppLifecycleHandler : IAppLifecycleHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public AppLifecycleHandler(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public string CurrentRoute { get; set; } = string.Empty;

    public Task OnAppClosing() => Task.CompletedTask;
}

/// <summary>
/// Law 0 (assetlen.md §4) — the product has to keep working when the contractor
/// is silent, which on site also means when the signal is. The native head can
/// answer this for real instead of throwing, so a capture screen can say what is
/// queued rather than what failed.
/// </summary>
public class ConnectivityService : IConnectivityService
{
    public bool HasInternet =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public string? ConnectionProfile =>
        Connectivity.Current.ConnectionProfiles.FirstOrDefault().ToString();
}
