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
}
