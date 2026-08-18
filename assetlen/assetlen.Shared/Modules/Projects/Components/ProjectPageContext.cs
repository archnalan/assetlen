using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Modules.Projects.Components;

/// <summary>
/// What a project screen is handed once <see cref="ProjectPage"/> has resolved
/// it: the project, and the caller's standing on that project.
/// <para>
/// The standing is a <b>mirror of the server's decision</b> so a page can render
/// the right surface without re-deriving the rules — never a substitute for it.
/// Every endpoint re-resolves access regardless of what the client believes, and
/// the two must not be allowed to drift into the client being the authority.
/// </para>
/// <para>
/// Note what is deliberately absent: nothing here reads a tenant-level role.
/// Sides are per project — the same person is a client on one and a mediator on
/// another — and inferring a side from a global role claim was a real bug.
/// </para>
/// </summary>
public sealed record ProjectPageContext(ProjectDto Project, ProjectAccessDto? Access)
{
    public string ProjectId => Project.Id;

    public string Currency => Project.Currency ?? "UGX";

    public bool IsClientSide => Access?.Side == ProjectSide.Client;

    /// <summary>One, at most two, per project. The single accountable name on everything that crosses.</summary>
    public bool IsMediator => Access?.IsMediator == true;

    public bool CanWrite => Access?.CanWrite == true;
    public bool CanManage => Access?.CanManage == true;

    /// <summary>Contractor-side members and mediators only.</summary>
    public bool CanSeeSiteLog => Access?.CanSeeSiteLog == true;

    /// <summary>The exposure gate — mediator, owner or manager.</summary>
    public bool CanExposeToClient => Access?.CanExposeToClient == true;

    // ─── Seat depth ──────────────────────────────────────────────
    // A side says which party you answer to; a seat says how much of the
    // engagement is yours. The bench the contractor staffs — fabricator,
    // photographer, foreman — reports on its own work and no more
    // (assetlen.md §10.1).

    /// <summary>A decision-maker for one of the two parties, as opposed to a trade brought on for one job.</summary>
    public bool IsPrincipal => Access?.Seat == ProjectSeat.Principal;

    public bool CanSeeMoney => Access?.CanSeeMoney == true;
    public bool CanSeeBrief => Access?.CanSeeBrief == true;
    public bool CanSeeDocuments => Access?.CanSeeDocuments == true;
    public bool CanSeeHistory => Access?.CanSeeHistory == true;
    public bool CanCapture => Access?.CanCapture == true;
    public bool CanSeeRegister => Access?.CanSeeRegister == true;

    /// <summary>Whose day starts at the camera. Changes where the project opens, not what they may do.</summary>
    public bool LandsOnCapture => Access?.LandsOnCapture == true;
}
