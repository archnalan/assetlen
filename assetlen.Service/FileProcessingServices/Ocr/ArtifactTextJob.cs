using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices.Ocr;

/// <summary>Hands an artifact to the OCR queue. Never blocks the upload that produced it.</summary>
public interface IArtifactTextQueue
{
    void Enqueue(string artifactId);
}

public sealed class HangfireArtifactTextQueue : IArtifactTextQueue
{
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<HangfireArtifactTextQueue> _logger;

    public HangfireArtifactTextQueue(IBackgroundJobClient jobs, ILogger<HangfireArtifactTextQueue> logger)
    {
        _jobs = jobs;
        _logger = logger;
    }

    public void Enqueue(string artifactId)
    {
        try
        {
            _jobs.Enqueue<ArtifactTextJob>(j => j.RunAsync(artifactId, CancellationToken.None));
        }
        catch (Exception ex)
        {
            // A queue outage must not fail an upload; the row stays Pending and the
            // project's "read pending files" call picks it up later.
            _logger.LogWarning(ex, "Could not queue OCR for artifact {ArtifactId}", artifactId);
        }
    }
}

/// <summary>
/// Reads one artifact's text into <c>tbl_ArtifactText</c> (plan.md P5).
/// <para>
/// Runs outside any request, so there is no tenant on the context: every query
/// here ignores the tenant filter and names the row it wants by id, and the text
/// row is stamped with the artifact's own tenant.
/// </para>
/// </summary>
public sealed class ArtifactTextJob
{
    private readonly AssetlenDbContext _context;
    private readonly IArtifactStorage _storage;
    private readonly IOcrService _ocr;
    private readonly ILogger<ArtifactTextJob> _logger;

    public ArtifactTextJob(AssetlenDbContext context, IArtifactStorage storage, IOcrService ocr, ILogger<ArtifactTextJob> logger)
    {
        _context = context;
        _storage = storage;
        _ocr = ocr;
        _logger = logger;
    }

    [Queue("ocr")]
    [AutomaticRetry(Attempts = 1)]
    public async Task RunAsync(string artifactId, CancellationToken ct)
    {
        var artifact = await _context.tbl_Artifacts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == artifactId && a.IsDeleted != true, ct);
        if (artifact is null) return;

        var row = await _context.tbl_ArtifactTexts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.ArtifactId == artifactId, ct);

        if (row is null)
        {
            row = new tbl_ArtifactText
            {
                ArtifactId = artifact.Id,
                ProjectId = artifact.ProjectId,
                TenantId = artifact.TenantId
            };
            _context.tbl_ArtifactTexts.Add(row);
        }
        else if (row.Status is ArtifactTextStatus.Done or ArtifactTextStatus.NoText or ArtifactTextStatus.Unsupported)
        {
            return;
        }

        row.Attempts++;
        row.Engine = _ocr.IsImage(artifact.MimeType) ? _ocr.ImageEngine
            : _ocr.IsAudio(artifact.MimeType) ? _ocr.AudioEngine : "text";

        if (!_ocr.CanRead(artifact.MimeType))
        {
            // An image with no engine is a gap to fix, not a file with no words in it.
            row.Status = _ocr.IsImage(artifact.MimeType) || _ocr.IsAudio(artifact.MimeType)
                ? ArtifactTextStatus.EngineUnavailable : ArtifactTextStatus.Unsupported;
            row.Error = row.Status == ArtifactTextStatus.EngineUnavailable
                ? (_ocr.IsAudio(artifact.MimeType) ? "No transcription engine can read this recording on this server." : "No OCR engine is configured on this server.")
                : $"Nothing here reads {artifact.MimeType ?? "this type"}.";
            await _context.SaveChangesAsync(ct);
            return;
        }

        var temp = Path.Combine(Path.GetTempPath(), $"assetlen-ocr-{Guid.NewGuid():N}{Path.GetExtension(artifact.StoragePath) ?? ""}");
        try
        {
            await using (var source = await _storage.OpenAsync(artifact.StoragePath!, ct))
            {
                if (source is null)
                {
                    row.Status = ArtifactTextStatus.Failed;
                    row.Error = "The stored file is missing.";
                    await _context.SaveChangesAsync(ct);
                    return;
                }

                await using var target = File.Create(temp);
                await source.CopyToAsync(target, ct);
            }

            var result = await _ocr.ReadAsync(temp, artifact.MimeType, ct);
            row.ExtractedAt = DateTime.UtcNow;

            if (!result.Success)
            {
                row.Status = ArtifactTextStatus.Failed;
                row.Error = result.Error;
            }
            else
            {
                var text = result.Text?.Trim();
                row.Text = string.IsNullOrEmpty(text) ? null : text;
                row.CharCount = row.Text?.Length ?? 0;
                row.Status = row.CharCount == 0 ? ArtifactTextStatus.NoText : ArtifactTextStatus.Done;
                row.Error = null;
            }

            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR failed for artifact {ArtifactId}", artifactId);
            row.Status = ArtifactTextStatus.Failed;
            row.Error = ex.Message.Length > 900 ? ex.Message[..900] : ex.Message;
            await _context.SaveChangesAsync(ct);
        }
        finally
        {
            try { File.Delete(temp); } catch { /* temp files are swept by the OS */ }
        }
    }
}
