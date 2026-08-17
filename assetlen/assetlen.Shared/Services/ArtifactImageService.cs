using assetlen.Shared.Apicalls;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace assetlen.Shared.Services;

/// <summary>
/// Turns an artifact address into something a browser will paint. Artifact
/// bytes sit behind an authenticated endpoint and an <c>&lt;img src&gt;</c>
/// carries no bearer token, so this fetches through .NET and hands back an
/// object URL. Anything that is not an artifact address passes through unchanged.
/// </summary>
public interface IArtifactImageService
{
    /// <summary>
    /// An address the browser can paint, or null if there is nothing to paint or
    /// the reader is not entitled to it. Never throws.
    /// </summary>
    ValueTask<string?> ResolveAsync(string? url);

    /// <summary>Drop every cached object URL — one account's covers must not survive into another's session.</summary>
    ValueTask ClearAsync();
}

public sealed class ArtifactImageService : IArtifactImageService
{
    /// <summary>Matches the two shapes <c>ArtifactDAL</c> emits, and nothing else.</summary>
    private static readonly Regex ArtifactUrl = new(
        @"^/api/Artifacts/(?<id>[^/]+)/(?<kind>thumbnail|content)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IArtifactsApi _artifacts;
    private readonly IJSRuntime _js;
    private readonly ILogger<ArtifactImageService> _logger;

    private readonly Dictionary<string, string?> _resolved = new(StringComparer.OrdinalIgnoreCase);

    // Deduped: the rail row, the card and the menu all draw the same cover.
    private readonly Dictionary<string, Task<string?>> _inFlight = new(StringComparer.OrdinalIgnoreCase);

    public ArtifactImageService(IArtifactsApi artifacts, IJSRuntime js, ILogger<ArtifactImageService> logger)
    {
        _artifacts = artifacts;
        _js = js;
        _logger = logger;
    }

    public async ValueTask<string?> ResolveAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var match = ArtifactUrl.Match(url);
        if (!match.Success) return url;   // already paintable — a CDN cover, a data URI

        var key = url;

        if (_resolved.TryGetValue(key, out var cached)) return cached;
        if (_inFlight.TryGetValue(key, out var running)) return await running;

        var task = FetchAsync(match.Groups["id"].Value, match.Groups["kind"].Value);
        _inFlight[key] = task;

        try
        {
            var result = await task;
            _resolved[key] = result;
            return result;
        }
        finally
        {
            _inFlight.Remove(key);
        }
    }

    private async Task<string?> FetchAsync(string artifactId, string kind)
    {
        try
        {
            using var response = kind.Equals("content", StringComparison.OrdinalIgnoreCase)
                ? await _artifacts.Download(artifactId)
                : await _artifacts.DownloadThumbnail(artifactId);

            if (!response.IsSuccessStatusCode)
            {
                // 403 is the exposure gate doing its job; the caller draws its
                // fallback mark and the row reads as one that never had a cover.
                _logger.LogDebug("Artifact {ArtifactId} image returned {Status}",
                    artifactId, (int)response.StatusCode);
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0) return null;

            var mime = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            return await _js.InvokeAsync<string>("assetlen.objectUrl", mime, bytes);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not resolve artifact image {ArtifactId}", artifactId);
            return null;
        }
    }

    public async ValueTask ClearAsync()
    {
        var urls = _resolved.Values.Where(v => v is not null).Cast<string>().ToArray();
        _resolved.Clear();
        _inFlight.Clear();

        if (urls.Length == 0) return;

        try { await _js.InvokeVoidAsync("assetlen.revokeObjectUrls", (object)urls); }
        catch (Exception ex) { _logger.LogDebug(ex, "Could not revoke cached image URLs"); }
    }
}
