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

    public async Task<ProjectAccessLevel> GetAccessAsync(string? projectId, string? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(userId))
            return ProjectAccessLevel.None;

        var project = await _context.tbl_Projects_RS
            .Include(p => p.ParentProject)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        return await GetAccessAsync(project, userId, ct);
    }

    public async Task<ProjectAccessLevel> GetAccessAsync(tbl_Project? project, string? userId, CancellationToken ct = default)
    {
        if (project is null || string.IsNullOrEmpty(userId))
            return ProjectAccessLevel.None;

        // Ownership — the project's own, or its parent's for a sub-project.
        if (project.InvestorId == userId || project.ProjectManagerId == userId
            || project.ParentProject?.InvestorId == userId
            || project.ParentProject?.ProjectManagerId == userId)
            return ProjectAccessLevel.Manage;

        // Membership — on this project or, for a sub-project, on its parent.
        // Highest specialization wins if somebody is a member of both.
        var specializations = await _context.tbl_ProjectMembers
            .Where(m => m.UserId == userId
                     && m.IsActive
                     && (m.ProjectId == project.Id
                         || (project.ParentProjectId != null && m.ProjectId == project.ParentProjectId)))
            .Select(m => m.Specialization)
            .ToListAsync(ct);

        if (specializations.Count == 0)
            return ProjectAccessLevel.None;

        // An Observer is a read-only stakeholder. Any other specialization —
        // clerk of works, engineer, the developer themselves — may contribute.
        return specializations.Any(s => s != ProjectMemberSpecialization.Observer)
            ? ProjectAccessLevel.Write
            : ProjectAccessLevel.Read;
    }

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
}
