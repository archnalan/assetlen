using mowt.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Service.DataAccess;

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

    // Navigation
    [ForeignKey("ProgressUpdateId")]
    public tbl_ProgressUpdate? ProgressUpdate { get; set; }

    [InverseProperty("ProgressImage")]
    public ICollection<tbl_ProgressComment> Comments { get; set; } = new List<tbl_ProgressComment>();
}
