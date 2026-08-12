using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IProjectSizingService"/>
public class ProjectSizingService : IProjectSizingService
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<ProjectSizingService> _logger;
    private readonly IProjectAccessService _access;

    public ProjectSizingService(
        AssetlenDbContext context,
        ILogger<ProjectSizingService> logger,
        IProjectAccessService access)
    {
        _context = context;
        _logger = logger;
        _access = access;
    }

    public async Task<ServiceResult<ProjectSizingDto>> GetAsync(
        string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var project = await LoadAsync(projectId, ct);
            if (project is null)
                return Fail(new NotFoundException("Project not found."));

            // Anyone on the project may see what it bills at. Hiding the tier
            // from the delivery side serves nobody and invites a surprise
            // argument about scope later.
            if (!await _access.CanReadAsync(project, userId, ct))
                return Fail(new ForbiddenException("Access denied."));

            return ServiceResult<ProjectSizingDto>.Success(await BuildAsync(project, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading sizing for project {ProjectId}", projectId);
            return Fail(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProjectSizingDto>> SetAreaAsync(
        ProjectAreaUpdateDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ProjectId))
                return Fail(new BadRequestException("ProjectId is required."));
            if (dto.FloorAreaSqm is < 0)
                return Fail(new BadRequestException("Floor area cannot be negative."));

            // Changing the area can change the bill, so it is owner/manager work.
            if (!await _access.CanManageAsync(dto.ProjectId, userId, ct))
                return Fail(new ForbiddenException("Only the project owner or manager can change the floor area."));

            var project = await LoadAsync(dto.ProjectId, ct, tracking: true);
            if (project is null)
                return Fail(new NotFoundException("Project not found."));

            // DerivedFromDrawing belongs to the drawing-reading step, not to a
            // caller who can simply claim it. Manual is likewise reserved for a
            // deliberate correction path.
            var source = dto.Source == ProjectSizeSource.DerivedFromDrawing
                ? ProjectSizeSource.Declared
                : dto.Source;

            project.FloorAreaSqm = dto.FloorAreaSqm;
            project.SizeSource = dto.FloorAreaSqm is null or <= 0 ? ProjectSizeSource.Unknown : source;

            var billable = await ApplyTierAsync(project, ct);
            await _context.SaveChangesAsync(ct);

            return ServiceResult<ProjectSizingDto>.Success(await BuildAsync(billable, ct, subjectId: project.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting area for project {ProjectId}", dto.ProjectId);
            return Fail(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProjectSizingDto>> ConfirmTierAsync(
        string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            if (!await _access.CanManageAsync(projectId, userId, ct))
                return Fail(new ForbiddenException("Only the project owner or manager can accept a tier change."));

            var project = await LoadAsync(projectId, ct, tracking: true);
            if (project is null)
                return Fail(new NotFoundException("Project not found."));

            var billable = await BillableParentAsync(project, ct, tracking: true);
            var measured = ProjectSizingPolicy.TierFor(await TotalAreaAsync(billable, ct));

            if (measured == billable.SizeTier)
                return ServiceResult<ProjectSizingDto>.Success(await BuildAsync(billable, ct));

            billable.SizeTier = measured;
            billable.SizeTierConfirmedById = userId;
            billable.SizeTierConfirmedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Project {ProjectId} billing tier confirmed as {Tier} by {UserId}",
                billable.Id, measured, userId);

            return ServiceResult<ProjectSizingDto>.Success(await BuildAsync(billable, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming tier for project {ProjectId}", projectId);
            return Fail(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProjectSizingDto>> RecomputeAsync(
        string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var project = await LoadAsync(projectId, ct, tracking: true);
            if (project is null)
                return Fail(new NotFoundException("Project not found."));
            if (!await _access.CanReadAsync(project, userId, ct))
                return Fail(new ForbiddenException("Access denied."));

            var billable = await ApplyTierAsync(project, ct);
            await _context.SaveChangesAsync(ct);

            return ServiceResult<ProjectSizingDto>.Success(await BuildAsync(billable, ct, subjectId: project.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recomputing sizing for project {ProjectId}", projectId);
            return Fail(new ServerErrorException(ex.Message));
        }
    }

    // ─── Internals ───────────────────────────────────────────────────────

    /// <summary>
    /// Apply a tier **downgrade** immediately and leave an upgrade pending.
    /// Returns the billable parent. Callers save.
    /// </summary>
    private async Task<tbl_Project> ApplyTierAsync(tbl_Project project, CancellationToken ct)
    {
        var billable = await BillableParentAsync(project, ct, tracking: true);
        var measured = ProjectSizingPolicy.TierFor(await TotalAreaAsync(billable, ct));

        if (ProjectSizingPolicy.RequiresConfirmation(billable.SizeTier, measured))
        {
            // Upgrade: costs money, so it waits for a person. BuildAsync surfaces
            // it as PendingTier.
            _logger.LogInformation(
                "Project {ProjectId} measures into {Measured} but bills at {Current}; awaiting confirmation",
                billable.Id, measured, billable.SizeTier);
            return billable;
        }

        if (measured != billable.SizeTier)
        {
            // Downgrade: apply at once. Continuing to bill a band the drawings
            // no longer justify is the one direction we must never sit on.
            billable.SizeTier = measured;
            billable.SizeTierConfirmedById = null;
            billable.SizeTierConfirmedAt = null;
        }

        return billable;
    }

    /// <summary>
    /// The project that bills. One level of nesting only, so this is the parent
    /// if there is one and the project itself otherwise.
    /// </summary>
    private async Task<tbl_Project> BillableParentAsync(tbl_Project project, CancellationToken ct, bool tracking = false)
    {
        if (string.IsNullOrEmpty(project.ParentProjectId))
            return project;

        var query = tracking ? _context.tbl_Projects_RS : _context.tbl_Projects_RS.AsNoTracking();
        return await query.FirstOrDefaultAsync(p => p.Id == project.ParentProjectId, ct) ?? project;
    }

    /// <summary>Parent's own area plus every sub-project's. The guest wing enlarges the invoice; it is not a second one.</summary>
    private async Task<decimal?> TotalAreaAsync(tbl_Project billable, CancellationToken ct)
    {
        var childTotal = await _context.tbl_Projects_RS
            .Where(p => p.ParentProjectId == billable.Id)
            .SumAsync(p => (decimal?)p.FloorAreaSqm ?? 0m, ct);

        var total = (billable.FloorAreaSqm ?? 0m) + childTotal;
        return total <= 0 ? null : total;
    }

    private async Task<ProjectSizingDto> BuildAsync(tbl_Project billable, CancellationToken ct, string? subjectId = null)
    {
        var total = await TotalAreaAsync(billable, ct);
        var measured = ProjectSizingPolicy.TierFor(total);

        var children = await _context.tbl_Projects_RS
            .Where(p => p.ParentProjectId == billable.Id)
            .Select(p => new ProjectAreaContributionDto
            {
                ProjectId = p.Id,
                ProjectName = p.ProjectName,
                AreaSqm = p.FloorAreaSqm,
                IsParent = false
            })
            .ToListAsync(ct);

        children.Insert(0, new ProjectAreaContributionDto
        {
            ProjectId = billable.Id,
            ProjectName = billable.ProjectName,
            AreaSqm = billable.FloorAreaSqm,
            IsParent = true
        });

        var pending = measured != billable.SizeTier ? measured : (ProjectSizeTier?)null;

        return new ProjectSizingDto
        {
            ProjectId = subjectId ?? billable.Id,
            BillableProjectId = billable.Id,
            OwnAreaSqm = billable.FloorAreaSqm,
            TotalAreaSqm = total,
            Tier = billable.SizeTier,
            Source = billable.SizeSource,
            Band = ProjectSizingPolicy.DescribeBand(billable.SizeTier),
            PendingTier = pending,
            PendingBand = pending is null ? null : ProjectSizingPolicy.DescribeBand(pending.Value),
            ConfirmedById = billable.SizeTierConfirmedById,
            ConfirmedAt = billable.SizeTierConfirmedAt,
            Contributions = children
        };
    }

    private Task<tbl_Project?> LoadAsync(string projectId, CancellationToken ct, bool tracking = false)
    {
        var query = _context.tbl_Projects_RS.Include(p => p.ParentProject);
        return (tracking ? query : query.AsNoTracking())
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);
    }

    private static ServiceResult<ProjectSizingDto> Fail(Exception ex) => ServiceResult<ProjectSizingDto>.Failure(ex);
}
