using assetlen.Service.DataAccess;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace assetlen.Service.DbServices;

/// <summary>
/// The canonical development world, in one place.
///
/// <para><b>Why this exists.</b> The end-to-end scripts create a fresh
/// timestamped project on every run, so a developer database accumulated a
/// dozen "Peter House 143207" rows. That made the portfolio screen look like
/// Peter runs a dozen engagements, and the UI was designed around that fiction.
/// He does not. The evidence thread is <b>one</b> residence with <b>one</b>
/// guest wing beneath it. The retaining wall, the external works, the doors and
/// windows and the finishes are <b>stages of that residence</b>, not projects —
/// and treating them as projects is what breaks billing (§10.3 rule 1: the
/// billable unit is the top-level project) as well as the reader's mental model.</para>
///
/// <para>Every id below is fixed, which is what makes the seed idempotent: it
/// can be called before every persona sign-in without ever producing a second
/// copy of anything.</para>
/// </summary>
public interface IDevSeedService
{
    Task<DevSeedResult> SeedAsync(CancellationToken ct = default);
}

public sealed class DevSeedResult
{
    public bool Created { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? SubProjectId { get; set; }
    public string? TenantId { get; set; }
    public int StageCount { get; set; }
    public int MemberCount { get; set; }
    public List<string> Notes { get; set; } = new();
}

public sealed class DevSeedService : IDevSeedService
{
    private readonly AssetlenDbContext _context;
    private readonly UserManager<AppUser> _users;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly ILogger<DevSeedService> _logger;

    public DevSeedService(
        AssetlenDbContext context,
        UserManager<AppUser> users,
        RoleManager<IdentityRole> roles,
        ILogger<DevSeedService> logger)
    {
        _context = context;
        _users = users;
        _roles = roles;
        _logger = logger;
    }

    // ── Fixed identity of the demo world ────────────────────────────────────
    private const string TenantId = "de300000-0000-4000-8000-000000000001";
    private const string ProjectId = "de300000-0000-4000-8000-000000000010";
    private const string SubProjectId = "de300000-0000-4000-8000-000000000011";
    private const string Password = "Assetlen#2026";

    private static string StageId(int n) => $"de300000-0000-4000-8000-0000000001{n:D2}";
    private static string WingStageId(int n) => $"de300000-0000-4000-8000-0000000002{n:D2}";
    private static string MemberId(int n) => $"de300000-0000-4000-8000-0000000003{n:D2}";
    private static string FundingId(int n) => $"de300000-0000-4000-8000-0000000004{n:D2}";
    private static string FlagId(int n) => $"de300000-0000-4000-8000-0000000005{n:D2}";
    private static string EntryId(int n) => $"de300000-0000-4000-8000-0000000006{n:D2}";
    private static string BudgetId(int n) => $"de300000-0000-4000-8000-0000000007{n:D2}";

    public async Task<DevSeedResult> SeedAsync(CancellationToken ct = default)
    {
        var result = new DevSeedResult { TenantId = TenantId, ProjectId = ProjectId, SubProjectId = SubProjectId };

        await EnsureRolesAsync();

        var tenant = await EnsureTenantAsync(ct);
        result.Created |= tenant;

        var peter = await EnsureUserAsync("peter@assetlen.dev", "peter", "Peter", "Ssembatya", UserRoles.Contractor, result);
        var dinah = await EnsureUserAsync("dinah@assetlen.dev", "dinah", "Dinah", "Ssembatya", UserRoles.Client, result);
        var nalan = await EnsureUserAsync("nalan@assetlen.dev", "nalan", "Nalan", "Kaggwa", UserRoles.Manager, result);
        var musa = await EnsureUserAsync("musa@assetlen.dev", "musa", "Musa", "Opio", UserRoles.Crew, result);

        // One human, one login, many accounts (§10.2). All four hold standing in
        // Peter's account because that is where the project lives — the delivery
        // side are guests in it, exactly as the ownership model requires.
        await EnsureMembershipAsync(peter, UserRoles.Contractor, ct);
        await EnsureMembershipAsync(dinah, UserRoles.Client, ct);
        await EnsureMembershipAsync(nalan, UserRoles.Manager, ct);
        await EnsureMembershipAsync(musa, UserRoles.Crew, ct);

        await EnsureProjectsAsync(peter, nalan, ct);
        result.StageCount = await EnsureStagesAsync(ct);
        result.MemberCount = await EnsureProjectMembersAsync(peter, dinah, nalan, musa, ct);
        await EnsureFundingAsync(peter, nalan, ct);
        await EnsureBudgetAsync(nalan, ct);
        await EnsureOpenQuestionsAsync(peter, dinah, nalan, ct);
        await EnsureSiteLogAsync(nalan, musa, ct);

        await _context.SaveChangesAsync(ct);

        result.ProjectName = "Kira Residence";
        result.Notes.Add("One top-level project. The guest wing is a sub-project; everything else is a stage.");
        result.Notes.Add($"All personas sign in with the password {Password}.");

        _logger.LogInformation("Dev demo seed complete for tenant {TenantId}", TenantId);
        return result;
    }

