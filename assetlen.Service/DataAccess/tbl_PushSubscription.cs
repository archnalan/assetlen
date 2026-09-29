using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess;

/// <summary>
/// One browser or phone that asked to be woken (assetlen.md §9 — notification
/// speed equal to WhatsApp). Belongs to a person, not a project: which projects
/// may wake them is decided per event by their standing on each.
/// </summary>
public class tbl_PushSubscription : BaseEntity
{
    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(1000)]
    public string? Endpoint { get; set; }

    /// <summary>SHA-256 of the endpoint, so the unique index stays small however long the push service's URL is.</summary>
    [MaxLength(64)]
    public string? EndpointHash { get; set; }

    [MaxLength(200)]
    public string? P256dh { get; set; }

    [MaxLength(100)]
    public string? Auth { get; set; }

    [MaxLength(300)]
    public string? UserAgent { get; set; }

    public DateTime? LastSuccessAt { get; set; }

    public int ConsecutiveFailures { get; set; }

    [ForeignKey("UserId")]
    public AppUser? User { get; set; }
}
