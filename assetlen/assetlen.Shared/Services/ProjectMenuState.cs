using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Services;

/// <summary>
/// Which project's actions are open, and where the reader asked for them.
/// One panel mounted in the shell — a menu rendered inside a card is clipped
/// by the card.
/// </summary>
public sealed class ProjectMenuState
{
    public event Action? OnChange;

    /// <summary>The project whose actions are showing, or null when nothing is open.</summary>
    public ProjectCardDto? Project { get; private set; }

    /// <summary>Viewport coordinates the reader asked at. The panel flips itself to stay on screen.</summary>
    public double X { get; private set; }
    public double Y { get; private set; }

    /// <summary>One level of nesting only — a wing cannot be pinned or given a wing of its own.</summary>
    public bool IsSubProject => !string.IsNullOrEmpty(Project?.ParentProjectId);

    public void Open(ProjectCardDto project, double x, double y)
    {
        Project = project;
        X = x;
        Y = y;
        OnChange?.Invoke();
    }

    public void Close()
    {
        if (Project is null) return;
        Project = null;
        OnChange?.Invoke();
    }
}
