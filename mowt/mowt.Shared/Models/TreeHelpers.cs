using System;
using System.Collections.Generic;
using System.Linq;

namespace mowt.Shared.Models
{
    /// <summary>
    /// Utilities for working with tree-structured DocumentSections.
    /// </summary>
    public static class TreeHelpers
    {
        public const int MaxDepth = 3;
        
        /// <summary>
        /// Build a tree from a flat list of sections.
        /// </summary>
        public static List<DocumentSection> BuildTree(IEnumerable<DocumentSection> flatSections)
        {
            var sections = flatSections.ToList();
            var lookup = sections.ToDictionary(s => s.Id, s => s);
            
            // Clear existing children
            foreach (var section in sections)
            {
                section.Children.Clear();
            }
            
            var roots = new List<DocumentSection>();
            
            foreach (var section in sections.OrderBy(s => s.SortOrder))
            {
                if (string.IsNullOrWhiteSpace(section.ParentId))
                {
                    roots.Add(section);
                }
                else if (lookup.TryGetValue(section.ParentId, out var parent))
                {
                    parent.Children.Add(section);
                }
                else
                {
                    // Orphaned section - treat as root
                    roots.Add(section);
                    section.ParentId = null;
                }
            }
            
            // Compute levels and display numbers
            ComputeTreeMetadata(roots);
            
            return roots;
        }
        
        /// <summary>
        /// Flatten a tree into a list in depth-first order.
        /// </summary>
        public static List<DocumentSection> FlattenTree(IEnumerable<DocumentSection> roots)
        {
            var result = new List<DocumentSection>();
            
            void Traverse(DocumentSection section)
            {
                result.Add(section);
                foreach (var child in section.Children.OrderBy(c => c.SortOrder))
                {
                    Traverse(child);
                }
            }
            
            foreach (var root in roots.OrderBy(r => r.SortOrder))
            {
                Traverse(root);
            }
            
            return result;
        }
        
        /// <summary>
        /// Compute Level and DisplayNumber for all nodes.
        /// </summary>
        public static void ComputeTreeMetadata(List<DocumentSection> roots)
        {
            void Traverse(DocumentSection section, int level, Stack<int> numberStack)
            {
                section.Level = level;
                section.DisplayNumber = string.Join(".", numberStack.Reverse());
                
                var childNumber = 1;
                foreach (var child in section.Children.OrderBy(c => c.SortOrder))
                {
                    numberStack.Push(childNumber);
                    Traverse(child, level + 1, numberStack);
                    numberStack.Pop();
                    childNumber++;
                }
            }
            
            var rootNumber = 1;
            foreach (var root in roots.OrderBy(r => r.SortOrder))
            {
                var stack = new Stack<int>();
                stack.Push(rootNumber);
                Traverse(root, 1, stack);
                rootNumber++;
            }
        }
        
        /// <summary>
        /// Normalize sort orders within each parent to be sequential (0, 1, 2, ...).
        /// </summary>
        public static void NormalizeSortOrders(List<DocumentSection> roots)
        {
            void NormalizeChildren(List<DocumentSection> siblings)
            {
                var ordered = siblings.OrderBy(s => s.SortOrder).ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    ordered[i].SortOrder = i;
                    NormalizeChildren(ordered[i].Children);
                }
            }
            
            NormalizeChildren(roots);
        }
        
        /// <summary>
        /// Get depth of a section (1 for root, 2 for child of root, etc.).
        /// </summary>
        public static int GetDepth(DocumentSection section, Dictionary<string, DocumentSection> lookup)
        {
            var depth = 1;
            var current = section;
            
            while (!string.IsNullOrWhiteSpace(current.ParentId) && 
                   lookup.TryGetValue(current.ParentId, out var parent))
            {
                depth++;
                current = parent;
                
                // Prevent infinite loops
                if (depth > MaxDepth + 1)
                    break;
            }
            
            return depth;
        }
        
