using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using assetlen.Shared.Models.Models;
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
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}")]
public class AssetlenHub : Hub
{
    private readonly ITenantProvider _tenant;
    private readonly ILogger<AssetlenHub> _logger;

    public AssetlenHub(ITenantProvider tenant, ILogger<AssetlenHub> logger)
    {
        _tenant = tenant;
        _logger = logger;
    }

    public Task JoinProject(string projectId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public Task LeaveProject(string projectId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public async Task JoinStream(string streamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, StreamGroup(streamId));
        if (!_tenant.IsExternal())
            await Groups.AddToGroupAsync(Context.ConnectionId, CrewStreamGroup(streamId));
    }

    public async Task LeaveStream(string streamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, StreamGroup(streamId));
        if (!_tenant.IsExternal())
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, CrewStreamGroup(streamId));
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
