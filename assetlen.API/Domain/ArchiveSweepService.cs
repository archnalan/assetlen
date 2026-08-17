using assetlen.Service.DbServices.ServiceInterfaces;

namespace assetlen.API.Domain;

/// <summary>
/// Empties the bin: soft-deletes archived projects once their thirty days are
/// up. Idempotent — the query asks what is older than the cutoff, not what came
/// due since the last pass, so a missed day costs nothing.
/// </summary>
public sealed class ArchiveSweepService : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Period = TimeSpan.FromHours(24);

    private readonly IServiceProvider _services;
    private readonly ILogger<ArchiveSweepService> _logger;

    public ArchiveSweepService(IServiceProvider services, ILogger<ArchiveSweepService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Period);

        do
        {
            await SweepAsync(stoppingToken);
        }
        while (await SafeWait(timer, stoppingToken));
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var projects = scope.ServiceProvider.GetRequiredService<IProjectDAL>();

            var result = await projects.PurgeExpiredArchives(ct);

            if (result.IsSuccess && result.Data > 0)
                _logger.LogInformation("Archive sweep removed {Count} expired project(s).", result.Data);
            else if (!result.IsSuccess)
                _logger.LogWarning("Archive sweep reported: {Message}", result.Error.Message);
        }
        catch (OperationCanceledException)
        {
            // Shutting down mid-sweep; the next boot re-reads the same cutoff.
        }
        catch (Exception ex)
        {
            // Never take the host down: a full bin is a nuisance, an API that
            // will not start is an outage.
            _logger.LogError(ex, "Archive sweep failed");
        }
    }

    private static async Task<bool> SafeWait(PeriodicTimer timer, CancellationToken ct)
    {
        try { return await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { return false; }
    }
}
