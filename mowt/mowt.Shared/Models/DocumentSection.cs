using Syncfusion.Blazor.RichTextEditor;

namespace mowt.Shared.Models
{
    public class DocumentSection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Parent section ID for tree structure. Null for root sections.
        /// </summary>
        public string? ParentId { get; set; }
        
        /// <summary>
        /// Depth in tree (1-3). Computed from parent chain.
        /// </summary>
        public int Level { get; set; } = 1;
        
        /// <summary>
        /// Sort order among siblings at the same parent level.
        /// </summary>
        public int SortOrder { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// Blazor component reference - not serialized.
        /// </summary>
        public SfRichTextEditor? EditorRef { get; set; }
        
        /// <summary>
        /// Whether this section is expanded in the TOC (children visible).
        /// </summary>
        public bool IsExpanded { get; set; } = true;
        
        /// <summary>
        /// Child sections for tree traversal.
        /// </summary>
        public List<DocumentSection> Children { get; set; } = new List<DocumentSection>();
        
        /// <summary>
        /// Computed display number like "2.1.3".
        /// </summary>
        public string DisplayNumber { get; set; } = string.Empty;
    }

}
