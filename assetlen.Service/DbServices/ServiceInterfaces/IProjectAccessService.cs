using assetlen.Service.DataAccess;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// The single authority on which projects a user may act in, which side of the
/// project they belong to, and whether they mediate between the two sides.
/// <para>
/// Access comes from two sources: project ownership (<c>InvestorId</c> /
/// <c>ProjectManagerId</c>) and an active <c>tbl_ProjectMember</c> row.
/// Sub-projects inherit both from their parent.
/// </para>
/// <para>
/// <b>Do not re-implement this check inside a DAL.</b> Four DALs each carried a
/// private <c>IsProjectStakeholder</c> that tested ownership only, so project
/// members — the client and the clerk of works — were visible on the dashboard
/// and got 403 everywhere else. See plan.md finding A1.
/// </para>
/// <para>
/// <b>Do not infer the side from a tenant-level role.</b> <c>UserRoles</c> is
/// global; sides are per project. Use <see cref="ResolveAsync(string?, string?, CancellationToken)"/>
/// and read <c>ProjectAccess.Side</c>.
/// </para>
/// </summary>
public interface IProjectAccessService
{
    /// <summary>
    /// Resolve level, side and mediator status in one pass. Prefer this over
    /// the boolean helpers whenever the caller needs to know <em>which</em>
    /// surface to serve, not merely whether to allow the call.
    /// </summary>
    Task<ProjectAccess> ResolveAsync(string? projectId, string? userId, CancellationToken ct = default);

    /// <inheritdoc cref="ResolveAsync(string?, string?, CancellationToken)"/>
    Task<ProjectAccess> ResolveAsync(tbl_Project? project, string? userId, CancellationToken ct = default);

    /// <summary>Resolve the caller's level in a project by id. One query.</summary>
    Task<ProjectAccessLevel> GetAccessAsync(string? projectId, string? userId, CancellationToken ct = default);

    /// <summary>
    /// Resolve against an already-loaded project. The caller must have
    /// <c>Include(p =&gt; p.ParentProject)</c>; membership still costs one query.
    /// </summary>
    Task<ProjectAccessLevel> GetAccessAsync(tbl_Project? project, string? userId, CancellationToken ct = default);

    Task<bool> CanReadAsync(string? projectId, string? userId, CancellationToken ct = default);
    Task<bool> CanReadAsync(tbl_Project? project, string? userId, CancellationToken ct = default);

    Task<bool> CanWriteAsync(string? projectId, string? userId, CancellationToken ct = default);
    Task<bool> CanWriteAsync(tbl_Project? project, string? userId, CancellationToken ct = default);

    /// <summary>Owner or manager only — funding, budget lines, membership.</summary>
    Task<bool> CanManageAsync(string? projectId, string? userId, CancellationToken ct = default);
    Task<bool> CanManageAsync(tbl_Project? project, string? userId, CancellationToken ct = default);

    /// <summary>
    /// May this user move material from the Site Log to the Client channel?
    /// True for mediators and for owner/manager authority. This is the gate on
    /// every exposure decision — see <c>ArtifactDAL.SetRefChannel</c>.
    /// </summary>
    Task<bool> CanExposeToClientAsync(tbl_Project? project, string? userId, CancellationToken ct = default);
}
