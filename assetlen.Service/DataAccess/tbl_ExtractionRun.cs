using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace assetlen.Service.DataAccess;

/// <summary>One pass of extraction over a project's ingested record — the receipt that makes the queue accountable.</summary>
public class tbl_ExtractionRun : BaseEntity
{
    [MaxLength(40)]
    public string? ProjectId { get; set; }

    [MaxLength(450)]
    public string? StartedById { get; set; }

    [MaxLength(40)]
    public string? Engine { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int MessagesRead { get; set; }

    public int ProposalsCreated { get; set; }

    public int ReadingsCreated { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
