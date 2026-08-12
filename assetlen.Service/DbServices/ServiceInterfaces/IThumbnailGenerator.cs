namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>Intrinsic dimensions of a decoded image.</summary>
public readonly record struct ImageDimensions(int Width, int Height);

/// <summary>
/// Derives thumbnails on ingest. Kept behind an interface because the image
/// library is the one third-party dependency in the artifact path — swapping
/// it must not touch <c>ArtifactDAL</c>.
/// </summary>
public interface IThumbnailGenerator
{
    /// <summary>True when this generator can decode the given MIME type.</summary>
    bool CanHandle(string? mimeType);

    /// <summary>
    /// Read <paramref name="source"/> and write a downscaled JPEG to the
    /// returned stream, along with the original's dimensions. Returns null when
    /// the bytes are not a decodable image — a PDF receipt is a legitimate
    /// artifact with no thumbnail, not an error.
    /// <para>The caller owns and disposes the returned stream.</para>
    /// </summary>
    Task<(Stream Thumbnail, ImageDimensions Source)?> CreateAsync(
        Stream source, int maxEdge, CancellationToken ct = default);
}
