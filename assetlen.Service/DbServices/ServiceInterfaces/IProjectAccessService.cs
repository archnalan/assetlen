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

    /// <summary>
    /// Resolve standing on many projects in one membership query, keyed by
    /// project id. For the dashboard, which renders every project the reader can
    /// see and needs each one's standing to decide what the card and its context
    /// menu may offer — resolving them one at a time is a query per tile.
    /// <para>Each project must carry its <c>ParentProject</c>, as for the single-project overload.</para>
    /// </summary>
    Task<IReadOnlyDictionary<string, ProjectAccess>> ResolveManyAsync(
        IEnumerable<tbl_Project> projects, string? userId, CancellationToken ct = default);

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
    /// May this user move material from the Site Diary to the Client channel?
    /// True for mediators and for owner/manager authority. This is the gate on
    /// every exposure decision — see <c>ArtifactDAL.SetRefChannel</c>.
    /// </summary>
    Task<bool> CanExposeToClientAsync(tbl_Project? project, string? userId, CancellationToken ct = default);
}

/// <summary>
/// The stage a newly created thing belongs to when the reader did not choose one.
/// <para>
/// Nothing on a project floats (CLAUDE.md §1). Asking "which stage?" on every
/// capture is the tax that drives people back to the chat, so the answer is
/// filled in and stays changeable.
/// </para>
/// </summary>
public interface IActiveStageService
{
    /// <summary>
    /// <paramref name="preferredStageId"/> when it is real and on this project,
    /// otherwise the project's active stage. Null only when the project has no
    /// stages at all.
    /// </summary>
    Task<string?> ResolveAsync(string? projectId, string? preferredStageId, CancellationToken ct = default);
}
