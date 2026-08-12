namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// Which of a project's two principal parties a member belongs to.
/// <para>
/// A project has exactly two sides and, between them, one or at most two
/// <em>mediators</em> — the architect / lead consultant who advises the client
/// and tasks the delivery team. Either side may hold any number of members.
/// </para>
/// <para>
/// This is the structural fact behind the Client / Crew channel split: a
/// <see cref="Contractor"/> member writes to the Site Log; only what a mediator
/// exposes crosses to a <see cref="Client"/> member. Never infer the side from
/// a tenant-level <c>UserRoles</c> value — a project's sides are per-project.
/// </para>
/// </summary>
public enum ProjectSide
{
    /// <summary>
    /// The paying side: the developer who funds stages, plus their
    /// representatives on the ground. Reads the Client Brief, never the Site Log.
    /// </summary>
    Client = 0,

    /// <summary>
    /// The delivery side: site engineer, clerk of works, crews, subcontractors.
    /// Captures into the Site Log without judgement about what the client wants.
    /// </summary>
    Contractor = 1
}

/// <summary>
/// Default side for a <see cref="ProjectMemberSpecialization"/>. A caller may
/// override it — an architect can sit on either side depending on who retained
/// them — but the default keeps the common case free of a second decision.
/// </summary>
public static class ProjectSideDefaults
{
    public static ProjectSide For(ProjectMemberSpecialization specialization) => specialization switch
    {
        ProjectMemberSpecialization.ClientOwner => ProjectSide.Client,
        ProjectMemberSpecialization.ClientRepresentative => ProjectSide.Client,
        ProjectMemberSpecialization.Observer => ProjectSide.Client,
        _ => ProjectSide.Contractor
    };

    /// <summary>
    /// Specializations that mediate between the two sides by default. The
    /// architect is the canonical case: retained by the client, directing the
    /// build, and the only person who sees both surfaces.
    /// </summary>
    public static bool MediatesByDefault(ProjectMemberSpecialization specialization) =>
        specialization is ProjectMemberSpecialization.Architect
                       or ProjectMemberSpecialization.Lead;

    /// <summary>Maximum mediators on one project. Two is a hard ceiling, not a default.</summary>
    public const int MaxMediators = 2;
}
