namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// How deep into a project a member's seat reaches.
/// <para>
/// A side says which party you answer to; a seat says how much of the
/// engagement is yours to see. Both principals' decision-makers hold the whole
/// picture. Everyone else was brought on to do one job and report on it — the
/// fabricator, the photographer, the foreman — and the money, the drawing
/// register and the raw thread are not part of that job.
/// </para>
/// <para>
/// This is the second half of assetlen.md §10.1: the developer names the two
/// principals, and the contractor staffs their own bench underneath. Peter never
/// asked to meet the aluminium fabricator, and the fabricator has no business
/// reading Peter's budget.
/// </para>
/// </summary>
public enum ProjectSeat
{
    /// <summary>
    /// A decision-maker for one of the two parties. Sees the engagement whole.
    /// </summary>
    Principal = 0,

    /// <summary>
    /// Brought on for one trade or task, under a principal. Sees the work they
    /// were invited for and nothing else.
    /// </summary>
    Support = 1
}

/// <summary>
/// Default seat for a <see cref="ProjectMemberSpecialization"/> — the sibling of
/// <see cref="ProjectSideDefaults"/>, derived rather than stored so an existing
/// roster classifies itself.
/// </summary>
public static class ProjectSeatDefaults
{
    public static ProjectSeat For(ProjectMemberSpecialization? specialization) => specialization switch
    {
        ProjectMemberSpecialization.ClientOwner => ProjectSeat.Principal,
        ProjectMemberSpecialization.ClientRepresentative => ProjectSeat.Principal,
        ProjectMemberSpecialization.Architect => ProjectSeat.Principal,
        ProjectMemberSpecialization.Lead => ProjectSeat.Principal,
        null => ProjectSeat.Principal,
        _ => ProjectSeat.Support
    };

    /// <summary>
    /// Whether this specialization builds or verifies against a drawing. A
    /// photographer does not, and a drawing register is the one place where a
    /// superseded revision in the wrong hands puts steel in the wrong place.
    /// </summary>
    public static bool ReadsDrawings(ProjectMemberSpecialization? specialization) =>
        For(specialization) == ProjectSeat.Principal
        || specialization is ProjectMemberSpecialization.Foreman
                          or ProjectMemberSpecialization.Engineer
                          or ProjectMemberSpecialization.Inspector
                          or ProjectMemberSpecialization.Subcontractor;

    /// <summary>
    /// Whether the daily job is the camera. These readers land on Capture, not
    /// on a dashboard they have no use for (Nalan.md, on the clerk of works).
    /// </summary>
    public static bool CapturesForALiving(ProjectMemberSpecialization? specialization) =>
        specialization is ProjectMemberSpecialization.Photographer;
}
