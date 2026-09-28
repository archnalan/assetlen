using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices.Extraction;

/// <summary>One message as extraction sees it. Pure data, so the rules can be run over a file with no database.</summary>
public sealed record ExtractionInput(
    string Id,
    DateTime SentAt,
    string Author,
    ProjectSide? AuthorSide,
    string? Body,
    bool IsSystem = false);

/// <summary>Something the thread appears to commit someone to. Becomes nothing until a person accepts it.</summary>
public sealed record ExtractionCandidate
{
    public required string MessageId { get; init; }
    public required ProposalKind Kind { get; init; }
    public required string Title { get; init; }
    public string? Detail { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public DateTime? DueDate { get; init; }
    public string? DateText { get; init; }
    public string? Quantity { get; init; }
    public CommitmentMaturity Maturity { get; init; } = CommitmentMaturity.Agreed;
    public ProjectSide? OwedBySide { get; init; }
    public string? PartyName { get; init; }
    public required string Rule { get; init; }
    public double Confidence { get; init; }
    public bool Contested { get; set; }
    public string? ContestNote { get; set; }
}

/// <summary>"Guest wing plastering is about 80% complete" — a reading, not a commitment.</summary>
public sealed record ExtractedReading(string MessageId, string Subject, decimal Percent, DateTime ObservedAt);

public sealed class ExtractionOutput
{
    public List<ExtractionCandidate> Candidates { get; } = new();
    public List<ExtractedReading> Readings { get; } = new();
    public List<string> Notes { get; } = new();
}

/// <summary>
/// Reads a run of ingested messages and proposes commitments (assetlen.md Law 3).
/// Implementations must return nothing for acknowledgements — "Okay", "Noted",
/// "Good progress" — because a queue Peter taps through blindly is worse than none.
/// </summary>
public interface IMessageExtractor
{
    string Engine { get; }

    /// <summary>False when the engine needs something this deployment does not have — a key, or consent.</summary>
    bool IsAvailable => true;

    Task<ExtractionOutput> ExtractAsync(IReadOnlyList<ExtractionInput> messages, CancellationToken ct = default);
}
