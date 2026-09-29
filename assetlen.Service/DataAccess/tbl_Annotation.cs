using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One version of one markup layer over an artifact (assetlen.md Law 2):
/// versioned, attributed, and never a new image. The original bytes are not
/// touched; a changed layer is a new row with the next <see cref="Version"/>,
/// and the one it replaced is kept.
/// </summary>
public class tbl_Annotation : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    /// <summary>The id of the layer's first version. Every version of one layer shares it.</summary>
    [MaxLength(40)]
    public string? LayerId { get; set; }

    public int Version { get; set; } = 1;

    [MaxLength(450)]
    public string? AuthorId { get; set; }

    public ProjectSide? AuthorSide { get; set; }

    /// <summary>Crew-only unless the author sits on the client side or a mediator exposes it (CLAUDE.md §5.4).</summary>
    public Channel Channel { get; set; } = Channel.Crew;

    /// <summary>The marks, as fractions of the image — see <c>AnnotationShapeDto</c>.</summary>
    public string? ShapesJson { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    /// <summary>The commitment this layer asked about, when it raised a query.</summary>
    [MaxLength(40)]
    public string? CommitmentId { get; set; }

    /// <summary>Set when a newer version of the same layer replaced this one.</summary>
    public DateTime? SupersededAt { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }

    [ForeignKey("AuthorId")]
    public AppUser? Author { get; set; }

    [ForeignKey("CommitmentId")]
    public tbl_Commitment? Commitment { get; set; }
}
