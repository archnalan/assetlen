using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class ProjectMemberDAL : IProjectMemberDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<ProjectMemberDAL> _logger;

    public ProjectMemberDAL(AssetlenDbContext context, ILogger<ProjectMemberDAL> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<ProjectMemberDto>> AddMember(ProjectMemberCreateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ProjectId))
                return ServiceResult<ProjectMemberDto>.Failure(new BadRequestException("ProjectId is required."));
            if (string.IsNullOrEmpty(dto.UserId) && string.IsNullOrEmpty(dto.UserEmail))
                return ServiceResult<ProjectMemberDto>.Failure(new BadRequestException("Either UserId or UserEmail must be supplied."));

            // Authorize: acting user must own or manage the project (parent if sub-project).
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project is null)
                return ServiceResult<ProjectMemberDto>.Failure(new NotFoundException("Project not found."));
            var ownerId = project.ParentProject?.InvestorId ?? project.InvestorId;
            var pmId = project.ParentProject?.ProjectManagerId ?? project.ProjectManagerId;
            if (ownerId != actingUserId && pmId != actingUserId &&
                project.InvestorId != actingUserId && project.ProjectManagerId != actingUserId)
                return ServiceResult<ProjectMemberDto>.Failure(new ForbiddenException("Only the project owner or manager can add members."));

            // Resolve target user.
            AppUser? user;
            if (!string.IsNullOrEmpty(dto.UserId))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            }
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.UserEmail);
            }
            if (user is null)
                return ServiceResult<ProjectMemberDto>.Failure(new NotFoundException("User not found."));

            // Reactivate an existing membership instead of duplicating.
            var existing = await _context.tbl_ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == dto.ProjectId && m.UserId == user.Id);
            tbl_ProjectMember member;
            if (existing is not null)
            {
                existing.Specialization = dto.Specialization;
                existing.Title = dto.Title;
                existing.IsActive = true;
                existing.LeftAt = null;
                existing.JoinedAt ??= DateTime.UtcNow;
                existing.AssignedById = actingUserId;
                member = existing;
            }
            else
            {
                member = new tbl_ProjectMember
                {
                    ProjectId = dto.ProjectId,
                    UserId = user.Id,
                    Specialization = dto.Specialization,
                    Title = dto.Title,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    AssignedById = actingUserId
                };
                _context.tbl_ProjectMembers.Add(member);
            }
            await _context.SaveChangesAsync();

            return ServiceResult<ProjectMemberDto>.Success(ToDto(member, user, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding project member");
            return ServiceResult<ProjectMemberDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<ProjectMemberDto>>> GetMembersByProject(string projectId, string actingUserId)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project is null)
                return ServiceResult<List<ProjectMemberDto>>.Failure(new NotFoundException("Project not found."));
            var ownerId = project.ParentProject?.InvestorId ?? project.InvestorId;
            var pmId = project.ParentProject?.ProjectManagerId ?? project.ProjectManagerId;
            if (ownerId != actingUserId && pmId != actingUserId &&
                project.InvestorId != actingUserId && project.ProjectManagerId != actingUserId)
                return ServiceResult<List<ProjectMemberDto>>.Failure(new ForbiddenException("Access denied."));

            var members = await _context.tbl_ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .Include(m => m.User)
                .Include(m => m.AssignedBy)
                .OrderByDescending(m => m.IsActive)
                .ThenBy(m => m.User!.FirstName)
                .AsNoTracking()
                .ToListAsync();

            var dtos = members.Select(m => ToDto(m, m.User, m.AssignedBy)).ToList();
            return ServiceResult<List<ProjectMemberDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing project members");
            return ServiceResult<List<ProjectMemberDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<bool>> DeactivateMember(string memberId, string actingUserId)
    {
        try
        {
            var member = await _context.tbl_ProjectMembers
                .Include(m => m.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(m => m.Id == memberId);
            if (member is null)
                return ServiceResult<bool>.Failure(new NotFoundException("Member not found."));

            var project = member.Project;
            var ownerId = project?.ParentProject?.InvestorId ?? project?.InvestorId;
            var pmId = project?.ParentProject?.ProjectManagerId ?? project?.ProjectManagerId;
            if (ownerId != actingUserId && pmId != actingUserId &&
                project?.InvestorId != actingUserId && project?.ProjectManagerId != actingUserId)
                return ServiceResult<bool>.Failure(new ForbiddenException("Only the project owner or manager can remove members."));

            member.IsActive = false;
            member.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating project member");
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    private static ProjectMemberDto ToDto(tbl_ProjectMember m, AppUser? user, AppUser? assignedBy) => new()
    {
        Id = m.Id,
        ProjectId = m.ProjectId,
        UserId = m.UserId,
        Specialization = m.Specialization,
        Title = m.Title,
        IsActive = m.IsActive,
        JoinedAt = m.JoinedAt,
        LeftAt = m.LeftAt,
        DateTimeCreated = m.DateTimeCreated,
        UserFullName = user is not null ? $"{user.FirstName} {user.LastName}".Trim() : null,
        UserEmail = user?.Email,
        UserProfilePicUrl = user?.ProfilePicUrl,
        AssignedByName = assignedBy is not null ? $"{assignedBy.FirstName} {assignedBy.LastName}".Trim() : null
    };
}
