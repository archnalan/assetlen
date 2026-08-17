using assetlen.Shared.Apicalls;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Microsoft.Extensions.Logging;

namespace assetlen.Shared.Services;

/// <summary>
/// The one project list, plus the chrome's own state (drawer, theme).
/// Home renders <see cref="Cards"/> and the rail renders <see cref="Roots"/>;
/// both are projections of the same array, so the two surfaces cannot disagree
/// about the reader's order.
/// </summary>
public sealed class ShellState
{
    private readonly IProjectsRSApi _projects;
    private readonly ILogger<ShellState> _logger;

    private readonly Dictionary<string, ProjectRef> _index = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ProjectCardDto> _cards = new();

    public ShellState(IProjectsRSApi projects, ILogger<ShellState> logger)
    {
        _projects = projects;
        _logger = logger;
    }

    public event Action? OnChange;

    /// <summary>Top-level projects in this reader's order, sub-projects nested.</summary>
    public IReadOnlyList<ProjectCardDto> Cards => _cards;

    /// <summary>The same list, reduced to what the rail and the breadcrumbs need.</summary>
    public IReadOnlyList<ProjectRef> Roots { get; private set; } = Array.Empty<ProjectRef>();

    public bool ProjectsLoaded { get; private set; }
    public bool DrawerOpen { get; private set; }

    /// <summary>"light" | "dark" | null (follow the operating system).</summary>
    public string? Theme { get; private set; }

    /// <summary>Pinned projects, oldest pin first. Never more than <see cref="MaxPins"/>.</summary>
    public IEnumerable<ProjectCardDto> Pinned => _cards.Where(c => c.IsPinned);

    /// <summary>The draggable list — everything that is not pinned, in the reader's order.</summary>
    public IEnumerable<ProjectCardDto> Unpinned => _cards.Where(c => !c.IsPinned);

    public int PinnedCount => _cards.Count(c => c.IsPinned);

    /// <summary>Mirrors <c>tbl_ProjectPreference.MaxPins</c>; the server is the real cap.</summary>
    public const int MaxPins = 3;

    public void ToggleDrawer() { DrawerOpen = !DrawerOpen; Notify(); }
    public void CloseDrawer() { if (DrawerOpen) { DrawerOpen = false; Notify(); } }

    public void SetTheme(string? theme) { Theme = theme; Notify(); }

    /// <summary>
    /// A project's display name, or null. Never an id: breadcrumbs use names,
    /// never IDs (CLAUDE.md §3), so the caller renders a placeholder instead.
    /// </summary>
    public ProjectRef? Find(string? projectId)
        => projectId is not null && _index.TryGetValue(projectId, out var p) ? p : null;

    /// <summary>The full card for a project, if the dashboard has been read. Null for a deep link that arrived first.</summary>
    public ProjectCardDto? FindCard(string? projectId)
    {
        if (string.IsNullOrEmpty(projectId)) return null;

        foreach (var card in _cards)
        {
            if (string.Equals(card.Id, projectId, StringComparison.OrdinalIgnoreCase)) return card;

            foreach (var sub in card.SubProjects)
                if (string.Equals(sub.Id, projectId, StringComparison.OrdinalIgnoreCase)) return sub;
        }

        return null;
    }

