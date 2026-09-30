using Hangfire;
using assetlen.Service.DbServices.ServiceInterfaces;

namespace assetlen.Service.FileProcessingServices;

/// <summary>
/// Re-dates every scheduled house each morning with nobody logged in (Law 0,
/// works-report.md §4.6): an item past its finish and not ticked goes late and
/// moves what follows it, and a wait that outlives its date runs on.
/// </summary>
public sealed class WorkPlanRedateJob
{
    private readonly IWorkPlanDAL _plans;

    public WorkPlanRedateJob(IWorkPlanDAL plans) => _plans = plans;

    [AutomaticRetry(Attempts = 1)]
    [DisableConcurrentExecution(600)]
    public Task RunAsync() => _plans.RedateAllAsync();
}
