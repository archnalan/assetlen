using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;

namespace assetlen.API.Controllers;

/// <summary>
/// The works report (works-report.md): live, issued, history, and the system's
/// own issuing. Every role reaches the controller; standing on the project
/// decides what comes back, and a support seat gets 404.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class WorksReportController : ControllerBase
{
    private readonly IWorksReportDAL _dal;
    private readonly ITenantProvider _tenant;
    private readonly IWebHostEnvironment _env;

    public WorksReportController(IWorksReportDAL dal, ITenantProvider tenant, IWebHostEnvironment env)
    {
        _dal = dal;
        _tenant = tenant;
        _env = env;
    }

    private ActionResult Answer<T>(ServiceResult<T> r) => r.IsSuccess ? Ok(r.Data) : StatusCode(r.StatusCode, r.Error.Message);

    [HttpGet]
    [ProducesResponseType(typeof(WorksReportDto), 200)]
    public async Task<ActionResult> Live([FromQuery] string projectId, [FromQuery] DateTime? asAt, CancellationToken ct) =>
        Answer(await _dal.GetLiveAsync(projectId, _tenant.GetUserId(), asAt, ct));

    [HttpPost]
    [ProducesResponseType(typeof(WorksReportDto), 200)]
    public async Task<ActionResult> Issue([FromBody] IssueReportDto dto, CancellationToken ct) =>
        Answer(await _dal.IssueAsync(dto, _tenant.GetUserId(), ct));

    [HttpGet]
    [ProducesResponseType(typeof(WorksReportDto), 200)]
    public async Task<ActionResult> Issued([FromQuery] string reportId, CancellationToken ct) =>
        Answer(await _dal.GetIssuedAsync(reportId, _tenant.GetUserId(), ct));

    [HttpGet]
    [ProducesResponseType(typeof(List<WorksReportSummaryDto>), 200)]
    public async Task<ActionResult> History([FromQuery] string projectId, CancellationToken ct) =>
        Answer(await _dal.GetHistoryAsync(projectId, _tenant.GetUserId(), ct));

    [HttpGet]
    [ProducesResponseType(typeof(ReportSettingsDto), 200)]
    public async Task<ActionResult> Settings([FromQuery] string projectId, CancellationToken ct) =>
        Answer(await _dal.GetSettingsAsync(projectId, _tenant.GetUserId(), ct));

    [HttpPut]
    [ProducesResponseType(typeof(ReportSettingsDto), 200)]
    public async Task<ActionResult> Settings([FromBody] ReportSettingsUpdateDto dto, CancellationToken ct) =>
        Answer(await _dal.UpdateSettingsAsync(dto, _tenant.GetUserId(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<ActionResult> QueuePosters([FromQuery] string projectId, CancellationToken ct) =>
        Answer(await _dal.QueuePostersAsync(projectId, _tenant.GetUserId(), ct));

    /// <summary>The narrative validator on its own, for a draft against the live report. Development hosts only.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(NarrativeCheckResultDto), 200)]
    public async Task<ActionResult> CheckNarrative([FromBody] NarrativeCheckDto dto, CancellationToken ct)
    {
        if (!_env.IsDevelopment()) return NotFound();
        return Answer(await _dal.CheckNarrativeAsync(dto, _tenant.GetUserId(), ct));
    }

    /// <summary>
    /// Runs the scheduled issuing now, as at a given moment. What the weekly and
    /// milestone jobs do on their own; exposed so a week with nobody logged in
    /// can be proved in minutes. Development hosts only — 404 anywhere else.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReportScheduleRunDto), 200)]
    public async Task<ActionResult> RunSchedule([FromQuery] DateTime? asAt, [FromQuery] string? projectId,
        [FromQuery] bool weekly = true, [FromQuery] bool milestones = true, CancellationToken ct = default)
    {
        if (!_env.IsDevelopment()) return NotFound();
        return Ok(await _dal.RunScheduleAsync(asAt, weekly, milestones, projectId, ct));
    }
}
