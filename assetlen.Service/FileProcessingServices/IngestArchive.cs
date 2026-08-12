using System.IO.Compression;
using System.Text;

namespace assetlen.Service.FileProcessingServices;

/// <summary>
/// A WhatsApp export as delivered: either a <c>.zip</c> holding the transcript
/// beside its media, or a bare <c>.txt</c> transcript with no media at all.
/// <para>
/// Both shapes are real and both must work. "Export chat → Without media" is the
/// smaller, likelier file and the one the corpus itself is; "With media" is the
/// one that makes Law 2 worth anything, because that is where 723 attachments
/// live. Detection is by content, not by file extension — a phone that hands
/// over <c>chat.zip.txt</c>, or a browser that guesses the wrong content type,
/// must not silently produce a zero-message import.
/// </para>
/// </summary>
public sealed class IngestArchive : IDisposable
{
    private readonly ZipArchive? _zip;
    private readonly Stream _source;
    private readonly string? _plainText;
    private readonly Dictionary<string, ZipArchiveEntry> _media = new(StringComparer.OrdinalIgnoreCase);
    private readonly ZipArchiveEntry? _transcriptEntry;

    /// <summary>Files in the archive that are not the transcript.</summary>
    public int MediaFileCount => _media.Count;

    /// <summary>True when a transcript was found. False means this was not an export.</summary>
    public bool HasTranscript => _transcriptEntry is not null || _plainText is not null;

    private IngestArchive(Stream source, ZipArchive? zip, ZipArchiveEntry? transcript, string? plainText)
    {
        _source = source;
        _zip = zip;
        _transcriptEntry = transcript;
        _plainText = plainText;
    }

    /// <summary>
    /// Open an export. <paramref name="seekable"/> must stay open for the
    /// lifetime of the returned object — media entries are read lazily from it.
    /// </summary>
    public static IngestArchive Open(Stream seekable)
    {
        if (!seekable.CanSeek)
            throw new ArgumentException("An export must be opened from a seekable stream.", nameof(seekable));

        seekable.Position = 0;

        if (!LooksLikeZip(seekable))
        {
            seekable.Position = 0;
            using var reader = new StreamReader(seekable, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            return new IngestArchive(seekable, null, null, reader.ReadToEnd());
        }

        seekable.Position = 0;
        var zip = new ZipArchive(seekable, ZipArchiveMode.Read, leaveOpen: true);

        var transcript = PickTranscript(zip);

        var archive = new IngestArchive(seekable, zip, transcript, null);
        foreach (var entry in zip.Entries)
        {
            if (ReferenceEquals(entry, transcript)) continue;
            if (entry.Length == 0) continue;                   // directory placeholder

            // Keyed on the leaf only: the transcript names attachments without a
            // path, and archives from different phones nest them differently.
            var leaf = Path.GetFileName(entry.FullName);
            if (leaf.Length == 0) continue;

            archive._media.TryAdd(leaf, entry);
        }

        return archive;
    }

    /// <summary>
    /// The chosen transcript's text, or empty when the archive held none.
    /// <para>
    /// Read as UTF-8. WhatsApp always exports UTF-8, and forcing it is safer than
    /// letting the platform's default mangle every non-ASCII name in the thread.
    /// </para>
    /// </summary>
    public string ReadTranscript()
    {
        if (_plainText is not null) return _plainText;
        if (_transcriptEntry is null) return string.Empty;

        using var stream = _transcriptEntry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Open one attachment by the name the transcript used, or null when the
    /// archive does not contain it — the normal case for an export without media.
    /// </summary>
    public Stream? OpenMedia(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName) || _media.Count == 0) return null;

        // Only ever the leaf, and never a traversal: a crafted export must not be
        // able to name "../../appsettings.json" and have it read back out.
        var leaf = Path.GetFileName(fileName.Replace('\\', '/'));
        if (string.IsNullOrEmpty(leaf)) return null;

        return _media.TryGetValue(leaf, out var entry) ? entry.Open() : null;
    }

    /// <summary>True when this attachment's bytes are actually present.</summary>
    public bool HasMedia(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName) || _media.Count == 0) return false;
        var leaf = Path.GetFileName(fileName.Replace('\\', '/'));
        return !string.IsNullOrEmpty(leaf) && _media.ContainsKey(leaf);
    }

    /// <summary>
    /// The transcript is the largest <c>.txt</c>, with iOS's fixed
    /// <c>_chat.txt</c> preferred outright. Largest rather than first because an
    /// archive can also carry a short readme, and picking that yields a
    /// confidently empty import.
    /// </summary>
    private static ZipArchiveEntry? PickTranscript(ZipArchive zip)
    {
        ZipArchiveEntry? best = null;

        foreach (var entry in zip.Entries)
        {
            if (!entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) continue;

            if (string.Equals(Path.GetFileName(entry.FullName), "_chat.txt", StringComparison.OrdinalIgnoreCase))
                return entry;

            if (best is null || entry.Length > best.Length) best = entry;
        }

        return best;
    }

    private static bool LooksLikeZip(Stream stream)
    {
        Span<byte> header = stackalloc byte[4];
        var read = stream.Read(header);
        return read == 4 && header[0] == 0x50 && header[1] == 0x4B
            && (header[2] == 0x03 || header[2] == 0x05 || header[2] == 0x07);
    }

    public void Dispose() => _zip?.Dispose();
}
