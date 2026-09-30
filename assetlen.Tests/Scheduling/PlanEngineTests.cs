using assetlen.Shared.Models.Scheduling;
using static assetlen.Tests.Scheduling.WorksPlan30Sep;

namespace assetlen.Tests.Scheduling;

/// <summary>
/// The acceptance test of works-report.md §4.6: fed the 30 Sep inputs, the
/// engine reproduces the issued plan exactly.
/// </summary>
public class PlanEngineTests
{
    private static readonly PlanEngine Engine = new();

    private static PlanOutcome Issued30Sep(bool teams = false, bool makeOnly = false) =>
        Engine.Compute(Input(Activities(teams: teams, makeOnly: makeOnly)));

    public static IEnumerable<object[]> Variants => new[]
    {
        new object[] { false, false },
        new object[] { true, false },
        new object[] { true, true }
    };

    [Theory]
    [MemberData(nameof(Variants))]
    public void Every_activity_lands_on_the_issued_dates(bool teams, bool makeOnly)
    {
        var plan = Issued30Sep(teams, makeOnly);

        foreach (var row in WorksPlan30Sep.Issued)
        {
            var a = plan[row.Key]!;
            if (makeOnly && row.Key == "alu-make")
            {
                Assert.Equal((row.Key, row.Start, row.Finish), (a.Key, a.MakeStart!.Value, a.MakeEnd!.Value));
                Assert.Equal((row.Start, row.Finish), (a.Start, a.Finish));
                continue;
            }
            Assert.Equal((row.Key, row.Start, row.Finish), (a.Key, a.Start, a.Finish));
            Assert.Equal((row.Key, row.MakeFrom, row.MakeTo), (a.Key, a.MakeStart, a.MakeEnd));
            Assert.Equal((row.Key, row.Cured), (a.Key, a.CureEnd));

            var hold = a.Waits.Where(w => w.Kind != WaitKind.Activity).OrderByDescending(w => w.End).FirstOrDefault();
            Assert.Equal((row.Key, row.HoldFrom, row.HoldTo), (a.Key, hold?.Start, hold?.End));
        }
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Works_complete_Wed_2_Dec_with_one_working_day_of_reserve_to_Fri_4_Dec(bool teams, bool makeOnly)
    {
        var plan = Issued30Sep(teams, makeOnly);

        Assert.Equal(D(12, 2), plan.WorksComplete);
        Assert.Equal(DayOfWeek.Wednesday, plan.WorksComplete!.Value.DayOfWeek);
        Assert.Equal(D(12, 4), plan.Handover);
        Assert.Equal(1, plan.ReserveDays);
        Assert.Equal(0, plan.DaysOver);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void The_aluminium_sequence_sets_the_date(bool teams, bool makeOnly)
    {
        var plan = Issued30Sep(teams, makeOnly);

        Assert.Equal(IssuedCriticalPath, plan.CriticalPath);
        Assert.All(plan.Activities.Where(a => a.Critical), a => Assert.Contains(a.Key, IssuedCriticalPath));
        Assert.Equal(IssuedCriticalPath.Length, plan.Activities.Count(a => a.Critical));
    }

    [Fact]
    public void S1_railing_and_panel_with_the_guest_wing_doors_and_main_house_louvres_last_completes_Sun_6_Dec()
    {
        var plan = Engine.Compute(Input(Activities(teams: true, s1: true)));

        Assert.Equal(D(12, 6), plan.WorksComplete);
        Assert.Equal(DayOfWeek.Sunday, plan.WorksComplete!.Value.DayOfWeek);
        Assert.Equal(new[] { "mh-dw", "alu-make", "gw-dw", "gw-louv", "mh-louv", "mh-touch", "ho-snag" }, plan.CriticalPath);
        Assert.Equal(0, plan.ReserveDays);
        Assert.Equal(2, plan.DaysOver); // Fri 4 and Sun 6 Dec
        Assert.Equal((D(11, 15), D(11, 17)), (plan["mh-rail"]!.Start, plan["mh-rail"]!.Finish));
        Assert.Equal((D(11, 15), D(11, 20)), (plan["gw-dw"]!.Start, plan["gw-dw"]!.Finish));
    }

    [Fact]
    public void Comparing_the_two_sequences_reads_as_the_30_Sep_table()
    {
        var s2 = Issued30Sep(teams: true);
        var s1 = Engine.Compute(Input(Activities(teams: true, s1: true)));

        var c = PlanComparison.Of(s2, s1);

        Assert.Equal("Moves works complete from Wed 2 Dec to Sun 6 Dec.", c.Sentences[0]);
        Assert.Equal("Reserve 1 → 0 days.", c.Sentences[1]);
        Assert.Contains(c.Sentences, s => s.StartsWith("Handover Fri 4 Dec no longer holds"));
        Assert.True(c.CriticalPathChanged);
    }

    [Fact]
    public void What_sets_the_date_cannot_slip_and_everything_else_says_what_it_can_absorb()
    {
        var plan = Issued30Sep(teams: true);

        foreach (var key in IssuedCriticalPath)
        {
            Assert.Equal((key, 0), (key, plan[key]!.FloatDays!.Value));
            Assert.Equal((key, 1), (key, plan[key]!.SlipDays!.Value)); // the one reserve day
        }

        // Touch-up ends Sun 22 Nov and snagging cannot start before Sun 29 Nov:
        // five working days of its own, six before the handover moves.
        Assert.Equal(5, plan["mh-touch"]!.FloatDays);
        Assert.Equal(6, plan["mh-touch"]!.SlipDays);
        Assert.True(plan["mh-stone"]!.FloatDays > 30);
    }

    [Fact]
    public void A_known_finish_re_dates_everything_downstream_and_says_so_before_saving()
    {
        var before = Issued30Sep(teams: true);
        var edited = Activities(teams: true).Select(a => a.Key == "mh-dw" ? a with { PinnedFinish = D(10, 23) } : a).ToList();
        var after = Engine.Compute(Input(edited));

        Assert.Equal(D(10, 23), after["mh-dw"]!.Finish);
        Assert.Equal(D(10, 25), after["mh-louv"]!.Start);
        Assert.Equal(D(12, 4), after.WorksComplete);
        Assert.Equal(0, after.ReserveDays);
        Assert.Equal(1, after.DaysOver);

        var c = PlanComparison.Of(before, after);
        Assert.Equal("Moves works complete from Wed 2 Dec to Fri 4 Dec.", c.Sentences[0]);
        Assert.Equal("Reserve 1 → 0 days.", c.Sentences[1]);
        Assert.False(c.CriticalPathChanged);
    }

    [Fact]
    public void A_tick_pins_the_actual_finish_and_moves_what_follows()
    {
        var ticked = Activities(teams: true).Select(a => a.Key == "mh-door" ? a with { DoneOn = D(10, 1) } : a).ToList();
        var plan = Engine.Compute(Input(ticked));

        Assert.True(plan["mh-door"]!.Done);
        Assert.Equal(D(10, 1), plan["mh-door"]!.Finish);
        Assert.Equal(D(10, 2), plan["ex-sept"]!.Start); // was Sun 4 Oct
        Assert.Null(plan["mh-door"]!.FloatDays);
    }

    [Fact]
    public void An_item_past_its_finish_and_not_ticked_is_late_and_what_follows_moves_with_today()
    {
        var plan = Engine.Compute(Input(Activities(teams: true), today: D(10, 5)));

        var door = plan["mh-door"]!;
        Assert.True(door.Late);
        Assert.Equal(D(10, 2), door.LateSince);
        Assert.Equal(D(10, 5), door.Finish);
        Assert.Equal(D(10, 6), plan["ex-sept"]!.Start);
    }

    [Theory]
    [InlineData(10, 3, 10, 4, 10, 5)]   // Saturday, the rest day: nobody finishes it before Sunday
    [InlineData(10, 9, 10, 11, 10, 12)] // the holiday: Saturday 10 Oct rests too, so Sunday, and septic on Monday
    public void A_late_item_is_held_at_the_next_working_day_never_on_a_rest_day(int tm, int td, int fm, int fd, int sm, int sd)
    {
        var plan = Engine.Compute(Input(Activities(teams: true), today: D(tm, td)));

        var door = plan["mh-door"]!;
        Assert.True(door.Late);
        Assert.Equal(D(fm, fd), door.Finish);
        Assert.True(Calendar.IsWorkDay(door.Finish));
        Assert.Equal(D(sm, sd), plan["ex-sept"]!.Start);
    }

    [Fact]
    public void Only_what_actually_holds_the_date_sets_it_when_a_booking_holds_the_last_activity()
    {
        // Snagging keeps its thirteen predecessors but also waits for the cleaning crew,
        // booked for 1 Dec. The aluminium sequence now has days in hand; the booking sets the date.
        var booked = Activities(teams: true).Select(a => a.Key != "ho-snag" ? a : a with
        {
            Waits = a.Waits.Append(new PlanWait { Kind = WaitKind.Arrival, Arrival = ArrivalKind.Booking, Until = D(12, 1), Title = "Cleaning crew booked" }).ToList()
        }).ToList();
        var plan = Engine.Compute(Input(booked));

        Assert.Equal(new[] { "ho-snag" }, plan.CriticalPath);
        Assert.All(plan.Activities.Where(a => a.Critical), a => Assert.Equal(0, a.FloatDays));
        Assert.True(plan["gw-louv"]!.FloatDays > 0);
        Assert.Contains(plan.Actions, x => x.ActivityKey == "ho-snag" && x.SetsTheDate);
    }

    [Fact]
    public void A_known_finish_sets_the_date_itself_not_what_came_before_it()
    {
        // The main-house louvres are known to run to 5 Nov. The doors before them no
        // longer decide when the aluminium queue moves on, so they can slip.
        var pinned = Activities(teams: true).Select(a => a.Key == "mh-louv" ? a with { PinnedFinish = D(11, 5) } : a).ToList();
        var plan = Engine.Compute(Input(pinned));

        Assert.Equal(new[] { "mh-louv", "alu-make", "gw-dw", "gw-louv", "ho-snag" }, plan.CriticalPath);
        Assert.False(plan["mh-dw"]!.Critical);
        Assert.True(plan["mh-dw"]!.FloatDays > 0);
        Assert.All(plan.Activities.Where(a => a.Critical), a => Assert.Equal(0, a.FloatDays));
    }

    [Fact]
    public void A_wait_that_outlives_its_date_is_flagged_and_with_the_calibration_on_extends_by_its_cause()
    {
        var today = D(10, 12);

        var off = Engine.Compute(Input(Activities(teams: true), today: today));
        var gypOff = off["mh-gyp"]!;
        var holdOff = gypOff.Waits.Single(w => w.Kind == WaitKind.Arrival);
        Assert.True(holdOff.Overdue);
        Assert.Equal(D(10, 7), holdOff.End);
        Assert.Equal(today, gypOff.Start);

        var on = Engine.Compute(Input(Activities(teams: true), today: today, lateness: LateWaitRule.Calibrated));
        var gypOn = on["mh-gyp"]!;
        var holdOn = gypOn.Waits.Single(w => w.Kind == WaitKind.Arrival);
        Assert.True(holdOn.Overdue);
        Assert.Equal(D(10, 7), holdOn.DueWas);
        Assert.Equal(D(10, 18), holdOn.End); // five working days for a delivery, from tomorrow
        Assert.Equal(D(10, 19), gypOn.Start);
    }

    [Fact]
    public void A_cleared_wait_ends_on_the_day_it_cleared()
    {
        var cleared = Activities(teams: true).Select(a => a.Key != "mh-kit" ? a : a with
        {
            Waits = a.Waits.Select(w => w.Kind == WaitKind.Arrival ? w with { Cleared = true, ClearedOn = D(10, 1) } : w).ToList()
        }).ToList();
        var plan = Engine.Compute(Input(cleared));

        Assert.Equal(D(10, 1), plan["mh-kit"]!.Waits.Single(w => w.Kind == WaitKind.Arrival).End);
        Assert.DoesNotContain(plan.Actions, x => x.ActivityKey == "mh-kit");
        Assert.Equal(D(11, 8), plan["mh-kit"]!.Start); // still behind the epoxy's cure
    }

    [Fact]
    public void What_must_happen_and_by_when_lists_every_arrival_by_its_kind_in_date_order()
    {
        var plan = Issued30Sep(teams: true);

        var actions = plan.Actions.Select(x => (x.ActivityKey, x.By, x.Kind)).ToList();
        Assert.Equal(new[]
        {
            ("ex-level", D(10, 1), ArrivalKind.Booking),
            ("mh-perg", D(10, 5), ArrivalKind.SignOff),
            ("mh-gyp", D(10, 7), ArrivalKind.Delivery),
            ("ex-wall", D(10, 12), ArrivalKind.Funding),
            ("mh-kit", D(10, 23), ArrivalKind.Order),
            ("mh-ward", D(10, 23), ArrivalKind.Delivery),
            ("gw-tank", D(10, 23), ArrivalKind.Purchase)
        }, actions);
        Assert.DoesNotContain(plan.Actions, x => x.SetsTheDate);
    }

    [Fact]
    public void The_same_inputs_always_give_the_same_plan()
    {
        var a = Issued30Sep(teams: true);
        var b = Issued30Sep(teams: true);

        Assert.Equal(a.Activities.Select(x => (x.Key, x.Start, x.Finish, x.FloatDays)), b.Activities.Select(x => (x.Key, x.Start, x.Finish, x.FloatDays)));
    }

    [Fact]
    public void An_order_that_loops_back_on_itself_is_refused_by_name()
    {
        var loop = Activities(teams: true).Select(a => a.Key != "mh-gyp" ? a : a with
        {
            Waits = a.Waits.Append(new PlanWait { Kind = WaitKind.Activity, ActivityKey = "mh-epoxy" }).ToList()
        }).ToList();

        var ex = Assert.Throws<PlanShapeException>(() => Engine.Compute(Input(loop)));
        Assert.Contains("loops back", ex.Message);
    }

    [Fact]
    public void A_wait_on_an_activity_not_on_the_plan_is_refused()
    {
        var bad = Activities(teams: true).Append(new PlanActivity
        {
            Key = "x", Title = "Stray", WorkDays = 1,
            Waits = new[] { new PlanWait { Kind = WaitKind.Activity, ActivityKey = "gw-dw-start" } }
        }).ToList();

        Assert.Throws<PlanShapeException>(() => Engine.Compute(Input(bad)));
    }

    [Fact]
    public void The_site_calendar_rests_on_Saturday_and_the_holiday_and_drying_counts_every_day()
    {
        var cal = Calendar;

        Assert.False(cal.IsWorkDay(D(10, 3)));  // Saturday
        Assert.False(cal.IsWorkDay(D(10, 9)));  // the holiday
        Assert.True(cal.IsWorkDay(D(10, 4)));   // Sunday is a working day
        Assert.Equal(D(10, 12), cal.AddWork(D(9, 30), 10));
        Assert.Equal(1, cal.WorkDaysBetween(D(12, 2), D(12, 4)));
    }
}
