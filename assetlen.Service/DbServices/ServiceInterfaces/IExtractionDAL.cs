using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// Extraction — the only path from forwarded material to a register when the
/// contractor is silent (plan.md P5, assetlen.md Law 0 and Law 3).
/// <para>
/// Proposals and the queue are for the principals (<c>ProjectAccess.CanSeeRegister</c>);
/// a support seat is answered as if there were no queue. Each proposal is exactly
/// as readable as the message it came from.
/// </para>
/// </summary>
public interface IExtractionDAL
{
    /// <summary>Read the project's ingested record and add any new proposals and readings. Idempotent.</summary>
    Task<ServiceResult<ExtractionRunDto>> RunAsync(ExtractionRunRequestDto dto, string userId, CancellationToken ct = default);

    /// <summary>The review queue with its accept-rate instrumentation.</summary>
    Task<ServiceResult<ExtractionQueueDto>> GetQueueAsync(string projectId, ProposalStatus? status, string userId, CancellationToken ct = default);

    /// <summary>Accept or reject in bulk. Accepting writes the commitment (or the blocker) with the message as its source.</summary>
    Task<ServiceResult<ProposalDecisionResultDto>> DecideAsync(ProposalDecisionDto dto, string userId, CancellationToken ct = default);

    Task<ServiceResult<ExtractionProposalDto>> EditAsync(ProposalEditDto dto, string userId, CancellationToken ct = default);

    Task<ServiceResult<List<ProgressReadingDto>>> GetReadingsAsync(string projectId, string? stageId, string userId, CancellationToken ct = default);

    Task<ServiceResult<ArtifactTextDto>> GetArtifactTextAsync(string artifactId, string userId, CancellationToken ct = default);

    /// <summary>Queue every file on the project that has not been read yet — the backfill once an engine is configured.</summary>
    Task<ServiceResult<OcrQueueResultDto>> QueueProjectOcrAsync(string projectId, string userId, CancellationToken ct = default);
}