        /// <summary>
        /// Check if moving dragId to be a child of targetId would create a cycle.
        /// </summary>
        public static bool WouldCreateCycle(string dragId, string targetId, Dictionary<string, DocumentSection> lookup)
        {
            if (dragId == targetId)
                return true;
            
            var current = targetId;
            while (!string.IsNullOrWhiteSpace(current) && lookup.TryGetValue(current, out var section))
            {
                if (section.ParentId == dragId)
                    return true;
                current = section.ParentId;
            }
            
            return false;
        }
        
        /// <summary>
        /// Move a section in the tree.
        /// </summary>
        public static void MoveSection(
            DocumentSection dragSection,
            DocumentSection? targetSection,
            DropPosition position,
            List<DocumentSection> roots,
            Dictionary<string, DocumentSection> lookup)
        {
            // Remove from current parent
            if (string.IsNullOrWhiteSpace(dragSection.ParentId))
            {
                roots.Remove(dragSection);
            }
            else if (lookup.TryGetValue(dragSection.ParentId, out var oldParent))
            {
                oldParent.Children.Remove(dragSection);
            }
            
            if (targetSection == null)
            {
                // Drop at root level
                dragSection.ParentId = null;
                roots.Add(dragSection);
                dragSection.SortOrder = roots.Count - 1;
            }
            else
            {
                switch (position)
                {
                    case DropPosition.Before:
                        // Insert as sibling before target
                        dragSection.ParentId = targetSection.ParentId;
                        var beforeSiblings = GetSiblings(targetSection, roots, lookup);
                        var beforeIndex = beforeSiblings.IndexOf(targetSection);
                        beforeSiblings.Insert(beforeIndex, dragSection);
                        ReindexSiblings(beforeSiblings);
                        break;
                        
                    case DropPosition.After:
                        // Insert as sibling after target
                        dragSection.ParentId = targetSection.ParentId;
                        var afterSiblings = GetSiblings(targetSection, roots, lookup);
                        var afterIndex = afterSiblings.IndexOf(targetSection);
                        afterSiblings.Insert(afterIndex + 1, dragSection);
                        ReindexSiblings(afterSiblings);
                        break;
                        
                    case DropPosition.Child:
                        // Make child of target
                        var targetDepth = GetDepth(targetSection, lookup);
                        if (targetDepth >= MaxDepth)
                        {
                            // Can't add child at max depth - treat as after instead
                            goto case DropPosition.After;
                        }
                        
                        dragSection.ParentId = targetSection.Id;
                        targetSection.Children.Add(dragSection);
                        dragSection.SortOrder = targetSection.Children.Count - 1;
                        break;
                }
            }
            
            // Recompute metadata
            ComputeTreeMetadata(roots);
        }
        
        private static List<DocumentSection> GetSiblings(
            DocumentSection section, 
            List<DocumentSection> roots, 
            Dictionary<string, DocumentSection> lookup)
        {
            if (string.IsNullOrWhiteSpace(section.ParentId))
            {
                return roots;
            }
            else if (lookup.TryGetValue(section.ParentId, out var parent))
            {
                return parent.Children;
            }
            
            return new List<DocumentSection>();
        }
        
        private static void ReindexSiblings(List<DocumentSection> siblings)
        {
            for (int i = 0; i < siblings.Count; i++)
            {
                siblings[i].SortOrder = i;
            }
        }
        
        /// <summary>
        /// Find a section by ID in the tree.
        /// </summary>
        public static DocumentSection? FindSection(List<DocumentSection> roots, string id)
        {
            DocumentSection? Search(DocumentSection section)
            {
                if (section.Id == id)
                    return section;
                
                foreach (var child in section.Children)
                {
                    var found = Search(child);
                    if (found != null)
                        return found;
                }
                
                return null;
            }
            
            foreach (var root in roots)
            {
                var found = Search(root);
                if (found != null)
                    return found;
            }
            
            return null;
        }
    }
    
    public enum DropPosition
    {
        Before,
        After,
        Child
    }
}
