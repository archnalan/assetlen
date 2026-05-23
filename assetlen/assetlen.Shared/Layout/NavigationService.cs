using assetlen.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.FluentUI.AspNetCore.Components;

namespace assetlen.Shared.Layout
{
    /// <summary>
    /// Source of truth for the sidebar in <see cref="MainLayout"/>. Only
    /// routes that actually exist live here — adding a NavItem that points
    /// to a non-existent page is a bug. Project-scoped surfaces (Site
    /// Journal, Issues, Budget, Timeline) intentionally live under the
    /// per-project tab strip in <c>ProjectDetail</c>, not here.
    /// </summary>
    public class NavigationService
    {
        private readonly IConfiguration _config;
        private readonly IFormFactor _formFactor;

        public NavigationService(IConfiguration config, IFormFactor formFactor)
        {
            _config = config;
            _formFactor = formFactor;
        }

        public List<NavItem> GetAllNavigationItems()
        {
            return new List<NavItem>
            {
                // Standalone entry points
                new NavItem("Portfolio",     "portfolio",       "", new Icons.Regular.Size20.Grid(), true),
                new NavItem("Manager",       "pm",              "", new Icons.Regular.Size20.PersonBoard(), true),
                new NavItem("New project",   "project/create",  "", new Icons.Regular.Size20.AddCircle(), true),
            };
        }

        public List<NavGroup> BuildNavigationStructure()
        {
            var items = GetAllNavigationItems();
            var rootGroups = new List<NavGroup>();

            // Every nav item right now is a standalone (no group). The
            // grouping machinery from the prior POS sidebar is intentionally
            // retired — re-introduce it only when ASSETLEN actually needs a
            // nested admin area.
            foreach (var item in items)
            {
                var group = new NavGroup(item.Title) { Icon = item.Icon };
                group.Items.Add(item);
                rootGroups.Add(group);
            }

            return rootGroups;
        }
    }
}
