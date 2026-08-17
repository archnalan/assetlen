using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Services;

/// <summary>
/// What the chrome needs to know and no page owns on its own: which projects
/// exist, what they are called, whether the drawer is open, and which theme is
/// in force.
/// <para>
/// Breadcrumbs are derived from the URL rather than pushed by each page. A page
/// that forgets to push leaves the previous page's trail on screen, and a trail
/// that lies about where you are is worse than none — Peter is the reader, and
/// getting lost is the failure this shell exists to prevent.
/// </para>
/// </summary>
public sealed class ShellState
{
    private readonly Dictionary<string, ProjectRef> _projects = new(StringComparer.OrdinalIgnoreCase);

    public event Action? OnChange;

    /// <summary>Top-level projects with their sub-projects, for the rail and the switcher.</summary>
    public IReadOnlyList<ProjectRef> Roots { get; private set; } = Array.Empty<ProjectRef>();

    public bool ProjectsLoaded { get; private set; }
    public bool DrawerOpen { get; private set; }

    /// <summary>"light" | "dark" | null (follow the operating system).</summary>
    public string? Theme { get; private set; }

    public void ToggleDrawer() { DrawerOpen = !DrawerOpen; Notify(); }
    public void CloseDrawer() { if (DrawerOpen) { DrawerOpen = false; Notify(); } }

    public void SetTheme(string? theme) { Theme = theme; Notify(); }

    /// <summary>
    /// Look up a project's display name. Returns null rather than an id — a
    /// breadcrumb showing a GUID is the exact failure CLAUDE.md §3 forbids, so
    /// the caller renders a placeholder instead of leaking one.
    /// </summary>
    public ProjectRef? Find(string? projectId)
        => projectId is not null && _projects.TryGetValue(projectId, out var p) ? p : null;

    /// <summary>Record a name learned from any response, so a deep link renders a real trail on first paint.</summary>
    public void Remember(string? id, string? name, string? parentId = null)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name)) return;

        if (_projects.TryGetValue(id, out var existing)
            && existing.Name == name
            && existing.ParentId == (parentId ?? existing.ParentId))
        {
            return;
        }

        _projects[id] = new ProjectRef(id, name, parentId ?? existing?.ParentId);
        Notify();
    }

    public void SetProjects(IEnumerable<ProjectCardDto> cards)
    {
        var roots = new List<ProjectRef>();

        foreach (var card in cards)
        {
            var children = card.SubProjects
                .Select(s => new ProjectRef(s.Id, s.ProjectName, card.Id))
                .ToList();

            var root = new ProjectRef(card.Id, card.ProjectName, card.ParentProjectId, children);
            roots.Add(root);

            _projects[card.Id] = root;
            foreach (var c in children) _projects[c.Id] = c;
        }

        Roots = roots;
        ProjectsLoaded = true;
        Notify();
    }

    /// <summary>Forget everything on sign-out. A cached project name surviving a user switch would show one account's work inside another's chrome.</summary>
    public void Clear()
    {
        _projects.Clear();
        Roots = Array.Empty<ProjectRef>();
        ProjectsLoaded = false;
        DrawerOpen = false;
        Notify();
    }

    private void Notify() => OnChange?.Invoke();
}

public sealed record ProjectRef(
    string Id,
    string Name,
    string? ParentId = null,
    IReadOnlyList<ProjectRef>? Children = null)
{
    public IReadOnlyList<ProjectRef> Children { get; init; } = Children ?? Array.Empty<ProjectRef>();
    public bool IsSubProject => !string.IsNullOrEmpty(ParentId);
}
