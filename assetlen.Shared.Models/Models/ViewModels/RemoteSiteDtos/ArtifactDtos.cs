using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

// ─── Artifact DTOs (P2 — assetlen.md Law 2) ──────────────────────────────
//
// The artifact is the file. The ref is a use of the file, and carries the
// caption, the ordering and the exposure. Nothing here transports bytes:
// upload is multipart, download is a streaming endpoint.

public class ArtifactDto : BaseDto
{
    public string? ProjectId { get; set; }
    public string? Sha256 { get; set; }
    public long ByteSize { get; set; }
    public string? MimeType { get; set; }
    public string? OriginalFileName { get; set; }
    public string? UploadedById { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime? CapturedAt { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    /// <summary>Streaming URL for the original — <c>/api/Artifacts/{id}/content</c>.</summary>
    public string? Url { get; set; }

    /// <summary>Streaming URL for the thumbnail, or null when the type has none.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// True when this upload matched an existing hash and no new artifact was
    /// created. The UI says <em>"this is already on the project"</em> instead
    /// of silently making a second copy.
    /// </summary>
    public bool WasDeduplicated { get; set; }

    /// <summary>How many places currently point at this artifact.</summary>
    public int ReferenceCount { get; set; }
}

/// <summary>One use of an artifact, with its own visibility.</summary>
public class ArtifactRefDto : BaseDto
{
    public string? ArtifactId { get; set; }
    public string? ProjectId { get; set; }
    public ArtifactTargetType TargetType { get; set; }
    public string? TargetId { get; set; }
    public Channel Channel { get; set; } = Channel.Crew;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public string? ExposedById { get; set; }
    public string? ExposedByName { get; set; }
    public DateTime? ExposedAt { get; set; }

    /// <summary>The file itself. Always populated on read.</summary>
    public ArtifactDto? Artifact { get; set; }
}

public class ArtifactRefCreateDto
{
    [Required]
    public string? ArtifactId { get; set; }

    [Required]
    public ArtifactTargetType TargetType { get; set; } = ArtifactTargetType.ProgressUpdate;

    [Required]
    public string? TargetId { get; set; }

    [MaxLength(300)]
    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }
}

/// <summary>
/// Expose or withdraw one artifact use. This is the mediator's decision and
/// the only way material crosses to the client side.
/// </summary>
public class ArtifactExposureDto
{
    [Required]
    public string? RefId { get; set; }

    public Channel Channel { get; set; } = Channel.Crew;
}

/// <summary>Expose or withdraw several uses at once — the batch curation path.</summary>
public class ArtifactExposureBatchDto
{
    [Required]
    public List<string> RefIds { get; set; } = new();

    public Channel Channel { get; set; } = Channel.Crew;
}

// ─── Controlled documents and revisions (amendment E2) ───────────────────

public class DocumentDto : BaseDto
{
    public string? ProjectId { get; set; }
    public DocumentKind Kind { get; set; }
    public string? Title { get; set; }
    public Channel Channel { get; set; } = Channel.Crew;
    public string? CurrentRevisionId { get; set; }

    /// <summary>Newest first. The first entry is the current revision.</summary>
    public List<ArtifactRevisionDto> Revisions { get; set; } = new();
}

public class ArtifactRevisionDto : BaseDto
{
    public string? DocumentId { get; set; }
    public string? ArtifactId { get; set; }
    public int RevisionNo { get; set; }
    public string? IssuedById { get; set; }
    public string? IssuedByName { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Notes { get; set; }
    public string? SupersededByRevisionId { get; set; }
    public bool IsCurrent { get; set; }
    public ArtifactDto? Artifact { get; set; }
}

public class DocumentCreateDto
{
    [Required]
    public string? ProjectId { get; set; }

    public DocumentKind Kind { get; set; } = DocumentKind.Other;

    [Required, MaxLength(200)]
    public string? Title { get; set; }

    public Channel Channel { get; set; } = Channel.Crew;
}

public class ArtifactRevisionCreateDto
{
    [Required]
    public string? DocumentId { get; set; }

    [Required]
    public string? ArtifactId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
