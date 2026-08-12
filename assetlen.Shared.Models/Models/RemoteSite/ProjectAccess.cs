namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// The complete answer to "what is this user, in this project?" — resolved once
/// by <c>IProjectAccessService</c> and never re-derived inline.
/// <para>
/// Three orthogonal facts. <see cref="Level"/> is <em>how much</em> they may do,
/// <see cref="Side"/> is <em>which party</em> they belong to, and
/// <see cref="IsMediator"/> is whether they may move material between the two.
/// Authorization needs all three: a client-side principal with
/// <c>Write</c> may comment and raise queries but must never see the Site Log,
/// and only a mediator may expose Site Log material to the Client channel.
/// </para>
/// </summary>
public readonly record struct ProjectAccess(
    ProjectAccessLevel Level,
    ProjectSide? Side,
    bool IsMediator)
{
    public static readonly ProjectAccess None = new(ProjectAccessLevel.None, null, false);

    public bool CanRead => Level >= ProjectAccessLevel.Read;
    public bool CanWrite => Level >= ProjectAccessLevel.Write;
    public bool CanManage => Level >= ProjectAccessLevel.Manage;

    /// <summary>
    /// True when this user is confined to the Client channel. Distinct from the
    /// tenant-level <c>ITenantProvider.IsExternal()</c>, which is global: a
    /// contractor-side employee is internal overall but may still be
    /// client-side on somebody else's project.
    /// </summary>
    public bool IsClientSide => Side == ProjectSide.Client;

    /// <summary>
    /// True when this user may read the unsanitised Site Log — contractor-side
    /// members, and mediators regardless of which side they sit on.
    /// </summary>
    public bool CanSeeSiteLog => Level >= ProjectAccessLevel.Read
                                 && (Side == ProjectSide.Contractor || IsMediator);

    /// <summary>
    /// True when this user may move material across the channel boundary.
    /// Mediators do this by role; project owners and managers by authority.
    /// </summary>
    public bool CanExposeToClient => IsMediator || Level >= ProjectAccessLevel.Manage;
}
