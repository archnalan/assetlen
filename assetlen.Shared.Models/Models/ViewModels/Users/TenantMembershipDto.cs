namespace assetlen.Shared.Models.Models.ViewModels.Users;

/// <summary>
/// One account a person can act in (assetlen.md §10.2). Nalan works for several
/// developers and appears as a guest in each, on one login.
/// </summary>
public class TenantMembershipDto
{
    public string? TenantId { get; set; }

    /// <summary>The organisation name, for the picker.</summary>
    public string? TenantName { get; set; }

    /// <summary>Where this user lands at sign-in.</summary>
    public bool IsDefault { get; set; }

    /// <summary>True for the account the current token is scoped to.</summary>
    public bool IsCurrent { get; set; }

    /// <summary>Roles held in <em>this</em> account, comma-separated. Null falls back to the global roles.</summary>
    public string? Roles { get; set; }

    public DateTime? JoinedAt { get; set; }
}
