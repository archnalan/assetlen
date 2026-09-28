using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

/// <summary>
/// One answer to "what did I approve on the balustrade?" (plan.md P6), already
/// filtered to what this reader may see on each project. The client renders it
/// and derives nothing.
/// </summary>
public class SearchResultDto
{
    public string Query { get; set; } = "";

    /// <summary>The words actually looked for, after question words were set aside.</summary>
    public List<string> Terms { get; set; } = new();

    /// <summary>Words that were not looked for ("what", "did", "approve") — shown so nothing is dropped silently.</summary>
    public List<string> SetAside { get; set; } = new();

    /// <summary>"full-text" or "substring". Said out loud so a slow or literal match is explained, not mysterious.</summary>
    public string Backend { get; set; } = "substring";

    public bool FullTextInstalled { get; set; }

    public int ProjectsSearched { get; set; }

    public List<SearchGroupDto> Groups { get; set; } = new();

    public int TotalHits => Groups.Sum(g => g.Total);

    /// <summary>
    /// Files on the searched projects whose text has not been read yet. A search
    /// that silently fails to look somewhere is worse than one that says where
    /// it could not look.
    /// </summary>
    public int FilesAwaitingText { get; set; }

    /// <summary>The OCR engine this server reads photos with, or null when it has none.</summary>
    public string? OcrEngine { get; set; }
}

public class SearchGroupDto
{
    public SearchHitKind Kind { get; set; }
    public string Label { get; set; } = "";

    /// <summary>Every match in the group, of which <see cref="Hits"/> is the best part.</summary>
    public int Total { get; set; }

    public List<SearchHitDto> Hits { get; set; } = new();
}

public class SearchHitDto
{
    public string Id { get; set; } = "";
    public SearchHitKind Kind { get; set; }

    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? StageId { get; set; }
    public string? StageName { get; set; }
    public StageGroup? StagePhase { get; set; }

    public string Title { get; set; } = "";

    /// <summary>The passage the words were found in, trimmed around the first match.</summary>
    public string? Snippet { get; set; }

    /// <summary>Plain words: "its title", "the text read from the photo", "the message it came from".</summary>
    public string? MatchedIn { get; set; }

    public DateTime? At { get; set; }

    /// <summary>In-app route to the object. Null for a file, which is opened through the authenticated download.</summary>
    public string? Href { get; set; }

    public string? ArtifactId { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public bool HasThumbnail { get; set; }

    public CommitmentKind? CommitmentKind { get; set; }
    public CommitmentMaturity? Maturity { get; set; }
    public CommitmentQueryState? QueryState { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }

    /// <summary>True when there is a figure but money is not part of this reader's seat.</summary>
    public bool AmountHidden { get; set; }

    public SearchProvenanceDto Provenance { get; set; } = new();

    public double Score { get; set; }
}

/// <summary>
/// Where a result came from and where it has got to. For a commitment, its own
/// strip; for a photo or a message, the strip of the commitment it proves,
/// bills or started — or a stated gap when it is tied to nothing yet.
/// </summary>
public class SearchProvenanceDto
{
    public SearchOrigin Origin { get; set; }

    /// <summary>"From the thread", "Shared into the project", "Recorded in the register".</summary>
    public string OriginLabel { get; set; } = "";

    /// <summary>The accountable face for this reader (assetlen.md §10.1), not necessarily the true author.</summary>
    public string? Who { get; set; }

    public DateTime? At { get; set; }

    /// <summary>"Evidence for", "Invoice for", "Source of" — how a file or message relates to <see cref="CommitmentTitle"/>.</summary>
    public string? Role { get; set; }

    public string? CommitmentId { get; set; }
    public string? CommitmentTitle { get; set; }

    public List<ProvenanceStepDto> Steps { get; set; } = new();

    /// <summary>A missing truth, stated as a finding: "Not tied to any commitment yet".</summary>
    public string? Gap { get; set; }
}

public class ProvenanceStepDto
{
    public ProvenanceStep Step { get; set; }
    public string Label { get; set; } = "";
    public bool Reached { get; set; }
    public DateTime? At { get; set; }

    /// <summary>True on the step this result itself is — the receipt is the "invoiced" step of its commitment.</summary>
    public bool IsThis { get; set; }
}
