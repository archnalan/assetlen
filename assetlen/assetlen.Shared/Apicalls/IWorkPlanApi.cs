using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>The work plan and knocking lines off it (works-report.md §4.5).</summary>
public interface IWorkPlanApi
{
    /// <summary>The whole plan, or one stage's checklist when <paramref name="stageId"/> is given.</summary>
    [Get("/api/WorkPlan/GetPlan")]
    Task<IApiResponse<WorkPlanDto>> GetPlan([Query] string projectId, [Query] string? stageId = null);

    /// <summary>One photo, as a part. No photo, no tick.</summary>
    [Multipart]
    [Post("/api/WorkPlan/Tick")]
    Task<IApiResponse<DeliverableDto>> Tick(
        [AliasAs("deliverableId")] string deliverableId,
        [AliasAs("photo")] StreamPart photo);

    [Put("/api/WorkPlan/Reopen")]
    Task<IApiResponse<DeliverableDto>> Reopen([Query] string deliverableId);

    [Get("/api/WorkPlan/GetSchedule")]
    Task<IApiResponse<ScheduleDto>> GetSchedule([Query] string projectId);

    [Post("/api/WorkPlan/PreviewSchedule")]
    Task<IApiResponse<SchedulePreviewDto>> PreviewSchedule([Body] SchedulePreviewRequestDto dto);

    [Post("/api/WorkPlan/SaveSchedule")]
    Task<IApiResponse<ScheduleDto>> SaveSchedule([Body] ScheduleChangeDto dto);
}
