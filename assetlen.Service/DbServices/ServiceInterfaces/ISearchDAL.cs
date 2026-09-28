using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// One search across every project the reader stands on — commitments, the
/// text read out of photos and documents, the imported thread, files, the Site
/// Diary, stages (plan.md P6).
/// <para>
/// Each source is filtered by the same rule as the surface that owns it, resolved
/// through <c>IProjectAccessService</c>: a result never shows what the reader
/// could not have opened directly. A hidden result is absent, not redacted.
/// </para>
/// </summary>
public interface ISearchDAL
{
    /// <param name="query">What the reader typed, question words and all.</param>
    /// <param name="projectId">Restrict to one project (and its sub-projects). Null searches all of them.</param>
    /// <param name="take">Results kept per group; totals still count every match.</param>
    Task<ServiceResult<SearchResultDto>> SearchAsync(
        string? query, string? projectId, int take, string userId, CancellationToken ct = default);
}
