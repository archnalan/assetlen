using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class FlagDAL : IFlagDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<FlagDAL> _logger;
    private readonly ITenantProvider _tenant;
    private readonly IProjectAccessService _access;

    public FlagDAL(AssetlenDbContext context, ILogger<FlagDAL> logger, ITenantProvider tenant, IProjectAccessService access)
    {
        _context = context;
        _logger = logger;
        _tenant = tenant;
        _access = access;
    }

    public async Task<ServiceResult<FlagDto>> AddFlag(FlagCreateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ProjectId))
                return ServiceResult<FlagDto>.Failure(new BadRequestException("ProjectId is required."));
            if (string.IsNullOrWhiteSpace(dto.Title))
                return ServiceResult<FlagDto>.Failure(new BadRequestException("Title is required."));

            var project = await LoadProjectWithParent(dto.ProjectId);
            if (project is null)
                return ServiceResult<FlagDto>.Failure(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, actingUserId);
            if (!access.CanRead)
                return ServiceResult<FlagDto>.Failure(new ForbiddenException("Access denied."));

            var flag = new tbl_Flag
            {
                ProjectId = dto.ProjectId,
                StageId = dto.StageId,
                ProgressUpdateId = dto.ProgressUpdateId,
                ProgressImageId = dto.ProgressImageId,
                Title = dto.Title,
                Description = dto.Description,
                Severity = dto.Severity,
                // A client-side principal can only raise on the Client channel —
                // decided per project, so the same person may raise a Crew-channel
                // query on a different project where they mediate.
                Channel = !access.CanSeeSiteLog ? Channel.Client : dto.Channel,
                Status = FlagStatus.Open,
                CreatedById = actingUserId,
                AssignedToId = dto.AssignedToId,
                DueDate = dto.DueDate
            };
            _context.tbl_Flags.Add(flag);
            await _context.SaveChangesAsync();

            return await GetFlag(flag.Id, actingUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding flag");
            return ServiceResult<FlagDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<FlagDto>> GetFlag(string flagId, string actingUserId)
    {
        try
        {
            var flag = await LoadFlagWithIncludes(flagId);
            if (flag is null)
                return ServiceResult<FlagDto>.Failure(new NotFoundException("Flag not found."));
            var access = await _access.ResolveAsync(flag.Project, actingUserId);
            if (!access.CanRead)
                return ServiceResult<FlagDto>.Failure(new ForbiddenException("Access denied."));
            if (!access.CanSeeSiteLog && flag.Channel != Channel.Client)
                return ServiceResult<FlagDto>.Failure(new NotFoundException("Flag not found."));
            return ServiceResult<FlagDto>.Success(ToDto(flag));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flag {FlagId}", flagId);
            return ServiceResult<FlagDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<FlagDto>>> GetFlagsByProject(string projectId, FlagStatus? status, string actingUserId)
    {
        try
        {
            var project = await LoadProjectWithParent(projectId);
            if (project is null)
                return ServiceResult<List<FlagDto>>.Failure(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, actingUserId);
            if (!access.CanRead)
                return ServiceResult<List<FlagDto>>.Failure(new ForbiddenException("Access denied."));

            var query = _context.tbl_Flags
                .Include(f => f.Project)
                .Include(f => f.Stage)
                .Include(f => f.CreatedBy)
                .Include(f => f.AssignedTo)
                .Include(f => f.ResolvedBy)
                .Where(f => f.ProjectId == projectId)
                .AsNoTracking();

            if (status.HasValue)
                query = query.Where(f => f.Status == status.Value);

            if (!access.CanSeeSiteLog)
                query = query.Where(f => f.Channel == Channel.Client);

            var flags = await query
                .OrderBy(f => f.Status)
                .ThenByDescending(f => f.Severity)
                .ThenByDescending(f => f.DateTimeCreated)
                .ToListAsync();

            return ServiceResult<List<FlagDto>>.Success(flags.Select(ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing flags for project {ProjectId}", projectId);
            return ServiceResult<List<FlagDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<FlagDto>>> GetFlagsByEntry(string progressUpdateId, string actingUserId)
    {
        try
        {
            var update = await _context.tbl_ProgressUpdates
                .Include(u => u.Project)
                    .ThenInclude(p => p!.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == progressUpdateId);
            if (update is null)
                return ServiceResult<List<FlagDto>>.Failure(new NotFoundException("Entry not found."));
            var access = await _access.ResolveAsync(update.Project, actingUserId);
            if (!access.CanRead)
                return ServiceResult<List<FlagDto>>.Failure(new ForbiddenException("Access denied."));

            var entryQuery = _context.tbl_Flags
                .Include(f => f.Project)
                .Include(f => f.Stage)
                .Include(f => f.CreatedBy)
                .Include(f => f.AssignedTo)
                .Include(f => f.ResolvedBy)
                .Where(f => f.ProgressUpdateId == progressUpdateId)
                .AsNoTracking();

            if (!access.CanSeeSiteLog)
                entryQuery = entryQuery.Where(f => f.Channel == Channel.Client);

            var flags = await entryQuery
                .OrderByDescending(f => f.Severity)
                .ThenByDescending(f => f.DateTimeCreated)
                .ToListAsync();

            return ServiceResult<List<FlagDto>>.Success(flags.Select(ToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing flags for entry {EntryId}", progressUpdateId);
            return ServiceResult<List<FlagDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<FlagDto>> UpdateFlag(FlagUpdateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Id))
                return ServiceResult<FlagDto>.Failure(new BadRequestException("Flag Id is required."));

            var flag = await _context.tbl_Flags
                .Include(f => f.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(f => f.Id == dto.Id);
            if (flag is null)
                return ServiceResult<FlagDto>.Failure(new NotFoundException("Flag not found."));
            if (!await _access.CanReadAsync(flag.Project, actingUserId))
                return ServiceResult<FlagDto>.Failure(new ForbiddenException("Access denied."));

            if (!string.IsNullOrWhiteSpace(dto.Title)) flag.Title = dto.Title;
            if (dto.Description is not null) flag.Description = dto.Description;
            if (dto.Severity.HasValue) flag.Severity = dto.Severity.Value;
            if (dto.AssignedToId is not null) flag.AssignedToId = dto.AssignedToId;
            if (dto.DueDate.HasValue) flag.DueDate = dto.DueDate;

            if (dto.Status.HasValue && dto.Status.Value != flag.Status)
            {
                flag.Status = dto.Status.Value;
                if (dto.Status.Value == FlagStatus.Resolved)
                {
                    flag.ResolvedById = actingUserId;
                    flag.ResolvedDate = DateTime.UtcNow;
                }
                else if (dto.Status.Value == FlagStatus.Open || dto.Status.Value == FlagStatus.InProgress)
                {
                    flag.ResolvedById = null;
                    flag.ResolvedDate = null;
                }
            }

            await _context.SaveChangesAsync();
            return await GetFlag(flag.Id, actingUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flag");
            return ServiceResult<FlagDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<FlagDto>> ResolveFlag(string flagId, string actingUserId)
    {
        return await UpdateFlag(new FlagUpdateDto { Id = flagId, Status = FlagStatus.Resolved }, actingUserId);
    }

    public async Task<ServiceResult<FlagDto>> NudgeFlag(string flagId, string actingUserId)
    {
        try
        {
            var flag = await _context.tbl_Flags
                .Include(f => f.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(f => f.Id == flagId);
            if (flag is null)
                return ServiceResult<FlagDto>.Failure(new NotFoundException("Flag not found."));
            if (!await _access.CanReadAsync(flag.Project, actingUserId))
                return ServiceResult<FlagDto>.Failure(new ForbiddenException("Access denied."));
            if (flag.Status == FlagStatus.Resolved || flag.Status == FlagStatus.Archived)
                return ServiceResult<FlagDto>.Failure(new BadRequestException("Cannot nudge a closed flag."));

            flag.LastNudgeAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetFlag(flag.Id, actingUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error nudging flag {FlagId}", flagId);
            return ServiceResult<FlagDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Helpers ──────────────────────────────────────────────

    private Task<tbl_Project?> LoadProjectWithParent(string projectId) =>
        _context.tbl_Projects_RS
            .Include(p => p.ParentProject)
            .FirstOrDefaultAsync(p => p.Id == projectId);

    private Task<tbl_Flag?> LoadFlagWithIncludes(string flagId) =>
        _context.tbl_Flags
            .Include(f => f.Project)
                .ThenInclude(p => p!.ParentProject)
            .Include(f => f.Stage)
            .Include(f => f.CreatedBy)
            .Include(f => f.AssignedTo)
            .Include(f => f.ResolvedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == flagId);

    private static string? FullName(AppUser? u) =>
        u is null ? null : $"{u.FirstName} {u.LastName}".Trim();

    private static FlagDto ToDto(tbl_Flag f) => new()
    {
        Id = f.Id,
        ProjectId = f.ProjectId,
        StageId = f.StageId,
        ProgressUpdateId = f.ProgressUpdateId,
        ProgressImageId = f.ProgressImageId,
        Title = f.Title,
        Description = f.Description,
        Status = f.Status,
        Severity = f.Severity,
        Channel = f.Channel,
        CreatedById = f.CreatedById,
        AssignedToId = f.AssignedToId,
        ResolvedById = f.ResolvedById,
        DueDate = f.DueDate,
        ResolvedDate = f.ResolvedDate,
        LastNudgeAt = f.LastNudgeAt,
        IsNudgeArchived = f.IsNudgeArchived,
        DateTimeCreated = f.DateTimeCreated,
        ProjectName = f.Project?.ProjectName,
        StageName = f.Stage?.StageName,
        CreatedByName = FullName(f.CreatedBy),
        AssignedToName = FullName(f.AssignedTo),
        ResolvedByName = FullName(f.ResolvedBy)
    };
}
