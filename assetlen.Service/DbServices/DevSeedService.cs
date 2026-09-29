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
    private static string DeliverableId(int n) => $"de300000-0000-4000-8000-0000000008{n:D2}";
    private static string CommitmentId(int n) => $"de300000-0000-4000-8000-0000000009{n:D2}";
    private static string VariationId(int n) => $"de300000-0000-4000-8000-0000000010{n:D2}";
    private static string ClaimId(int n) => $"de300000-0000-4000-8000-0000000011{n:D2}";
    private static string LinkId(int n) => $"de300000-0000-4000-8000-0000000012{n:D2}";

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

        // The narrowest seat on the project, and the reason the seat model
        // exists: Grace was brought on to photograph the work and report on it.
        // She is not a decision-maker on either side, so the money, the register
        // and the drawing set are not part of her job — and her project opens on
        // Capture rather than on a dashboard she has no use for.
        var grace = await EnsureUserAsync("grace@assetlen.dev", "grace", "Grace", "Nabirye", UserRoles.Crew, result);

        // One human, one login, many accounts (§10.2). All of them hold standing
        // in Peter's account because that is where the project lives — the
        // delivery side are guests in it, exactly as the ownership model requires.
        await EnsureMembershipAsync(peter, UserRoles.Contractor, ct);
        await EnsureMembershipAsync(dinah, UserRoles.Client, ct);
        await EnsureMembershipAsync(nalan, UserRoles.Manager, ct);
        await EnsureMembershipAsync(musa, UserRoles.Crew, ct);
        await EnsureMembershipAsync(grace, UserRoles.Crew, ct);

        await EnsureProjectsAsync(peter, nalan, ct);
        result.StageCount = await EnsureStagesAsync(ct);
        result.MemberCount = await EnsureProjectMembersAsync(peter, dinah, nalan, musa, grace, ct);
        await EnsureFundingAsync(peter, nalan, ct);
        await EnsureBudgetAsync(nalan, ct);
        await EnsureOpenQuestionsAsync(peter, dinah, nalan, ct);
        await EnsureSiteLogAsync(nalan, musa, ct);
        await EnsureRegisterAsync(peter, nalan, ct);
        await EnsureLedgerAsync(peter, nalan, ct);

        await _context.SaveChangesAsync(ct);

        result.ProjectName = "Riverstone Residence";
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
        var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower());

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
                .AnyAsync(u => u.UserName != null && u.UserName.ToLower() == candidate.ToLower());

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
                ProjectName = "Riverstone Residence",
                Description = "Four-bedroom residence on a sloping plot, with a guest wing and external works. "
                            + "Designed and built by the same practice; funded stage by stage.",
                Location = "Riverstone Heights",
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
                Location = "Riverstone Heights",
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

        // The phase each stage belongs to, which is what gives it an accent.
        // Kept beside the list rather than inside the tuple, which is already
        // ten fields wide. Two stages sharing a phase — plastering and finishes,
        // the retaining wall and external works — share an accent, and that is
        // the correct answer rather than a collision.
        var phases = new Dictionary<string, StageGroup>
        {
            [StageId(1)] = StageGroup.Preliminaries,
            [StageId(2)] = StageGroup.Substructure,
            [StageId(3)] = StageGroup.Superstructure,
            [StageId(4)] = StageGroup.ExternalWorks,
            [StageId(5)] = StageGroup.Roofing,
            [StageId(6)] = StageGroup.Envelope,
            [StageId(7)] = StageGroup.Finishes,
            [StageId(8)] = StageGroup.Finishes,
            [StageId(9)] = StageGroup.ExternalWorks,
            [WingStageId(1)] = StageGroup.Substructure,
            [WingStageId(2)] = StageGroup.Superstructure,
            [WingStageId(3)] = StageGroup.Finishes
        };

        var existing = await _context.tbl_Stages.IgnoreQueryFilters()
            .Where(s => s.ProjectId == ProjectId || s.ProjectId == SubProjectId)
            .ToListAsync(ct);

        var existingIds = existing.Select(s => s.Id).ToList();

        // A demo database seeded before phases existed would render every stage
        // in the same neutral grey, which reads as the feature being broken.
        foreach (var stage in existing.Where(s => s.Phase == StageGroup.Custom))
        {
            if (phases.TryGetValue(stage.Id, out var known)) stage.Phase = known;
        }

        foreach (var s in stages.Where(s => !existingIds.Contains(s.Id)))
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
                BaselineStartDate = DateTime.SpecifyKind(s.Start, DateTimeKind.Utc),
                BaselineEndDate = DateTime.SpecifyKind(s.End, DateTimeKind.Utc),
                ActualEndDate = s.Status == StageStatus.Completed
                    ? DateTime.SpecifyKind(s.End, DateTimeKind.Utc)
                    : null,
                CompletionPercentage = s.Complete,
                DisplayOrder = s.Order,
                Status = s.Status,
                Phase = phases.TryGetValue(s.Id, out var phase) ? phase : StageGroup.Custom
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
        AppUser peter, AppUser dinah, AppUser nalan, AppUser musa, AppUser grace, CancellationToken ct)
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
                ProjectMemberSpecialization.Foreman, "Site foreman"),

            (MemberId(9), ProjectId, grace.Id, ProjectSide.Contractor, false,
                ProjectMemberSpecialization.Photographer, "Progress photography"),

            (MemberId(10), SubProjectId, grace.Id, ProjectSide.Contractor, false,
                ProjectMemberSpecialization.Photographer, "Progress photography")
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
            var when = DateTime.SpecifyKind(e.When, DateTimeKind.Utc);
            await _context.tbl_ProgressUpdates.IgnoreQueryFilters().Where(u => u.Id == e.Id)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.DateTimeCreated, when), ct);
        }
    }

    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The register for assetlen.md §11 test 1: the sixteen retaining-wall
    /// commitments hand-extracted in whatsapp-evidence.md §7, as Peter would see
    /// them. Pseudonymised like the rest of this seed — the brands and figures
    /// the extraction has to work on are kept; people, suppliers' locations and
    /// accounts are not.
    /// <para>
    /// Filed by Peter from his own import (Law 0): the contractor never logged in
    /// to make this page. Nalan is the accountable face on every item because
    /// he is the mediator, not because he typed it.
    /// </para>
    /// </summary>
    private async Task EnsureRegisterAsync(AppUser peter, AppUser nalan, CancellationToken ct)
    {
        var wall = StageId(4);

        // ── The checklist of the retaining wall ─────────────────────────────
        var deliverables = new (int N, string Title, DeliverableStatus Status, DateTime? Due)[]
        {
            (1, "Design and specification agreed", DeliverableStatus.Done, null),
            (2, "Excavation, first half", DeliverableStatus.Done, Utc(2026, 6, 12)),
            (3, "Materials specified and on site", DeliverableStatus.InProgress, null),
            (4, "Drainage pipes placed", DeliverableStatus.Done, null),
            (5, "First half cast", DeliverableStatus.Done, null),
            (6, "Second half cast", DeliverableStatus.InProgress, null),
            (7, "Backfill and close the excavation", DeliverableStatus.NotStarted, null)
        };

        var haveDeliverables = await _context.tbl_Deliverables.IgnoreQueryFilters()
            .Where(d => d.ProjectId == ProjectId).Select(d => d.Id).ToListAsync(ct);
        foreach (var d in deliverables.Where(d => !haveDeliverables.Contains(DeliverableId(d.N))))
        {
            _context.tbl_Deliverables.Add(new tbl_Deliverable
            {
                Id = DeliverableId(d.N),
                TenantId = TenantId,
                ProjectId = ProjectId,
                StageId = wall,
                Title = d.Title,
                DisplayOrder = d.N,
                Status = d.Status,
                DueDate = d.Due,
                CompletedAt = d.Status == DeliverableStatus.Done ? Utc(2026, 6, 29) : null,
                CompletedById = d.Status == DeliverableStatus.Done ? peter.Id : null
            });
        }

        // ── The sixteen (evidence §7), fifteen commitments and one blocker ──
        const string mediator = "de300000-0000-4000-8000-000000000303"; // MemberId(3), Nalan
        const string developer = "de300000-0000-4000-8000-000000000301"; // MemberId(1), Peter

        var items = new (int N, int? Deliv, CommitmentKind Kind, string Title, string? Body,
                         CommitmentMaturity Maturity, CommitmentSource Source, DateTime Agreed,
                         decimal? Amount, DateTime? Due, string? Supersedes)[]
        {
            // #1 — the spec change the wall exists because of, re-confirmed in June.
            (1, 1, CommitmentKind.Spec, "Reinforced concrete retaining wall, not stone pitching",
                "Conserves compound space. Agreed 3 Dec; re-confirmed 11 Jun before excavation. "
                + "\"Except for the change in retaining wall… nothing else is affected.\"",
                CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2025, 12, 3), null, null, CommitmentId(17)),
            (2, 3, CommitmentKind.Material, "Do not procure YOGI; minimum Plumol",
                null, CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 6, 11), null, null, null),
            (3, 3, CommitmentKind.Material, "Cement: CEM II, the named works brand",
                null, CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 6, 11), null, null, null),
            (4, 4, CommitmentKind.Material, "6-inch drainage pipe: heavy-duty Gentex",
                null, CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2026, 6, 11), null, null, null),
            (5, 3, CommitmentKind.Material, "Aggregate machine-crushed, not hand-crushed",
                null, CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 6, 11), null, null, null),
            (6, 3, CommitmentKind.Material, "Sand: lake sand from the named pit",
                null, CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 6, 11), null, null, null),
            (7, null, CommitmentKind.Price, "Labour: formwork, earthworks and concrete",
                "Quoted as one figure for the three trades.",
                CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 6, 11), 12_000_000m, null, null),
            (8, 2, CommitmentKind.Date, "Excavation phased; first half ready by 12 Jun",
                null, CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2026, 6, 11), null, Utc(2026, 6, 12), null),
            (9, 3, CommitmentKind.Material, "150 bags cement for the excavated section",
                null, CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2026, 6, 13), null, null, null),
            (10, 3, CommitmentKind.Material, "5 trucks hardcore, first",
                "Outstanding when the other materials were confirmed on site (17 Jun).",
                CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 6, 13), null, null, null),
            (11, 3, CommitmentKind.Material, "1 sinotruck sand, 1 sinotruck aggregate",
                null, CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2026, 6, 13), null, null, null),
            (12, 3, CommitmentKind.Material, "Steel: 20 × T16, 160 × T12, 30 × R8",
                null, CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2026, 6, 13), null, null, null),
            // #13 is the blocker — seeded as a flag below, because that is what a blocker is.
            (14, 1, CommitmentKind.Choice, "Electrical points on the interior face of the wall",
                "Raised 25 Jun. The conduits go in before the second half is cast, so the answer is owed before then.",
                CommitmentMaturity.InDiscussion, CommitmentSource.Ingested, Utc(2026, 6, 25), null, null, null),
            (15, 3, CommitmentKind.Material, "Materials confirmed on site; hardcore outstanding",
                "Cement, sand, aggregate and steel delivered against the 13 Jun order.",
                CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2026, 6, 17), null, null, null),
            (16, 5, CommitmentKind.Spec, "First half cast; formwork struck to place the drainage pipes",
                null, CommitmentMaturity.Delivered, CommitmentSource.Ingested, Utc(2026, 6, 29), null, null, null),

            // The statement #1 replaced — kept, because the old spec is the reason for the variation.
            (17, 1, CommitmentKind.Spec, "Stone-pitched retaining structure (original bill)",
                null, CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2025, 10, 2), null, null, null),

            // Outside the sixteen: one spoken agreement awaiting Peter, and a date
            // restated once — the two shapes the register exists to hold.
            (18, null, CommitmentKind.Spec, "Terrace units follow in the second shipment",
                "Agreed on site with the windows contractor. Two terrace units were not in the first container.",
                CommitmentMaturity.Agreed, CommitmentSource.Verbal, Utc(2026, 9, 23), null, null, null),
            (19, null, CommitmentKind.Date, "Main house complete by 30 Sep",
                "Set by the developer.",
                CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 8, 13), null, Utc(2026, 9, 30), null),
            (20, null, CommitmentKind.Date, "Main house complete by 30 Sep — on track",
                "Restated by the contractor.",
                CommitmentMaturity.Agreed, CommitmentSource.Ingested, Utc(2026, 8, 22), null, Utc(2026, 9, 30), CommitmentId(19))
        };

        var haveCommitments = await _context.tbl_Commitments.IgnoreQueryFilters()
            .Where(c => c.ProjectId == ProjectId).Select(c => c.Id).ToListAsync(ct);

        foreach (var i in items.Where(i => !haveCommitments.Contains(CommitmentId(i.N))))
        {
            // #18 and #20 were said by the contractor; the rest Peter filed from
            // the thread he forwarded himself.
            var byContractor = i.N is 18 or 20;
            var stage = i.N switch { 18 => StageId(6), 19 or 20 => StageId(7), _ => wall };

            _context.tbl_Commitments.Add(new tbl_Commitment
            {
                Id = CommitmentId(i.N),
                TenantId = TenantId,
                ProjectId = ProjectId,
                StageId = stage,
                DeliverableId = i.Deliv is { } dn ? DeliverableId(dn) : null,
                Kind = i.Kind,
                Title = i.Title,
                Body = i.Body,
                Maturity = i.Maturity,
                QueryState = CommitmentQueryState.None,
                SourceChannel = i.Source,
                AccountableMemberId = mediator,
                AgreedById = i.N == 19 ? peter.Id : nalan.Id,
                AgreedWithMemberId = i.N == 18 ? MemberId(20) : i.N == 19 ? mediator : developer,
                AgreedAt = i.Agreed,
                RecordedById = byContractor ? nalan.Id : peter.Id,
                RecordedBySide = byContractor ? ProjectSide.Contractor : ProjectSide.Client,
                Amount = i.Amount,
                Currency = i.Amount is null ? null : "UGX",
                DueDate = i.Due,
                OwedBySide = i.Kind == CommitmentKind.Choice ? ProjectSide.Client : null,
                SupersedesId = i.Supersedes,
                DeliveredAt = i.Maturity >= CommitmentMaturity.Delivered ? i.Agreed.AddDays(i.N == 15 ? 0 : 4) : null
            });
        }

        await _context.SaveChangesAsync(ct);

        // The superseded statements, now that their successors exist.
        foreach (var (oldN, newN, when) in new[] { (17, 1, Utc(2025, 12, 3)), (19, 20, Utc(2026, 8, 22)) })
        {
            var old = await _context.tbl_Commitments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == CommitmentId(oldN), ct);
            if (old is not null && old.SupersededAt is null)
            {
                old.SupersededAt = when;
                old.SupersededById = CommitmentId(newN);
            }
        }

        // ── #13 the blocker, #7's dispute, and a third-party blocker ────────
        var flags = new (int N, string Title, string Desc, string? Commitment, string? OwnerMember, string? OwnerParty,
                         string CreatedBy, string? AssignedTo, DateTime Raised, FlagSeverity Sev)[]
        {
            (7, "Open excavation is a danger to neighbours and children",
                "\"I need this closed.\" Raised again on 1 Jul.", null, mediator, null,
                peter.Id, nalan.Id, Utc(2026, 7, 1), FlagSeverity.High),
            (8, "UGX 12M is too high for labour",
                "Queried the same day it was quoted; renegotiated on a call. No figure on record since.",
                CommitmentId(7), null, null, peter.Id, nalan.Id, Utc(2026, 6, 11), FlagSeverity.High),
            (9, "Guest-wing aluminium not fabricated; four openings without windows",
                "Window team slow to respond.", null, MemberId(20), null,
                nalan.Id, nalan.Id, Utc(2026, 9, 23), FlagSeverity.Medium)
        };

        var haveFlags = await _context.tbl_Flags.IgnoreQueryFilters()
            .Where(f => f.ProjectId == ProjectId).Select(f => f.Id).ToListAsync(ct);
        foreach (var f in flags.Where(f => !haveFlags.Contains(FlagId(f.N))))
        {
            _context.tbl_Flags.Add(new tbl_Flag
            {
                Id = FlagId(f.N),
                TenantId = TenantId,
                ProjectId = ProjectId,
                StageId = f.N == 9 ? StageId(6) : wall,
                Title = f.Title,
                Description = f.Desc,
                Severity = f.Sev,
                Channel = Channel.Client,
                Status = FlagStatus.Open,
                CreatedById = f.CreatedBy,
                AssignedToId = f.AssignedTo,
                CommitmentId = f.Commitment,
                OwnerMemberId = f.OwnerMember,
                OwnerPartyName = f.OwnerParty
            });
        }

        // A query on #7 is #7 in QueryRaised — the two must never disagree.
        var labour = await _context.tbl_Commitments.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == CommitmentId(7), ct);
        if (labour is not null && labour.QueryState == CommitmentQueryState.None)
        {
            labour.QueryState = CommitmentQueryState.QueryRaised;
            labour.DisputeNote = "UGX 12M is too high for labour.";
        }

        // ── Backlinks: the deliveries point at what they deliver ────────────
        var links = new (int N, int From, CommitmentLinkTarget Type, string Target, CommitmentLinkRelation Rel)[]
        {
            (1, 15, CommitmentLinkTarget.Commitment, CommitmentId(9), CommitmentLinkRelation.Evidence),
            (2, 15, CommitmentLinkTarget.Commitment, CommitmentId(11), CommitmentLinkRelation.Evidence),
            (3, 15, CommitmentLinkTarget.Commitment, CommitmentId(12), CommitmentLinkRelation.Evidence),
            (4, 16, CommitmentLinkTarget.Commitment, CommitmentId(8), CommitmentLinkRelation.Evidence),
            (5, 16, CommitmentLinkTarget.Commitment, CommitmentId(4), CommitmentLinkRelation.Evidence),
            (6, 1, CommitmentLinkTarget.Variation, VariationId(1), CommitmentLinkRelation.Relates)
        };

        var haveLinks = await _context.tbl_CommitmentLinks.IgnoreQueryFilters()
            .Where(l => l.ProjectId == ProjectId).Select(l => l.Id).ToListAsync(ct);
        foreach (var l in links.Where(l => !haveLinks.Contains(LinkId(l.N))))
        {
            _context.tbl_CommitmentLinks.Add(new tbl_CommitmentLink
            {
                Id = LinkId(l.N),
                TenantId = TenantId,
                ProjectId = ProjectId,
                CommitmentId = CommitmentId(l.From),
                TargetType = l.Type,
                TargetId = l.Target,
                Relation = l.Rel,
                CreatedById = peter.Id
            });
        }

        await _context.SaveChangesAsync(ct);

        // Blockers have an age, and the age is the finding. Backdated round EF
        // for the same reason as the site diary above.
        foreach (var f in flags)
        {
            var id = FlagId(f.N);
            DateTime? raised = f.Raised;
            await _context.tbl_Flags.IgnoreQueryFilters().Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.DateTimeCreated, raised), ct);
        }
    }

    /// <summary>
    /// Claims and variations, so the stage ledger reads funded → claimed →
    /// cleared → carried forward with real movement in it — including the one
    /// sentence the evidence thread turns on: <i>"issue a receipt and carry the
    /// balance towards the next stage."</i>
    /// </summary>
    private async Task EnsureLedgerAsync(AppUser peter, AppUser nalan, CancellationToken ct)
    {
        // The money suite leaves a release on the first stage every run. On a
        // developer database that reads as the drawings stage being funded five
        // times over, and the carried-forward column turns into fiction. Only
        // the suite's own rows are swept, and only here.
        var debris = await _context.tbl_FundingEntries.IgnoreQueryFilters()
            .Where(f => f.ProjectId == ProjectId && (f.Notes == "Chain test" || f.Notes == "Conversion test") && f.IsDeleted != true)
            .ToListAsync(ct);
        foreach (var d in debris) d.IsDeleted = true;

        // The same suite's capture and question, filed to prove nothing floats.
        // Left in place they piled up as open blockers on the demo's report.
        var loose = await _context.tbl_Flags.IgnoreQueryFilters()
            .Where(f => f.ProjectId == ProjectId && f.IsDeleted != true
                        && (f.Title == "Chain auto-link question" || f.Title == "Auto-link question"))
            .ToListAsync(ct);
        foreach (var f in loose) f.IsDeleted = true;
        var captures = await _context.tbl_ProgressUpdates.IgnoreQueryFilters()
            .Where(u => u.ProjectId == ProjectId && u.IsDeleted != true
                        && (u.Description == "Chain auto-link" || u.Description == "Chain deliberate"
                            || u.Description == "Auto-link test" || u.Description == "Deliberate stage"))
            .ToListAsync(ct);
        foreach (var u in captures) u.IsDeleted = true;

        var claims = new (int N, string Stage, decimal Amount, DateTime At, ClaimStatus Status, string Note)[]
        {
            (1, StageId(1), 42_000_000m, Utc(2025, 11, 28), ClaimStatus.Cleared, "Drawing set and approvals, final account."),
            (2, StageId(2), 60_000_000m, Utc(2026, 1, 10), ClaimStatus.Cleared, "Substructure to slab level."),
            (3, StageId(2), 56_500_000m, Utc(2026, 2, 14), ClaimStatus.Cleared, "Substructure, final account."),
            (4, StageId(3), 180_000_000m, Utc(2026, 4, 1), ClaimStatus.Cleared, "Frame and first-floor slab."),
            (5, StageId(3), 85_000_000m, Utc(2026, 5, 30), ClaimStatus.Cleared, "Added floor, as varied."),
            (6, StageId(4), 38_000_000m, Utc(2026, 6, 30), ClaimStatus.Cleared, "First half cast."),
            (7, StageId(4), 20_000_000m, Utc(2026, 8, 16), ClaimStatus.Claimed, "Second lift."),
            (8, StageId(5), 94_000_000m, Utc(2026, 7, 10), ClaimStatus.Cleared, "Roofing, final account.")
        };

        // A claim filed or cleared by hand while walking the demo would otherwise
        // stay on the seeded stage ledger forever, and the carried-forward figures
        // the suites read would drift with every walkthrough. The demo project is
        // restored; every other project is left alone.
        var seededIds = claims.Select(c => ClaimId(c.N)).ToHashSet();
        var existing = await _context.tbl_StageClaims.IgnoreQueryFilters()
            .Where(c => c.ProjectId == ProjectId).ToListAsync(ct);
        foreach (var e in existing.Where(e => !seededIds.Contains(e.Id) && e.IsDeleted != true))
            e.IsDeleted = true;
        foreach (var c in claims)
        {
            var e = existing.FirstOrDefault(x => x.Id == ClaimId(c.N));
            if (e is null) continue;
            e.IsDeleted = false;
            e.Amount = c.Amount;
            e.Status = c.Status;
            e.ClearedAmount = null;
            e.QueryNote = null;
            e.ClearedAt = c.Status == ClaimStatus.Cleared ? c.At.AddDays(3) : null;
            e.ClearedById = c.Status == ClaimStatus.Cleared ? peter.Id : null;
        }

        var haveClaims = existing.Select(c => c.Id).ToList();
        foreach (var c in claims.Where(c => !haveClaims.Contains(ClaimId(c.N))))
        {
            _context.tbl_StageClaims.Add(new tbl_StageClaim
            {
                Id = ClaimId(c.N),
                TenantId = TenantId,
                ProjectId = ProjectId,
                StageId = c.Stage,
                Amount = c.Amount,
                ClaimedAt = c.At,
                ClaimedById = nalan.Id,
                Note = c.Note,
                Status = c.Status,
                ClearedAt = c.Status == ClaimStatus.Cleared ? c.At.AddDays(3) : null,
                ClearedById = c.Status == ClaimStatus.Cleared ? peter.Id : null
            });
        }

        // The variations the thread agreed and never recorded (evidence F4).
        var variations = new (int N, string Stage, string Title, string Reason, decimal? Cost, DateTime Raised,
                              VariationStatus Status, DateTime? Decided, string? Commitment)[]
        {
            (1, StageId(4), "Stone pitching → reinforced concrete retaining wall",
                "Conserves compound space. Impact assessed in a chat message; never priced.",
                null, Utc(2025, 12, 3), VariationStatus.Approved, Utc(2025, 12, 3), CommitmentId(1)),
            (2, StageId(1), "Washroom removed from the first-floor common room",
                "Landing claimed as room; new window W05 added to the schedule.",
                null, Utc(2025, 11, 21), VariationStatus.Approved, Utc(2025, 11, 21), null),
            (3, StageId(3), "Additional floor",
                "\"Let me know the additional cost.\"",
                85_000_000m, Utc(2026, 1, 30), VariationStatus.Approved, Utc(2026, 4, 9), null),
            (4, StageId(3), "Balconies deleted",
                "\"We have saved approximately 15M.\"",
                -15_000_000m, Utc(2026, 5, 25), VariationStatus.Approved, Utc(2026, 5, 25), null),
            (5, StageId(8), "Floor tiles pulled from the quote pending the epoxy decision",
                "Deferred to phase 3 of the finishes.",
                null, Utc(2026, 7, 29), VariationStatus.Proposed, null, null)
        };

        var haveVariations = await _context.tbl_Variations.IgnoreQueryFilters()
            .Where(v => v.ProjectId == ProjectId).Select(v => v.Id).ToListAsync(ct);
        foreach (var v in variations.Where(v => !haveVariations.Contains(VariationId(v.N))))
        {
            _context.tbl_Variations.Add(new tbl_Variation
            {
                Id = VariationId(v.N),
                TenantId = TenantId,
                ProjectId = ProjectId,
                StageId = v.Stage,
                CommitmentId = v.Commitment,
                Title = v.Title,
                Reason = v.Reason,
                CostDelta = v.Cost,
                Currency = "UGX",
                Status = v.Status,
                RaisedById = nalan.Id,
                RaisedAt = v.Raised,
                ApprovedById = v.Decided is null ? null : peter.Id,
                ApprovedAt = v.Decided
            });
        }

        await _context.SaveChangesAsync(ct);
    }
}
