using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One message as it arrived, raw and immutable (plan.md P3).
/// <para>
/// <b>Extraction reads this; nothing else writes it.</b> The whole tier-1 thesis
/// is that Peter's year of history already exists and already reached him — it
/// is simply unreadable. This table is that history, landed verbatim, so P5 can
/// propose commitments from it and P6 can search it. It is deliberately a dumb
/// record: no maturity, no state, no curation. Interpretation happens later and
/// somewhere else, and can be re-run without re-importing.
/// </para>
/// <para>
/// Nothing here is ever edited. The corpus contains fourteen edits and ten
/// deletions (<c>whatsapp-evidence.md</c> F6), and a typo inverted a design
/// verdict for nine hours — an append-only record is the point, not an
/// implementation detail.
/// </para>
/// </summary>
public class tbl_IngestedMessage : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>The run that brought this in. Null only for rows predating a batch record.</summary>
    [MaxLength(40)]
    public string? BatchId { get; set; }

    public IngestSourceType SourceType { get; set; } = IngestSourceType.WhatsAppExport;

    /// <summary>
    /// The author exactly as the export names them — a display name, a phone
    /// number, or the empty string for a system notice. Kept verbatim even after
    /// mapping, because the mapping is a claim about the export and the export is
    /// the evidence for it.
    /// </summary>
    [MaxLength(200)]
    public string? ExternalAuthor { get; set; }

    /// <summary>
    /// The <c>tbl_ProjectMember</c> this author was mapped to, if any.
    /// <para>
    /// An export names people who may never hold a login — the windows
    /// contractor, the district planner. Those map to an off-platform member row
    /// carrying <c>PartyName</c>, which is why attribution survives import at
    /// all. Null means nobody claimed this participant, and the messages still
    /// land: an unattributed record beats a dropped one.
    /// </para>
    /// </summary>
    [MaxLength(40)]
    public string? AuthorMemberId { get; set; }

    /// <summary>When it was sent, per the export. Not when it was imported.</summary>
    public DateTime SentAt { get; set; }

    [MaxLength(8000)]
    public string? Body { get; set; }

    /// <summary>The attachment, once stored. Null when the message was text, or its file was not in the archive.</summary>
    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    /// <summary>
    /// The attachment's name as the transcript referenced it, kept even when the
    /// file itself was absent. <c>IMG-20260716-WA0009.jpg</c> is the only handle
    /// on a photo an "export without media" mentions but does not contain.
    /// </summary>
    [MaxLength(260)]
    public string? MediaFileName { get; set; }

    /// <summary>True when the transcript line was a WhatsApp notice rather than a person speaking.</summary>
    public bool IsSystemMessage { get; set; }

    /// <summary>Position in the parsed transcript. The only reliable tiebreak within one minute.</summary>
    public int SequenceNo { get; set; }

    /// <summary>
    /// Identity of this message for re-import, hashed from
    /// <c>(SentAt, ExternalAuthor, Body, MediaFileName, occurrence)</c> and
    /// unique per project.
    /// <para>
    /// <b>The occurrence ordinal is load-bearing.</b> Android exports timestamp to
    /// the minute, and the corpus's normal posting pattern is thirteen to
    /// eighteen photos inside one — all from the same author, all with the body
    /// <c>&lt;Media omitted&gt;</c>. Without an ordinal those eighteen frames hash
    /// identically and an import silently keeps one. With it, the eighteen stay
    /// eighteen and a re-import of an overlapping export still adds nothing,
    /// because the same transcript yields the same ordinals.
    /// </para>
    /// </summary>
    [MaxLength(64)]
    public string? DedupeKey { get; set; }

    // Navigation
    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("BatchId")]
    public tbl_IngestBatch? Batch { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }

    [ForeignKey("AuthorMemberId")]
    public tbl_ProjectMember? AuthorMember { get; set; }
}
