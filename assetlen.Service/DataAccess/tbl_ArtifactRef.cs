using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// A pointer to a <see cref="tbl_Artifact"/> from somewhere it is used, and
/// <b>the unit of exposure</b>.
/// <para>
/// This is where the mediator's decision lives. A capture lands with every ref
/// on <see cref="Channel.Crew"/> — fail-closed. The architect then exposes
/// <em>individual</em> frames to <see cref="Channel.Client"/>. Promoting the
/// parent entry no longer drags eighteen photos across with it: exposure is
/// per-artifact, because the observed behaviour it replaces is forwarding a
/// batch, and a per-entry switch reproduces exactly that.
/// </para>
/// <para>
/// Nothing here copies bytes. The same artifact may hold a Crew ref on a Site
/// Log entry and a Client ref on a brief block simultaneously.
/// </para>
/// </summary>
public class tbl_ArtifactRef : BaseEntity
{
    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    /// <summary>Denormalised from the artifact so exposure queries need no join.</summary>
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    public ArtifactTargetType TargetType { get; set; } = ArtifactTargetType.ProgressUpdate;

    [MaxLength(40)]
    public string? TargetId { get; set; }

    /// <summary>
    /// Visibility of <em>this use</em> of the artifact. Default
    /// <see cref="Channel.Crew"/> — nothing reaches the client side until a
    /// mediator says so. Enforced in every read path; see
    /// <c>ArtifactDAL.VisibleRefsQuery</c>.
    /// </summary>
    public Channel Channel { get; set; } = Channel.Crew;

    [MaxLength(300)]
    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>Who moved this across the channel boundary, and when. Null while Crew-only.</summary>
    [MaxLength(450)]
    public string? ExposedById { get; set; }

    public DateTime? ExposedAt { get; set; }

    // Navigation
    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }

    [ForeignKey("ExposedById")]
    public AppUser? ExposedBy { get; set; }
}
