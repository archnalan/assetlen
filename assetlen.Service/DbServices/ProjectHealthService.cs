using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <summary>
/// Simple rule engine for project health calculations.
/// No complex forecasting — MVP clarity.
/// </summary>
public class ProjectHealthService : IProjectHealthService
{
    public decimal CalculateFundingPercentage(decimal? totalBudget, decimal totalFunded)
    {
        var budget = totalBudget ?? 0;
        if (budget <= 0) return 0;
        return Math.Round(totalFunded / budget * 100, 2);
    }

    public decimal CalculateCompletionPercentage(IEnumerable<StageDto> stages)
    {
        var stageList = stages?.ToList();
        if (stageList == null || stageList.Count == 0) return 0;

        // Weighted by budget amount
        decimal totalBudget = stageList.Sum(s => s.BudgetAmount ?? 0);
        if (totalBudget <= 0)
        {
            // Equal weight fallback
            return Math.Round(stageList.Average(s => s.CompletionPercentage ?? 0), 2);
        }

        decimal weightedSum = stageList.Sum(s => (s.CompletionPercentage ?? 0) * (s.BudgetAmount ?? 0));
        return Math.Round(weightedSum / totalBudget, 2);
    }

    public int CalculateTimelineVariance(DateTime? expectedEnd, DateTime? revisedEnd)
    {
        var endDate = revisedEnd ?? expectedEnd;
        if (endDate == null) return 0;

        return (int)(DateTime.UtcNow - endDate.Value).TotalDays;
    }

    /// <summary>
    /// Simple MVP risk rules:
    ///   Red   — timeline overdue > 14 days
    ///   Yellow — funded% − completed% > 20, or no update in 7 days, or overdue 1-14 days
    ///   Green  — everything healthy
    /// </summary>
    public RiskLevel CalculateRiskLevel(
        decimal fundedPct, decimal completedPct,
        DateTime? expectedEnd, DateTime? lastUpdateDate)
    {
        // Rule 1: Timeline overdue > 14 days → Red
        if (expectedEnd.HasValue && DateTime.UtcNow > expectedEnd.Value.AddDays(14))
            return RiskLevel.Red;

        // Rule 2: Funding-to-completion gap > 20% → Yellow
        if (fundedPct - completedPct > 20)
            return RiskLevel.Yellow;

        // Rule 3: No update in 7 days → Yellow
        if (lastUpdateDate.HasValue && (DateTime.UtcNow - lastUpdateDate.Value).TotalDays > 7)
            return RiskLevel.Yellow;

        // Rule 4: Timeline overdue 1-14 days → Yellow
        if (expectedEnd.HasValue && DateTime.UtcNow > expectedEnd.Value)
            return RiskLevel.Yellow;

        return RiskLevel.Green;
    }
}
