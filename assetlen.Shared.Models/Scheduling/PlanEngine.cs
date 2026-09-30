namespace assetlen.Shared.Models.Scheduling;

/// <summary>
/// The scheduler (works-report.md §4.6): every date follows from durations,
/// waits and order on the site calendar, so a change shows its effect on the
/// handover before anyone saves it. Pure and deterministic — the same inputs
/// always give the same plan — and it knows nothing about storage.
/// <para>
/// A faithful port of the hand-built scheduler that produced the 30 Sep plan:
/// an activity starts after what it waits on, is made off site, waits for
/// anything that holds only its site work, runs its working days, then cures.
/// The critical path is found by walking back from the last activity through
/// the latest-finishing predecessor, for as long as that predecessor is what held it.
/// </para>
/// </summary>
public sealed class PlanEngine
{
    private const int SlackCap = 250;

    public PlanOutcome Compute(PlanInput input)
    {
        var pass = Pass.Run(input, input.Activities);
        var slack = new Dictionary<string, (int? Float, int? Slip)>(StringComparer.Ordinal);

        if (input.WithSlack && pass.WorksComplete is { } wc0)
        {
            for (var i = 0; i < input.Activities.Count; i++)
            {
                var a = input.Activities[i];
                if (a.DoneOn is not null) continue;

                DateOnly WcAfter(int k)
                {
                    var list = input.Activities.ToArray();
                    list[i] = Delay(a, k, input.Calendar);
                    return Pass.Run(input, list).WorksComplete!.Value;
                }

                var fl = Search(k => WcAfter(k) == wc0);
                int? slip = input.Handover is not { } h ? null
                    : wc0 < h ? Search(k => WcAfter(k) < h)
                    : fl;
                slack[a.Key] = (fl, slip);
            }
        }

        return pass.ToOutcome(slack);
    }

    /// <summary>The largest k in [0, cap] for which a monotone test still holds.</summary>
    private static int Search(Func<int, bool> holds)
    {
        if (holds(SlackCap)) return SlackCap;
        int lo = 0, hi = SlackCap;
        while (hi - lo > 1)
        {
            var mid = (lo + hi) / 2;
            if (holds(mid)) lo = mid; else hi = mid;
        }
        return lo;
    }

    /// <summary>The same activity running k working days over.</summary>
    private static PlanActivity Delay(PlanActivity a, int k, SiteCalendar cal)
    {
        if (k == 0) return a;
        if (a.PinnedFinish is { } pin) return a with { PinnedFinish = cal.AddWork(pin.AddDays(1), k) };
        if (a.WorkDays > 0) return a with { WorkDays = a.WorkDays + k };
        if (a.MakeDays > 0) return a with { MakeDays = a.MakeDays + k };
        return a with { WorkDays = 1 + k };
    }

    private sealed class Computed
    {
        public required PlanActivity Activity { get; init; }
        public int Order { get; init; }
        public DateOnly Start;
        public DateOnly Finish;
        public DateOnly? MakeStart;
        public DateOnly? MakeEnd;
        public DateOnly? CureEnd;
        public DateOnly Fin => CureEnd ?? Finish;
        public List<WaitSpan> Waits = new();
        public bool Late;
        public DateOnly? LateSince;

        /// <summary>The day its predecessors and the team ahead let it begin, before making.</summary>
        public DateOnly Gate;

        /// <summary>A wait on the site work alone pushed it past what came before it.</summary>
        public bool HeldAfterMaking;
    }

    private sealed class Pass
    {
        private readonly PlanInput _input;
        private readonly SiteCalendar _cal;
        private readonly Dictionary<string, PlanActivity> _byKey = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _order = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _teamPred = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Computed> _done = new(StringComparer.Ordinal);
        private readonly HashSet<string> _visiting = new(StringComparer.Ordinal);

        public DateOnly? WorksComplete { get; private set; }
        private List<string> _critical = new();

