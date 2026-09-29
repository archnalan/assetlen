using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A still frame and a length for a video (works-report.md §5.3). Without it
/// a fifth of the footage prints as blank tiles. The poster is itself an
/// artifact, stored once like any other file.
/// </summary>
public class tbl_ArtifactPoster : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    [MaxLength(40)]
    public string? PosterArtifactId { get; set; }

    /// <summary>Null when it could not be measured — shown as "length unknown", never as zero.</summary>
    public double? DurationSeconds { get; set; }

    public PosterStatus Status { get; set; } = PosterStatus.Pending;

    [MaxLength(500)]
    public string? Note { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }

    [ForeignKey("PosterArtifactId")]
    public tbl_Artifact? PosterArtifact { get; set; }
}
