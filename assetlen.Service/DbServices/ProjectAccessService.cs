using Microsoft.EntityFrameworkCore;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IProjectAccessService"/>
public class ProjectAccessService : IProjectAccessService
{
    private readonly AssetlenDbContext _context;

    public ProjectAccessService(AssetlenDbContext context) => _context = context;

    public async Task<ProjectAccess> ResolveAsync(string? projectId, string? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(userId))
            return ProjectAccess.None;

        var project = await _context.tbl_Projects_RS
            .Include(p => p.ParentProject)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        return await ResolveAsync(project, userId, ct);
    }

    public async Task<ProjectAccess> ResolveAsync(tbl_Project? project, string? userId, CancellationToken ct = default)
    {
        if (project is null || string.IsNullOrEmpty(userId))
            return ProjectAccess.None;

        // Membership is read first even for owners: it carries the side and the
        // mediator flag, which ownership alone cannot tell us. A project created
        // before this model existed may have no rows at all — the ownership
        // fallback below keeps those projects usable.
        var memberships = await _context.tbl_ProjectMembers
            .Where(m => m.UserId == userId
                     && m.IsActive
                     && (m.ProjectId == project.Id
                         || (project.ParentProjectId != null && m.ProjectId == project.ParentProjectId)))
            .Select(m => new { m.Specialization, m.Side, m.IsMediator })
            .ToListAsync(ct);

        var isMediator = memberships.Any(m => m.IsMediator);

        // Ownership — the project's own, or its parent's for a sub-project.
        var isInvestor = project.InvestorId == userId || project.ParentProject?.InvestorId == userId;
        var isManager = project.ProjectManagerId == userId || project.ParentProject?.ProjectManagerId == userId;

        if (isInvestor || isManager)
        {
            // An explicit membership row still decides the side — the investor
            // is client-side, the project manager contractor-side, unless a row
            // says otherwise. Owners get Manage either way.
            var ownerSide = memberships.Count > 0
                ? HighestSide(memberships.Select(m => m.Side))
                : (isInvestor ? ProjectSide.Client : ProjectSide.Contractor);

            // The project manager mediates by default when nobody else does —
            // otherwise a legacy project would have no one able to expose.
            return new ProjectAccess(
                ProjectAccessLevel.Manage,
                ownerSide,
                isMediator || (memberships.Count == 0 && isManager),
                DeepestSeat(memberships.Select(m => m.Specialization)));
        }

        if (memberships.Count == 0)
            return ProjectAccess.None;

        // An Observer is a read-only stakeholder. Any other specialization —
        // clerk of works, engineer, the developer themselves — may contribute.
        var level = memberships.Any(m => m.Specialization != ProjectMemberSpecialization.Observer)
            ? ProjectAccessLevel.Write
            : ProjectAccessLevel.Read;

        return new ProjectAccess(
            level,
            HighestSide(memberships.Select(m => m.Side)),
            isMediator,
            DeepestSeat(memberships.Select(m => m.Specialization)));
    }

    /// <summary>
    /// The specialization to judge this reader by when they hold more than one
    /// row — foreman on the house, architect on the wing. The wider seat wins,
    /// for the same reason <see cref="HighestSide"/> exists: narrowing on the
    /// strength of a second membership would take away something the first one
    /// already granted.
    /// </summary>
    private static ProjectMemberSpecialization? DeepestSeat(IEnumerable<ProjectMemberSpecialization> specializations)
    {
        ProjectMemberSpecialization? widest = null;

        foreach (var spec in specializations)
        {
            if (ProjectSeatDefaults.For(spec) == ProjectSeat.Principal) return spec;
            if (widest is null || ProjectSeatDefaults.ReadsDrawings(spec)) widest = spec;
        }

        return widest;
    }

    /// <summary>
    /// A user who is somehow a member on both sides (member of the parent as
    /// client, of the sub-project as contractor) is treated as contractor-side:
    /// the side that sees <em>more</em> loses no information, and failing the
    /// other way would hide the Site Log from someone entitled to it.
    /// </summary>
    private static ProjectSide HighestSide(IEnumerable<ProjectSide> sides) =>
        sides.Any(s => s == ProjectSide.Contractor) ? ProjectSide.Contractor : ProjectSide.Client;

    public async Task<ProjectAccessLevel> GetAccessAsync(string? projectId, string? userId, CancellationToken ct = default) =>
        (await ResolveAsync(projectId, userId, ct)).Level;

    public async Task<ProjectAccessLevel> GetAccessAsync(tbl_Project? project, string? userId, CancellationToken ct = default) =>
        (await ResolveAsync(project, userId, ct)).Level;

    public async Task<bool> CanReadAsync(string? projectId, string? userId, CancellationToken ct = default) =>
        await GetAccessAsync(projectId, userId, ct) >= ProjectAccessLevel.Read;

    public async Task<bool> CanReadAsync(tbl_Project? project, string? userId, CancellationToken ct = default) =>
        await GetAccessAsync(project, userId, ct) >= ProjectAccessLevel.Read;

    public async Task<bool> CanWriteAsync(string? projectId, string? userId, CancellationToken ct = default) =>
        await GetAccessAsync(projectId, userId, ct) >= ProjectAccessLevel.Write;

    public async Task<bool> CanWriteAsync(tbl_Project? project, string? userId, CancellationToken ct = default) =>
        await GetAccessAsync(project, userId, ct) >= ProjectAccessLevel.Write;

    public async Task<bool> CanManageAsync(string? projectId, string? userId, CancellationToken ct = default) =>
        await GetAccessAsync(projectId, userId, ct) >= ProjectAccessLevel.Manage;

    public async Task<bool> CanManageAsync(tbl_Project? project, string? userId, CancellationToken ct = default) =>
        await GetAccessAsync(project, userId, ct) >= ProjectAccessLevel.Manage;

    public async Task<bool> CanExposeToClientAsync(tbl_Project? project, string? userId, CancellationToken ct = default) =>
        (await ResolveAsync(project, userId, ct)).CanExposeToClient;
}
