using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One frame used on a Site Log entry: a pointer to a <see cref="tbl_Artifact"/>
/// plus its caption, its order, and — the part that matters — <b>its own
/// visibility</b>.
/// <para>
/// This row owns no bytes. Before P2 it carried a base64 data URI, so the same
/// photo posted twice was stored twice and there was no way to point at one.
/// </para>
/// <para>
/// <b><see cref="Channel"/> is enforced.</b> It existed before P2 and no query
/// read it, so promoting an entry pushed every one of its photos to the client.
/// Given a real capture is thirteen to eighteen frames at a time, that toggle
/// was a batch forward wearing a different name. Exposure is per frame now.
/// </para>
/// <para>
/// Comments and Flags hang off this row rather than off the artifact, because a
/// question is about <em>this use</em> of a photo on <em>this entry</em>. P5
/// folds this into <see cref="tbl_ArtifactRef"/> once the Brief needs one
/// uniform ref type across entries, commitments and brief blocks.
/// </para>
/// </summary>
public class tbl_ProgressImage : BaseEntity
{
    [MaxLength(40)]
    public string? ProgressUpdateId { get; set; }

    /// <summary>The canonical file. Null only on rows predating the artifact store.</summary>
    [MaxLength(40)]
    public string? ArtifactId { get; set; }

    /// <summary>
    /// Legacy inline URL. Retained so a row without an
    /// <see cref="ArtifactId"/> still renders; new captures never set it.
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    [MaxLength(300)]
    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>
    /// Visibility of this frame. Default <see cref="Channel.Crew"/> —
    /// fail-closed. Only a mediator (or owner/manager authority) may move it to
    /// <see cref="Channel.Client"/>; see <c>ProgressDAL.SetImageChannel</c>.
    /// </summary>
    public Channel Channel { get; set; } = Channel.Crew;

    /// <summary>Who exposed this frame to the client side, and when.</summary>
    [MaxLength(450)]
    public string? ExposedById { get; set; }

    public DateTime? ExposedAt { get; set; }

    // Navigation
    [ForeignKey("ProgressUpdateId")]
    public tbl_ProgressUpdate? ProgressUpdate { get; set; }

    [ForeignKey("ArtifactId")]
    public tbl_Artifact? Artifact { get; set; }

    [ForeignKey("ExposedById")]
    public AppUser? ExposedBy { get; set; }

    [InverseProperty("ProgressImage")]
    public ICollection<tbl_ProgressComment> Comments { get; set; } = new List<tbl_ProgressComment>();

    [InverseProperty("ProgressImage")]
    public ICollection<tbl_Flag> Flags { get; set; } = new List<tbl_Flag>();
}
