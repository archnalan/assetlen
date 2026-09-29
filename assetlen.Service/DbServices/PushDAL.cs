using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.FileProcessingServices.Push;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <summary>A person's devices, and what was sent to them. Never anyone else's.</summary>
public interface IPushDAL
{
    Task<PushStatusDto> GetStatus(string userId, CancellationToken ct = default);
    Task<ServiceResult<PushStatusDto>> Subscribe(PushSubscriptionCreateDto dto, string userId, CancellationToken ct = default);
    Task<ServiceResult<PushStatusDto>> Unsubscribe(string endpoint, string userId, CancellationToken ct = default);
    Task<List<PushDeliveryDto>> GetMyDeliveries(int take, string userId, CancellationToken ct = default);
    Task<int> SendTest(string userId, CancellationToken ct = default);
}

public sealed class PushDAL : IPushDAL
{
    private readonly AssetlenDbContext _context;
    private readonly VapidKeys _keys;
    private readonly INotifier _notifier;
    private readonly ILogger<PushDAL> _logger;

    public PushDAL(AssetlenDbContext context, VapidKeys keys, INotifier notifier, ILogger<PushDAL> logger)
    {
        _context = context;
        _keys = keys;
        _notifier = notifier;
        _logger = logger;
    }

    private IQueryable<tbl_PushSubscription> Mine(string userId) =>
        _context.tbl_PushSubscriptions.IgnoreQueryFilters().Where(s => s.UserId == userId && s.IsDeleted != true);

    public async Task<PushStatusDto> GetStatus(string userId, CancellationToken ct = default)
    {
        var subs = await Mine(userId).AsNoTracking().ToListAsync(ct);
        return new PushStatusDto
        {
            PublicKey = _keys.PublicKey,
            Subscriptions = subs.Count,
            LastDeliveredAt = subs.Max(s => s.LastSuccessAt),
            Enabled = subs.Count > 0
        };
    }

    public async Task<ServiceResult<PushStatusDto>> Subscribe(PushSubscriptionCreateDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            if (!Uri.TryCreate(dto.Endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                return ServiceResult<PushStatusDto>.Failure(new BadRequestException("A push endpoint must be an https address."));
            try
            {
                if (WebPushCrypto.FromB64Url(dto.P256dh!).Length != 65 || WebPushCrypto.FromB64Url(dto.Auth!).Length < 16)
                    return ServiceResult<PushStatusDto>.Failure(new BadRequestException("The subscription keys are not valid."));
            }
            catch (FormatException)
            {
                return ServiceResult<PushStatusDto>.Failure(new BadRequestException("The subscription keys are not valid."));
            }

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dto.Endpoint!))).ToLowerInvariant();
            var row = await _context.tbl_PushSubscriptions.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.EndpointHash == hash, ct);
            var tenantId = await _context.Users.IgnoreQueryFilters().Where(u => u.Id == userId).Select(u => u.TenantId).FirstOrDefaultAsync(ct);
            if (row is null)
            {
                row = new tbl_PushSubscription { EndpointHash = hash, TenantId = tenantId };
                _context.tbl_PushSubscriptions.Add(row);
            }
            // A device changes hands on sign-in; the endpoint follows whoever subscribed it last.
            row.UserId = userId;
            row.Endpoint = dto.Endpoint;
            row.P256dh = dto.P256dh;
            row.Auth = dto.Auth;
            row.UserAgent = dto.UserAgent;
            row.IsDeleted = false;
            row.ConsecutiveFailures = 0;
            await _context.SaveChangesAsync(ct);
            return ServiceResult<PushStatusDto>.Success(await GetStatus(userId, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not store a push subscription");
            return ServiceResult<PushStatusDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<PushStatusDto>> Unsubscribe(string endpoint, string userId, CancellationToken ct = default)
    {
        var rows = await Mine(userId).Where(s => s.Endpoint == endpoint).ToListAsync(ct);
        foreach (var r in rows) r.IsDeleted = true;
        await _context.SaveChangesAsync(ct);
        return ServiceResult<PushStatusDto>.Success(await GetStatus(userId, ct));
    }

    public async Task<List<PushDeliveryDto>> GetMyDeliveries(int take, string userId, CancellationToken ct = default) =>
        await _context.tbl_PushDeliveries.IgnoreQueryFilters().AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.QueuedAt)
            .Take(Math.Clamp(take, 1, 100))
            .Select(d => new PushDeliveryDto
            {
                Id = d.Id,
                Kind = d.Kind,
                Title = d.Title,
                Body = d.Body,
                Url = d.Url,
                Status = d.Status,
                QueuedAt = d.QueuedAt,
                SentAt = d.SentAt,
                HttpStatus = d.HttpStatus,
                LatencyMs = d.SentAt == null ? null : (int?)EF.Functions.DateDiffMillisecond(d.QueuedAt, d.SentAt.Value)
            })
            .ToListAsync(ct);

    public Task<int> SendTest(string userId, CancellationToken ct = default) =>
        _notifier.NotifyUsersAsync(new[] { userId }, null, PushKind.Test,
            "ASSETLEN", "Notifications are on for this device.", "/", ct);
}
