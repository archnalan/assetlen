using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One attempt to wake one device. Kept so "was he told?" has an answer, and so
/// the latency against WhatsApp is measured rather than asserted.
/// </summary>
public class tbl_PushDelivery : BaseEntity
{
    [MaxLength(40)]
    public string? SubscriptionId { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(40)]
    public string? ProjectId { get; set; }

    public PushKind Kind { get; set; }

    [MaxLength(120)]
    public string? Title { get; set; }

    [MaxLength(300)]
    public string? Body { get; set; }

    [MaxLength(300)]
    public string? Url { get; set; }

    public PushDeliveryStatus Status { get; set; }

    public DateTime QueuedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public int? HttpStatus { get; set; }

    [MaxLength(500)]
    public string? Error { get; set; }

    [ForeignKey("SubscriptionId")]
    public tbl_PushSubscription? Subscription { get; set; }
}
