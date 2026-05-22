using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.statics;

namespace assetlen.Shared.Services;

public class StreamHubService : IStreamHubService
{
    private readonly IStorageService _localStorage;
    private readonly ILogger<StreamHubService> _logger;
    private readonly Uri _baseUri;
    private HubConnection? _connection;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public StreamHubService(IStorageService localStorage, ILogger<StreamHubService> logger, Uri apiBase)
    {
        _localStorage = localStorage;
        _logger = logger;
        _baseUri = apiBase;
    }

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public event Action<StreamCommentEvent>? StreamCommentReceived;

    public async Task EnsureStartedAsync()
    {
        if (_connection is not null && _connection.State != HubConnectionState.Disconnected) return;

        await _gate.WaitAsync();
        try
        {
            if (_connection is null)
            {
                var hubUri = new Uri(_baseUri, "/hubs/assetlen");
                _connection = new HubConnectionBuilder()
                    .WithUrl(hubUri, opts =>
                    {
                        opts.AccessTokenProvider = async () =>
                        {
                            var session = await _localStorage.GetItemAsync<LoginResponseDto>(StorageKeys.SessionState);
                            return session?.token;
                        };
                    })
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<StreamCommentEvent>("ReceiveStreamComment", evt =>
                {
                    try { StreamCommentReceived?.Invoke(evt); }
                    catch (Exception ex) { _logger.LogError(ex, "StreamCommentReceived handler threw"); }
                });

                _connection.Reconnected += _ =>
                {
                    _logger.LogInformation("AssetlenHub reconnected");
                    return Task.CompletedTask;
                };
                _connection.Closed += ex =>
                {
                    _logger.LogWarning(ex, "AssetlenHub closed");
                    return Task.CompletedTask;
                };
            }

            if (_connection.State == HubConnectionState.Disconnected)
            {
                try { await _connection.StartAsync(); }
                catch (Exception ex) { _logger.LogError(ex, "AssetlenHub start failed"); }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task JoinStreamAsync(string streamId)
    {
        await EnsureStartedAsync();
        if (IsConnected) await _connection!.InvokeAsync("JoinStream", streamId);
    }

    public async Task LeaveStreamAsync(string streamId)
    {
        if (IsConnected) await _connection!.InvokeAsync("LeaveStream", streamId);
    }

    public async Task JoinProjectAsync(string projectId)
    {
        await EnsureStartedAsync();
        if (IsConnected) await _connection!.InvokeAsync("JoinProject", projectId);
    }

    public async Task LeaveProjectAsync(string projectId)
    {
        if (IsConnected) await _connection!.InvokeAsync("LeaveProject", projectId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            try { await _connection.DisposeAsync(); }
            catch { /* swallow on teardown */ }
        }
        _gate.Dispose();
    }
}
