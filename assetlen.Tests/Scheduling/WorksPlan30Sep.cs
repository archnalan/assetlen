using assetlen.Shared.Models.Scheduling;

namespace assetlen.Tests.Scheduling;

/// <summary>
/// The inputs of the 30 Sep works plan, as the hand-built scheduler held them
/// (plan3.mjs, outside the repo): six-day week with Saturday rest, 9 Oct off,
/// plan as at Wed 30 Sep, committed handover Fri 4 Dec.
/// </summary>
internal static class WorksPlan30Sep
{
    public static DateOnly D(int m, int d) => new(2026, m, d);

    public static readonly DateOnly Today = D(9, 30);
    public static readonly DateOnly Handover = D(12, 4);
    public static SiteCalendar Calendar => new(DayOfWeek.Saturday, new[] { D(10, 9) });

    public const string Aluminium = "Aluminium team";

    private static PlanWait After(string key) => new() { Kind = WaitKind.Activity, ActivityKey = key };
    private static PlanWait SiteAfter(string key) => new() { Kind = WaitKind.Activity, ActivityKey = key, AfterMaking = true };
    private static PlanWait SiteWith(string key) => new() { Kind = WaitKind.Activity, ActivityKey = key, AfterMaking = true, Link = WaitLink.StartToStart };
    private static PlanWait Arrives(int days, string why, ArrivalKind kind, DateOnly? from = null) =>
        new() { Kind = WaitKind.Arrival, Arrival = kind, Days = days, Title = why, From = from };
    private static PlanWait Drying(int days, DateOnly from, string why) =>
        new() { Kind = WaitKind.Drying, Days = days, CalendarDays = true, From = from, Title = why };
    private static PlanWait ArrivesBy(DateOnly until, string why, ArrivalKind kind) =>
        new() { Kind = WaitKind.Arrival, Arrival = kind, Until = until, Title = why, AfterMaking = true };

    private static PlanActivity A(string key, string title, string area, string trade, int wd, params PlanWait[] waits) =>
        new() { Key = key, Title = title, Area = area, Trade = trade, WorkDays = wd, Waits = waits };

