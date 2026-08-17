namespace assetlen.Shared.Models;

/// <summary>
/// The four states every fetching surface must be able to render (CLAUDE.md
/// §4.2). Held as one enum so a page cannot express "loaded and also errored",
/// and so <c>StateBlock</c> can refuse to render content the page thinks it has.
/// </summary>
public enum LoadPhase
{
    Loading,
    Ready,
    Empty,
    Error
}
