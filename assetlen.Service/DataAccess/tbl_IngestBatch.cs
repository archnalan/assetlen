using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One import run — the receipt for a trip through the front door (plan.md P3).
/// <para>
/// A batch exists so an import is <b>accountable</b>. Peter drops a year of
/// history in once; if the run silently half-worked he has no way to tell an
/// export that contained nothing from an importer that dropped it. The counts
/// here are what the UI reports back, and what the exit assertions read.
/// </para>
/// </summary>
public class tbl_IngestBatch : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    public IngestSourceType SourceType { get; set; } = IngestSourceType.WhatsAppExport;

    public IngestBatchStatus Status { get; set; } = IngestBatchStatus.Previewed;

    /// <summary>
    /// The uploaded archive, stored as an ordinary artifact.
    /// <para>
    /// Law 2 applied to the export itself: the same <c>.zip</c> uploaded twice is
    /// one artifact, so a second attempt at the same import is recognisable
    /// before a single row is written. It also means the commit step re-reads the
    /// bytes from the store rather than holding a hundred megabytes in memory
    /// between two HTTP calls.
    /// </para>
    /// </summary>
    [MaxLength(40)]
    public string? ArchiveArtifactId { get; set; }

    [MaxLength(260)]
    public string? OriginalFileName { get; set; }

    [MaxLength(450)]
    public string? ImportedById { get; set; }

    /// <summary>
    /// Which side of the project imported this material, captured at import time.
    /// <para>
    /// <b>This is the read gate for every message in the batch.</b> It is stored
    /// rather than re-derived because a person's side can change — Peter can be
    /// stood down as mediator — and material must not become readable, or stop
    /// being readable, because of a later roster edit. A client-side import is
    /// Peter's own forwarded record and is exactly as private as his phone
    /// (assetlen.md §5); a contractor-side import is Site Log material.
    /// </para>
    /// </summary>
    public ProjectSide ImportedSide { get; set; } = ProjectSide.Client;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // ─── What the run found and what it did ──────────────────────────────
    // Parsed vs imported are separate numbers on purpose. "1,529 parsed, 0 new"
    // is a successful re-import; "1,529 parsed, 1,529 new" on a second run is
    // the duplicate bug the exit criterion tests for.

    public int ParsedMessageCount { get; set; }

    public int ImportedMessageCount { get; set; }

    /// <summary>Parsed, already present, skipped. The proof that re-import is safe.</summary>
    public int DuplicateMessageCount { get; set; }

    /// <summary>Messages that carried a media marker, whether or not the file was in the archive.</summary>
    public int MediaMessageCount { get; set; }

    public int NewArtifactCount { get; set; }

    /// <summary>Media files whose bytes matched an artifact already on the project. Law 2, proving itself.</summary>
    public int DuplicateArtifactCount { get; set; }

    /// <summary>
    /// Media markers with no matching file in the archive — the normal case for
    /// an "export without media", where every attachment is <c>&lt;Media omitted&gt;</c>.
    /// Reported rather than swallowed: it is the difference between a thin import
    /// and a broken one.
    /// </summary>
    public int UnmatchedMediaCount { get; set; }

    public int ParticipantCount { get; set; }

    public DateTime? FirstMessageAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public ExportDateOrder DateOrder { get; set; } = ExportDateOrder.AssumedDayFirst;

    /// <summary>Parser warnings and the failure reason, newline separated. Surfaced verbatim.</summary>
    [MaxLength(4000)]
    public string? Notes { get; set; }

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("ImportedById")]
    public AppUser? ImportedBy { get; set; }

    [ForeignKey("ArchiveArtifactId")]
    public tbl_Artifact? ArchiveArtifact { get; set; }
}
