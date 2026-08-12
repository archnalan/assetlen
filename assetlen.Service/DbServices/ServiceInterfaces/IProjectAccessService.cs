using assetlen.Service.DataAccess;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// The single authority on which projects a user may act in.
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
/// </summary>
public interface IProjectAccessService
{
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

    /// <summary>Owner or manager only — funding, budget lines, membership, publishing.</summary>
    Task<bool> CanManageAsync(string? projectId, string? userId, CancellationToken ct = default);
    Task<bool> CanManageAsync(tbl_Project? project, string? userId, CancellationToken ct = default);
}
