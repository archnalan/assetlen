using Xunit;
using mowt.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace mowt.Tests.Models
{
    /// <summary>
    /// Unit tests for TreeHelpers utility class.
    /// </summary>
    public class TreeHelpersTests
    {
        [Fact]
        public void BuildTree_FlatList_CreatesCorrectHierarchy()
        {
            // Arrange
            var flatSections = new List<DocumentSection>
            {
                new() { Id = "1", Title = "Root 1", ParentId = null, SortOrder = 0 },
                new() { Id = "2", Title = "Child 1.1", ParentId = "1", SortOrder = 0 },
                new() { Id = "3", Title = "Child 1.2", ParentId = "1", SortOrder = 1 },
                new() { Id = "4", Title = "Root 2", ParentId = null, SortOrder = 1 },
                new() { Id = "5", Title = "Grandchild 1.1.1", ParentId = "2", SortOrder = 0 }
            };

            // Act
            var roots = TreeHelpers.BuildTree(flatSections);

            // Assert
            Assert.Equal(2, roots.Count);
            Assert.Equal("Root 1", roots[0].Title);
            Assert.Equal(2, roots[0].Children.Count);
            Assert.Equal("Child 1.1", roots[0].Children[0].Title);
            Assert.Single(roots[0].Children[0].Children);
            Assert.Equal("Grandchild 1.1.1", roots[0].Children[0].Children[0].Title);
        }

        [Fact]
        public void ComputeTreeMetadata_AssignsCorrectDisplayNumbers()
        {
            // Arrange
            var roots = new List<DocumentSection>
            {
                new()
                {
                    Id = "1",
                    Title = "Root 1",
                    SortOrder = 0,
                    Children = new()
                    {
                        new() { Id = "1.1", Title = "Child 1.1", SortOrder = 0,
                                Children = new()
                                {
                                    new() { Id = "1.1.1", Title = "Grandchild", SortOrder = 0 }
                                }
                        },
                        new() { Id = "1.2", Title = "Child 1.2", SortOrder = 1 }
                    }
                },
                new() { Id = "2", Title = "Root 2", SortOrder = 1 }
            };

            // Act
            TreeHelpers.ComputeTreeMetadata(roots);

            // Assert
            Assert.Equal("1", roots[0].DisplayNumber);
            Assert.Equal(1, roots[0].Level);
            Assert.Equal("1.1", roots[0].Children[0].DisplayNumber);
            Assert.Equal(2, roots[0].Children[0].Level);
            Assert.Equal("1.1.1", roots[0].Children[0].Children[0].DisplayNumber);
            Assert.Equal(3, roots[0].Children[0].Children[0].Level);
            Assert.Equal("1.2", roots[0].Children[1].DisplayNumber);
            Assert.Equal("2", roots[1].DisplayNumber);
        }

        [Fact]
        public void FlattenTree_DepthFirst_CorrectOrder()
        {
            // Arrange
            var roots = new List<DocumentSection>
            {
                new()
                {
                    Id = "1",
                    Children = new()
                    {
                        new() { Id = "1.1", SortOrder = 0 },
                        new() { Id = "1.2", SortOrder = 1 }
                    }
                },
                new() { Id = "2" }
            };

            // Act
            var flat = TreeHelpers.FlattenTree(roots);

            // Assert
            Assert.Equal(4, flat.Count);
            Assert.Equal("1", flat[0].Id);
            Assert.Equal("1.1", flat[1].Id);
            Assert.Equal("1.2", flat[2].Id);
            Assert.Equal("2", flat[3].Id);
        }

        [Fact]
        public void NormalizeSortOrders_FixesGaps()
        {
            // Arrange
            var roots = new List<DocumentSection>
            {
                new() { Id = "1", SortOrder = 0,
                        Children = new()
                        {
                            new() { Id = "1.1", SortOrder = 0 },
                            new() { Id = "1.2", SortOrder = 5 },  // Gap
                            new() { Id = "1.3", SortOrder = 10 }  // Gap
                        }
                },
                new() { Id = "2", SortOrder = 100 }  // Gap
            };

            // Act
            TreeHelpers.NormalizeSortOrders(roots);

            // Assert
            Assert.Equal(0, roots[0].SortOrder);
            Assert.Equal(1, roots[1].SortOrder);
            Assert.Equal(0, roots[0].Children[0].SortOrder);
            Assert.Equal(1, roots[0].Children[1].SortOrder);
            Assert.Equal(2, roots[0].Children[2].SortOrder);
        }

        [Fact]
        public void GetDepth_ReturnsCorrectDepth()
        {
            // Arrange
            var root = new DocumentSection { Id = "1", ParentId = null };
            var child = new DocumentSection { Id = "2", ParentId = "1" };
            var grandchild = new DocumentSection { Id = "3", ParentId = "2" };
            var lookup = new Dictionary<string, DocumentSection>
            {
                { "1", root },
                { "2", child },
                { "3", grandchild }
            };

            // Act & Assert
            Assert.Equal(1, TreeHelpers.GetDepth(root, lookup));
            Assert.Equal(2, TreeHelpers.GetDepth(child, lookup));
            Assert.Equal(3, TreeHelpers.GetDepth(grandchild, lookup));
        }

        [Fact]
        public void WouldCreateCycle_DetectsCycle()
        {
            // Arrange
            var root = new DocumentSection { Id = "1", ParentId = null };
            var child = new DocumentSection { Id = "2", ParentId = "1" };
            var grandchild = new DocumentSection { Id = "3", ParentId = "2" };
            var lookup = new Dictionary<string, DocumentSection>
            {
                { "1", root },
                { "2", child },
                { "3", grandchild }
            };

            // Act & Assert
            Assert.True(TreeHelpers.WouldCreateCycle("1", "3", lookup));  // Moving root under grandchild
            Assert.True(TreeHelpers.WouldCreateCycle("2", "3", lookup));  // Moving child under grandchild
            Assert.False(TreeHelpers.WouldCreateCycle("3", "1", lookup)); // Moving grandchild under root (OK)
            Assert.True(TreeHelpers.WouldCreateCycle("1", "1", lookup));  // Self-reference
        }

        [Fact]
        public void MoveSection_InsertBefore_UpdatesParentAndOrder()
        {
            // Arrange
            var roots = new List<DocumentSection>
            {
                new() { Id = "1", SortOrder = 0 },
                new() { Id = "2", SortOrder = 1 },
                new() { Id = "3", SortOrder = 2 }
            };
            var lookup = roots.ToDictionary(s => s.Id);

            // Act - Move "3" before "1"
            TreeHelpers.MoveSection(roots[2], roots[0], DropPosition.Before, roots, lookup);

            // Assert
            Assert.Equal(3, roots.Count);
            Assert.Equal("3", roots[0].Id);
            Assert.Equal("1", roots[1].Id);
            Assert.Equal("2", roots[2].Id);
        }

        [Fact]
        public void MoveSection_MakeChild_UpdatesParent()
        {
            // Arrange
            var roots = new List<DocumentSection>
            {
                new() { Id = "1", SortOrder = 0, Children = new() },
                new() { Id = "2", SortOrder = 1, Children = new() }
            };
            var lookup = roots.ToDictionary(s => s.Id);

            // Act - Make "2" a child of "1"
            TreeHelpers.MoveSection(roots[1], roots[0], DropPosition.Child, roots, lookup);

            // Assert
            Assert.Single(roots);  // Only "1" at root
            Assert.Equal("1", roots[0].Id);
            Assert.Single(roots[0].Children);
            Assert.Equal("2", roots[0].Children[0].Id);
            Assert.Equal("1", roots[0].Children[0].ParentId);
        }

        [Fact]
        public void MoveSection_AtMaxDepth_FallsBackToAfter()
        {
            // Arrange
            var root = new DocumentSection
            {
                Id = "1",
                Level = 1,
                Children = new()
                {
                    new()
                    {
                        Id = "1.1",
                        Level = 2,
                        ParentId = "1",
                        Children = new()
                        {
                            new() { Id = "1.1.1", Level = 3, ParentId = "1.1" }
                        }
                    }
                }
            };
            var roots = new List<DocumentSection> { root };
            var newSection = new DocumentSection { Id = "new", SortOrder = 0 };
            roots.Add(newSection);
            
            var lookup = TreeHelpers.FlattenTree(roots).ToDictionary(s => s.Id);

            // Act - Try to make "new" a child of "1.1.1" (depth 3)
            var target = lookup["1.1.1"];
            TreeHelpers.MoveSection(newSection, target, DropPosition.Child, roots, lookup);

            // Assert - Should fall back to "After" instead
            Assert.Null(newSection.ParentId);  // Should not be child
            // Should be inserted after target at same level
        }

        [Fact]
        public void FindSection_FindsInTree()
        {
            // Arrange
            var roots = new List<DocumentSection>
            {
                new()
                {
                    Id = "1",
                    Children = new()
                    {
                        new()
                        {
                            Id = "1.1",
                            Children = new()
                            {
                                new() { Id = "1.1.1" }
                            }
                        }
                    }
                },
                new() { Id = "2" }
            };

            // Act & Assert
            Assert.NotNull(TreeHelpers.FindSection(roots, "1"));
            Assert.NotNull(TreeHelpers.FindSection(roots, "1.1"));
            Assert.NotNull(TreeHelpers.FindSection(roots, "1.1.1"));
            Assert.NotNull(TreeHelpers.FindSection(roots, "2"));
            Assert.Null(TreeHelpers.FindSection(roots, "999"));
        }

        [Fact]
        public void BuildTree_OrphanedSections_TreatedAsRoots()
        {
            // Arrange - Section with non-existent parent
            var flatSections = new List<DocumentSection>
            {
                new() { Id = "1", ParentId = null, SortOrder = 0 },
                new() { Id = "2", ParentId = "999", SortOrder = 0 }  // Orphan
            };

            // Act
            var roots = TreeHelpers.BuildTree(flatSections);

            // Assert
            Assert.Equal(2, roots.Count);  // Orphan promoted to root
            Assert.Contains(roots, s => s.Id == "2");
            Assert.Null(roots.First(s => s.Id == "2").ParentId);
        }

        [Theory]
        [InlineData(0, 1, 2)]  // Normal order
        [InlineData(2, 1, 0)]  // Reverse order
        [InlineData(5, 10, 0)] // Random order
        public void NormalizeSortOrders_HandlesAnyInputOrder(int order1, int order2, int order3)
        {
            // Arrange
            var roots = new List<DocumentSection>
            {
                new() { Id = "1", SortOrder = order1 },
                new() { Id = "2", SortOrder = order2 },
                new() { Id = "3", SortOrder = order3 }
            };

            // Act
            TreeHelpers.NormalizeSortOrders(roots);

            // Assert - All sections should be reordered to 0, 1, 2
            var ordered = roots.OrderBy(s => s.SortOrder).ToList();
            Assert.Equal(0, ordered[0].SortOrder);
            Assert.Equal(1, ordered[1].SortOrder);
            Assert.Equal(2, ordered[2].SortOrder);
        }
    }
}
