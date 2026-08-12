using assetlen.Shared.Apicalls;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace assetlen.Shared.Services;

/// <summary>
/// Fetches artifact bytes through the authenticated pipeline and hands them to
/// the browser. A plain <c>href</c> or <c>src</c> carries no bearer token and
/// resolves against the WASM host, so every download goes .NET → bytes → blob.
/// </summary>
public interface IArtifactDownloadService
{
    /// <summary>Save an artifact to the user's downloads. False if it could not be fetched.</summary>
    Task<bool> SaveAsync(string artifactId, string fileName, string? mimeType = null);

    /// <summary>Open in a new tab, falling back to a save if the popup is blocked.</summary>
    Task<bool> OpenAsync(string artifactId, string fileName, string? mimeType = null);
}

public sealed class ArtifactDownloadService : IArtifactDownloadService, IAsyncDisposable
{
    private const string ModulePath = "./_content/assetlen.Shared/artifact-download.js";

    private readonly IArtifactsApi _artifacts;
    private readonly IJSRuntime _js;
    private readonly ILogger<ArtifactDownloadService> _logger;
    private IJSObjectReference? _module;

    public ArtifactDownloadService(
        IArtifactsApi artifacts, IJSRuntime js, ILogger<ArtifactDownloadService> logger)
    {
        _artifacts = artifacts;
        _js = js;
        _logger = logger;
    }

    public Task<bool> SaveAsync(string artifactId, string fileName, string? mimeType = null) =>
        RunAsync(artifactId, fileName, mimeType, preview: false);

    public Task<bool> OpenAsync(string artifactId, string fileName, string? mimeType = null) =>
        RunAsync(artifactId, fileName, mimeType, preview: true);

    private async Task<bool> RunAsync(string artifactId, string fileName, string? mimeType, bool preview)
    {
        if (string.IsNullOrEmpty(artifactId)) return false;

        try
        {
            using var response = await _artifacts.Download(artifactId);
            if (!response.IsSuccessStatusCode)
            {
                // 404 here is usually the exposure gate, not a missing file.
                _logger.LogWarning("Artifact {ArtifactId} download returned {Status}",
                    artifactId, (int)response.StatusCode);
                return false;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var type = mimeType
                ?? response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            var module = await ModuleAsync();

            if (preview && await module.InvokeAsync<bool>("open", type, bytes))
                return true;

            await module.InvokeVoidAsync("save", fileName, type, bytes);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not download artifact {ArtifactId}", artifactId);
            return false;
        }
    }

    private async ValueTask<IJSObjectReference> ModuleAsync() =>
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", ModulePath);

    public async ValueTask DisposeAsync()
    {
        if (_module is null) return;
        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone; nothing left to dispose.
        }
    }
}
