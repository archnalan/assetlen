using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Services;

/// <summary>
/// Thin façade over the SignalR connection to /hubs/assetlen. Components
/// subscribe to <see cref="StreamCommentReceived"/> and Join/Leave specific
/// streams; the service multiplexes a single connection across the page tree.
/// </summary>
public interface IStreamHubService : IAsyncDisposable
{
    bool IsConnected { get; }

    Task EnsureStartedAsync();
    Task JoinStreamAsync(string streamId);
    Task LeaveStreamAsync(string streamId);
    Task JoinProjectAsync(string projectId);
    Task LeaveProjectAsync(string projectId);

    /// <summary>Fired when a comment lands on a stream the user joined.</summary>
    event Action<StreamCommentEvent>? StreamCommentReceived;
}
