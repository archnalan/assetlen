using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.statics;

namespace assetlen.Service.Hubs;

/// <summary>
/// Single per-tenant hub for live presence + chat (Streams).
///
/// Group conventions:
///   project-{projectId}         : everyone in the project (presence)
///   stream-{streamId}           : everyone in this stream (currently
///                                 streamId == ProgressUpdateId)
///   stream-{streamId}:crew      : internal users only (Channel.Crew traffic)
///
/// A Client-channel message broadcasts to stream-{streamId}; a Crew message
/// broadcasts to stream-{streamId}:crew, so external principals never see
/// internal chatter even via the live transport.
///
/// Which side a caller is on is resolved <b>per project</b>. It used to be read
/// from the tenant-global role claim, which gave one person the same standing on
/// every project — so a mediator on one project was treated as internal on all
/// of them, and the live transport leaked crew traffic the REST layer correctly
/// withheld.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}")]
public class AssetlenHub : Hub
{
    private readonly ITenantProvider _tenant;
    private readonly AssetlenDbContext _context;
    private readonly IProjectAccessService _access;
    private readonly ILogger<AssetlenHub> _logger;

    public AssetlenHub(
        ITenantProvider tenant,
        AssetlenDbContext context,
        IProjectAccessService access,
        ILogger<AssetlenHub> logger)
    {
        _tenant = tenant;
        _context = context;
        _access = access;
        _logger = logger;
    }

    public async Task JoinProject(string projectId)
    {
        // Presence is not public: joining the group is how a connection starts
        // receiving that project's broadcasts.
        if (!await _access.CanReadAsync(projectId, _tenant.GetUserId()))
            throw new HubException("You do not have access to this project.");

        await Groups.AddToGroupAsync(Context.ConnectionId, ProjectGroup(projectId));
    }

    public Task LeaveProject(string projectId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public async Task JoinStream(string streamId)
    {
        var access = await ResolveStreamAccessAsync(streamId);
        if (!access.CanRead)
            throw new HubException("You do not have access to this stream.");

        await Groups.AddToGroupAsync(Context.ConnectionId, StreamGroup(streamId));

        // The crew group carries unsanitised Site Log traffic. Same test the
        // REST layer applies: contractor-side, or the mediator.
        if (access.CanSeeSiteLog)
            await Groups.AddToGroupAsync(Context.ConnectionId, CrewStreamGroup(streamId));
    }

    public async Task LeaveStream(string streamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, StreamGroup(streamId));

        // Leave unconditionally — a membership change mid-connection must not
        // strand a connection inside the crew group it can no longer justify.
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, CrewStreamGroup(streamId));
    }

    /// <summary>
    /// A stream id is a ProgressUpdate id; the side is a property of that
    /// entry's project, so the project has to be looked up to answer at all.
    /// </summary>
    private async Task<ProjectAccess> ResolveStreamAccessAsync(string streamId)
    {
        var projectId = await _context.tbl_ProgressUpdates
            .Where(u => u.Id == streamId)
            .Select(u => u.ProjectId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(projectId))
            return ProjectAccess.None;

        return await _access.ResolveAsync(projectId, _tenant.GetUserId());
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("AssetlenHub connect user={UserId} cnx={Cnx}",
            Context.UserIdentifier, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("AssetlenHub disconnect user={UserId} cnx={Cnx}",
            Context.UserIdentifier, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public static string ProjectGroup(string projectId) => $"project-{projectId}";
    public static string StreamGroup(string streamId) => $"stream-{streamId}";
    public static string CrewStreamGroup(string streamId) => $"stream-{streamId}:crew";
}
