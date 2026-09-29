using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// The works report (works-report.md): where everything stands and what is
/// next, assembled from the record with nobody writing it, and issued as a
/// frozen, hashed snapshot. It works from Peter's own import and declarations
/// with the contractor silent (Law 0), and it issues with the model switched off.
/// <para>
/// Access is resolved once through <c>IProjectAccessService</c>: the report is a
/// principal's document (support seats get 404), the side decides the audience,
/// and the money section exists only for a reader on the money.
/// </para>
/// </summary>
public interface IWorksReportDAL
{
    /// <summary>"As at now", never stored. <paramref name="asAt"/> previews the record as it stood at the end of a past day.</summary>
    Task<ServiceResult<WorksReportDto>> GetLiveAsync(string projectId, string userId, DateTime? asAt = null, CancellationToken ct = default);

    /// <summary>Freeze a snapshot: content hashed, previous report linked, delivered to the audience.</summary>
    Task<ServiceResult<WorksReportDto>> IssueAsync(IssueReportDto dto, string userId, CancellationToken ct = default);

    /// <summary>An issued report exactly as it was issued, redacted only for what this reader may not see.</summary>
    Task<ServiceResult<WorksReportDto>> GetIssuedAsync(string reportId, string userId, CancellationToken ct = default);

    Task<ServiceResult<List<WorksReportSummaryDto>>> GetHistoryAsync(string projectId, string userId, CancellationToken ct = default);

    Task<ServiceResult<ReportSettingsDto>> GetSettingsAsync(string projectId, string userId, CancellationToken ct = default);

    Task<ServiceResult<ReportSettingsDto>> UpdateSettingsAsync(ReportSettingsUpdateDto dto, string userId, CancellationToken ct = default);

    /// <summary>Queue posters for a project's videos that have none yet.</summary>
    /// <summary>Put a draft to the validator against one of the live report's targets — the guard, testable on its own.</summary>
    Task<ServiceResult<NarrativeCheckResultDto>> CheckNarrativeAsync(NarrativeCheckDto dto, string userId, CancellationToken ct = default);

    Task<ServiceResult<int>> QueuePostersAsync(string projectId, string userId, CancellationToken ct = default);

    /// <summary>
    /// The system's own issuing (works-report.md §7): weekly, and on milestones —
    /// a stage completing or a completion date lapsing. Runs with nobody logged in.
    /// </summary>
    Task<ReportScheduleRunDto> RunScheduleAsync(DateTime? asAt, bool weekly, bool milestones, string? projectId = null, CancellationToken ct = default);
}
