using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices.Report;

/// <summary>Hands a video to the poster queue. Never blocks the upload that produced it.</summary>
public interface IVideoPosterQueue
{
    void Enqueue(string artifactId);
}

public sealed class HangfireVideoPosterQueue : IVideoPosterQueue
{
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<HangfireVideoPosterQueue> _logger;

    public HangfireVideoPosterQueue(IBackgroundJobClient jobs, ILogger<HangfireVideoPosterQueue> logger)
    {
        _jobs = jobs;
        _logger = logger;
    }

    public void Enqueue(string artifactId)
    {
        try
        {
            _jobs.Enqueue<VideoPosterJob>(j => j.RunAsync(artifactId, CancellationToken.None));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not queue a poster for artifact {ArtifactId}", artifactId);
        }
    }
}

/// <summary>
/// Extracts a still frame and the length of a video with ffmpeg (works-report.md §5.3).
/// <para>
/// The frame becomes the video's thumbnail, so every surface that already
/// shows thumbnails shows the poster, through the same visibility checks. With
/// no ffmpeg on the server the row says so, and the report prints
/// "length unknown" rather than a blank tile or a zero.
/// </para>
/// </summary>
public sealed class VideoPosterJob
{
    private readonly AssetlenDbContext _context;
    private readonly IArtifactStorage _storage;
    private readonly IThumbnailGenerator _thumbnails;
    private readonly ILogger<VideoPosterJob> _logger;
    private readonly string? _ffmpeg;
    private readonly string? _ffprobe;

    public VideoPosterJob(AssetlenDbContext context, IArtifactStorage storage, IThumbnailGenerator thumbnails,
        IConfiguration config, ILogger<VideoPosterJob> logger)
    {
        _context = context;
        _storage = storage;
        _thumbnails = thumbnails;
        _logger = logger;
        _ffmpeg = Locate(config["Media:FfmpegPath"], "ffmpeg");
        _ffprobe = Locate(config["Media:FfprobePath"], "ffprobe")
                   ?? (_ffmpeg is null ? null : Locate(Path.Combine(Path.GetDirectoryName(_ffmpeg) ?? "", "ffprobe" + ExeSuffix), "ffprobe"));
    }

    private static string ExeSuffix => OperatingSystem.IsWindows() ? ".exe" : "";

    public static bool IsVideo(string? mime) => mime is not null && mime.StartsWith("video/", StringComparison.OrdinalIgnoreCase);

    [AutomaticRetry(Attempts = 1)]
    public async Task RunAsync(string artifactId, CancellationToken ct)
    {
        var video = await _context.tbl_Artifacts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == artifactId && a.IsDeleted != true, ct);
        if (video is null || !IsVideo(video.MimeType) || video.StoragePath is null) return;

