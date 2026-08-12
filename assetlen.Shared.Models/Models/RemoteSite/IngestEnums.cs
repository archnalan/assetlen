namespace assetlen.Shared.Models.Models.RemoteSite;

/// <summary>
/// Where ingested material came in from (assetlen.md D3 — <em>WhatsApp is not
/// replaced, it is ingested</em>).
/// <para>
/// This is not the same idea as a Commitment's <c>SourceChannel</c>, which
/// records how an <em>agreement</em> was reached. This records how the raw bytes
/// entered Assetlen, and it exists so a message can always be traced back to the
/// door it came through.
/// </para>
/// </summary>
public enum IngestSourceType
{
    /// <summary>A WhatsApp chat export — the <c>.txt</c> transcript, with or without its media.</summary>
    WhatsAppExport = 0,

    /// <summary>One item pushed from a phone's share sheet. The ongoing trickle.</summary>
    ShareSheet = 1,

    /// <summary>Forwarded to the project's inbound address, attachments included.</summary>
    Email = 2,

    /// <summary>Typed straight in. Present so nothing has to lie about its origin.</summary>
    Manual = 3
}

/// <summary>
/// Lifecycle of one import run. A run is recorded even when it fails — a silent
/// failed import is indistinguishable from an export that contained nothing, and
/// Peter has no other way to tell those apart.
/// </summary>
public enum IngestBatchStatus
{
    /// <summary>Archive stored and parsed; awaiting the author mapping and a commit.</summary>
    Previewed = 0,

    Importing = 1,

    Completed = 2,

    Failed = 3
}

/// <summary>
/// How the day and month of an ambiguous export date were read.
/// <para>
/// <c>03/12/2025</c> is 3 December or 12 March depending on the phone that
/// produced the export, and WhatsApp records no locale. Guessing wrong shifts
/// most of a year's history by months, so the choice is resolved across the
/// whole file and then <b>reported</b> rather than hidden.
/// </para>
/// </summary>
public enum ExportDateOrder
{
    /// <summary>No date in the file disambiguated it; the default was applied.</summary>
    AssumedDayFirst = 0,

    /// <summary>Proven: at least one date had a first component above 12.</summary>
    DayFirst = 1,

    /// <summary>Proven: at least one date had a second component above 12.</summary>
    MonthFirst = 2
}
