using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// The front door (plan.md P3, assetlen.md D3 — <em>WhatsApp is not replaced, it
/// is ingested</em>).
/// <para>
/// Everything here must work with the contractor silent (Law 0). Peter uploads
/// his own export, from his own phone, and gets a searchable record without
/// anyone else logging in. That is the whole tier-1 thesis and this interface is
/// where it enters the system.
/// </para>
/// <para>
/// Import is two calls, not one. <see cref="PreviewArchiveAsync"/> stores the
/// archive and reads it; <see cref="CommitImportAsync"/> writes rows. The split
/// exists because attribution is the one part of an import that is expensive to
/// undo — filing 1,055 messages against the wrong person is worse than not
/// importing them.
/// </para>
/// </summary>
public interface IIngestDAL
{
    // ─── WhatsApp export ─────────────────────────────────────────────────

    /// <summary>
    /// Store an export archive and report what it contains. Writes no messages.
    /// <para>
    /// Accepts a <c>.zip</c> (transcript plus media) or a bare <c>.txt</c>
    /// transcript. The archive itself is stored as an artifact, so the same
    /// export uploaded twice is recognised before anything is parsed.
    /// </para>
    /// </summary>
    Task<ServiceResult<IngestPreviewDto>> PreviewArchiveAsync(
        Stream archive, string? fileName, string? contentType,
        string projectId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Apply a previewed batch: map authors, store media, write messages.
    /// Idempotent — re-committing a batch whose messages already exist adds none.
    /// </summary>
    Task<ServiceResult<IngestBatchDto>> CommitImportAsync(
        IngestCommitDto dto, string userId, CancellationToken ct = default);

    // ─── The ongoing trickle ─────────────────────────────────────────────

    /// <summary>
    /// One item from a phone's share sheet: a file, some text, or both.
    /// Parity target is forwarding inside WhatsApp (assetlen.md §9), so this
    /// takes no options and asks no questions.
    /// </summary>
    Task<ServiceResult<IngestedMessageDto>> CaptureShareAsync(
        Stream? file, string? fileName, string? contentType,
        ShareCaptureDto dto, string userId, CancellationToken ct = default);

    /// <summary>
    /// Accept a forwarded email addressed to a project's inbound key.
    /// <para>
    /// <b>Unauthenticated in the user sense</b> — the caller is a mail relay. The
    /// project is resolved from the address, and the shared secret is checked by
    /// the controller before this is reached.
    /// </para>
    /// </summary>
    Task<ServiceResult<IngestBatchDto>> ReceiveEmailAsync(
        InboundEmailDto dto, CancellationToken ct = default);

    /// <summary>The project's inbound address, minted on first request.</summary>
    Task<ServiceResult<ProjectInboxDto>> GetInboxAsync(
        string projectId, string userId, CancellationToken ct = default);

    /// <summary>Revoke the current inbound address and mint a new one.</summary>
    Task<ServiceResult<ProjectInboxDto>> ResetInboxAsync(
        string projectId, string userId, CancellationToken ct = default);

    // ─── Reading the raw record ──────────────────────────────────────────

    Task<ServiceResult<List<IngestBatchDto>>> GetBatchesAsync(
        string projectId, string userId, CancellationToken ct = default);

    Task<ServiceResult<IngestBatchDto>> GetBatchAsync(
        string batchId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Paged read of ingested messages. Gated on the importing side, not on the
    /// reader's role — see <c>tbl_IngestBatch.ImportedSide</c>.
    /// </summary>
    Task<ServiceResult<IngestedMessagePageDto>> GetMessagesAsync(
        IngestedMessageQueryDto query, string userId, CancellationToken ct = default);
}