    /// <summary>Record a name learned from any response, so a deep link renders a real trail on first paint.</summary>
    public void Remember(string? id, string? name, string? parentId = null)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name)) return;

        if (_index.TryGetValue(id, out var existing)
            && existing.Name == name
            && existing.ParentId == (parentId ?? existing.ParentId))
        {
            return;
        }

        // Keep what the dashboard already taught us — a name from a detail
        // response must not blank the cover the rail is drawing with.
        _index[id] = new ProjectRef(
            id, name, parentId ?? existing?.ParentId, existing?.Children, existing?.Thumb);

        Notify();
    }

    public void SetProjects(IEnumerable<ProjectCardDto> cards)
    {
        _cards.Clear();
        _cards.AddRange(cards);

        ProjectsLoaded = true;
        Reindex();
    }

    /// <summary>
    /// Place a just-created project without waiting for the next dashboard read.
    /// It lands at the bottom, where the arrangement rules put an untouched
    /// project, so it will still be there after a reload.
    /// </summary>
    public void AddProject(ProjectDto created)
    {
        if (string.IsNullOrEmpty(created.Id)) return;

        var card = new ProjectCardDto
        {
            Id = created.Id,
            ProjectName = created.ProjectName ?? "Untitled project",
            Location = created.Location,
            CurrentStageName = created.CurrentStageName,
            TimelineStatus = "On Track",
            Status = created.Status,
            Currency = created.Currency,
            TotalBudget = created.TotalBudget ?? 0,
            CoverImageUrl = created.CoverImageUrl,
            LatestImageUrl = created.CoverImageUrl,
            IsSubscriptionActive = created.IsSubscriptionActive,
            ParentProjectId = created.ParentProjectId,
            SortOrder = int.MaxValue
        };

        if (!string.IsNullOrEmpty(created.ParentProjectId))
        {
            var parent = _cards.FirstOrDefault(c =>
                string.Equals(c.Id, created.ParentProjectId, StringComparison.OrdinalIgnoreCase));

            if (parent is null) return;   // parent not loaded yet; the next read will place it

            // A wing with no photograph of its own wears the house's face —
            // the same rule ProjectDAL.InheritCover applies server-side.
            if (string.IsNullOrEmpty(card.LatestImageUrl))
            {
                var borrowed = parent.CoverImageUrl ?? parent.RecentImageUrls.FirstOrDefault() ?? parent.LatestImageUrl;
                if (!string.IsNullOrEmpty(borrowed))
                {
                    card.LatestImageUrl = borrowed;
                    card.RecentImageUrls = new List<string> { borrowed };
                    card.CoverInherited = true;
                }
            }

            parent.SubProjects.Add(card);
            parent.SubProjectCount = parent.SubProjects.Count;
        }
        else
        {
            _cards.Add(card);
        }

        Reindex();
    }

    /// <summary>Take a project out of the list the moment it is archived, without waiting for a reload.</summary>
    public void RemoveProject(string? projectId)
    {
        if (string.IsNullOrEmpty(projectId)) return;

        var removed = _cards.RemoveAll(c => string.Equals(c.Id, projectId, StringComparison.OrdinalIgnoreCase)) > 0;

        if (!removed)
        {
            foreach (var parent in _cards)
            {
                if (parent.SubProjects.RemoveAll(s =>
                        string.Equals(s.Id, projectId, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    parent.SubProjectCount = parent.SubProjects.Count;
                    removed = true;
                    break;
                }
            }
        }

        if (removed) Reindex();
    }

    /// <summary>Repoint a project's cover, everywhere it is drawn, without a reload.</summary>
    public void SetCover(string? projectId, string? url)
    {
        var card = FindCard(projectId);
        if (card is null) return;

        card.CoverImageUrl = url;
        card.CoverInherited = false;

        if (string.IsNullOrEmpty(url))
        {
            card.RecentImageUrls.Clear();
            card.LatestImageUrl = null;

            var parent = _cards.FirstOrDefault(c => c.SubProjects.Contains(card));
            var borrowed = parent?.CoverImageUrl ?? parent?.RecentImageUrls.FirstOrDefault() ?? parent?.LatestImageUrl;

            if (!string.IsNullOrEmpty(borrowed))
            {
                card.LatestImageUrl = borrowed;
                card.RecentImageUrls.Add(borrowed);
                card.CoverInherited = true;
            }
        }
        else
        {
            card.RecentImageUrls.Remove(url);
            card.RecentImageUrls.Insert(0, url);
            card.LatestImageUrl = url;

            foreach (var sub in card.SubProjects.Where(s => s.CoverInherited))
            {
                sub.LatestImageUrl = url;
                sub.RecentImageUrls = new List<string> { url };
            }
        }

        Reindex();
    }

    /// <summary>Drop the open-question count on a project to zero once they have all been closed.</summary>
    public void ClearOpenIssues(string? projectId)
    {
        var card = FindCard(projectId);
        if (card is null || card.OpenIssueCount == 0) return;

        card.OpenIssueCount = 0;
        Notify();
    }

    // ── Arranging ───────────────────────────────────────────────────────────

    /// <summary>
    /// Move a project within <see cref="Unpinned"/> and record it. Returns false
    /// if the write failed, so the caller can tell the reader the order they can
    /// see is not the order that was saved.
    /// </summary>
    public async Task<bool> ReorderAsync(int from, int to)
    {
        var unpinned = _cards.Where(c => !c.IsPinned).ToList();

        if (from < 0 || from >= unpinned.Count || to < 0 || to >= unpinned.Count || from == to)
            return true;

        var moved = unpinned[from];
        unpinned.RemoveAt(from);
        unpinned.Insert(to, moved);

        for (var i = 0; i < unpinned.Count; i++) unpinned[i].SortOrder = i;

        Resort();
        Notify();

        try
        {
            var payload = new ProjectOrderUpdateDto
            {
                Items = unpinned.Select((c, i) => new ProjectOrderItemDto { ProjectId = c.Id, SortOrder = i }).ToList()
            };

            var response = await _projects.ReorderProjects(payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save the project order");
            return false;
        }
    }

    /// <summary>Pin or unpin a project. Returns the refusal in words, or null on success.</summary>
    public async Task<string?> SetPinnedAsync(string projectId, bool pinned)
    {
        var card = _cards.FirstOrDefault(c => string.Equals(c.Id, projectId, StringComparison.OrdinalIgnoreCase));
        if (card is null) return "That project is not in your list.";

        if (pinned && PinnedCount >= MaxPins && !card.IsPinned)
            return $"You can pin up to {MaxPins} projects. Unpin one first.";

        var previous = card.IsPinned;
        card.IsPinned = pinned;
        Resort();
        Notify();

        try
        {
            var response = await _projects.SetProjectPinned(projectId, pinned);
            if (response.IsSuccessStatusCode) return null;

            card.IsPinned = previous;
            Resort();
            Notify();
            return "That didn't save. Try again.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not pin {ProjectId}", projectId);
            card.IsPinned = previous;
            Resort();
            Notify();
            return "That didn't save. Try again.";
        }
    }

    /// <summary>
    /// Pins first, then the arranged list. Mirrors <c>ProjectDAL.ApplyArrangement</c>
    /// so the screen matches what the next read returns.
    /// </summary>
    private void Resort()
    {
        var pinOrder = _cards.Where(c => c.IsPinned).Select((c, i) => (c.Id, i))
            .ToDictionary(t => t.Id, t => t.i, StringComparer.OrdinalIgnoreCase);

        _cards.Sort((a, b) =>
        {
            if (a.IsPinned != b.IsPinned) return a.IsPinned ? -1 : 1;
            if (a.IsPinned && b.IsPinned)
                return pinOrder.GetValueOrDefault(a.Id).CompareTo(pinOrder.GetValueOrDefault(b.Id));

            return a.SortOrder != b.SortOrder
                ? a.SortOrder.CompareTo(b.SortOrder)
                : string.Compare(a.ProjectName, b.ProjectName, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>Rebuild the rail's projection and the name index, then wake every reader.</summary>
    private void Reindex()
    {
        var roots = new List<ProjectRef>();

        foreach (var card in _cards)
        {
            var children = card.SubProjects
                .Select(s => new ProjectRef(s.Id, s.ProjectName, card.Id, Thumb: Cover(s)))
                .ToList();

            var root = new ProjectRef(card.Id, card.ProjectName, card.ParentProjectId, children, Cover(card));
            roots.Add(root);

            _index[card.Id] = root;
            foreach (var c in children) _index[c.Id] = c;
        }

        Roots = roots;
        Notify();
    }

    /// <summary>A cover the reader chose wins; otherwise the most recent frame posted here.</summary>
    private static string? Cover(ProjectCardDto card)
        => (string.IsNullOrWhiteSpace(card.CoverImageUrl) ? null : card.CoverImageUrl)
           ?? card.RecentImageUrls.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))
           ?? (string.IsNullOrWhiteSpace(card.LatestImageUrl) ? null : card.LatestImageUrl);

    /// <summary>Forget everything on sign-out — a cached name surviving a user switch shows one account's work inside another's chrome.</summary>
    public void Clear()
    {
        _index.Clear();
        _cards.Clear();
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
    IReadOnlyList<ProjectRef>? Children = null,

    /// <summary>Cover frame for the rail, if this project has one. Null is normal, not an error.</summary>
    string? Thumb = null)
{
    public IReadOnlyList<ProjectRef> Children { get; init; } = Children ?? Array.Empty<ProjectRef>();
    public bool IsSubProject => !string.IsNullOrEmpty(ParentId);
}
