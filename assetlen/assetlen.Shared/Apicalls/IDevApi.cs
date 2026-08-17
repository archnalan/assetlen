using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>
/// Development-only endpoints. Every one of them answers 404 outside a
/// Development host, so a deployed build exposes no surface at all — the check
/// is on the server, never on whether the client chose to call.
/// </summary>
public interface IDevApi
{
    /// <summary>
    /// Provisions the canonical demo world: Peter's account, his three
    /// counterparts, and <b>one</b> project — Kira Residence, with the guest
    /// wing as its sub-project and everything else a stage.
    /// <para>
    /// Idempotent. Re-running returns the same ids and creates nothing, which
    /// is what makes it safe to call before every persona sign-in.
    /// </para>
    /// </summary>
    [Post("/api/Dev/SeedDemo")]
    Task<IApiResponse<DevSeedResultDto>> SeedDemo();
}

public class DevSeedResultDto
{
    public bool Created { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? SubProjectId { get; set; }
    public string? TenantId { get; set; }
    public int StageCount { get; set; }
    public int MemberCount { get; set; }
    public List<string> Notes { get; set; } = new();
}
