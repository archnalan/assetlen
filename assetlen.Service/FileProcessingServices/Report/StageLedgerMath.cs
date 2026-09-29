using assetlen.Service.DataAccess;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.FileProcessingServices.Report;

/// <summary>
/// The stage ledger's arithmetic, on rows already loaded: funded → claimed →
/// cleared → carried forward. One copy, so the Money tab and the works report
/// cannot disagree about the same release.
/// </summary>
public static class StageLedgerMath
{
    public static StageLedgerDto Build(string projectId, string currency, IReadOnlyList<tbl_Stage> stages,
        IReadOnlyList<tbl_FundingEntry> funding, IReadOnlyList<tbl_StageClaim> claims, IReadOnlyList<tbl_Variation> variations)
    {
        // Majors in order, each followed by its own sub-stages: the order a
        // reader walks the build in, and the order a balance is carried.
        var byParent = stages.Where(s => s.ParentStageId != null)
            .GroupBy(s => s.ParentStageId!)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.DisplayOrder).ToList());
        var ordered = new List<tbl_Stage>();
        foreach (var major in stages.Where(s => s.ParentStageId == null || stages.All(p => p.Id != s.ParentStageId))
                                    .OrderBy(s => s.DisplayOrder))
        {
            ordered.Add(major);
            if (byParent.TryGetValue(major.Id, out var subs)) ordered.AddRange(subs);
        }

        var rows = new List<StageLedgerRowDto>();
        decimal carry = 0m;
        foreach (var s in ordered)
        {
            var forStage = funding.Where(f => f.StageId == s.Id).ToList();

            // A settled release counts at what landed, not what was sent.
            var funded = forStage
                .Where(f => f.Status is FundingStatus.Confirmed or FundingStatus.Settled)
                .Sum(f => f.ReceivedAmount ?? f.Amount);
            var pending = forStage
                .Where(f => f.Status is FundingStatus.Pending or FundingStatus.AmountQueried)
                .Sum(f => f.ReceivedAmount ?? f.Amount);

            var stageClaims = claims.Where(c => c.StageId == s.Id && c.Status != ClaimStatus.Withdrawn).ToList();
            var claimed = stageClaims.Sum(c => c.Amount);
            var cleared = stageClaims.Where(c => c.Status == ClaimStatus.Cleared).Sum(c => c.ClearedAmount ?? c.Amount);
            var awaiting = stageClaims.Where(c => c.Status == ClaimStatus.Claimed).Sum(c => c.Amount);

            var stageVariations = variations.Where(v => v.StageId == s.Id).ToList();

            var carriedIn = carry;
            var inHand = carriedIn + funded - cleared;
            var closed = s.Status == StageStatus.Completed;

            // "Issue a receipt and carry the balance towards the next stage"
            // (evidence F3): only a closed stage passes its balance on. An open
            // one keeps it in hand, because the work it pays for is not done.
            carry = closed ? inHand : 0m;

            rows.Add(new StageLedgerRowDto
            {
                StageId = s.Id,
                StageName = s.StageName,
                ParentStageId = s.ParentStageId,
                DisplayOrder = s.DisplayOrder,
                Phase = s.Phase,
                Status = s.Status,
                Budget = s.BudgetAmount ?? 0m,
                Funded = funded,
                PendingFunding = pending,
                CarriedIn = carriedIn,
                Claimed = claimed,
                Cleared = cleared,
                AwaitingClearance = awaiting,
                InHand = inHand,
                CarriedForward = closed ? inHand : null,
                VariationsApproved = stageVariations.Where(v => v.Status == VariationStatus.Approved).Sum(v => v.CostDelta ?? 0m),
                VariationsProposed = stageVariations.Count(v => v.Status == VariationStatus.Proposed),
                VariationsUncosted = stageVariations.Count(v => v.CostDelta is null
                                                                && v.Status is VariationStatus.Proposed or VariationStatus.Approved)
            });
        }

        return new StageLedgerDto
        {
            ProjectId = projectId,
            Currency = currency,
            Rows = rows,
            TotalBudget = rows.Sum(r => r.Budget),
            TotalFunded = rows.Sum(r => r.Funded),
            TotalPending = rows.Sum(r => r.PendingFunding),
            TotalClaimed = rows.Sum(r => r.Claimed),
            TotalCleared = rows.Sum(r => r.Cleared),

            // Conservation: whatever was funded and not cleared is still held
            // somewhere, carried or not. The total never depends on ordering.
            TotalInHand = rows.Sum(r => r.Funded) - rows.Sum(r => r.Cleared),
            TotalVariationsApproved = rows.Sum(r => r.VariationsApproved)
        };
    }
}
