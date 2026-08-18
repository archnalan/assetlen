namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// The complete answer to "what is this user, in this project?" — resolved once
/// by <c>IProjectAccessService</c> and never re-derived inline.
/// <para>
/// Four orthogonal facts. <see cref="Level"/> is <em>how much</em> they may do,
/// <see cref="Side"/> is <em>which party</em> they belong to,
/// <see cref="IsMediator"/> is whether they may move material between the two,
/// and <see cref="Specialization"/> is <em>what they were brought on to do</em>.
/// Authorization needs all of them: a client-side principal with
/// <c>Write</c> may comment and raise queries but must never see the Site Diary,
/// only a mediator may expose Site Diary material to the Client channel, and a
/// photographer with the same Write level sees neither the money nor the
/// drawings.
/// </para>
/// </summary>
public readonly record struct ProjectAccess(
    ProjectAccessLevel Level,
    ProjectSide? Side,
    bool IsMediator,
    ProjectMemberSpecialization? Specialization = null,
    bool? HandlesMoney = null)
{
    public static readonly ProjectAccess None = new(ProjectAccessLevel.None, null, false);

    public bool CanRead => Level >= ProjectAccessLevel.Read;
    public bool CanWrite => Level >= ProjectAccessLevel.Write;
    public bool CanManage => Level >= ProjectAccessLevel.Manage;

    /// <summary>How deep this seat reaches — see <see cref="ProjectSeat"/>.</summary>
    public ProjectSeat Seat => ProjectSeatDefaults.For(Specialization);

    /// <summary>
    /// True when this user is confined to the Client channel. Distinct from the
    /// tenant-level <c>ITenantProvider.IsExternal()</c>, which is global: a
    /// contractor-side employee is internal overall but may still be
    /// client-side on somebody else's project.
    /// </summary>
    public bool IsClientSide => Side == ProjectSide.Client;

    /// <summary>
    /// True when this user may read the unsanitised Site Diary — contractor-side
    /// members, and mediators regardless of which side they sit on.
    /// </summary>
    public bool CanSeeSiteLog => Level >= ProjectAccessLevel.Read
                                 && (Side == ProjectSide.Contractor || IsMediator);

    /// <summary>
    /// True when this user may move material across the channel boundary.
    /// Mediators do this by role; project owners and managers by authority.
    /// </summary>
    public bool CanExposeToClient => IsMediator || Level >= ProjectAccessLevel.Manage;

    /// <summary>
    /// Whether money is part of this seat. By default the two parties'
    /// decision-makers and the mediator between them — a release a foreman can
    /// read is a wage negotiation nobody asked for.
    /// <para>
    /// Gated on the seat rather than on <see cref="Level"/>, so a second
    /// principal who does not own the project — the representative who settles
    /// finishes — is still one of the people the budget is between.
    /// </para>
    /// <para>
    /// <see cref="HandlesMoney"/> overrides it in either direction, per project.
    /// Somebody put on the money deliberately — an engineer asked to follow
    /// releases and account for them alongside the architect — gets it without
    /// being promoted to a principal everywhere else.
    /// </para>
    /// </summary>
    public bool CanSeeMoney => CanRead && (HandlesMoney ?? (Seat == ProjectSeat.Principal || IsMediator));

    /// <summary>
    /// Whether the Client Brief is part of this seat. It is written *for* the
    /// client side, so they read it and whoever publishes it reads it; the bench
    /// doing the work has its own surface and does not need the curated one.
    /// </summary>
    public bool CanSeeBrief => CanRead && (IsClientSide || CanExposeToClient);

    /// <summary>
    /// Whether the drawing register is part of this seat. Whoever builds from a
    /// drawing or approves one reads it; the rest of the bench does not.
    /// </summary>
    public bool CanSeeDocuments => CanRead && ProjectSeatDefaults.ReadsDrawings(Specialization);

    /// <summary>
    /// Whether the raw ingested pile is part of this seat. It is the delivery
    /// side's own history and it is unsanitised twice over — nothing edits it
    /// and nobody curated it (CLAUDE.md §4.3).
    /// </summary>
    public bool CanSeeHistory => CanSeeSiteLog && Seat == ProjectSeat.Principal;

    /// <summary>Whether this seat posts to the Site Diary.</summary>
    public bool CanCapture => CanWrite && CanSeeSiteLog;

    /// <summary>
    /// Whether the register of commitments is part of this seat. A commitment is
    /// addressed to a decision-maker; a support seat has nothing to answer there.
    /// </summary>
    public bool CanSeeRegister => CanRead && Seat == ProjectSeat.Principal;

    /// <summary>
    /// Whether this reader's day starts at the camera rather than at a
    /// dashboard. Changes where the project opens, not what they may do.
    /// </summary>
    public bool LandsOnCapture => CanCapture && ProjectSeatDefaults.CapturesForALiving(Specialization);
}