        var row = await _context.tbl_ArtifactPosters.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ArtifactId == artifactId, ct);
        if (row is null)
        {
            row = new tbl_ArtifactPoster { ArtifactId = video.Id, ProjectId = video.ProjectId, TenantId = video.TenantId };
            _context.tbl_ArtifactPosters.Add(row);
        }
        else if (row.Status == PosterStatus.Done) return;

        if (_ffmpeg is null)
        {
            row.Status = PosterStatus.EngineUnavailable;
            row.Note = "No ffmpeg on this server; set Media:FfmpegPath. The video is kept and its length is unknown.";
            await _context.SaveChangesAsync(ct);
            return;
        }

        var work = Path.Combine(Path.GetTempPath(), "assetlen-poster-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(work);
        try
        {
            var ext = Path.GetExtension(video.StoragePath);
            var input = Path.Combine(work, "in" + (string.IsNullOrEmpty(ext) ? ".mp4" : ext));
            await using (var src = await _storage.OpenAsync(video.StoragePath, ct))
            {
                if (src is null)
                {
                    row.Status = PosterStatus.Failed;
                    row.Note = "The stored file could not be read.";
                    await _context.SaveChangesAsync(ct);
                    return;
                }
                await using var dst = File.Create(input);
                await src.CopyToAsync(dst, ct);
            }

            row.DurationSeconds = await ProbeDurationAsync(input, ct);

            // A second in, where a phone video has usually stopped being a blur;
            // the very first frame for a clip shorter than that.
            var poster = Path.Combine(work, "poster.jpg");
            var seek = row.DurationSeconds is > 1.5 ? "1" : "0";
            var (code, err) = await ExecAsync(_ffmpeg, ["-hide_banner", "-loglevel", "error", "-ss", seek, "-i", input, "-frames:v", "1", "-q:v", "3", "-y", poster], ct);
            if (code != 0 || !File.Exists(poster))
            {
                row.Status = PosterStatus.Failed;
                row.Note = Cap($"ffmpeg could not read a frame: {err}", 500);
                await _context.SaveChangesAsync(ct);
                return;
            }

            var bytes = await File.ReadAllBytesAsync(poster, ct);
            var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var posterArtifact = await _context.tbl_Artifacts.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Sha256 == sha && a.ProjectId == video.ProjectId, ct);
            var thumbPath = posterArtifact?.ThumbnailPath;
            if (posterArtifact is null)
            {
                string storage;
                await using (var ms = new MemoryStream(bytes)) storage = await _storage.PutAsync(sha, ".jpg", ms, ct);
                int? w = null, h = null;
                await using (var ms = new MemoryStream(bytes))
                {
                    var generated = await _thumbnails.CreateAsync(ms, 480, ct);
                    if (generated is { } g)
                    {
                        await using var t = g.Thumbnail;
                        thumbPath = await _storage.PutThumbnailAsync(sha, ".jpg", t, ct);
                        w = g.Source.Width;
                        h = g.Source.Height;
                    }
                }
                posterArtifact = new tbl_Artifact
                {
                    ProjectId = video.ProjectId,
                    TenantId = video.TenantId,
                    Sha256 = sha,
                    ByteSize = bytes.Length,
                    MimeType = "image/jpeg",
                    StoragePath = storage,
                    ThumbnailPath = thumbPath,
                    OriginalFileName = Cap(Path.GetFileNameWithoutExtension(video.OriginalFileName ?? "video") + " (poster).jpg", 260),
                    UploadedById = video.UploadedById,
                    CapturedAt = video.CapturedAt,
                    Width = w,
                    Height = h
                };
                _context.tbl_Artifacts.Add(posterArtifact);
                await _context.SaveChangesAsync(ct);
            }

            video.ThumbnailPath ??= thumbPath;
            row.PosterArtifactId = posterArtifact.Id;
            row.Status = PosterStatus.Done;
            row.Note = row.DurationSeconds is null ? "Frame read; length could not be measured." : null;
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Poster failed for {ArtifactId}", artifactId);
            row.Status = PosterStatus.Failed;
            row.Note = Cap(ex.Message, 500);
            await _context.SaveChangesAsync(ct);
        }
        finally
        {
            try { Directory.Delete(work, true); } catch { /* temp folder */ }
        }
    }

    private async Task<double?> ProbeDurationAsync(string input, CancellationToken ct)
    {
        if (_ffprobe is null) return null;
        var (code, output) = await ExecAsync(_ffprobe, ["-v", "error", "-show_entries", "format=duration", "-of", "default=noprint_wrappers=1:nokey=1", input], ct, stdout: true);
        return code == 0 && double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) && d > 0
            ? Math.Round(d, 1) : null;
    }

    private static async Task<(int Code, string Text)> ExecAsync(string exe, string[] args, CancellationToken ct, bool stdout = false)
    {
        var psi = new ProcessStartInfo(exe)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
        var outTask = p.StandardOutput.ReadToEndAsync(ct);
        var errTask = p.StandardError.ReadToEndAsync(ct);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));
        try
        {
            await p.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            try { p.Kill(true); } catch { /* already gone */ }
            return (-1, "timed out");
        }
        return (p.ExitCode, stdout ? await outTask : await errTask);
    }

    /// <summary>A configured path, else the tool on the PATH, else null.</summary>
    private static string? Locate(string? configured, string tool)
    {
        if (!string.IsNullOrWhiteSpace(configured)) return File.Exists(configured) ? configured : null;
        var name = tool + ExeSuffix;
        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(dir.Trim(), name);
                if (File.Exists(candidate)) return candidate;
            }
            catch { /* malformed PATH entry */ }
        }
        return null;
    }

    private static string Cap(string s, int max) => s.Length <= max ? s : s[..max];
}
