using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

/// <summary>The commitments register — deliverables, commitments, links, accountability.</summary>
public interface ICommitmentsApi
{
    [Get("/api/Commitments/GetDeliverables")]
    Task<IApiResponse<List<DeliverableDto>>> GetDeliverables([Query] string projectId, [Query] string? stageId = null);

    [Post("/api/Commitments/AddDeliverable")]
    Task<IApiResponse<DeliverableDto>> AddDeliverable([Body] DeliverableCreateDto dto);

    [Put("/api/Commitments/UpdateDeliverable")]
    Task<IApiResponse<DeliverableDto>> UpdateDeliverable([Body] DeliverableUpdateDto dto);

    [Delete("/api/Commitments/DeleteDeliverable")]
    Task<IApiResponse<bool>> DeleteDeliverable([Query] string deliverableId);

    [Get("/api/Commitments/GetCommitments")]
    Task<IApiResponse<List<CommitmentDto>>> GetCommitments(
        [Query] string projectId, [Query] string? stageId = null, [Query] bool includeSuperseded = false);

    [Get("/api/Commitments/GetCommitment")]
    Task<IApiResponse<CommitmentDto>> GetCommitment([Query] string commitmentId);

    [Get("/api/Commitments/GetChain")]
    Task<IApiResponse<List<CommitmentDto>>> GetChain([Query] string commitmentId);

    [Post("/api/Commitments/AddCommitment")]
    Task<IApiResponse<CommitmentDto>> AddCommitment([Body] CommitmentCreateDto dto);

    [Post("/api/Commitments/LogDecision")]
    Task<IApiResponse<CommitmentDto>> LogDecision([Body] CommitmentCreateDto dto);

    [Put("/api/Commitments/Confirm")]
    Task<IApiResponse<CommitmentDto>> Confirm([Query] string commitmentId);

    [Put("/api/Commitments/Dispute")]
    Task<IApiResponse<CommitmentDto>> Dispute([Body] CommitmentNoteDto dto);

    [Put("/api/Commitments/RaiseQuery")]
    Task<IApiResponse<CommitmentDto>> RaiseQuery([Body] CommitmentNoteDto dto);

    [Put("/api/Commitments/ResolveQuery")]
    Task<IApiResponse<CommitmentDto>> ResolveQuery([Body] CommitmentResolveDto dto);

    [Put("/api/Commitments/SetMaturity")]
    Task<IApiResponse<CommitmentDto>> SetMaturity([Body] CommitmentMaturityDto dto);

    [Put("/api/Commitments/Clear")]
    Task<IApiResponse<CommitmentDto>> Clear([Query] string commitmentId);

    [Post("/api/Commitments/Restate")]
    Task<IApiResponse<CommitmentDto>> Restate([Body] CommitmentRestateDto dto);

    [Post("/api/Commitments/AddLink")]
    Task<IApiResponse<CommitmentLinkDto>> AddLink([Body] CommitmentLinkCreateDto dto);

    [Delete("/api/Commitments/RemoveLink")]
    Task<IApiResponse<bool>> RemoveLink([Query] string linkId);

    [Get("/api/Commitments/GetLinks")]
    Task<IApiResponse<List<CommitmentLinkDto>>> GetLinks([Query] string commitmentId);

    [Get("/api/Commitments/GetBacklinks")]
    Task<IApiResponse<List<CommitmentLinkDto>>> GetBacklinks(
        [Query] string projectId, [Query] CommitmentLinkTarget targetType, [Query] string targetId);

    [Get("/api/Commitments/GetAccountability")]
    Task<IApiResponse<AccountabilityDto>> GetAccountability([Query] string projectId);
}

/// <summary>The stage ledger, its claims, and the variation register.</summary>
public interface ILedgerApi
{
    [Get("/api/Ledger/GetStageLedger")]
    Task<IApiResponse<StageLedgerDto>> GetStageLedger([Query] string projectId);

    [Get("/api/Ledger/GetClaims")]
    Task<IApiResponse<List<StageClaimDto>>> GetClaims([Query] string projectId, [Query] string? stageId = null);

    [Post("/api/Ledger/AddClaim")]
    Task<IApiResponse<StageClaimDto>> AddClaim([Body] StageClaimCreateDto dto);

    [Put("/api/Ledger/DecideClaim")]
    Task<IApiResponse<StageClaimDto>> DecideClaim([Body] StageClaimDecisionDto dto);

    [Put("/api/Ledger/WithdrawClaim")]
    Task<IApiResponse<StageClaimDto>> WithdrawClaim([Query] string claimId);

    [Get("/api/Ledger/GetVariations")]
    Task<IApiResponse<List<VariationDto>>> GetVariations([Query] string projectId);

    [Post("/api/Ledger/AddVariation")]
    Task<IApiResponse<VariationDto>> AddVariation([Body] VariationCreateDto dto);

    [Put("/api/Ledger/DecideVariation")]
    Task<IApiResponse<VariationDto>> DecideVariation([Body] VariationDecisionDto dto);
}
