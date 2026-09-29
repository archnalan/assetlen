using Microsoft.EntityFrameworkCore;
using assetlen.Service.DataAccess;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.DbServices;

/// <summary>
/// The one place a captured frame crosses the channel boundary, whoever asked —
/// the mediator by hand, the cutoff on his behalf, or a claim carrying it as
/// evidence. Frame and pointer move together, so the bytes, search and the brief
/// can never disagree about what the client side may see (assetlen.md §5).
/// </summary>
public interface IFrameExposure
{
    /// <summary>The mediator's user id and name — the one accountable face (§10.1).</summary>
    Task<(string? UserId, string? Name)> AccountableFaceAsync(string projectId, CancellationToken ct = default);

    /// <summary>Point each new frame's artifact at its entry, on the frame's own channel.</summary>
    Task AddRefsAsync(tbl_ProgressUpdate entry, IEnumerable<tbl_ProgressImage> frames, CancellationToken ct = default);

    /// <summary>Send tracked frames across. Carries their entry with them; saves.</summary>
    Task<int> ExposeAsync(IReadOnlyCollection<tbl_ProgressImage> frames, string exposedById, CancellationToken ct = default);

    /// <summary>Pull tracked frames back to the Site Diary; saves.</summary>
    Task WithdrawAsync(IReadOnlyCollection<tbl_ProgressImage> frames, CancellationToken ct = default);
}

public sealed class FrameExposureService : IFrameExposure
{
    private readonly AssetlenDbContext _context;

    public FrameExposureService(AssetlenDbContext context) => _context = context;

    public async Task<(string? UserId, string? Name)> AccountableFaceAsync(string projectId, CancellationToken ct = default)
    {
        var parentId = await _context.tbl_Projects_RS.IgnoreQueryFilters()
            .Where(p => p.Id == projectId).Select(p => p.ParentProjectId).FirstOrDefaultAsync(ct);

        var mediator = await _context.tbl_ProjectMembers.IgnoreQueryFilters().AsNoTracking()
            .Include(m => m.User)
            .Where(m => (m.ProjectId == projectId || (parentId != null && m.ProjectId == parentId))
                        && m.IsActive && m.IsMediator && m.IsDeleted != true)
            .OrderBy(m => m.ProjectId == projectId ? 0 : 1)
            .ThenBy(m => m.JoinedAt)
            .FirstOrDefaultAsync(ct);

        if (mediator is null) return (null, null);
        var name = mediator.User is { } u ? $"{u.FirstName} {u.LastName}".Trim() : null;
        return (mediator.UserId, string.IsNullOrEmpty(name) ? mediator.PartyName ?? mediator.Title : name);
    }