    /// <summary>
    /// S2, the issued sequence. With <paramref name="teams"/> the aluminium team's
    /// five jobs are a team queue instead of a chain of predecessors; the dates
    /// must not change. <paramref name="s1"/> is the alternative the plan was
    /// compared with: railing and panel go in with the guest-wing doors, and the
    /// main-house louvres come last.
    /// </summary>
    public static List<PlanActivity> Activities(bool teams = false, bool s1 = false, bool makeOnly = false)
    {
        if (s1 && !teams) throw new ArgumentException("S1 is expressed as a team queue.");
        const string MH = "Main house", GW = "Guest wing", EX = "External works", HO = "Handover";

        PlanActivity Alu(string key, string title, string area, int wd, int order, params PlanWait[] explicitAfter) =>
            teams
                ? A(key, title, area, Aluminium, wd) with { TeamKey = Aluminium, QueueOrder = order }
                : A(key, title, area, Aluminium, wd, explicitAfter);

        var railWait = s1 ? SiteWith("gw-dw") : SiteAfter("mh-dw");

        var list = new List<PlanActivity>
        {
            A("mh-door", "Door opening adjustments", MH, "Own crew", 4) with { ActualStart = D(9, 29) },
            Alu("mh-dw", "Doors & windows installation", MH, 19, 1) with { ActualStart = D(9, 29) },
            A("mh-stone", "Foundation stonework finishing", MH, "Masons", 10),
            A("mh-terr", "Terrazzo grinding & polishing", MH, "Terrazzo crew", 14),
            A("mh-gyp", "Gypsum ceiling", MH, "Ceiling crew", 14, Arrives(3, "Boards and frames on site", ArrivalKind.Delivery)) with { EarliestStart = D(10, 5) },
            A("mh-wire", "Wiring", MH, "Electrician", 3, After("mh-gyp")),
            A("mh-epoxy", "Epoxy floor", MH, "Epoxy team", 6, After("mh-wire"), Drying(20, D(10, 2), "Screed drying")) with { CureDays = 3 },
            A("mh-paint", "Painting: primer, then finish coats", MH, "Painters", 24) with { EarliestStart = D(10, 5) },
            A("mh-rail", "Railing", MH, "Metal fabricator", 3, railWait) with { MakeDays = 10 },
            A("mh-panel", "Perforated wall panel, rear elevation", MH, "Metal fabricator", 4, railWait) with { MakeDays = 12 },
            A("mh-perg", "Pergola", MH, "Metal fabricator", 4, Arrives(5, "Design sign-off, materials", ArrivalKind.SignOff)) with { MakeDays = 10 },
            A("mh-kit", "Kitchen installation", MH, "Kitchen supplier", 5, Arrives(20, "Order and lead time", ArrivalKind.Order), After("mh-epoxy")),
            A("mh-okit", "Outdoor kitchen installations", MH, "Kitchen supplier", 5, After("mh-perg")),
            Alu("mh-louv", "Louvre making & installation on ducts", MH, 3, s1 ? 5 : 2, After("mh-dw")),
            Alu("alu-make", "Louvre, door & window making", GW, 20, s1 ? 2 : 3, After("mh-louv")),
            Alu("gw-dw", "Doors & windows installation", GW, 6, s1 ? 3 : 4, After("alu-make")),
            Alu("gw-louv", "Louvre installation on ducts", GW, 3, s1 ? 4 : 5, After("gw-dw")),
            A("mh-ward", "Wardrobes", MH, "Joinery", 6, Arrives(20, "Wardrobes made and delivered", ArrivalKind.Delivery), After("mh-epoxy")),
            A("mh-plumb", "Plumbing accessories installation", MH, "Plumber", 5, After("mh-epoxy"), After("mh-kit")),
            A("gw-under", "Undercoat", GW, "Painters", 8) with { EarliestStart = D(10, 5) },
            A("gw-stone", "Foundation stonework finishing", GW, "Masons", 12),
            A("gw-tile", "Tiling", GW, "Tilers", 18) with { EarliestStart = D(10, 5) },
            A("gw-tank", "Tank stand installation", GW, "Metal fabricator", 3, ArrivesBy(D(10, 23), "Water pump and tanks acquired", ArrivalKind.Purchase)) with { MakeDays = 6 },
            A("gw-wire", "Wiring", GW, "Electrician", 2, After("gw-tile")),
            A("gw-plumb", "Plumbing accessories installation", GW, "Plumber", 4, After("gw-tile")),
            A("ex-sept", "Closing the septic tank", EX, "Own crew", 6, After("mh-door")),
            A("ex-level", "Site levelling", EX, "Plant hire + crew", 4, After("ex-sept"), Arrives(2, "Plant hire confirmed", ArrivalKind.Booking)),
            A("ex-wall", "Boundary wall", EX, "Masons", 16, After("ex-level"), Arrives(10, "Funding and materials", ArrivalKind.Funding, Today)),
            A("ex-pave", "Paving", EX, "Paving crew", 14, After("ex-wall"), After("mh-okit")),
            A("mh-touch", "Final paint touch-up", HO, "Painters", 3,
                After("mh-paint"), After("mh-plumb"), After("mh-louv"), After("mh-rail"), After("mh-panel"), After("mh-kit"), After("mh-ward"), After("mh-dw")),
            A("ho-snag", "Snagging & cleaning", HO, "All trades", 4,
                After("mh-touch"), After("gw-plumb"), After("gw-wire"), After("gw-louv"), After("gw-dw"), After("ex-pave"), After("gw-tank"),
                After("mh-okit"), After("gw-under"), After("mh-stone"), After("gw-stone"), After("mh-terr"), After("mh-door"))
        };

        if (makeOnly)
        {
            var i = list.FindIndex(a => a.Key == "alu-make");
            list[i] = list[i] with { WorkDays = 0, MakeDays = 20 };
        }
        return list;
    }

