using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;
using SantasWorkshop.Data;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for ResourceManager core functionality.
    /// Tests resource tracking, addition, consumption, and validation.
    /// </summary>
    public class ResourceManagerTests
    {
        private GameObject managerObject;
        private ResourceManager resourceManager;

        [SetUp]
        public void Setup()
        {
            // Create a GameObject with ResourceManager component
            managerObject = new GameObject("TestResourceManager");
            resourceManager = managerObject.AddComponent<ResourceManager>();
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up the GameObject after each test
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
        }

        #region AddResources Tests

        [Test]
        public void AddResource_IncreasesCount()
        {
            // Arrange
            string resourceId = "wood";
            int amount = 100;

            // Act
            resourceManager.AddResource(resourceId, amount);

            // Assert
            Assert.AreEqual(100, resourceManager.GetResourceCount(resourceId), 
                "Resource count should be 100 after adding 100 units");
        }

        [Test]
        public void AddResources_WithArray_IncreasesCount()
        {
            // Arrange
            var resources = new ResourceStack[]
            {
                new ResourceStack("wood", 50),
                new ResourceStack("stone", 75)
            };

            // Act
            resourceManager.AddResources(resources);

            // Assert
            Assert.AreEqual(50, resourceManager.GetResourceCount("wood"), 
                "Wood count should be 50");
            Assert.AreEqual(75, resourceManager.GetResourceCount("stone"), 
                "Stone count should be 75");
        }

        [Test]
        public void AddResource_Multiple_AccumulatesCount()
        {
            // Arrange
            string resourceId = "iron";

            // Act
            resourceManager.AddResource(resourceId, 50);
            resourceManager.AddResource(resourceId, 30);
            resourceManager.AddResource(resourceId, 20);

            // Assert
            Assert.AreEqual(100, resourceManager.GetResourceCount(resourceId), 
                "Resource count should accumulate to 100");
        }

        [Test]
        public void AddResource_WithZeroAmount_DoesNotChange()
        {
            // Arrange
            string resourceId = "wood";
            resourceManager.AddResource(resourceId, 50);

            // Act
            resourceManager.AddResource(resourceId, 0);

            // Assert
            Assert.AreEqual(50, resourceManager.GetResourceCount(resourceId), 
                "Resource count should remain 50 when adding 0");
        }

        [Test]
        public void AddResource_WithNegativeAmount_DoesNotChange()
        {
            // Arrange
            string resourceId = "wood";
            resourceManager.AddResource(resourceId, 50);

            // Act
            resourceManager.AddResource(resourceId, -10);

            // Assert
            Assert.AreEqual(50, resourceManager.GetResourceCount(resourceId), 
                "Resource count should remain 50 when adding negative amount");
        }

        #endregion

        #region TryConsumeResources Tests

        [Test]
        public void TryConsumeResources_WithSufficientResources_ReturnsTrue()
        {
            // Arrange
            resourceManager.AddResource("wood", 100);
            var resources = new ResourceStack[]
            {
                new ResourceStack("wood", 50)
            };

            // Act
            bool result = resourceManager.TryConsumeResources(resources);

            // Assert
            Assert.IsTrue(result, "Should return true when sufficient resources exist");
            Assert.AreEqual(50, resourceManager.GetResourceCount("wood"), 
                "Should have 50 wood remaining after consuming 50");
        }

        [Test]
        public void TryConsumeResources_WithInsufficientResources_ReturnsFalse()
        {
            // Arrange
            resourceManager.AddResource("wood", 30);
            var resources = new ResourceStack[]
            {
                new ResourceStack("wood", 50)
            };

            // Act
            bool result = resourceManager.TryConsumeResources(resources);

            // Assert
            Assert.IsFalse(result, "Should return false when insufficient resources exist");
            Assert.AreEqual(30, resourceManager.GetResourceCount("wood"), 
                "Resource count should remain unchanged when consumption fails");
        }

        [Test]
        public void TryConsumeResources_WithMultipleResources_AllOrNothing()
        {
            // Arrange
            resourceManager.AddResource("wood", 100);
            resourceManager.AddResource("stone", 50);
            var resources = new ResourceStack[]
            {
                new ResourceStack("wood", 50),
                new ResourceStack("stone", 75) // Not enough stone
            };

            // Act
            bool result = resourceManager.TryConsumeResources(resources);

            // Assert
            Assert.IsFalse(result, "Should return false when any resource is insufficient");
            Assert.AreEqual(100, resourceManager.GetResourceCount("wood"), 
                "Wood should remain unchanged when consumption fails");
            Assert.AreEqual(50, resourceManager.GetResourceCount("stone"), 
                "Stone should remain unchanged when consumption fails");
        }

        [Test]
        public void TryConsumeResources_WithExactAmount_ReturnsTrue()
        {
            // Arrange
            resourceManager.AddResource("iron", 100);
            var resources = new ResourceStack[]
            {
                new ResourceStack("iron", 100)
            };

            // Act
            bool result = resourceManager.TryConsumeResources(resources);

            // Assert
            Assert.IsTrue(result, "Should return true when consuming exact amount");
            Assert.AreEqual(0, resourceManager.GetResourceCount("iron"), 
                "Should have 0 iron remaining after consuming all");
        }

        [Test]
        public void TryConsumeResources_WithEmptyArray_ReturnsTrue()
        {
            // Arrange
            var resources = new ResourceStack[] { };

            // Act
            bool result = resourceManager.TryConsumeResources(resources);

            // Assert
            Assert.IsTrue(result, "Should return true when consuming empty array");
        }

        [Test]
        public void TryConsumeResources_WithNull_ReturnsTrue()
        {
            // Act
            bool result = resourceManager.TryConsumeResources(null);

            // Assert
            Assert.IsTrue(result, "Should return true when consuming null");
        }

        #endregion

        #region HasResource Tests

        [Test]
        public void HasResource_WithSufficientAmount_ReturnsTrue()
        {
            // Arrange
            resourceManager.AddResource("wood", 100);

            // Act
            bool result = resourceManager.HasResource("wood", 50);

            // Assert
            Assert.IsTrue(result, "Should return true when sufficient resources exist");
        }

        [Test]
        public void HasResource_WithInsufficientAmount_ReturnsFalse()
        {
            // Arrange
            resourceManager.AddResource("wood", 30);

            // Act
            bool result = resourceManager.HasResource("wood", 50);

            // Assert
            Assert.IsFalse(result, "Should return false when insufficient resources exist");
        }

        [Test]
        public void HasResource_WithExactAmount_ReturnsTrue()
        {
            // Arrange
            resourceManager.AddResource("stone", 100);

            // Act
            bool result = resourceManager.HasResource("stone", 100);

            // Assert
            Assert.IsTrue(result, "Should return true when exact amount exists");
        }

        [Test]
        public void HasResource_WithNonexistentResource_ReturnsFalse()
        {
            // Act
            bool result = resourceManager.HasResource("nonexistent", 1);

            // Assert
            Assert.IsFalse(result, "Should return false for nonexistent resource");
        }

        [Test]
        public void HasResource_WithZeroAmount_ReturnsTrue()
        {
            // Arrange
            resourceManager.AddResource("wood", 50);

            // Act
            bool result = resourceManager.HasResource("wood", 0);

            // Assert
            Assert.IsTrue(result, "Should return true when checking for 0 amount");
        }

        #endregion

        #region GetResourceCount Tests

        [Test]
        public void GetResourceCount_ReturnsCorrectValue()
        {
            // Arrange
            resourceManager.AddResource("wood", 150);

            // Act
            long count = resourceManager.GetResourceCount("wood");

            // Assert
            Assert.AreEqual(150, count, "Should return correct resource count");
        }

        [Test]
        public void GetResourceCount_ForNonexistentResource_ReturnsZero()
        {
            // Act
            long count = resourceManager.GetResourceCount("nonexistent");

            // Assert
            Assert.AreEqual(0, count, "Should return 0 for nonexistent resource");
        }

        #endregion

        #region SetResourceCount Tests

        [Test]
        public void SetResourceCount_SetsCorrectValue()
        {
            // Act
            resourceManager.SetResourceCount("wood", 500);

            // Assert
            Assert.AreEqual(500, resourceManager.GetResourceCount("wood"), 
                "Should set resource count to 500");
        }

        [Test]
        public void SetResourceCount_OverwritesPreviousValue()
        {
            // Arrange
            resourceManager.AddResource("iron", 100);

            // Act
            resourceManager.SetResourceCount("iron", 250);

            // Assert
            Assert.AreEqual(250, resourceManager.GetResourceCount("iron"), 
                "Should overwrite previous value");
        }

        #endregion

        #region Query Methods Tests

        [Test]
        public void GetResourceTypeCount_ReturnsCorrectCount()
        {
            // Arrange
            resourceManager.AddResource("wood", 100);
            resourceManager.AddResource("stone", 50);
            resourceManager.AddResource("iron", 75);

            // Act
            int count = resourceManager.GetResourceTypeCount();

            // Assert
            Assert.AreEqual(3, count, "Should return 3 resource types");
        }

        [Test]
        public void GetAllResourceCounts_ReturnsCopy()
        {
            // Arrange
            resourceManager.AddResource("wood", 100);
            resourceManager.AddResource("stone", 50);

            // Act
            var counts = resourceManager.GetAllResourceCounts();

            // Assert
            Assert.AreEqual(2, counts.Count, "Should return dictionary with 2 entries");
            Assert.AreEqual(100, counts["wood"], "Wood count should be 100");
            Assert.AreEqual(50, counts["stone"], "Stone count should be 50");
        }

        #endregion

        #region Save/Load Tests

        [Test]
        public void ClearAllResources_RemovesAllResources()
        {
            // Arrange
            resourceManager.AddResource("wood", 100);
            resourceManager.AddResource("stone", 50);

            // Act
            resourceManager.ClearAllResources();

            // Assert
            Assert.AreEqual(0, resourceManager.GetResourceTypeCount(), 
                "Should have 0 resource types after clearing");
        }

        [Test]
        public void LoadResourceCounts_LoadsCorrectly()
        {
            // Arrange
            var resourceCounts = new System.Collections.Generic.Dictionary<string, long>
            {
                { "wood", 200 },
                { "stone", 150 },
                { "iron", 100 }
            };

            // Act
            resourceManager.LoadResourceCounts(resourceCounts);

            // Assert
            Assert.AreEqual(200, resourceManager.GetResourceCount("wood"), 
                "Wood should be loaded correctly");
            Assert.AreEqual(150, resourceManager.GetResourceCount("stone"), 
                "Stone should be loaded correctly");
            Assert.AreEqual(100, resourceManager.GetResourceCount("iron"), 
                "Iron should be loaded correctly");
        }

        #endregion
    }
}
