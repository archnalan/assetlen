using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

/// <summary>
/// One mark on a layer. Every coordinate is a fraction (0–1) of the original's
/// width or height, so a layer drawn on a phone lands on the same line of the
/// receipt on a desktop.
/// </summary>
public class AnnotationShapeDto
{
    public AnnotationShapeKind Kind { get; set; }

    /// <summary>Ellipse and rect: the bounding box. Arrow: from (X, Y) to (X + W, Y + H).</summary>
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
    public double H { get; set; }

    /// <summary>Path only: x0, y0, x1, y1, …</summary>
    public List<double>? Points { get; set; }
}

/// <summary>One version of one layer. The original file is never touched (Law 2).</summary>
public class AnnotationDto : BaseDto
{
    public string? LayerId { get; set; }
    public int Version { get; set; }
    public int VersionCount { get; set; }
    public bool IsCurrent { get; set; }

    public string? ArtifactId { get; set; }
    public string? ProjectId { get; set; }

    /// <summary>
    /// The author — or, for a client-side reader looking at a delivery-side
    /// layer, the accountable face (assetlen.md §10.1).
    /// </summary>
    public string? AuthorName { get; set; }
    public ProjectSide? AuthorSide { get; set; }
    public bool IsMine { get; set; }

    public Channel Channel { get; set; }
    public List<AnnotationShapeDto> Shapes { get; set; } = new();

    /// <summary>The question asked, or a caption.</summary>
    public string? Note { get; set; }

    public string? CommitmentId { get; set; }
    public string? CommitmentTitle { get; set; }
    public CommitmentQueryState? CommitmentQueryState { get; set; }

    /// <summary>The current statement of that commitment — after a resolution it has a new id.</summary>
    public string? CurrentCommitmentId { get; set; }

    public bool CanEdit { get; set; }
    public bool CanExpose { get; set; }
}

/// <summary>The original plus every layer this reader may see over it.</summary>
public class AnnotatedArtifactDto
{
    public ArtifactDto? Artifact { get; set; }
    public List<AnnotationDto> Layers { get; set; } = new();

    /// <summary>Earlier versions of the layers above, newest first. Never deleted.</summary>
    public List<AnnotationDto> History { get; set; } = new();

    public bool CanAnnotate { get; set; }

    /// <summary>Whether this reader may turn a circle into a question on the register.</summary>
    public bool CanAsk { get; set; }

    /// <summary>Commitments this file is already linked to — the likely subject of a question.</summary>
    public List<CommitmentLinkDto> LinkedCommitments { get; set; } = new();
}

public class AnnotationSaveDto
{
    [Required, MaxLength(40)] public string? ArtifactId { get; set; }

    /// <summary>Absent for a new layer; present to add a version to one of your own.</summary>
    [MaxLength(40)] public string? LayerId { get; set; }

    public List<AnnotationShapeDto> Shapes { get; set; } = new();
    [MaxLength(1000)] public string? Note { get; set; }
}

/// <summary>Circle the thing, ask why — the query lands on the commitment, the circle stays on the file.</summary>
public class AnnotationAskDto
{
    [Required, MaxLength(40)] public string? ArtifactId { get; set; }
    [Required, MaxLength(40)] public string? CommitmentId { get; set; }
    public List<AnnotationShapeDto> Shapes { get; set; } = new();
    [Required, MaxLength(1000)] public string? Question { get; set; }
}

public class AnnotationExposeDto
{
    [Required, MaxLength(40)] public string? LayerId { get; set; }
    public Channel Channel { get; set; }
}

/// <summary>The markup a question on a commitment was asked with.</summary>
public class AskResultDto
{
    public AnnotationDto? Annotation { get; set; }
    public CommitmentDto? Commitment { get; set; }
}