    public static PlanInput Input(IReadOnlyList<PlanActivity> activities, DateOnly? today = null, LateWaitRule? lateness = null) => new()
    {
        Today = today ?? Today,
        Calendar = Calendar,
        Activities = activities,
        Handover = Handover,
        Lateness = lateness ?? LateWaitRule.Off
    };

    /// <summary>What plan3.mjs printed for S2: site start and finish, and any hold, making and curing.</summary>
    public static readonly (string Key, DateOnly Start, DateOnly Finish, DateOnly? HoldFrom, DateOnly? HoldTo, DateOnly? MakeFrom, DateOnly? MakeTo, DateOnly? Cured)[] Issued =
    {
        ("mh-door", D(9, 29), D(10, 2), null, null, null, null, null),
        ("mh-dw", D(9, 29), D(10, 21), null, null, null, null, null),
        ("mh-stone", D(9, 30), D(10, 12), null, null, null, null, null),
        ("mh-terr", D(9, 30), D(10, 16), null, null, null, null, null),
        ("mh-gyp", D(10, 8), D(10, 25), D(10, 5), D(10, 7), null, null, null),
        ("mh-wire", D(10, 26), D(10, 28), null, null, null, null, null),
        ("mh-epoxy", D(10, 29), D(11, 4), D(10, 2), D(10, 21), null, null, D(11, 7)),
        ("mh-paint", D(10, 5), D(11, 2), null, null, null, null, null),
        ("mh-rail", D(10, 22), D(10, 25), null, null, D(9, 30), D(10, 12), null),
        ("mh-panel", D(10, 22), D(10, 26), null, null, D(9, 30), D(10, 14), null),
        ("mh-perg", D(10, 19), D(10, 22), D(9, 30), D(10, 5), D(10, 6), D(10, 18), null),
        ("mh-kit", D(11, 8), D(11, 12), D(9, 30), D(10, 23), null, null, null),
        ("mh-okit", D(10, 23), D(10, 28), null, null, null, null, null),
        ("mh-louv", D(10, 22), D(10, 25), null, null, null, null, null),
        ("alu-make", D(10, 26), D(11, 17), null, null, null, null, null),
        ("gw-dw", D(11, 18), D(11, 24), null, null, null, null, null),
        ("gw-louv", D(11, 25), D(11, 27), null, null, null, null, null),
        ("mh-ward", D(11, 8), D(11, 13), D(9, 30), D(10, 23), null, null, null),
        ("mh-plumb", D(11, 13), D(11, 18), null, null, null, null, null),
        ("gw-under", D(10, 5), D(10, 14), null, null, null, null, null),
        ("gw-stone", D(9, 30), D(10, 14), null, null, null, null, null),
        ("gw-tile", D(10, 5), D(10, 26), null, null, null, null, null),
        ("gw-tank", D(10, 25), D(10, 27), D(10, 7), D(10, 23), D(9, 30), D(10, 6), null),
        ("gw-wire", D(10, 27), D(10, 28), null, null, null, null, null),
        ("gw-plumb", D(10, 27), D(10, 30), null, null, null, null, null),
        ("ex-sept", D(10, 4), D(10, 11), null, null, null, null, null),
        ("ex-level", D(10, 12), D(10, 15), D(9, 30), D(10, 1), null, null, null),
        ("ex-wall", D(10, 16), D(11, 3), D(9, 30), D(10, 12), null, null, null),
        ("ex-pave", D(11, 4), D(11, 19), null, null, null, null, null),
        ("mh-touch", D(11, 19), D(11, 22), null, null, null, null, null),
        ("ho-snag", D(11, 29), D(12, 2), null, null, null, null, null)
    };

    public static readonly string[] IssuedCriticalPath = { "mh-dw", "mh-louv", "alu-make", "gw-dw", "gw-louv", "ho-snag" };
}
