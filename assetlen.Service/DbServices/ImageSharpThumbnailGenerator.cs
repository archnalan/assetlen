using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using assetlen.Service.DbServices.ServiceInterfaces;

namespace assetlen.Service.DbServices;

/// <summary>
/// ImageSharp thumbnailer. Pure managed — no native assets to deploy, which
/// matters because the API is hosted and the client is WASM.
/// </summary>
public class ImageSharpThumbnailGenerator : IThumbnailGenerator
{
    private readonly ILogger<ImageSharpThumbnailGenerator> _logger;

    public ImageSharpThumbnailGenerator(ILogger<ImageSharpThumbnailGenerator> logger) => _logger = logger;

    public bool CanHandle(string? mimeType) =>
        mimeType is not null && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    public async Task<(Stream Thumbnail, ImageDimensions Source)?> CreateAsync(
        Stream source, int maxEdge, CancellationToken ct = default)
    {
        try
        {
            if (source.CanSeek) source.Position = 0;

            using var image = await Image.LoadAsync(source, ct);
            var dimensions = new ImageDimensions(image.Width, image.Height);

            // Honour the EXIF orientation before measuring — a portrait photo
            // from a phone is stored landscape with a rotation flag, and a
            // thumbnail that ignores it comes out sideways.
            image.Mutate(x => x.AutoOrient());

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxEdge, maxEdge),
                Mode = ResizeMode.Max,     // preserve aspect; never upscale past the source
                Sampler = KnownResamplers.Lanczos3
            }));

            // Strip metadata: thumbnails travel to the client side, and EXIF
            // carries GPS coordinates the site crew never consented to share.
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;

            var output = new MemoryStream();
            await image.SaveAsync(output, new JpegEncoder { Quality = 78 }, ct);
            output.Position = 0;

            return (output, dimensions);
        }
        catch (UnknownImageFormatException)
        {
            // A PDF or a document. Legitimate artifact, no thumbnail.
            return null;
        }
        catch (InvalidImageContentException ex)
        {
            _logger.LogWarning(ex, "Artifact declared an image type but could not be decoded");
            return null;
        }
    }
}
