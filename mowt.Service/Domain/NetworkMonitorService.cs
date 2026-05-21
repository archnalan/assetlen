using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using mowt.API.Domain.Interfaces;
using mowt.Service.DbServices.ServiceInterfaces;

namespace mowt.API.Domain;

public class NetworkMonitorService : BackgroundService
{
    private readonly ILogger<NetworkMonitorService> _logger;
    private readonly IServiceProvider _services;
    private readonly HttpClient _httpClient;
    private static bool _lastKnownInternetState;
    private volatile bool _lastLocalNetworkStatus;
    private ConcurrentQueue<bool> _stateQueue = new ConcurrentQueue<bool>();
    private Timer _pollingTimer;

    public NetworkMonitorService(
        ILogger<NetworkMonitorService> logger,
        IServiceProvider services,
        HttpClient httpClient)
    {
        _logger = logger;
        _services = services;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Initial state setup
            _lastLocalNetworkStatus = NetworkInterface.GetIsNetworkAvailable();
            _lastKnownInternetState = _lastLocalNetworkStatus && await PerformInternetCheckAsync();
            _logger.LogInformation($"Initial state - Local: {_lastLocalNetworkStatus}, Internet: {_lastKnownInternetState}");

            // Subscribe to network changes
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;

            // Start polling timer for local network status only
            _pollingTimer = new Timer(_ =>
            {
                var currentLocalState = NetworkInterface.GetIsNetworkAvailable();
                if (currentLocalState != _lastLocalNetworkStatus)
                {
                    _lastLocalNetworkStatus = currentLocalState;
                    _stateQueue.Enqueue(currentLocalState);
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            // State processing loop
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_stateQueue.TryDequeue(out var newLocalState))
                {
                    await HandleLocalStateChange(newLocalState);
                }
                await Task.Delay(100, stoppingToken);
            }
        }
        finally
        {
            _pollingTimer?.Dispose();
            NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
        }
    }

    private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
    {
        if (e.IsAvailable != _lastLocalNetworkStatus)
        {
            _lastLocalNetworkStatus = e.IsAvailable;
            _stateQueue.Enqueue(e.IsAvailable);
        }
    }

    private async Task HandleLocalStateChange(bool newLocalState)
    {
        try
        {
            // Handle local network down scenario
            if (!newLocalState)
            {
                if (_lastKnownInternetState)
                {
                    _logger.LogInformation("Network connection lost");
                    _lastKnownInternetState = false;
                }
                return;
            }

            // Local network is up - verify actual internet connectivity
            var internetAvailable = await PerformInternetCheckAsync();

            if (internetAvailable == _lastKnownInternetState)
                return;

            _logger.LogInformation($"Internet state changed from {_lastKnownInternetState} to {internetAvailable}");

            if (internetAvailable)
            {
                _logger.LogInformation("Internet restored. Triggering recovery...");
                await TriggerRecoveryActionAsync();
            }

            _lastKnownInternetState = internetAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling network state change");
        }
    }

    private async Task<bool> PerformInternetCheckAsync()
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
            return false;

        using var scope = _services.CreateScope();
        var isOnline = scope.ServiceProvider.GetRequiredService<IOnlineIdentityVerifier>();

        if (isOnline.IsOnlineApi())
            return true;

        var syncDAL = scope.ServiceProvider.GetRequiredService<ISyncDAL>();
        return await syncDAL.IsInternetAvailable();
    }

    private async Task TriggerRecoveryActionAsync()
    {
        using var scope = _services.CreateScope();
        var recoveryService = scope.ServiceProvider.GetRequiredService<IRecoveryActionService>();
        await recoveryService.PerformRecoveryAsync();
    }
}