namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>When an undecided item has to be decided, and whether it is time to say so.</summary>
public sealed record DecideBy(DateTime Date, string Reason, bool Surfaced, bool IsKickoff);

/// <summary>
/// Law 4 — speak only when waiting costs something. A decide-by date is computed
/// backwards from the start of the work it gates; with no start date there is
/// no cost to waiting yet, so there is nothing to say.
/// </summary>
public static class DecideByRule
{
    /// <summary>How far ahead of the decide-by date the item starts to speak.</summary>
    public const int WarnDays = 14;

    /// <summary>A parked idea is handed back when its stage is this close to starting.</summary>
    public const int KickoffDays = 7;

    public static DecideBy? Compute(
        CommitmentMaturity maturity,
        string? parkedStageName, DateTime? parkedStageStart,
        string? dependsOnStageName, DateTime? dependsOnStageStart,
        int? leadTimeDays, DateTime today)
    {
        if (maturity >= CommitmentMaturity.Agreed) return null;

        var lead = Math.Max(0, leadTimeDays ?? 0);
        var leadWords = lead > 0 ? $", with {lead} days' lead time" : "";

        (DateTime Date, string Reason)? best = null;
        void Offer(DateTime date, string reason)
        {
            if (best is null || date < best.Value.Date) best = (date, reason);
        }

        if (parkedStageStart is { } ps)
            Offer(ps.Date.AddDays(-lead), $"{parkedStageName ?? "Its stage"} starts {ps:d MMM}{leadWords}");

        // A physical dependency — the duct that must be in before the driveway
        // is poured — binds earlier than the stage the idea is parked for.
        if (dependsOnStageStart is { } ds)
            Offer(ds.Date.AddDays(-lead),
                $"Has to be settled before {dependsOnStageName ?? "the work it depends on"} starts {ds:d MMM}{leadWords}");

        if (best is null) return null;

        var kickoff = maturity == CommitmentMaturity.Idea
                      && parkedStageStart is { } start && start.Date <= today.AddDays(KickoffDays);
        var surfaced = today >= best.Value.Date.AddDays(-WarnDays) || kickoff;

        var reason = best.Value.Reason;
        if (kickoff && best.Value.Date >= parkedStageStart!.Value.Date.AddDays(-lead))
        {
            reason = parkedStageStart.Value.Date <= today
                ? $"{parkedStageName ?? "Its stage"} has started — this was parked for it"
                : $"{parkedStageName ?? "Its stage"} starts {parkedStageStart:d MMM} — this was parked for it";
        }

        return new DecideBy(best.Value.Date, reason, surfaced, kickoff);
    }
}
