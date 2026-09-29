using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// An issued works report — frozen, hashed and citable (works-report.md §2.5).
/// <para>
/// Never updated after it is written. The next report shows what changed since
/// this one; it does not rewrite it, which is what lets it be forwarded,
/// printed and pulled in a dispute like a valuation report.
/// </para>
/// </summary>
public class tbl_WorksReport : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    public DateTime AsAt { get; set; }

    public DateTime WindowFrom { get; set; }

    /// <summary>Which side it was written for. A client-side report never carries Crew-channel material.</summary>
    public ProjectSide Audience { get; set; } = ProjectSide.Client;

    /// <summary>Null when the system issued it on its own (Law 0: nobody had to log in).</summary>
    [MaxLength(450)]
    public string? IssuedById { get; set; }

    public DateTime IssuedAt { get; set; }

    public ReportIssueKind IssueKind { get; set; } = ReportIssueKind.Manual;

    /// <summary>For a milestone: what set it off, in words.</summary>
    [MaxLength(300)]
    public string? IssueReason { get; set; }

    /// <summary>Identity of a milestone trigger, so the same stage completing never issues twice.</summary>
    [MaxLength(120)]
    public string? TriggerKey { get; set; }

    [MaxLength(1000)]
    public string? CoveringNote { get; set; }

    /// <summary>Every figure, date and state on the page, as assembled from the record.</summary>
    public string? SnapshotJson { get; set; }

    /// <summary>The descriptions, stored so an issued report never changes when a model does (§6.4).</summary>
    public string? NarrativeJson { get; set; }

    [MaxLength(64)]
    public string? ContentSha256 { get; set; }

    [MaxLength(40)]
    public string? PreviousReportId { get; set; }

    /// <summary>Who it reached, in words — "in the app for Peter, Dinah".</summary>
    [MaxLength(500)]
    public string? DeliveryNote { get; set; }

    public DateTime? DeliveredAt { get; set; }

    [ForeignKey("ProjectId")]
    public tbl_Project? Project { get; set; }

    [ForeignKey("IssuedById")]
    public AppUser? IssuedBy { get; set; }
}
