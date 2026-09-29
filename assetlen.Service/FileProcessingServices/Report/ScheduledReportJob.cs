using Hangfire;
using assetlen.Service.DbServices.ServiceInterfaces;

namespace assetlen.Service.FileProcessingServices.Report;

/// <summary>
/// Issues works reports with nobody logged in (works-report.md §7, Law 0):
/// weekly on Sunday evening, and whenever a stage completes or a completion
/// date passes unmet.
/// </summary>
public sealed class ScheduledReportJob
{
    private readonly IWorksReportDAL _reports;

    public ScheduledReportJob(IWorksReportDAL reports) => _reports = reports;

    [AutomaticRetry(Attempts = 1)]
    [DisableConcurrentExecution(600)]
    public Task WeeklyAsync() => _reports.RunScheduleAsync(null, weekly: true, milestones: false);

    [AutomaticRetry(Attempts = 1)]
    [DisableConcurrentExecution(600)]
    public Task MilestonesAsync() => _reports.RunScheduleAsync(null, weekly: false, milestones: true);
}
