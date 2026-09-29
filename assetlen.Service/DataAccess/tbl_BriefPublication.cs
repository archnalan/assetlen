using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// The day a project's curated frames crossed to the client side. The brief
/// publishes at the cutoff whether or not the mediator touched it (assetlen.md §5);
/// this row is how the cutoff knows it already has.
/// </summary>
public class tbl_BriefPublication : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    /// <summary>The local day published, at midnight.</summary>
    public DateTime Day { get; set; }

    public DateTime PublishedAt { get; set; }

    public BriefPublishTrigger Trigger { get; set; }

    /// <summary>The person who pressed publish, or the mediator named on a cutoff.</summary>
    [MaxLength(450)]
    public string? PublishedById { get; set; }

    public int FramesExposed { get; set; }

    public int FramesDropped { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("PublishedById")]
    public AppUser? PublishedBy { get; set; }
}
