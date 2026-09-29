using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>
/// The works report (works-report.md): the live page, issued snapshots, their
/// history, and the owner's drafting setting — all filtered on the server.
/// </summary>
public interface IWorksReportApi
{
    [Get("/api/WorksReport/Live")]
    Task<IApiResponse<WorksReportDto>> Live([Query] string projectId, [Query(Format = "yyyy-MM-dd")] DateTime? asAt = null);

    [Post("/api/WorksReport/Issue")]
    Task<IApiResponse<WorksReportDto>> Issue([Body] IssueReportDto dto);

    [Get("/api/WorksReport/Issued")]
    Task<IApiResponse<WorksReportDto>> Issued([Query] string reportId);

    [Get("/api/WorksReport/History")]
    Task<IApiResponse<List<WorksReportSummaryDto>>> History([Query] string projectId);

    [Get("/api/WorksReport/Settings")]
    Task<IApiResponse<ReportSettingsDto>> Settings([Query] string projectId);

    [Put("/api/WorksReport/Settings")]
    Task<IApiResponse<ReportSettingsDto>> UpdateSettings([Body] ReportSettingsUpdateDto dto);
}
