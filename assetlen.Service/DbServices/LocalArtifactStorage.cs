using Microsoft.Extensions.Logging;
using assetlen.Service.DbServices.ServiceInterfaces;

namespace assetlen.Service.DbServices;

/// <summary>
/// Local content-addressed storage under a configured root, sharded two hex
/// characters deep so no directory accumulates unbounded entries.
/// <para>
/// <c>artifacts/9f/9f2b1c….jpg</c> — the name <em>is</em> the hash, so an
/// identical re-upload lands on the identical path and the write is skipped.
/// </para>
/// </summary>
public class LocalArtifactStorage : IArtifactStorage
{
    private readonly string _root;
    private readonly ILogger<LocalArtifactStorage> _logger;

    private const string ArtifactsFolder = "artifacts";
    private const string ThumbnailSuffix = "_thumb";

    public LocalArtifactStorage(string root, ILogger<LocalArtifactStorage> logger)
    {
        _root = root;
        _logger = logger;
    }

    public Task<string> PutAsync(string sha256, string extension, Stream content, CancellationToken ct = default) =>
        WriteAsync(RelativePath(sha256, extension, thumbnail: false), content, ct);

    public Task<string> PutThumbnailAsync(string sha256, string extension, Stream content, CancellationToken ct = default) =>
        WriteAsync(RelativePath(sha256, extension, thumbnail: true), content, ct);

    private async Task<string> WriteAsync(string relativePath, Stream content, CancellationToken ct)
    {
        var absolute = Absolute(relativePath);

        // Content-addressed: same hash, same bytes. A second write would be
        // byte-identical, so skip it rather than risk a torn concurrent write.
        if (File.Exists(absolute))
            return relativePath;

        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

        // Write to a temp name and move into place, so a crash mid-write cannot
        // leave a truncated file sitting at a valid content address forever.
        var temp = absolute + ".tmp-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            await using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                if (content.CanSeek) content.Position = 0;
                await content.CopyToAsync(file, ct);
            }

            File.Move(temp, absolute, overwrite: false);
        }
        catch (IOException) when (File.Exists(absolute))
        {
            // Another request stored the same content first. Identical bytes —
            // theirs is as good as ours.
            _logger.LogDebug("Artifact {Path} already written concurrently", relativePath);
        }
        finally
        {
            if (File.Exists(temp))
            {
                try { File.Delete(temp); } catch { /* best effort */ }
            }
        }

        return relativePath;
    }

    public Task<Stream?> OpenAsync(string relativePath, CancellationToken ct = default)
    {
        var absolute = Absolute(relativePath);
        if (!File.Exists(absolute))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default) =>
        Task.FromResult(File.Exists(Absolute(relativePath)));

    private static string RelativePath(string sha256, string extension, bool thumbnail)
    {
        var shard = sha256[..2];
        var name = thumbnail ? sha256 + ThumbnailSuffix : sha256;
        return $"{ArtifactsFolder}/{shard}/{name}{extension}";
    }

    /// <summary>
    /// Resolve to an absolute path, refusing anything that escapes the root.
    /// Paths come from the database, but a traversal bug upstream must not turn
    /// into arbitrary file reads.
    /// </summary>
    private string Absolute(string relativePath)
    {
        var combined = Path.GetFullPath(Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootFull = Path.GetFullPath(_root);

        if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Artifact path escapes the storage root: {relativePath}");

        return combined;
    }
}