    public async Task AddRefsAsync(tbl_ProgressUpdate entry, IEnumerable<tbl_ProgressImage> frames, CancellationToken ct = default)
    {
        // One pointer per file per entry: the same photo attached twice is still one use of it (Law 2).
        var existing = await _context.tbl_ArtifactRefs.IgnoreQueryFilters()
            .Where(r => r.TargetType == ArtifactTargetType.ProgressUpdate && r.TargetId == entry.Id)
            .Select(r => r.ArtifactId).ToListAsync(ct);
        var seen = new HashSet<string?>(existing);
        foreach (var frame in frames.Where(f => f.ArtifactId is not null))
        {
            if (!seen.Add(frame.ArtifactId)) continue;
            _context.tbl_ArtifactRefs.Add(new tbl_ArtifactRef
            {
                ArtifactId = frame.ArtifactId,
                ProjectId = entry.ProjectId,
                TenantId = entry.TenantId,
                TargetType = ArtifactTargetType.ProgressUpdate,
                TargetId = entry.Id,
                Channel = frame.Channel,
                Caption = frame.Caption,
                DisplayOrder = frame.DisplayOrder,
                ExposedById = frame.ExposedById,
                ExposedAt = frame.ExposedAt
            });
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> ExposeAsync(IReadOnlyCollection<tbl_ProgressImage> frames, string exposedById, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var changed = 0;
        foreach (var frame in frames.Where(f => f.Channel != Channel.Client))
        {
            frame.Channel = Channel.Client;
            frame.ExposedById = exposedById;
            frame.ExposedAt = now;
            changed++;
        }

        // A frame is unreachable on an entry the client cannot open.
        var entryIds = frames.Select(f => f.ProgressUpdateId).OfType<string>().Distinct().ToList();
        var entries = await _context.tbl_ProgressUpdates.IgnoreQueryFilters().Where(u => entryIds.Contains(u.Id)).ToListAsync(ct);
        foreach (var e in entries.Where(e => e.Channel != Channel.Client)) e.Channel = Channel.Client;

        await SyncRefsAsync(frames, ct);
        await _context.SaveChangesAsync(ct);
        return changed;
    }

    public async Task WithdrawAsync(IReadOnlyCollection<tbl_ProgressImage> frames, CancellationToken ct = default)
    {
        foreach (var frame in frames)
        {
            frame.Channel = Channel.Crew;
            frame.ExposedById = null;
            frame.ExposedAt = null;
        }
        await SyncRefsAsync(frames, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// A pointer is on the client side while any frame of that file on that entry is.
    /// </summary>
    private async Task SyncRefsAsync(IReadOnlyCollection<tbl_ProgressImage> frames, CancellationToken ct)
    {
        var entryIds = frames.Select(f => f.ProgressUpdateId).OfType<string>().Distinct().ToList();
        var artifactIds = frames.Select(f => f.ArtifactId).OfType<string>().Distinct().ToList();
        if (artifactIds.Count == 0) return;

        // Tracked, so the frames changed a moment ago are read with their new channel.
        var siblings = await _context.tbl_ProgressImages.IgnoreQueryFilters()
            .Where(i => i.ProgressUpdateId != null && entryIds.Contains(i.ProgressUpdateId)
                        && i.ArtifactId != null && artifactIds.Contains(i.ArtifactId) && i.IsDeleted != true)
            .ToListAsync(ct);
        var refs = await _context.tbl_ArtifactRefs.IgnoreQueryFilters()
            .Where(r => r.TargetType == ArtifactTargetType.ProgressUpdate
                        && r.TargetId != null && entryIds.Contains(r.TargetId)
                        && r.ArtifactId != null && artifactIds.Contains(r.ArtifactId))
            .ToListAsync(ct);

        foreach (var group in siblings.GroupBy(f => (f.ProgressUpdateId, f.ArtifactId)))
        {
            var shown = group.Where(f => f.Channel == Channel.Client).OrderBy(f => f.ExposedAt).FirstOrDefault();
            var r = refs.FirstOrDefault(x => x.TargetId == group.Key.ProgressUpdateId && x.ArtifactId == group.Key.ArtifactId);
            if (r is null)
            {
                var entry = await _context.tbl_ProgressUpdates.IgnoreQueryFilters().AsNoTracking()
                    .Where(u => u.Id == group.Key.ProgressUpdateId)
                    .Select(u => new { u.ProjectId, u.TenantId }).FirstOrDefaultAsync(ct);
                if (entry is null) continue;
                var first = group.OrderBy(f => f.DisplayOrder).First();
                r = new tbl_ArtifactRef
                {
                    ArtifactId = group.Key.ArtifactId,
                    ProjectId = entry.ProjectId,
                    TenantId = entry.TenantId,
                    TargetType = ArtifactTargetType.ProgressUpdate,
                    TargetId = group.Key.ProgressUpdateId,
                    Caption = first.Caption,
                    DisplayOrder = first.DisplayOrder
                };
                _context.tbl_ArtifactRefs.Add(r);
            }
            r.Channel = shown is null ? Channel.Crew : Channel.Client;
            r.ExposedById = shown?.ExposedById;
            r.ExposedAt = shown?.ExposedAt;
        }
    }
}
