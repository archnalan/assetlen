using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── Ingest DTOs (P3 — assetlen.md D3, Law 0) ────────────────────────────
//
// The front door. Peter forwards a year of history; Assetlen restructures it
// with no contractor involvement whatsoever. Import is deliberately two steps —
// preview, then commit — because the one irreversible thing here is attribution,
// and nobody should discover after the fact that 1,055 messages were filed
// against the wrong person.

/// <summary>
/// What an uploaded export contains, before anything is written.
/// <para>
/// Returned by the upload step. The archive is already stored (as an ordinary
/// artifact), the transcript is already parsed, but no message row exists yet.
/// Nothing here is a commitment to import.
/// </para>
/// </summary>
public class IngestPreviewDto
{
    /// <summary>The batch this preview belongs to. Pass it back to commit.</summary>
    public string? BatchId { get; set; }

    public string? ProjectId { get; set; }

    public string? OriginalFileName { get; set; }

    public IngestSourceType SourceType { get; set; } = IngestSourceType.WhatsAppExport;

    /// <summary>The canonical archive. Identical to a previous upload's when the same export is sent twice.</summary>
    public string? ArchiveArtifactId { get; set; }

    /// <summary>
    /// Which side this material will land on — the uploader's, resolved per
    /// project. Shown before the commit because it decides who can read the
    /// result: a delivery-side import is Site Log material and does not cross to
    /// the client, and Peter's own forwarded record does not cross the other way.
    /// </summary>
    public ProjectSide ImportedSide { get; set; } = ProjectSide.Client;

    public int MessageCount { get; set; }

    /// <summary>
    /// How many of those are already on this project. A re-import of an
    /// overlapping export should show a large number here and add almost
    /// nothing — that is the guarantee, stated before the user acts on it.
    /// </summary>
    public int AlreadyImportedCount { get; set; }

    public int NewMessageCount { get; set; }

    /// <summary>Messages carrying an attachment, whether or not its bytes are in the archive.</summary>
    public int MediaMessageCount { get; set; }

    /// <summary>Media files actually present in the archive and matched to a message.</summary>
    public int MediaFilesAvailable { get; set; }

    /// <summary>
    /// Attachments the transcript names but the archive does not contain. Normal
    /// and expected for an "export without media" — shown so a thin import is
    /// never mistaken for a broken one.
    /// </summary>
    public int MediaFilesMissing { get; set; }

    public DateTime? FirstMessageAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    /// <summary>How day and month were read. Surfaced because a wrong guess shifts a year.</summary>
    public ExportDateOrder DateOrder { get; set; }

    public List<IngestParticipantDto> Participants { get; set; } = new();

    /// <summary>Parser warnings, verbatim. Never suppressed.</summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// One name from the export, and who Assetlen thinks it is.
/// <para>
/// An export names people who may hold no login at all — the windows contractor,
/// the district planner. Mapping them to an off-platform member is what lets
/// attribution survive import (whatsapp-evidence.md §1).
/// </para>
/// </summary>
public class IngestParticipantDto
{
    /// <summary>The display name exactly as the export writes it.</summary>
    public string? ExternalAuthor { get; set; }

    public int MessageCount { get; set; }

    public int MediaCount { get; set; }

    /// <summary>An existing member whose name matches. A suggestion only — never applied on its own.</summary>
    public string? SuggestedMemberId { get; set; }

    public string? SuggestedMemberName { get; set; }

    /// <summary>True when a previous import of this project already mapped this name.</summary>
    public bool AlreadyMapped { get; set; }
}

/// <summary>Where one export name should be filed.</summary>
public class IngestAuthorMapDto
{
    [Required]
    public string? ExternalAuthor { get; set; }

    /// <summary>Map onto an existing member. Wins over <see cref="CreateAsPartyName"/>.</summary>
    public string? MemberId { get; set; }

    /// <summary>
    /// Create an off-platform member with this display name and map onto it —
    /// the path for a party who will never sign in.
    /// </summary>
    [MaxLength(200)]
    public string? CreateAsPartyName { get; set; }

    /// <summary>Side for a member created here. Ignored when <see cref="MemberId"/> is set.</summary>
    public ProjectSide Side { get; set; } = ProjectSide.Contractor;

    /// <summary>Specialization for a member created here.</summary>
    public ProjectMemberSpecialization Specialization { get; set; } = ProjectMemberSpecialization.Other;
}

/// <summary>Commit a previewed batch. Unmapped participants still import, unattributed.</summary>
public class IngestCommitDto
{
    [Required]
    public string? BatchId { get; set; }

