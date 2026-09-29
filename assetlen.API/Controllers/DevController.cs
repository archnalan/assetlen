using assetlen.Service.DataAccess;
using assetlen.Service.DbServices;
using assetlen.Service.FileProcessingServices.Push;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace assetlen.API.Controllers;

/// <summary>
/// Development-only utilities.
///
/// <para><b>The gate is here, on the server.</b> Every action answers 404
/// outside a Development host, whatever the caller believes about the
/// environment. Hiding the buttons in the client would be a UI decision; this
/// is the actual control, and 404 rather than 403 so a deployed build does not
/// even confirm the endpoint exists.</para>
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class DevController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IDevSeedService _seed;
    private readonly ILogger<DevController> _logger;
    private readonly IPushDAL _push;
    private readonly VapidKeys _vapid;
    private readonly AssetlenDbContext _context;
    private readonly ITenantProvider _tenant;

    public DevController(IWebHostEnvironment env, IDevSeedService seed, ILogger<DevController> logger,
        IPushDAL push, VapidKeys vapid, AssetlenDbContext context, ITenantProvider tenant)
    {
        _env = env;
        _seed = seed;
        _logger = logger;
        _push = push;
        _vapid = vapid;
        _context = context;
        _tenant = tenant;
    }

    /// <summary>
    /// Provisions the canonical demo world and returns what it found or made.
    /// Idempotent: safe to call before every persona sign-in.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SeedDemo(CancellationToken ct)
    {
        if (!_env.IsDevelopment()) return NotFound();

        try
        {
            var result = await _seed.SeedAsync(ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dev demo seed failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Subscribes the caller to a stand-in push service on this host that holds the
    /// browser's half of the keys, so encryption, VAPID and latency are tested end
    /// to end without a phone.
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> PushSinkSubscribe(CancellationToken ct)
    {
        if (!_env.IsDevelopment()) return NotFound();
        var (id, p256dh, auth) = DevPushSink.Create();
        var endpoint = $"{Request.Scheme}://{Request.Host}/api/Dev/PushSink/{id}";
        var r = await _push.Subscribe(new PushSubscriptionCreateDto { Endpoint = endpoint, P256dh = p256dh, Auth = auth, UserAgent = "dev-sink" },
            _tenant.GetUserId(), ct);
        return r.IsSuccess ? Ok(new { sinkId = id, endpoint }) : StatusCode(r.StatusCode, r.Error.Message);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> PushSink(string id)
    {
        if (!_env.IsDevelopment()) return NotFound();
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        var known = DevPushSink.Receive(id, ms.ToArray(), Request.Headers.ContentEncoding.ToString(),
            Request.Headers.Authorization.ToString(), _vapid.PublicKey);
        return known ? StatusCode(201) : StatusCode(410);
    }

    [HttpGet("{id}")]
    public IActionResult PushSinkInbox(string id)
    {
        if (!_env.IsDevelopment()) return NotFound();
        return Ok(DevPushSink.Inbox(id).Select(r => new { at = r.At, payload = r.Payload, vapidValid = r.VapidValid, encoding = r.Encoding, error = r.Error }));
    }

    /// <summary>
    /// How often an account has signed in — the proof that a suite ran with the
    /// contractor silent (Law 0), rather than merely with his token unused.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LoginStats([FromQuery] string email, CancellationToken ct)
    {
        if (!_env.IsDevelopment()) return NotFound();
        var user = await _context.Users.IgnoreQueryFilters().AsNoTracking()
            .Where(u => u.Email != null && u.Email.ToLower() == (email ?? "").ToLower()).Select(u => new { u.Id }).FirstOrDefaultAsync(ct);
        if (user is null) return Ok(new { exists = false, logins = 0, lastLoginAt = (DateTime?)null });
        var tokens = await _context.RefreshTokens.IgnoreQueryFilters().AsNoTracking()
            .Where(t => t.UserId == user.Id).ToListAsync(ct);
        return Ok(new
        {
            exists = true,
            logins = tokens.Count,
            lastLoginAt = tokens.Count == 0 ? (DateTime?)null : tokens.Max(t => t.LastLoginAt)
        });
    }
}
