using System.Collections.Concurrent;
using System.Numerics;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using assetlen.Service.DbServices.ServiceInterfaces;

namespace assetlen.Service.FileProcessingServices.Brief;

/// <summary>
/// Which frames were taken from the same place. Seventeen chronological frames
/// read as <i>"Nothing much changed"</i>; one before/after pair from the same
/// vantage point does not (whatsapp-evidence.md F2), so the brief pairs them.
/// <para>
/// A 64-bit difference hash of the thumbnail: it keeps the large shapes of a
/// view — openings, roof line, horizon — and ignores the surface texture that
/// the work itself changes. Keyed by content hash, because an artifact's bytes
/// never change (Law 2); nothing is stored, so a better fingerprint later is a
/// code change and not a migration.
/// </para>
/// </summary>
public interface IVantageIndex
{
    /// <summary>The fingerprint, or null when the file is not a picture this server can read.</summary>
    Task<ulong?> FingerprintAsync(string sha256, string? mimeType, string? thumbnailPath, string? storagePath, CancellationToken ct = default);
}

public sealed class VantageIndex : IVantageIndex
{
    /// <summary>Below this fraction of differing bits two frames are called the same view.</summary>
    public const double SameViewThreshold = 0.25;

    private readonly IArtifactStorage _storage;
    private readonly ILogger<VantageIndex> _logger;
    private readonly ConcurrentDictionary<string, ulong?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public VantageIndex(IArtifactStorage storage, ILogger<VantageIndex> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<ulong?> FingerprintAsync(string sha256, string? mimeType, string? thumbnailPath, string? storagePath, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(sha256, out var known)) return known;
        if (mimeType is null || !mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return Remember(sha256, null);

        var path = thumbnailPath ?? storagePath;
        if (string.IsNullOrEmpty(path)) return Remember(sha256, null);

        try
        {
            await using var stream = await _storage.OpenAsync(path, ct);
            if (stream is null) return null;
            using var image = await Image.LoadAsync<L8>(stream, ct);
            return Remember(sha256, DifferenceHash(image));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No fingerprint for {Sha}", sha256);
            return Remember(sha256, null);
        }
    }

    private ulong? Remember(string sha, ulong? value)
    {
        _cache[sha] = value;
        return value;
    }

    public static ulong DifferenceHash(Image<L8> image)
    {
        image.Mutate(x => x.AutoOrient().Resize(new ResizeOptions { Size = new Size(9, 8), Mode = ResizeMode.Stretch }));

        ulong hash = 0;
        var bit = 0;
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++, bit++)
            if (image[x, y].PackedValue > image[x + 1, y].PackedValue)
                hash |= 1UL << bit;

        return hash;
    }

    /// <summary>0 for the same framing, 1 for every bit different.</summary>
    public static double Distance(ulong a, ulong b) => BitOperations.PopCount(a ^ b) / 64.0;
}
