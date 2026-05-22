using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_ProgressImage : BaseEntity
{
    [MaxLength(40)]
    public string? ProgressUpdateId { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    [MaxLength(300)]
    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>
    /// Per-image channel override. Null = inherit from the parent
    /// ProgressUpdate's Channel. Use a non-null value when a single image
    /// should be promoted to Client (or kept Crew-only) independently of
    /// the rest of the entry.
    /// </summary>
    public Channel? Channel { get; set; }

    // Navigation
    [ForeignKey("ProgressUpdateId")]
    public tbl_ProgressUpdate? ProgressUpdate { get; set; }

    [InverseProperty("ProgressImage")]
    public ICollection<tbl_ProgressComment> Comments { get; set; } = new List<tbl_ProgressComment>();

    [InverseProperty("ProgressImage")]
    public ICollection<tbl_Flag> Flags { get; set; } = new List<tbl_Flag>();
}
