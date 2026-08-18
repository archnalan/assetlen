using Microsoft.EntityFrameworkCore;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.DbServices;

/// <summary>
/// Which stage a thing belongs to when nobody said.
/// <para>
/// Everything on a project hangs off a funded piece of work — a photograph, a
/// drawing, a release, a question (CLAUDE.md §1: <i>nothing floats</i>). But
/// asking "which stage?" on every single capture is exactly the tax that sends
/// people back to the chat, so anything created without a stage is attached to
/// the one currently in progress, and the reader can move it deliberately.
/// </para>
/// <para>
/// This is also what makes search worth having: a corpus where every item knows
/// its stage can be read a phase at a time, and one where they float cannot.
/// </para>
/// </summary>
public class ActiveStageService : IActiveStageService
{
    private readonly AssetlenDbContext _context;

    public ActiveStageService(AssetlenDbContext context) => _context = context;

    public async Task<string?> ResolveAsync(string? projectId, string? preferredStageId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(projectId)) return null;

        // An explicit choice always wins, provided it really is on this project —
        // a stage id from another project would silently file the item somewhere
        // the reader cannot see.
        if (!string.IsNullOrEmpty(preferredStageId))
        {
            var valid = await _context.tbl_Stages
                .AnyAsync(s => s.Id == preferredStageId && s.ProjectId == projectId, ct);

            if (valid) return preferredStageId;
        }

        var stages = await _context.tbl_Stages
            .Where(s => s.ProjectId == projectId)
            .Select(s => new { s.Id, s.Status, s.DisplayOrder })
            .AsNoTracking()
            .ToListAsync(ct);

        if (stages.Count == 0) return null;

        // The earliest stage still in progress. Several can be open at once on a
        // real site — walling upstairs while the drains go in — and the earliest
        // is the one the work is actually chasing.
        var live = stages
            .Where(s => s.Status == StageStatus.InProgress)
            .OrderBy(s => s.DisplayOrder)
            .FirstOrDefault();

        if (live is not null) return live.Id;

        // Nothing open: the next one not yet started, so material captured
        // before a stage is opened lands where the work is about to happen
        // rather than on the last thing that finished.
        var next = stages
            .Where(s => s.Status == StageStatus.NotStarted)
            .OrderBy(s => s.DisplayOrder)
            .FirstOrDefault();

        return next?.Id ?? stages.OrderByDescending(s => s.DisplayOrder).First().Id;
    }
}