    public List<IngestAuthorMapDto> AuthorMappings { get; set; } = new();
}

/// <summary>The receipt for one import run.</summary>
public class IngestBatchDto : BaseDto
{
    public string? ProjectId { get; set; }
    public IngestSourceType SourceType { get; set; }
    public IngestBatchStatus Status { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ArchiveArtifactId { get; set; }
    public string? ImportedById { get; set; }
    public string? ImportedByName { get; set; }
    public ProjectSide ImportedSide { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int ParsedMessageCount { get; set; }
    public int ImportedMessageCount { get; set; }
    public int DuplicateMessageCount { get; set; }
    public int MediaMessageCount { get; set; }
    public int NewArtifactCount { get; set; }

    /// <summary>The same bytes arriving twice. Law 2, measured.</summary>
    public int DuplicateArtifactCount { get; set; }

    public int UnmatchedMediaCount { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime? FirstMessageAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public ExportDateOrder DateOrder { get; set; }
    public string? Notes { get; set; }
}

/// <summary>One raw message, as it arrived.</summary>
public class IngestedMessageDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? BatchId { get; set; }
    public IngestSourceType SourceType { get; set; }

    /// <summary>The name the export used — kept even after mapping, because it is the evidence.</summary>
    public string? ExternalAuthor { get; set; }

    public string? AuthorMemberId { get; set; }

    /// <summary>Resolved display name of the mapped member, when there is one.</summary>
    public string? AuthorMemberName { get; set; }

    public ProjectSide? AuthorSide { get; set; }

    public DateTime SentAt { get; set; }
    public string? Body { get; set; }
    public string? ArtifactId { get; set; }
    public string? MediaFileName { get; set; }

    /// <summary>Thumbnail endpoint for an attached image, or null.</summary>
    public string? ThumbnailUrl { get; set; }

    public bool IsSystemMessage { get; set; }
    public int SequenceNo { get; set; }
}

/// <summary>A window onto the raw record. Paged — a year is 1,529 messages.</summary>
public class IngestedMessagePageDto
{
    public List<IngestedMessageDto> Messages { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

/// <summary>Filter for reading the raw record back.</summary>
public class IngestedMessageQueryDto
{
    [Required]
    public string? ProjectId { get; set; }

    public string? BatchId { get; set; }

    /// <summary>Free-text contains-match on the body. Full-text search proper arrives in P6.</summary>
    public string? Search { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    /// <summary>Only messages that carried an attachment.</summary>
    public bool MediaOnly { get; set; }

    public int Skip { get; set; }
    public int Take { get; set; } = 100;
}

/// <summary>
/// The project's inbound mail address — the ongoing-trickle door.
/// <para>
/// A capability, not an identifier: anyone holding the address can post into the
/// project, so it is revocable by reissue and never derived from the project id.
/// </para>
/// </summary>
public class ProjectInboxDto
{
    public string? ProjectId { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime? LastReceivedAt { get; set; }
    public int ReceivedCount { get; set; }
}

/// <summary>
/// One item pushed from a phone's share sheet. Sent as multipart: this describes
/// the scalar parts that travel beside the file.
/// </summary>
public class ShareCaptureDto
{
    [Required]
    public string? ProjectId { get; set; }

    [MaxLength(8000)]
    public string? Text { get; set; }

    /// <summary>When it was captured, if the sender knows. Defaults to now.</summary>
    public DateTime? SentAt { get; set; }
}

/// <summary>
/// An inbound email, as a mail provider's webhook posts it.
/// <para>
/// Authenticated by a shared secret header rather than a bearer token — the
/// caller is a mail relay, not a signed-in person. The project is addressed by
/// the key in the recipient, so a forged body cannot choose a project it does
/// not already hold the address for.
/// </para>
/// </summary>
public class InboundEmailDto
{
    /// <summary>The address it was sent to, carrying the project key.</summary>
    [Required]
    public string? To { get; set; }

    public string? From { get; set; }
    public string? Subject { get; set; }
    public string? TextBody { get; set; }
    public DateTime? SentAt { get; set; }

    public List<InboundEmailAttachmentDto> Attachments { get; set; } = new();
}

public class InboundEmailAttachmentDto
{
    [MaxLength(260)]
    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    /// <summary>
    /// Base64 payload — the shape every mail webhook provider uses. Tolerated
    /// here because the sender is a relay we do not control; the interactive
    /// upload paths remain multipart.
    /// </summary>
    public string? ContentBase64 { get; set; }
}
