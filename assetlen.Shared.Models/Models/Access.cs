namespace assetlen.Shared.Models.Models;

/// <summary>
/// Controls row-level visibility across tenants.
/// Default (null / Private) = visible only to the owning tenant.
/// </summary>
public enum Access
{
    /// <summary>Only the owning tenant can see this row (default).</summary>
    Private = 0,

    /// <summary>Visible to tenants that have been explicitly shared with.</summary>
    Shared = 1,

    /// <summary>Visible to every tenant.</summary>
    Public = 2,

    /// <summary>Hidden from all normal queries (system/internal use only).</summary>
    Protected = 3
}
