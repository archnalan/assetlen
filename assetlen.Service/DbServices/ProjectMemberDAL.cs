using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <summary>
/// Membership: who is on a project, which of its two sides they belong to, and
/// which one or two of them mediate between the sides.
/// </summary>
public class ProjectMemberDAL : IProjectMemberDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<ProjectMemberDAL> _logger;
    private readonly IProjectAccessService _access;

    public ProjectMemberDAL(
        AssetlenDbContext context,
        ILogger<ProjectMemberDAL> logger,
        IProjectAccessService access)
    {
        _context = context;
        _logger = logger;
        _access = access;
    }

    public async Task<ServiceResult<ProjectMemberDto>> AddMember(ProjectMemberCreateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ProjectId))
                return ServiceResult<ProjectMemberDto>.Failure(new BadRequestException("ProjectId is required."));

            var isUnregisteredParty = string.IsNullOrEmpty(dto.UserId)
                                   && string.IsNullOrEmpty(dto.UserEmail);

            if (isUnregisteredParty && string.IsNullOrWhiteSpace(dto.PartyName))
                return ServiceResult<ProjectMemberDto>.Failure(new BadRequestException(
                    "Supply UserId or UserEmail for a platform user, or PartyName for an off-platform party."));

            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project is null)
                return ServiceResult<ProjectMemberDto>.Failure(new NotFoundException("Project not found."));

            // The side defaults from the specialization but is stored
            // explicitly — an architect or engineer sits on whichever side
            // retained them, and that varies per project.
            var side = dto.Side ?? ProjectSideDefaults.For(dto.Specialization);
            var isMediator = dto.IsMediator ?? ProjectSideDefaults.MediatesByDefault(dto.Specialization);

            // Routed through the single access authority — a private ownership
            // test here is the A1 bug reappearing under a different name.
            var access = await _access.ResolveAsync(project, actingUserId);
            var refusal = CheckMayStaff(access, side, isMediator);
            if (refusal is not null)
                return ServiceResult<ProjectMemberDto>.Failure(refusal);

            AppUser? user = null;
            if (!isUnregisteredParty)
            {
                user = !string.IsNullOrEmpty(dto.UserId)
                    ? await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId)
                    : await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.UserEmail);

                if (user is null)
                    return ServiceResult<ProjectMemberDto>.Failure(new NotFoundException("User not found."));
            }

            // Reactivate an existing membership instead of duplicating.
            var existing = user is null
                ? null
                : await _context.tbl_ProjectMembers
                    .FirstOrDefaultAsync(m => m.ProjectId == dto.ProjectId && m.UserId == user.Id);

            if (isMediator)
            {
                var capCheck = await CheckMediatorCapAsync(dto.ProjectId, existing?.Id);
                if (capCheck is not null)
                    return ServiceResult<ProjectMemberDto>.Failure(capCheck);
            }

            tbl_ProjectMember member;
            if (existing is not null)
            {
                existing.Specialization = dto.Specialization;
                existing.Title = dto.Title;
                existing.Side = side;
                existing.IsMediator = isMediator;
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
                    UserId = user?.Id,
                    PartyName = user is null ? dto.PartyName?.Trim() : null,
                    Specialization = dto.Specialization,
                    Title = dto.Title,
                    Side = side,
                    IsMediator = isMediator,
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

    /// <summary>
    /// Who may put someone on the roster, and on which side.
    /// <para>
    /// Owner and manager may do anything. A <b>mediator may staff their own
    /// side</b> and only their own side — assetlen.md D5: Peter appoints the
    /// mediator, the mediator staffs the delivery side. Without this the
    /// architect could not add his own foreman, and Peter would be back to
    /// hiring the subcontractors himself, which is the arrangement the product
    /// exists to end.
    /// </para>
    /// <para>
    /// Appointing another mediator is never delegated. It is the one decision
    /// that changes whose name is accountable for what crosses (§10.1), and it
    /// belongs to the person who owns the project.
    /// </para>
    /// Returns an exception to fail with, or null when the action is allowed.
    /// </summary>
    private static Exception? CheckMayStaff(ProjectAccess access, ProjectSide targetSide, bool appointingMediator)
    {
        if (access.CanManage) return null;

        if (!access.IsMediator)
            return new ForbiddenException("Only the project owner, a manager or a mediator can change the roster.");

        if (appointingMediator)
            return new ForbiddenException("Only the project owner can appoint a mediator.");

        if (access.Side != targetSide)
            return new ForbiddenException("A mediator can only staff their own side of the project.");

        return null;
    }

    /// <summary>
    /// A project has one mediator, or at most two. More than that and "what
    /// Peter sees is what the architect chose" stops being true — the whole
    /// point of the two-sided model is a single accountable filter.
    /// Returns an exception to fail with, or null when the cap allows it.
    /// </summary>
    private async Task<Exception?> CheckMediatorCapAsync(string projectId, string? excludingMemberId)
    {
        var current = await _context.tbl_ProjectMembers
            .CountAsync(m => m.ProjectId == projectId
                          && m.IsMediator
                          && m.IsActive
                          && (excludingMemberId == null || m.Id != excludingMemberId));

        return current >= ProjectSideDefaults.MaxMediators
            ? new ConflictException(
                $"This project already has {ProjectSideDefaults.MaxMediators} mediators. " +
                "Stand one down before appointing another.")
            : null;
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

            // Any stakeholder may see who else is on the project. Knowing that
            // a clerk of works and an engineer exist is not privileged — the
            // client needs it to know who they are dealing with, and hiding it
            // was an artefact of the old owner-only check.
            var access = await _access.ResolveAsync(project, actingUserId);
            if (!access.CanRead)
                return ServiceResult<List<ProjectMemberDto>>.Failure(new ForbiddenException("Access denied."));

            var members = await _context.tbl_ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .Include(m => m.User)
                .Include(m => m.AssignedBy)
                .OrderByDescending(m => m.IsActive)
                .ThenByDescending(m => m.IsMediator)
                .ThenBy(m => m.Side)
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

            // A mediator may remove people from their own side — the same
            // authority that let them add them. Removing a mediator is not
            // delegated; see CheckMayStaff.
            var access = await _access.ResolveAsync(member.Project, actingUserId);
            var refusal = CheckMayStaff(access, member.Side, appointingMediator: false);
            if (refusal is not null)
                return ServiceResult<bool>.Failure(refusal);

            if (member.IsMediator && !access.CanManage)
                return ServiceResult<bool>.Failure(new ForbiddenException(
                    "Only the project owner can stand a mediator down."));

            // Standing down the last mediator would leave nobody able to expose
            // anything, and the client side would silently go dark.
            if (member.IsMediator && member.IsActive)
            {
                var remaining = await _context.tbl_ProjectMembers
                    .CountAsync(m => m.ProjectId == member.ProjectId
                                  && m.IsMediator && m.IsActive && m.Id != member.Id);
                if (remaining == 0)
                    return ServiceResult<bool>.Failure(new ConflictException(
                        "This is the project's only mediator. Appoint another before standing this one down."));
            }

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

    public async Task<ServiceResult<ProjectMemberDto>> UpdateMember(ProjectMemberUpdateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.MemberId))
                return ServiceResult<ProjectMemberDto>.Failure(new BadRequestException("MemberId is required."));

            var member = await _context.tbl_ProjectMembers
                .Include(m => m.User)
                .Include(m => m.AssignedBy)
                .Include(m => m.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(m => m.Id == dto.MemberId);
            if (member is null)
                return ServiceResult<ProjectMemberDto>.Failure(new NotFoundException("Member not found."));

            // Both the side they are on now and the side they would land on
            // must be within the caller's reach — otherwise a mediator could
            // move one of their own people onto the client side, which is a way
            // of writing the other party's roster.
            var access = await _access.ResolveAsync(member.Project, actingUserId);
            var appointing = dto.IsMediator == true && !member.IsMediator;
            var refusal = CheckMayStaff(access, member.Side, appointing)
                       ?? (dto.Side is { } moveTo ? CheckMayStaff(access, moveTo, appointing) : null);
            if (refusal is not null)
                return ServiceResult<ProjectMemberDto>.Failure(refusal);

            // Standing a mediator down changes who is accountable, so it stays
            // with the owner even though the person sits on the caller's side.
            if (dto.IsMediator == false && member.IsMediator && !access.CanManage)
                return ServiceResult<ProjectMemberDto>.Failure(new ForbiddenException(
                    "Only the project owner can stand a mediator down."));

            if (!member.IsActive)
                return ServiceResult<ProjectMemberDto>.Failure(new BadRequestException(
                    "This member has been removed from the project. Add them again to change their standing."));

            // Appointing: the cap applies. Standing down: the project must keep
            // at least one mediator, or nothing can ever be exposed again.
            if (dto.IsMediator is not null && dto.IsMediator != member.IsMediator)
            {
                if (dto.IsMediator == true)
                {
                    var capCheck = await CheckMediatorCapAsync(member.ProjectId!, member.Id);
                    if (capCheck is not null)
                        return ServiceResult<ProjectMemberDto>.Failure(capCheck);
                }
                else
                {
                    var remaining = await _context.tbl_ProjectMembers
                        .CountAsync(m => m.ProjectId == member.ProjectId
                                      && m.IsMediator && m.IsActive && m.Id != member.Id);
                    if (remaining == 0)
                        return ServiceResult<ProjectMemberDto>.Failure(new ConflictException(
                            "This is the project's only mediator. Appoint another before standing this one down."));
                }

                member.IsMediator = dto.IsMediator.Value;
            }

            if (dto.Side is not null) member.Side = dto.Side.Value;
            if (dto.Specialization is not null) member.Specialization = dto.Specialization.Value;
            if (dto.Title is not null) member.Title = string.IsNullOrWhiteSpace(dto.Title) ? null : dto.Title.Trim();

            member.AssignedById = actingUserId;
            await _context.SaveChangesAsync();

            return ServiceResult<ProjectMemberDto>.Success(ToDto(member, member.User, member.AssignedBy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project member {MemberId}", dto.MemberId);
            return ServiceResult<ProjectMemberDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ProjectAccessDto>> GetMyStanding(string projectId, string actingUserId)
    {
        try
        {
            var access = await _access.ResolveAsync(projectId, actingUserId);
            return ServiceResult<ProjectAccessDto>.Success(new ProjectAccessDto
            {
                ProjectId = projectId,
                Level = access.Level,
                Side = access.Side,
                IsMediator = access.IsMediator,
                CanRead = access.CanRead,
                CanWrite = access.CanWrite,
                CanManage = access.CanManage,
                CanSeeSiteLog = access.CanSeeSiteLog,
                CanExposeToClient = access.CanExposeToClient
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving standing on project {ProjectId}", projectId);
            return ServiceResult<ProjectAccessDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    private static ProjectMemberDto ToDto(tbl_ProjectMember m, AppUser? user, AppUser? assignedBy) => new()
    {
        Id = m.Id,
        ProjectId = m.ProjectId,
        UserId = m.UserId,
        PartyName = m.PartyName,
        Side = m.Side,
        IsMediator = m.IsMediator,
        Specialization = m.Specialization,
        Title = m.Title,
        IsActive = m.IsActive,
        JoinedAt = m.JoinedAt,
        LeftAt = m.LeftAt,
        DateTimeCreated = m.DateTimeCreated,
        UserFullName = user is not null
            ? $"{user.FirstName} {user.LastName}".Trim()
            : m.PartyName,
        UserEmail = user?.Email,
        UserProfilePicUrl = user?.ProfilePicUrl,
        AssignedByName = assignedBy is not null ? $"{assignedBy.FirstName} {assignedBy.LastName}".Trim() : null
    };
}