    // ── Platform ────────────────────────────────────────────────────────────

    private async Task EnsureRolesAsync()
    {
        foreach (var role in UserRoles.All)
        {
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task<bool> EnsureTenantAsync(CancellationToken ct)
    {
        if (await _context.tbl_Tenants.IgnoreQueryFilters().AnyAsync(t => t.TenantId == TenantId, ct))
            return false;

        _context.tbl_Tenants.Add(new tbl_Tenant
        {
            TenantId = TenantId,
            Name = "Ssembatya Developments",
            Country = "Uganda",
            City = "Kampala",
            Industry = "Property development",
            IsActive = true,
            LastRenewal = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task<AppUser> EnsureUserAsync(
        string email, string userName, string first, string last, string role, DevSeedResult result)
    {
        var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);

        if (user is not null)
        {
            // A persona whose account drifted — a password reset, an unconfirmed
            // email — would fail to sign in with a message about credentials,
            // which sends the reader hunting in the wrong place.
            var changed = false;
            if (!user.EmailConfirmed) { user.EmailConfirmed = true; changed = true; }
            if (user.TenantId != TenantId) { user.TenantId = TenantId; changed = true; }
            if (user.IsDeleted == true) { user.IsDeleted = false; changed = true; }
            if (changed) await _users.UpdateAsync(user);

            if (!await _users.IsInRoleAsync(user, role)) await _users.AddToRoleAsync(user, role);
            return user;
        }

        // A development database usually has history. The end-to-end scripts and
        // earlier hand testing leave accounts behind, and AspNetUsers has a
        // unique index on the normalised user name — so a stale "peter" from a
        // previous run makes the whole seed fail on its first insert with an
        // error that says nothing about why.
        userName = await FreeUserNameAsync(userName);

        user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            FirstName = first,
            LastName = last,
            TenantId = TenantId,

            // Login rejects an unconfirmed phone number outright, so the demo
            // personas deliberately have none.
            PhoneNumber = null,
            PhoneNumberConfirmed = false
        };

        var created = await _users.CreateAsync(user, Password);
        if (!created.Succeeded)
        {
            var reason = string.Join("; ", created.Errors.Select(e => e.Description));
            result.Notes.Add($"Could not create {email}: {reason}");
            throw new InvalidOperationException($"Dev seed could not create {email}: {reason}");
        }

        await _users.AddToRoleAsync(user, role);
        result.Created = true;
        return user;
    }

    /// <summary>
    /// The first free variant of a user name. Sign-in resolves the personas by
    /// <em>email</em>, which is fixed and unique, so a suffixed user name costs
    /// nothing and keeps the seed idempotent on a database with history.
    /// </summary>
    private async Task<string> FreeUserNameAsync(string preferred)
    {
        var candidate = preferred;

        for (var attempt = 2; attempt < 100; attempt++)
        {
            var taken = await _context.Users.IgnoreQueryFilters()
                .AnyAsync(u => u.UserName == candidate);

            if (!taken) return candidate;

            candidate = $"{preferred}{attempt}";
        }

        return $"{preferred}{Guid.NewGuid().ToString("N")[..6]}";
    }

    private async Task EnsureMembershipAsync(AppUser user, string role, CancellationToken ct)
    {
        var exists = await _context.tbl_TenantMemberships
            .IgnoreQueryFilters()
            .AnyAsync(m => m.UserId == user.Id && m.TenantId == TenantId, ct);

        if (exists) return;

        _context.tbl_TenantMemberships.Add(new tbl_TenantMembership
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            TenantId = TenantId,
            Roles = role,
            IsDefault = true,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });
    }

    // ── The project ─────────────────────────────────────────────────────────

    /// <summary>
    /// A cover for each demo project, as an inline SVG.
    /// <para>
    /// The seeded world has no photographs in it, which meant everything that
    /// renders a project's cover — the dashboard card carousel, and the nav
    /// rail's thumbnail — fell through to its placeholder and could not be seen
    /// working at all. A flat drawing-sheet elevation is enough to prove the
    /// path and is honest about being demo material, which a stock photograph
    /// would not be.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Kept small on purpose: <c>tbl_Project_RS.CoverImageUrl</c> is
    /// <c>MaxLength(500)</c> — the column was sized for a URL, not for inline
    /// bytes — and an over-long value fails the insert outright rather than
    /// degrading. Three paths on a 32×20 grid is the whole drawing.
    /// </remarks>
    private static string Cover(string sky, string ground, string mass) =>
        "data:image/svg+xml;utf8," + Uri.EscapeDataString(
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 32 20\">"
          + $"<path fill=\"{sky}\" d=\"M0 0h32v20H0z\"/>"
          + $"<path fill=\"{ground}\" d=\"M0 15h32v5H0z\"/>"
          + $"<path fill=\"{mass}\" d=\"M6 15V8l5-3 5 3v7zm11 0V9l5-2v8z\"/>"
          + "</svg>");

    private async Task EnsureProjectsAsync(AppUser peter, AppUser nalan, CancellationToken ct)
    {
        var start = new DateTime(2025, 9, 18, 0, 0, 0, DateTimeKind.Utc);

        if (!await _context.tbl_Projects_RS.IgnoreQueryFilters().AnyAsync(p => p.Id == ProjectId, ct))
        {
            _context.tbl_Projects_RS.Add(new tbl_Project
            {
                Id = ProjectId,
                TenantId = TenantId,
                OwnerTenantId = TenantId,
                ProjectName = "Kira Residence",
                Description = "Four-bedroom residence on a sloping plot, with a guest wing and external works. "
                            + "Designed and built by the same practice; funded stage by stage.",
                Location = "Kira, Wakiso",
                TotalBudget = 1_170_000_000m,
                Currency = "UGX",
                ExpectedStartDate = start,
                ExpectedCompletionDate = new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc),
                RevisedCompletionDate = new DateTime(2027, 2, 28, 0, 0, 0, DateTimeKind.Utc),
                InvestorId = peter.Id,
                ProjectManagerId = nalan.Id,
                Status = ProjectStatus.Active,
                CoverImageUrl = Cover("#b9cbd8", "#8a8477", "#6f6a61"),

                // 470 own + 150 in the wing = 620 m² → Medium (§10.3). The tier
                // is stored, not recomputed on read, so retuning a threshold
                // never silently re-bills a live project.
                FloorAreaSqm = 470m,
                SizeTier = ProjectSizeTier.Medium,
                SizeSource = ProjectSizeSource.Declared,
                SizeTierConfirmedById = peter.Id,
                SizeTierConfirmedAt = start,

                IsSubscriptionActive = true
            });
        }

        if (!await _context.tbl_Projects_RS.IgnoreQueryFilters().AnyAsync(p => p.Id == SubProjectId, ct))
        {
            _context.tbl_Projects_RS.Add(new tbl_Project
            {
                Id = SubProjectId,
                TenantId = TenantId,
                OwnerTenantId = TenantId,
                ParentProjectId = ProjectId,
                ProjectName = "Guest Wing",
                Description = "Two-bedroom guest wing at the rear of the plot. A sub-project: it enlarges the "
                            + "residence's billable area rather than becoming a second invoice.",
                Location = "Kira, Wakiso",
                TotalBudget = 206_000_000m,
                Currency = "UGX",
                ExpectedStartDate = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                ExpectedCompletionDate = new DateTime(2026, 12, 20, 0, 0, 0, DateTimeKind.Utc),
                InvestorId = peter.Id,
                ProjectManagerId = nalan.Id,
                Status = ProjectStatus.Active,
                CoverImageUrl = Cover("#d8cdb9", "#7d7566", "#8d6a4f"),
                FloorAreaSqm = 150m,
                SizeSource = ProjectSizeSource.Declared,
                IsSubscriptionActive = true
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// The nine stages of the residence and the three of the wing.
    /// <para>
    /// These are the things the evidence thread calls "the retaining wall", "the
    /// external works", "doors and windows" — real funded courses of work inside
    /// one engagement. Each is a stage, and a stage is what Peter funds up front.
    /// </para>
    /// </summary>
    private async Task<int> EnsureStagesAsync(CancellationToken ct)
    {
        var stages = new (string Id, string Project, int Order, string Name, string Desc, decimal Budget,
                          DateTime Start, DateTime End, StageStatus Status, decimal Complete)[]
        {
            (StageId(1), ProjectId, 1, "Approvals & drawings",
             "Architectural set, structural design, physical planner and building-control approval.",
             42_000_000m, new(2025, 9, 18), new(2025, 11, 30), StageStatus.Completed, 100m),

            (StageId(2), ProjectId, 2, "Substructure",
             "Excavation, footings, column bases and ground-floor slab.",
             118_000_000m, new(2025, 12, 1), new(2026, 2, 14), StageStatus.Completed, 100m),

            (StageId(3), ProjectId, 3, "Superstructure",
             "Frame, block work and suspended slabs. Extra floor added by variation on 30 Jan.",
             265_000_000m, new(2026, 2, 15), new(2026, 5, 30), StageStatus.Completed, 100m),

            (StageId(4), ProjectId, 4, "Retaining wall",
             "Retaining structure to the north boundary where the plot falls away.",
             74_000_000m, new(2026, 6, 4), new(2026, 8, 29), StageStatus.InProgress, 62m),

            (StageId(5), ProjectId, 5, "Roofing",
             "Trusses, cover and rainwater goods.",
             96_000_000m, new(2026, 5, 20), new(2026, 7, 10), StageStatus.Completed, 100m),

            (StageId(6), ProjectId, 6, "Doors & windows",
             "Imported aluminium set plus burglar bars. Shipping drove the finishing programme.",
             132_000_000m, new(2026, 2, 20), new(2026, 9, 15), StageStatus.InProgress, 71m),

            (StageId(7), ProjectId, 7, "Plastering & screed",
             "Internal and external plaster, floor screeds. Waited on the door and window set.",
             88_000_000m, new(2026, 7, 1), new(2026, 9, 30), StageStatus.InProgress, 38m),

            (StageId(8), ProjectId, 8, "Finishes",
             "Tiling, gypsum ceilings, wardrobes, paint and sanitary ware.",
             210_000_000m, new(2026, 9, 15), new(2026, 12, 20), StageStatus.NotStarted, 0m),

            (StageId(9), ProjectId, 9, "External works",
             "Driveway, boundary wall, gate and landscaping. Boundary line parked pending the neighbour.",
             145_000_000m, new(2026, 10, 1), new(2027, 2, 20), StageStatus.NotStarted, 0m),

            (WingStageId(1), SubProjectId, 1, "Wing substructure",
             "Footings and slab to the guest wing.",
             46_000_000m, new(2026, 1, 12), new(2026, 3, 8), StageStatus.Completed, 100m),

            (WingStageId(2), SubProjectId, 2, "Wing superstructure",
             "Walls, ring beam and roof to the guest wing.",
             92_000_000m, new(2026, 3, 10), new(2026, 8, 1), StageStatus.InProgress, 55m),

            (WingStageId(3), SubProjectId, 3, "Wing finishes",
             "Plaster, floors and fittings.",
             68_000_000m, new(2026, 9, 1), new(2026, 12, 20), StageStatus.NotStarted, 0m)
        };

        var existing = await _context.tbl_Stages.IgnoreQueryFilters()
            .Where(s => s.ProjectId == ProjectId || s.ProjectId == SubProjectId)
            .Select(s => s.Id).ToListAsync(ct);

        foreach (var s in stages.Where(s => !existing.Contains(s.Id)))
        {
            _context.tbl_Stages.Add(new tbl_Stage
            {
                Id = s.Id,
                TenantId = TenantId,
                ProjectId = s.Project,
                StageName = s.Name,
                Description = s.Desc,
                BudgetAmount = s.Budget,
                StartDate = DateTime.SpecifyKind(s.Start, DateTimeKind.Utc),
                ExpectedEndDate = DateTime.SpecifyKind(s.End, DateTimeKind.Utc),
                ActualEndDate = s.Status == StageStatus.Completed
                    ? DateTime.SpecifyKind(s.End, DateTimeKind.Utc)
                    : null,
                CompletionPercentage = s.Complete,
                DisplayOrder = s.Order,
                Status = s.Status
            });
        }

        await _context.SaveChangesAsync(ct);
        return stages.Length;
    }

    /// <summary>
    /// Two sides and one mediator (§10.1). Peter and Dinah are the client side;
    /// Nalan mediates and Musa is delivery. Nalan is the single accountable name
    /// on everything that crosses, whoever actually produced it.
    /// </summary>
    private async Task<int> EnsureProjectMembersAsync(
        AppUser peter, AppUser dinah, AppUser nalan, AppUser musa, CancellationToken ct)
    {
        var members = new (string Id, string Project, string UserId, ProjectSide Side, bool Mediator,
                           ProjectMemberSpecialization Spec, string Title)[]
        {
            (MemberId(1), ProjectId, peter.Id, ProjectSide.Client, false,
                ProjectMemberSpecialization.ClientOwner, "Developer"),

            (MemberId(2), ProjectId, dinah.Id, ProjectSide.Client, false,
                ProjectMemberSpecialization.ClientRepresentative, "Representative — finishes and layouts"),

            (MemberId(3), ProjectId, nalan.Id, ProjectSide.Contractor, true,
                ProjectMemberSpecialization.Architect, "Architect-contractor — accountable face"),

            (MemberId(4), ProjectId, musa.Id, ProjectSide.Contractor, false,
                ProjectMemberSpecialization.Foreman, "Site foreman"),

            (MemberId(5), SubProjectId, peter.Id, ProjectSide.Client, false,
                ProjectMemberSpecialization.ClientOwner, "Developer"),

            (MemberId(6), SubProjectId, dinah.Id, ProjectSide.Client, false,
                ProjectMemberSpecialization.ClientRepresentative, "Representative"),

            (MemberId(7), SubProjectId, nalan.Id, ProjectSide.Contractor, true,
                ProjectMemberSpecialization.Architect, "Architect-contractor"),

            (MemberId(8), SubProjectId, musa.Id, ProjectSide.Contractor, false,
                ProjectMemberSpecialization.Foreman, "Site foreman")
        };

        var existing = await _context.tbl_ProjectMembers.IgnoreQueryFilters()
            .Where(m => m.ProjectId == ProjectId || m.ProjectId == SubProjectId)
            .Select(m => m.Id).ToListAsync(ct);

        foreach (var m in members.Where(m => !existing.Contains(m.Id)))
        {
            _context.tbl_ProjectMembers.Add(new tbl_ProjectMember
            {
                Id = m.Id,
                TenantId = TenantId,
                ProjectId = m.Project,
                UserId = m.UserId,
                Side = m.Side,
                IsMediator = m.Mediator,
                Specialization = m.Spec,
                Title = m.Title,
                IsActive = true,
                JoinedAt = new DateTime(2025, 9, 18, 0, 0, 0, DateTimeKind.Utc),
                AssignedById = peter.Id
            });
        }

        // Off-platform parties: real counterparties on the thread who will never
        // hold a login, but who must be nameable on a commitment.
        var offPlatform = new (string Id, string Name, ProjectSide Side, ProjectMemberSpecialization Spec, string Title)[]
        {
            (MemberId(20), "Sunrise Aluminium Ltd", ProjectSide.Contractor,
                ProjectMemberSpecialization.Subcontractor, "Doors and windows fabricator"),
            (MemberId(21), "Eng. Barbra Nakato", ProjectSide.Contractor,
                ProjectMemberSpecialization.Engineer, "Consulting structural engineer")
        };

        foreach (var p in offPlatform.Where(p => !existing.Contains(p.Id)))
        {
            _context.tbl_ProjectMembers.Add(new tbl_ProjectMember
            {
                Id = p.Id,
                TenantId = TenantId,
                ProjectId = ProjectId,
                UserId = null,
                PartyName = p.Name,
                Side = p.Side,
                IsMediator = false,
                Specialization = p.Spec,
                Title = p.Title,
                IsActive = true,
                JoinedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                AssignedById = nalan.Id
            });
        }

        await _context.SaveChangesAsync(ct);
        return members.Length + offPlatform.Length;
    }

    /// <summary>
    /// Funding, so the money screen has a real position to report. Amounts are
    /// deliberately below stage budgets on live stages — funded-versus-claimed
    /// only means something when the two differ.
    /// </summary>
    private async Task EnsureFundingAsync(AppUser peter, AppUser nalan, CancellationToken ct)
    {
        var entries = new (string Id, string Project, string Stage, decimal Amount, DateTime Paid,
                           FundingStatus Status, string Note)[]
        {
            (FundingId(1), ProjectId, StageId(1), 42_000_000m, new(2025, 9, 26), FundingStatus.Confirmed,
                "Approvals and drawing set, paid in full."),
            (FundingId(2), ProjectId, StageId(2), 118_000_000m, new(2025, 12, 3), FundingStatus.Confirmed,
                "Substructure, funded up front."),
            (FundingId(3), ProjectId, StageId(3), 180_000_000m, new(2026, 2, 18), FundingStatus.Confirmed,
                "Superstructure, first tranche."),
            (FundingId(4), ProjectId, StageId(3), 85_000_000m, new(2026, 4, 9), FundingStatus.Confirmed,
                "Superstructure, balance after the added floor."),
            (FundingId(5), ProjectId, StageId(5), 96_000_000m, new(2026, 5, 22), FundingStatus.Confirmed,
                "Roofing."),
            (FundingId(6), ProjectId, StageId(6), 96_000_000m, new(2026, 2, 24), FundingStatus.Confirmed,
                "Doors and windows deposit to the fabricator."),
            (FundingId(7), ProjectId, StageId(4), 40_000_000m, new(2026, 6, 11), FundingStatus.Confirmed,
                "Retaining wall, first release."),
            (FundingId(8), ProjectId, StageId(7), 30_000_000m, new(2026, 7, 8), FundingStatus.Confirmed,
                "Plastering, materials."),

            // Deliberately pending: this is what "needs you" is for.
            (FundingId(9), ProjectId, StageId(4), 22_000_000m, new(2026, 8, 12), FundingStatus.Pending,
                "Retaining wall, second release — awaiting confirmation."),
            (FundingId(10), SubProjectId, WingStageId(1), 46_000_000m, new(2026, 1, 15), FundingStatus.Confirmed,
                "Guest wing substructure."),
            (FundingId(11), SubProjectId, WingStageId(2), 55_000_000m, new(2026, 3, 14), FundingStatus.Confirmed,
                "Guest wing superstructure, first release.")
        };

        var existing = await _context.tbl_FundingEntries.IgnoreQueryFilters()
            .Where(f => f.ProjectId == ProjectId || f.ProjectId == SubProjectId)
            .Select(f => f.Id).ToListAsync(ct);

        foreach (var e in entries.Where(e => !existing.Contains(e.Id)))
        {
            _context.tbl_FundingEntries.Add(new tbl_FundingEntry
            {
                Id = e.Id,
                TenantId = TenantId,
                ProjectId = e.Project,
                StageId = e.Stage,
                Amount = e.Amount,
                PaymentDate = DateTime.SpecifyKind(e.Paid, DateTimeKind.Utc),
                PaidById = peter.Id,
                ConfirmedById = e.Status == FundingStatus.Confirmed ? nalan.Id : null,
                ConfirmationDate = e.Status == FundingStatus.Confirmed
                    ? DateTime.SpecifyKind(e.Paid.AddDays(1), DateTimeKind.Utc)
                    : null,
                Status = e.Status,
                Notes = e.Note
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task EnsureBudgetAsync(AppUser nalan, CancellationToken ct)
    {
        var items = new (string Id, string Stage, string Title, BudgetCategory Cat, decimal Planned, int Order)[]
        {
            (BudgetId(1), StageId(4), "Machine-crushed aggregate and lake sand", BudgetCategory.Materials, 21_000_000m, 1),
            (BudgetId(2), StageId(4), "CEM II cement — 150 bags", BudgetCategory.Materials, 18_500_000m, 2),
            (BudgetId(3), StageId(4), "Retaining-wall labour", BudgetCategory.Labor, 26_000_000m, 3),
            (BudgetId(4), StageId(6), "Imported aluminium set", BudgetCategory.Materials, 96_000_000m, 4),
            (BudgetId(5), StageId(6), "Burglar bars — fabrication and fitting", BudgetCategory.Labor, 21_000_000m, 5),
            (BudgetId(6), StageId(7), "Plaster sand and cement", BudgetCategory.Materials, 34_000_000m, 6),
            (BudgetId(7), StageId(7), "Plastering labour", BudgetCategory.Labor, 41_000_000m, 7)
        };

        var existing = await _context.tbl_BudgetLineItems.IgnoreQueryFilters()
            .Where(b => b.ProjectId == ProjectId)
            .Select(b => b.Id).ToListAsync(ct);

        foreach (var i in items.Where(i => !existing.Contains(i.Id)))
        {
            _context.tbl_BudgetLineItems.Add(new tbl_BudgetLineItem
            {
                Id = i.Id,
                TenantId = TenantId,
                ProjectId = ProjectId,
                StageId = i.Stage,
                Title = i.Title,
                Category = i.Cat,
                PlannedAmount = i.Planned,
                DisplayOrder = i.Order,
                CreatedById = nalan.Id
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// The open questions. Until the commitment model lands (plan.md P4) a Flag
    /// is the closest thing the schema has to "a decision Peter owes", and it is
    /// what the Needs-you surface reads. Client-channel ones reach the client
    /// side; Crew-channel ones are the delivery side's own.
    /// </summary>
    private async Task EnsureOpenQuestionsAsync(AppUser peter, AppUser dinah, AppUser nalan, CancellationToken ct)
    {
        var flags = new (string Id, string Stage, string Title, string Desc, FlagSeverity Sev,
                         Channel Ch, string CreatedBy, string AssignedTo, DateTime? Due, FlagStatus Status)[]
        {
            (FlagId(1), StageId(6), "Burglar-bar design needs a decision",
             "The fabricator has stopped pending a pattern. The delivered sample has a cross in the middle, "
             + "which was rejected on sight. A choice by the date below keeps the finishing programme intact.",
             FlagSeverity.High, Channel.Client, nalan.Id, dinah.Id, new DateTime(2026, 8, 24), FlagStatus.Open),

            (FlagId(2), StageId(9), "Gate position blocks the driveway pour",
             "The boundary wall is parked pending the neighbour, but the gate power duct must be laid before "
             + "the driveway is poured. Decide the gate position or the duct gets buried.",
             FlagSeverity.High, Channel.Client, nalan.Id, peter.Id, new DateTime(2026, 9, 20), FlagStatus.Open),

            (FlagId(3), StageId(8), "Epoxy or tile to the ground floor",
             "Deferred to the finishes phase. A price sits behind each; the answer changes the screed spec, "
             + "so it is cheaper to settle before plastering closes.",
             FlagSeverity.Medium, Channel.Client, nalan.Id, dinah.Id, new DateTime(2026, 9, 30), FlagStatus.Open),

            (FlagId(4), StageId(4), "Second release on the retaining wall",
             "Funds recorded on 12 Aug are unconfirmed. Nothing is blocked yet; the wall's second lift is.",
             FlagSeverity.Medium, Channel.Client, nalan.Id, peter.Id, new DateTime(2026, 8, 20), FlagStatus.Open),

            (FlagId(5), StageId(7), "Plaster mix inconsistent on the east elevation",
             "Two batches came up different. Re-doing the affected bays before the screed goes down.",
             FlagSeverity.Medium, Channel.Crew, nalan.Id, nalan.Id, null, FlagStatus.InProgress),

            (FlagId(6), StageId(3), "Extra floor — cost delta agreed",
             "Added 30 Jan. Cost delta agreed and funded on 9 Apr.",
             FlagSeverity.Low, Channel.Client, peter.Id, nalan.Id, null, FlagStatus.Resolved)
        };

        var existing = await _context.tbl_Flags.IgnoreQueryFilters()
            .Where(f => f.ProjectId == ProjectId)
            .Select(f => f.Id).ToListAsync(ct);

        foreach (var f in flags.Where(f => !existing.Contains(f.Id)))
        {
            _context.tbl_Flags.Add(new tbl_Flag
            {
                Id = f.Id,
                TenantId = TenantId,
                ProjectId = ProjectId,
                StageId = f.Stage,
                Title = f.Title,
                Description = f.Desc,
                Severity = f.Sev,
                Channel = f.Ch,
                CreatedById = f.CreatedBy,
                AssignedToId = f.AssignedTo,
                Status = f.Status,
                DueDate = f.Due is null ? null : DateTime.SpecifyKind(f.Due.Value, DateTimeKind.Utc),
                ResolvedById = f.Status == FlagStatus.Resolved ? nalan.Id : null,
                ResolvedDate = f.Status == FlagStatus.Resolved
                    ? new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc) : null
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// A handful of Site Diary entries so "what moved" is not empty on first open.
    /// Most are Crew — fail-closed is the rule, and a client-side reader seeing
    /// all of these would prove nothing about the channel boundary.
    /// </summary>
    private async Task EnsureSiteLogAsync(AppUser nalan, AppUser musa, CancellationToken ct)
    {
        var entries = new (string Id, string Project, string Stage, string Desc, decimal Pct,
                           Channel Ch, string By, DateTime When, bool Issues)[]
        {
            (EntryId(1), ProjectId, StageId(4), "Second lift formwork struck. Face is clean; no honeycombing on the north run.",
                62m, Channel.Client, musa.Id, new(2026, 8, 15, 19, 40, 0), false),

            (EntryId(2), ProjectId, StageId(7), "Plastering started on the east elevation. Two bays flagged for a re-do.",
                38m, Channel.Crew, musa.Id, new(2026, 8, 14, 21, 10, 0), true),

            (EntryId(3), ProjectId, StageId(6), "Aluminium set cleared and delivered to site. Burglar bars still held.",
                71m, Channel.Client, nalan.Id, new(2026, 8, 11, 23, 15, 0), false),

            (EntryId(4), SubProjectId, WingStageId(2), "Ring beam poured on the guest wing. Curing through the weekend.",
                55m, Channel.Client, musa.Id, new(2026, 8, 9, 20, 5, 0), false),

            (EntryId(5), ProjectId, StageId(4), "Aggregate delivery short by two trips. Supplier notified.",
                58m, Channel.Crew, musa.Id, new(2026, 8, 7, 18, 30, 0), true)
        };

        var existing = await _context.tbl_ProgressUpdates.IgnoreQueryFilters()
            .Where(p => p.ProjectId == ProjectId || p.ProjectId == SubProjectId)
            .Select(p => p.Id).ToListAsync(ct);

        foreach (var e in entries.Where(e => !existing.Contains(e.Id)))
        {
            _context.tbl_ProgressUpdates.Add(new tbl_ProgressUpdate
            {
                Id = e.Id,
                TenantId = TenantId,
                ProjectId = e.Project,
                StageId = e.Stage,
                Description = e.Desc,
                CompletionPercentage = e.Pct,
                HasIssues = e.Issues,
                Channel = e.Ch,
                CreatedById = e.By,
                ApprovalStatus = ApprovalStatus.Pending,
                DateTimeCreated = DateTime.SpecifyKind(e.When, DateTimeKind.Utc)
            });
        }

        await _context.SaveChangesAsync(ct);

        // Backdating has to go round EF.
        //
        // UpdateTimestamps stamps DateTimeCreated = UtcNow on every insert and
        // then, on update, explicitly clears IsModified on that column — which
        // is correct behaviour for the application and fatal for a seed. Left
        // alone, all five entries land on today, "what moved yesterday" shows
        // the whole log, and the one screen Peter opens every morning is a lie
        // the first time anyone looks at it.
        foreach (var e in entries)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE tbl_ProgressUpdates SET DateTimeCreated = {0} WHERE Id = {1}",
                new object[] { DateTime.SpecifyKind(e.When, DateTimeKind.Utc), e.Id },
                ct);
        }
    }
}
