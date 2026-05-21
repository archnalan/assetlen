using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

public class tbl_ProgressComment : BaseEntity
{
    [MaxLength(40)]
    public string? ProgressUpdateId { get; set; }

    [MaxLength(40)]
    public string? ProgressImageId { get; set; }

    [MaxLength(2000)]
    public string? CommentText { get; set; }

    [MaxLength(450)]
    public string? AuthorId { get; set; }

    [MaxLength(40)]
    public string? ParentCommentId { get; set; }

    // Navigation
    [ForeignKey("ProgressUpdateId")]
    public tbl_ProgressUpdate? ProgressUpdate { get; set; }

    [ForeignKey("ProgressImageId")]
    public tbl_ProgressImage? ProgressImage { get; set; }

    [ForeignKey("AuthorId")]
    public AppUser? Author { get; set; }

    [ForeignKey("ParentCommentId")]
    public tbl_ProgressComment? ParentComment { get; set; }

    [InverseProperty("ParentComment")]
    public ICollection<tbl_ProgressComment> Replies { get; set; } = new List<tbl_ProgressComment>();
}
