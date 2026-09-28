using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// The register: deliverables, commitments, their links, and accountability.
/// Readable by the principals on either side (<c>ProjectAccess.CanSeeRegister</c>);
/// a support seat is answered as if the register did not exist.
/// </summary>
public interface ICommitmentDAL
{
    // ─── Deliverables ─────────────────────────────────────────
    Task<ServiceResult<List<DeliverableDto>>> GetDeliverables(string projectId, string? stageId, string userId);
    Task<ServiceResult<DeliverableDto>> AddDeliverable(DeliverableCreateDto dto, string userId);
    Task<ServiceResult<DeliverableDto>> UpdateDeliverable(DeliverableUpdateDto dto, string userId);
    Task<ServiceResult<bool>> DeleteDeliverable(string deliverableId, string userId);

    // ─── Commitments ──────────────────────────────────────────

    /// <summary>Current statements only, unless <paramref name="includeSuperseded"/>.</summary>
    Task<ServiceResult<List<CommitmentDto>>> GetCommitments(string projectId, string? stageId, bool includeSuperseded, string userId);
    Task<ServiceResult<CommitmentDto>> GetCommitment(string commitmentId, string userId);

    /// <summary>Every statement of one commitment, oldest first — the restatement history a date strip draws.</summary>
    Task<ServiceResult<List<CommitmentDto>>> GetChain(string commitmentId, string userId);

    Task<ServiceResult<CommitmentDto>> AddCommitment(CommitmentCreateDto dto, string userId);

    /// <summary>One tap from a call or a meeting: Agreed, Verbal, attributed to both parties, awaiting the other side.</summary>
    Task<ServiceResult<CommitmentDto>> LogDecision(CommitmentCreateDto dto, string userId);

    Task<ServiceResult<CommitmentDto>> Confirm(string commitmentId, string userId);

    /// <summary>"That's not what we said" — flips it to QueryRaised. It never silently becomes truth.</summary>
    Task<ServiceResult<CommitmentDto>> Dispute(CommitmentNoteDto dto, string userId);

    Task<ServiceResult<CommitmentDto>> RaiseQuery(CommitmentNoteDto dto, string userId);

    /// <summary>Resolution writes into the item. A changed figure is a restatement, so the old one survives.</summary>
    Task<ServiceResult<CommitmentDto>> ResolveQuery(CommitmentResolveDto dto, string userId);

    Task<ServiceResult<CommitmentDto>> SetMaturity(CommitmentMaturityDto dto, string userId);
    Task<ServiceResult<CommitmentDto>> Clear(string commitmentId, string userId);
    Task<ServiceResult<CommitmentDto>> Restate(CommitmentRestateDto dto, string userId);

    // ─── Links, both directions ───────────────────────────────
    Task<ServiceResult<CommitmentLinkDto>> AddLink(CommitmentLinkCreateDto dto, string userId);
    Task<ServiceResult<bool>> RemoveLink(string linkId, string userId);
    Task<ServiceResult<List<CommitmentLinkDto>>> GetLinks(string commitmentId, string userId);

    /// <summary>What a photo, a release or a message is evidence for.</summary>
    Task<ServiceResult<List<CommitmentLinkDto>>> GetBacklinks(string projectId, CommitmentLinkTarget targetType, string targetId, string userId);

    // ─── Accountability is a query, not a feature ─────────────
    Task<ServiceResult<AccountabilityDto>> GetAccountability(string projectId, string userId);
}

/// <summary>
/// The stage money ledger — funded → claimed → cleared → carried forward — and
/// the variation register. Money seats only (<c>ProjectAccess.CanSeeMoney</c>).
/// </summary>
public interface ILedgerDAL
{
    Task<ServiceResult<StageLedgerDto>> GetStageLedger(string projectId, string userId);

    Task<ServiceResult<List<StageClaimDto>>> GetClaims(string projectId, string? stageId, string userId);
    Task<ServiceResult<StageClaimDto>> AddClaim(StageClaimCreateDto dto, string userId);

    /// <summary>The funder clears a claim, or queries it. The side claiming never clears its own.</summary>
    Task<ServiceResult<StageClaimDto>> DecideClaim(StageClaimDecisionDto dto, string userId);
    Task<ServiceResult<StageClaimDto>> WithdrawClaim(string claimId, string userId);

    Task<ServiceResult<List<VariationDto>>> GetVariations(string projectId, string userId);
    Task<ServiceResult<VariationDto>> AddVariation(VariationCreateDto dto, string userId);
    Task<ServiceResult<VariationDto>> DecideVariation(VariationDecisionDto dto, string userId);
}
