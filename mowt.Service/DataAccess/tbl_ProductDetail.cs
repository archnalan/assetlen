using mowt.Shared.Models.Models;

namespace mowt.Service.DataAccess
{
    public class tbl_ProductDetail : BaseEntity
    {
        public string ProductId { get; set; } = string.Empty;
        public int Level { get; set; } = 1; // Default to H1 as requested
        public int SortOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        /// <summary>
        /// Parent section ID for tree structure. Null for root sections.
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// Computed display number like "2.1.3".
        /// </summary>
        public string DisplayNumber { get; set; } = string.Empty;
    }
}
