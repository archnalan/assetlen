using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Layout
{
    public class NavItem
    {
        public string Title { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public Icon? Icon { get; set; }
        public Color? IconColor { get; set; } = Color.Accent;
        public string GroupPath { get; set; } = string.Empty;
        public bool ExactMatch { get; set; } = false;

        public NavItem() { }

        public NavItem(string title, string href, string groupPath, Icon? icon = null, bool exactMatch = false)
        {
            Title = title;
            Href = href;
            GroupPath = groupPath;
            Icon = icon;
            ExactMatch = exactMatch;
        }
    }

    public class NavGroup
    {
        public string Title { get; set; } = string.Empty;
        public Icon? Icon { get; set; }
        public List<NavGroup> SubGroups { get; set; } = new();
        public List<NavItem> Items { get; set; } = new();
        public bool Expanded { get; set; }
        public string ParentPath { get; set; } = string.Empty;
        public string FullPath => string.IsNullOrEmpty(ParentPath) ? Title : $"{ParentPath} > {Title}";
        public NavGroup(string title, Icon? icon = null, bool expanded = false)
        {
            Title = title;
            Icon = icon;
            Expanded = expanded;
        }
    }
}
