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
            .Select(m => new Seat(m.Specialization, m.Side, m.IsMediator, m.HandlesMoney))
            .ToListAsync(ct);

        return Decide(project, Owners.Of(project.ParentProject), memberships, userId);
    }

    public async Task<IReadOnlyDictionary<string, ProjectAccess>> ResolveManyAsync(
        IEnumerable<tbl_Project> projects, string? userId, CancellationToken ct = default)
    {
        var list = projects.Where(p => p is not null).ToList();
        if (list.Count == 0 || string.IsNullOrEmpty(userId))
            return new Dictionary<string, ProjectAccess>();

        var parentIds = list.Select(p => p.ParentProjectId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList()!;

        // A sub-project inherits its parent's ownership, and callers reach this
        // with the parent unloaded — off a SubProjects navigation, say. Reading
        // project.ParentProject there is silently null, which would strip a
        // developer of Manage on his own guest wing. Resolve the parents' owners
        // explicitly instead of trusting the graph.
        var parentOwners = new Dictionary<string, Owners>();
        foreach (var p in list.Where(p => p.ParentProject is not null))
            parentOwners[p.ParentProjectId!] = Owners.Of(p.ParentProject);

        var missing = parentIds.Where(id => !parentOwners.ContainsKey(id!)).ToList();
        if (missing.Count > 0)
        {
            var fetched = await _context.tbl_Projects_RS
                .Where(p => missing.Contains(p.Id))
                .Select(p => new { p.Id, p.InvestorId, p.ProjectManagerId })
                .AsNoTracking()
                .ToListAsync(ct);

            foreach (var p in fetched)
                parentOwners[p.Id] = new Owners(p.InvestorId, p.ProjectManagerId);
        }

        // Every project's own id plus every parent id, so a sub-project still
        // inherits the membership held on its house.
        var ids = list.Select(p => p.Id).Concat(parentIds!).Distinct().ToList();

        var rows = await _context.tbl_ProjectMembers
            .Where(m => m.UserId == userId && m.IsActive && ids.Contains(m.ProjectId!))
            .Select(m => new { m.ProjectId, Seat = new Seat(m.Specialization, m.Side, m.IsMediator, m.HandlesMoney) })
            .ToListAsync(ct);

        var byProject = rows
            .GroupBy(r => r.ProjectId!)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Seat).ToList());

        var resolved = new Dictionary<string, ProjectAccess>(list.Count);

        foreach (var project in list)
        {
            var seats = new List<Seat>();
            if (byProject.TryGetValue(project.Id, out var own)) seats.AddRange(own);
            if (project.ParentProjectId is { } parentId && byProject.TryGetValue(parentId, out var inherited))
                seats.AddRange(inherited);

            var parent = project.ParentProjectId is { } pid && parentOwners.TryGetValue(pid, out var o)
                ? o : Owners.Empty;

            resolved[project.Id] = Decide(project, parent, seats, userId);
        }

        return resolved;
    }

    /// <summary>The two ownership columns of a parent project, carried without the entity.</summary>
    private readonly record struct Owners(string? InvestorId, string? ProjectManagerId)
    {
        public static readonly Owners Empty = new(null, null);

        public static Owners Of(tbl_Project? parent) =>
            parent is null ? Empty : new Owners(parent.InvestorId, parent.ProjectManagerId);
    }

    /// <summary>One membership row, reduced to the three facts standing turns on.</summary>
    private readonly record struct Seat(ProjectMemberSpecialization Specialization, ProjectSide Side, bool IsMediator, bool? HandlesMoney);

    /// <summary>
    /// The rule set itself, with the data already in hand. Both the single and
    /// the batch path funnel through here so the dashboard can never grant a
    /// card something the project page would refuse.
    /// </summary>
    private static ProjectAccess Decide(tbl_Project project, Owners parent, List<Seat> memberships, string userId)
    {
        var isMediator = memberships.Any(m => m.IsMediator);

        // Ownership — the project's own, or its parent's for a sub-project.
        var isInvestor = project.InvestorId == userId || parent.InvestorId == userId;
        var isManager = project.ProjectManagerId == userId || parent.ProjectManagerId == userId;

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
                DeepestSeat(memberships.Select(m => m.Specialization)),
                MoneyGrant(memberships));
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
            DeepestSeat(memberships.Select(m => m.Specialization)),
            MoneyGrant(memberships));
    }

    /// <summary>
    /// Whether money has been put on this person's seat explicitly, across all
    /// the rows they hold here. An explicit grant on any row wins, then an
    /// explicit refusal; null when nobody said either way, which leaves
    /// <see cref="ProjectAccess.CanSeeMoney"/> on its seat default.
    /// <para>
    /// Granted beats refused for the same reason the widest seat wins: a second
    /// membership must not take away what the first one gave.
    /// </para>
    /// </summary>
    private static bool? MoneyGrant(List<Seat> memberships)
    {
        if (memberships.Any(m => m.HandlesMoney == true)) return true;
        if (memberships.Any(m => m.HandlesMoney == false)) return false;
        return null;
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
    /// other way would hide the Site Diary from someone entitled to it.
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