        private Pass(PlanInput input, IReadOnlyList<PlanActivity> activities)
        {
            _input = input;
            _cal = input.Calendar;
            for (var i = 0; i < activities.Count; i++)
            {
                var a = activities[i];
                if (!_byKey.TryAdd(a.Key, a)) throw new PlanShapeException($"Two activities share the key {a.Key}.");
                _order[a.Key] = i;
            }
            foreach (var a in activities)
                foreach (var w in a.Waits.Where(w => w.Kind == WaitKind.Activity))
                {
                    if (w.ActivityKey is null || !_byKey.ContainsKey(w.ActivityKey))
                        throw new PlanShapeException($"{Name(a)} waits on an activity that is not on the plan.");
                    if (w.ActivityKey == a.Key)
                        throw new PlanShapeException($"{Name(a)} cannot wait on itself.");
                }

            // A team is a queue: the next job starts when the last one finishes (§4.4).
            foreach (var team in activities.Select((a, i) => (a, i))
                         .Where(x => !string.IsNullOrWhiteSpace(x.a.TeamKey))
                         .GroupBy(x => x.a.TeamKey!.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var queue = team.OrderBy(x => x.a.QueueOrder).ThenBy(x => x.i).Select(x => x.a.Key).ToList();
                for (var q = 1; q < queue.Count; q++) _teamPred[queue[q]] = queue[q - 1];
            }
        }

        public static Pass Run(PlanInput input, IReadOnlyList<PlanActivity> activities)
        {
            var pass = new Pass(input, activities);
            foreach (var a in activities) pass.Sched(a.Key);
            pass.Finish(activities);
            return pass;
        }

        private static string Name(PlanActivity a) => string.IsNullOrWhiteSpace(a.Title) ? a.Key : a.Title!;

        private Computed Sched(string key)
        {
            if (_done.TryGetValue(key, out var ready)) return ready;
            var a = _byKey[key];
            if (!_visiting.Add(key)) throw new PlanShapeException($"The order loops back on itself at {Name(a)}.");

            var today = _input.Today;
            var earliest = a.EarliestStart ?? today;
            var c = new Computed { Activity = a, Order = _order[key] };
            var bindIdx = -1;
            var waits = a.Waits.Select((w, i) => (w, i)).ToList();
            var s = a.ActualStart ?? (earliest < today ? today : earliest);
            var started = a.ActualStart is not null;

            void Raise(DateOnly candidate, int idx)
            {
                if (candidate > s) { s = candidate; bindIdx = idx; }
            }

            // What holds the whole activity: predecessors, the team ahead of it, and waits before making.
            foreach (var (w, i) in waits.Where(x => !x.w.AfterMaking))
            {
                if (w.Kind == WaitKind.Activity)
                {
                    var p = Sched(w.ActivityKey!);
                    if (!started) Raise(w.Link == WaitLink.FinishToStart ? p.Fin.AddDays(1) : p.Start, i);
                    c.Waits.Add(Span(w, i, null, null));
                }
                else
                {
                    var hs = w.From ?? earliest;
                    var span = Clear(w, i, hs);
                    c.Waits.Add(span);
                    if (!started) Raise(span.End!.Value.AddDays(1), i);
                }
            }
            if (_teamPred.TryGetValue(key, out var ahead))
            {
                var p = Sched(ahead);
                if (!started) Raise(p.Fin.AddDays(1), -2);
            }
            c.Gate = s;

            if (!started && a.MakeDays > 0)
            {
                var ms = _cal.NextWork(s);
                var me = _cal.AddWork(ms, a.MakeDays);
                c.MakeStart = ms;
                c.MakeEnd = me;
                s = me.AddDays(1);
            }

            // What holds only the site work: the doors must be in before the railing is fixed.
            var siteFrom = s;
            foreach (var (w, i) in waits.Where(x => x.w.AfterMaking))
            {
                if (w.Kind == WaitKind.Activity)
                {
                    var p = Sched(w.ActivityKey!);
                    if (!started) Raise(w.Link == WaitLink.FinishToStart ? p.Finish.AddDays(1) : p.Start, i);
                    c.Waits.Add(Span(w, i, null, null));
                }
                else
                {
                    var span = Clear(w, i, w.From ?? s);
                    c.Waits.Add(span);
                    if (!started) Raise(span.End!.Value.AddDays(1), i);
                }
            }

            c.HeldAfterMaking = s > siteFrom;

            if (a.WorkDays > 0)
            {
                c.Start = a.ActualStart ?? _cal.NextWork(s);
                c.Finish = _cal.AddWork(c.Start, a.WorkDays);
            }
            else if (c.MakeStart is { } ms0)
            {
                c.Start = ms0;
                c.Finish = c.MakeEnd!.Value;
            }
            else
            {
                c.Start = a.ActualStart ?? _cal.NextWork(s);
                c.Finish = c.Start;
            }

            if (a.PinnedFinish is { } pin) c.Finish = pin;
            if (a.DoneOn is { } doneOn) c.Finish = doneOn;
            if (c.Start > c.Finish) c.Start = c.Finish;

            // Not ticked and past its finish: it is late, and what follows it moves
            // with today — never left quietly on time (§4.6). Nobody finishes it on the
            // rest day or a holiday, so it is held at the next working day.
            if (a.DoneOn is null && c.Finish < today)
            {
                c.Late = true;
                c.LateSince = c.Finish;
                c.Finish = _cal.NextWork(today);
            }
            if (a.CureDays > 0) c.CureEnd = c.Finish.AddDays(a.CureDays);

            if (bindIdx >= 0)
                c.Waits = c.Waits.Select(w => w.Index == bindIdx ? w with { Binding = true } : w).ToList();

            _visiting.Remove(key);
            _done[key] = c;
            return c;
        }

        private static WaitSpan Span(PlanWait w, int i, DateOnly? start, DateOnly? end) => new()
        {
            Index = i,
            Kind = w.Kind,
            Arrival = w.Kind == WaitKind.Arrival ? w.Arrival ?? ArrivalKind.Delivery : null,
            Title = w.Title,
            ActivityKey = w.ActivityKey,
            Link = w.Link,
            AfterMaking = w.AfterMaking,
            Start = start,
            End = end,
            Cleared = w.Cleared
        };

        /// <summary>When a wait clears: its fixed day, or its days counted from when it starts.</summary>
        private WaitSpan Clear(PlanWait w, int i, DateOnly hs)
        {
            DateOnly he = w.Until
                ?? (w.Days <= 0 ? hs.AddDays(-1)
                    : w.CalendarDays || w.Kind == WaitKind.Drying ? hs.AddDays(w.Days - 1)
                    : _cal.AddWork(hs, w.Days));

            if (w.Cleared)
                return Span(w, i, hs, w.ClearedOn ?? he);

            var today = _input.Today;
            if (w.Kind == WaitKind.Arrival && he < today)
            {
                // Outlived its day and nobody has said it came. With the calibration on it
                // runs on by its cause's measured lag; either way it is flagged, not hidden.
                var rule = _input.Lateness;
                if (rule.Enabled)
                {
                    var lag = rule.ArrivalLag.TryGetValue(w.Arrival ?? ArrivalKind.Delivery, out var l) ? l : 5;
                    return Span(w, i, hs, _cal.AddWork(today.AddDays(1), Math.Max(1, lag))) with { Overdue = true, DueWas = he };
                }
                return Span(w, i, hs, he) with { Overdue = true };
            }
            return Span(w, i, hs, he);
        }

        private void Finish(IReadOnlyList<PlanActivity> activities)
        {
            if (activities.Count == 0) return;

            // The last to finish sets works complete; on a tie, the later on the list (the close-out).
            var terminal = _done.Values.OrderBy(x => x.Fin).ThenBy(x => x.Order).Last();
            WorksComplete = terminal.Fin;

            // "Sets the date" is said only of what holds it: the walk stops where something
            // else set the date — a wait, a known finish, work already begun, an item held
            // at today — so no line both sets the date and can slip.
            var chain = new List<string>();
            var cur = terminal;
            while (cur is not null && chain.Count <= activities.Count)
            {
                chain.Add(cur.Activity.Key);
                var a = cur.Activity;
                if (a.ActualStart is not null || a.DoneOn is not null || a.PinnedFinish is not null || cur.Late || cur.HeldAfterMaking) break;
                var preds = cur.Activity.Waits
                    .Where(w => w.Kind == WaitKind.Activity && !w.AfterMaking && w.Link == WaitLink.FinishToStart)
                    .Select(w => w.ActivityKey!)
                    .ToList();
                if (_teamPred.TryGetValue(cur.Activity.Key, out var ahead)) preds.Add(ahead);
                if (preds.Count == 0) break;
                var latest = preds.Select(p => _done[p]).Aggregate((x, y) => x.Fin >= y.Fin ? x : y);
                if (_cal.NextWork(latest.Fin.AddDays(1)) != _cal.NextWork(cur.Gate)) break;
                cur = latest;
            }
            chain.Reverse();
            _critical = chain;
        }

        public PlanOutcome ToOutcome(IReadOnlyDictionary<string, (int? Float, int? Slip)> slack)
        {
            var critical = _critical.ToHashSet(StringComparer.Ordinal);
            var handover = _input.Handover;
            int? reserve = null;
            var over = 0;
            if (handover is { } h && WorksComplete is { } wc)
            {
                if (wc < h) reserve = _cal.WorkDaysBetween(wc, h);
                else { reserve = 0; over = _cal.WorkDaysInclusive(h, wc); }
            }

            var plans = _done.Values.OrderBy(x => x.Order).Select(x =>
            {
                var a = x.Activity;
                var (fl, slip) = slack.TryGetValue(a.Key, out var v) ? v : (null, null);
                return new ActivityPlan
                {
                    Key = a.Key,
                    Title = a.Title,
                    Area = a.Area,
                    Trade = a.Trade,
                    Start = x.Start,
                    Finish = x.Finish,
                    MakeStart = x.MakeStart,
                    MakeEnd = x.MakeEnd,
                    CureEnd = x.CureEnd,
                    Waits = x.Waits,
                    Done = a.DoneOn is not null,
                    Started = a.ActualStart is not null || a.DoneOn is not null,
                    Late = x.Late,
                    LateSince = x.LateSince,
                    Critical = critical.Contains(a.Key),
                    FloatDays = fl,
                    SlipDays = slip
                };
            }).ToList();

            var actions = plans
                .Where(p => !p.Done)
                .SelectMany(p => p.Waits
                    .Where(w => w.Kind == WaitKind.Arrival && !w.Cleared && w.End is not null)
                    .Select(w => new PlanAction
                    {
                        By = w.End!.Value,
                        What = w.Title,
                        Kind = w.Arrival ?? ArrivalKind.Delivery,
                        ActivityKey = p.Key,
                        ActivityTitle = p.Title,
                        SetsTheDate = p.Critical,
                        Overdue = w.Overdue
                    }))
                .OrderBy(x => x.By)
                .ToList();

            return new PlanOutcome
            {
                Today = _input.Today,
                WorksComplete = WorksComplete,
                Handover = handover,
                ReserveDays = reserve,
                DaysOver = over,
                CriticalPath = _critical,
                Activities = plans,
                Actions = actions
            };
        }
    }
}
