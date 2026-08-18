using System.Text.Json;
using assetlen.Shared.Services;

namespace assetlen.Maui.Services;

/// <summary>
/// The native stand-in for browser localStorage: the access token and session
/// state survive an app restart the same way they survive a page reload.
/// </summary>
/// <remarks>
/// The web client's <c>StorageServiceWeb</c> cannot be reused here. It reaches
/// for <c>ISyncLocalStorageService</c>, which needs <c>IJSInProcessRuntime</c>,
/// and a BlazorWebView's JS runtime is always out-of-process — every write
/// would have silently no-opped and the user would never stay signed in.
/// Values are JSON-encoded so a round-trip matches Blazored.LocalStorage's.
/// </remarks>
public class StorageServiceMaui : IStorageService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public event Action? OnTokenRemoved;

    public Task<T> GetItemAsync<T>(string objectKey)
    {
        var raw = Preferences.Default.Get<string?>(objectKey, null);
        if (string.IsNullOrEmpty(raw)) return Task.FromResult(default(T)!);

        try
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(raw, Json)!);
        }
        catch (JsonException)
        {
            // A key written by an older build in a different shape must not take
            // the app down on start-up; treat it as absent and let it be rewritten.
            Preferences.Default.Remove(objectKey);
            return Task.FromResult(default(T)!);
        }
    }

    public Task SetItemAsync<T>(string objectKey, T objectValue)
    {
        Preferences.Default.Set(objectKey, JsonSerializer.Serialize(objectValue, Json));
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string objectKey)
    {
        Preferences.Default.Remove(objectKey);
        OnTokenRemoved?.Invoke();
        return Task.CompletedTask;
    }
}
