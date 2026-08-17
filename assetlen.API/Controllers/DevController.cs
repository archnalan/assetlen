using assetlen.Service.DbServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public DevController(IWebHostEnvironment env, IDevSeedService seed, ILogger<DevController> logger)
    {
        _env = env;
        _seed = seed;
        _logger = logger;
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
}
