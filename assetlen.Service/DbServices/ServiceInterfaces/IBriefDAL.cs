using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// Peter's two surfaces (plan.md P7): the home that opens on every project at
/// once, and the daily brief that assembles itself. Both run on material Peter
/// forwarded himself and publish whether or not the contractor ever logs in (Law 0).
/// <para>
/// Every source is filtered by the rule of the surface that owns it, resolved
/// through <c>IProjectAccessService</c> — except the truth floor, which reaches
/// the client side regardless of which channel it was recorded on (assetlen.md §5).
/// </para>
/// </summary>
public interface IBriefDAL
{
    /// <summary>Every project the reader stands on — money position, what they owe, what moved — from one membership query.</summary>
    Task<ServiceResult<HomeDto>> GetHomeAsync(string userId, int movedDays, CancellationToken ct = default);

    /// <summary>Everything owed by the reader across every project, soonest by-when first. What the rail counts.</summary>
    Task<ServiceResult<List<OwedItemDto>>> GetOwedAsync(string userId, CancellationToken ct = default);

    /// <param name="day">The last day of the window; today when null.</param>
    /// <param name="days">1 to 31.</param>
    Task<ServiceResult<DailyBriefDto>> GetBriefAsync(string projectId, DateTime? day, int days, string userId, CancellationToken ct = default);
}
