namespace assetlen.Shared.Models.Scheduling;

/// <summary>
/// Which days count on this site. Work runs on working days; drying and curing
/// run on every day (works-report.md §4.4, §4.6).
/// </summary>
public sealed class SiteCalendar
{
    private readonly HashSet<DateOnly> _holidays;

    public SiteCalendar(DayOfWeek restDay = DayOfWeek.Saturday, IEnumerable<DateOnly>? holidays = null)
    {
        RestDay = restDay;
        _holidays = new HashSet<DateOnly>(holidays ?? Array.Empty<DateOnly>());
    }

    public DayOfWeek RestDay { get; }

    public IReadOnlyCollection<DateOnly> Holidays => _holidays;

    public bool IsWorkDay(DateOnly d) => d.DayOfWeek != RestDay && !_holidays.Contains(d);

    /// <summary>The day itself if it is a working day, else the next one.</summary>
    public DateOnly NextWork(DateOnly d)
    {
        while (!IsWorkDay(d)) d = d.AddDays(1);
        return d;
    }

    /// <summary>The last day of <paramref name="n"/> working days that begin on or after <paramref name="start"/>.</summary>
    public DateOnly AddWork(DateOnly start, int n)
    {
        var d = NextWork(start);
        for (var c = 1; c < n;)
        {
            d = d.AddDays(1);
            if (IsWorkDay(d)) c++;
        }
        return d;
    }

    /// <summary>Working days strictly between two days.</summary>
    public int WorkDaysBetween(DateOnly after, DateOnly before)
    {
        var n = 0;
        for (var d = after.AddDays(1); d < before; d = d.AddDays(1))
            if (IsWorkDay(d)) n++;
        return n;
    }

    /// <summary>Working days from one day to another, both counted.</summary>
    public int WorkDaysInclusive(DateOnly from, DateOnly to)
    {
        var n = 0;
        for (var d = from; d <= to; d = d.AddDays(1))
            if (IsWorkDay(d)) n++;
        return n;
    }
}
