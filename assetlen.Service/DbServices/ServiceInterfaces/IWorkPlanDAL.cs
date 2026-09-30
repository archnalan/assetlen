using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// The work plan and knocking items off it (works-report.md §4.5): one list of
/// every line on the house and its sub-projects, ticked Done on one photo.
/// </summary>
public interface IWorkPlanDAL
{
    /// <summary>
    /// The whole plan for this reader, or one stage's checklist when <paramref name="stageId"/> is given.
    /// Each line carries whether this reader may tick it and its full tick history.
    /// </summary>
    Task<ServiceResult<WorkPlanDto>> GetPlan(string projectId, string? stageId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Knock a line off on one photo. No photo, no tick. Ticking a line that is
    /// already done returns it as it stands.
    /// </summary>
    Task<ServiceResult<DeliverableDto>> Tick(string deliverableId, CaptureFile? photo, string userId, CancellationToken ct = default);

    /// <summary>Take the tick back. The photo, its entry and the earlier tick stay on record.</summary>
    Task<ServiceResult<DeliverableDto>> Reopen(string deliverableId, string userId, CancellationToken ct = default);

    // ─── The scheduler (works-report.md §4.6) ───────────────────────

    /// <summary>The house's plan, computed now (or as at a past or future day), for this reader.</summary>
    Task<ServiceResult<ScheduleDto>> GetSchedule(string projectId, DateOnly? asAt, string userId, CancellationToken ct = default);

    /// <summary>What a proposed change — and optionally a second one — would do. Nothing is saved.</summary>
    Task<ServiceResult<SchedulePreviewDto>> PreviewSchedule(SchedulePreviewRequestDto dto, string userId, CancellationToken ct = default);

    /// <summary>Apply a change, re-date the plan and store the result. A new handover restates its commitment.</summary>
    Task<ServiceResult<ScheduleDto>> SaveSchedule(ScheduleChangeDto dto, string userId, CancellationToken ct = default);

    /// <summary>Re-date the plan from today and store it.</summary>
    Task<ServiceResult<ScheduleDto>> Recompute(string projectId, string userId, CancellationToken ct = default);

    /// <summary>Re-date every scheduled house with nobody logged in (Law 0). Returns how many moved through.</summary>
    Task<int> RedateAllAsync(CancellationToken ct = default);

    /// <summary>Re-date one scheduled house from today, with nobody logged in.</summary>
    Task<bool> RedateProjectAsync(string projectId, CancellationToken ct = default);
}
