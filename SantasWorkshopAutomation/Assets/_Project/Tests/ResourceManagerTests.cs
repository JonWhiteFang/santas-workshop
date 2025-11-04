using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Core;
using SantasWorkshop.Data;
using System.Collections.Generic;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for the ResourceManager class.
    /// Tests core functionality including adding, consuming, querying, and saving/loading resources.
    /// </summary>
    [TestFixture]
    public class ResourceManagerTests
    {
        private GameObject _managerObject;
        private ResourceManager _resourceManager;
        private ResourceData _testWood;
        private ResourceData _testIron;
        private ResourceData _testCoal;

        /// <summary>
        /// Setup method called before each test.
        /// Creates a ResourceManager instance and test resource data.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Create GameObject with ResourceManager component
            _managerObject = new GameObject("TestResourceManager");
            _resourceManager = _managerObject.AddComponent<ResourceManager>();

            // Create test resources programmatically
            _testWood = ScriptableObject.CreateInstance<ResourceData>();
            _testWood.resourceId = "test_wood";
            _testWood.displayName = "Test Wood";
            _testWood.stackSize = 100;
            _testWood.baseValue = 10;

            _testIron = ScriptableObject.CreateInstance<ResourceData>();
            _testIron.resourceId = "test_iron";
            _testIron.displayName = "Test Iron";
            _testIron.stackSize = 100;
            _testIron.baseValue = 20;

            _testCoal = ScriptableObject.CreateInstance<ResourceData>();
            _testCoal.resourceId = "test_coal";
            _testCoal.displayName = "Test Coal";
            _testCoal.stackSize = 50;
            _testCoal.baseValue = 15;

            // Manually register test resources in the manager
            // Since we can't use Resources.Load in tests, we'll use reflection to access private fields
            var databaseField = typeof(ResourceManager).GetField("_resourceDatabase", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var countsField = typeof(ResourceManager).GetField("_globalResourceCounts", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var capacitiesField = typeof(ResourceManager).GetField("_resourceCapacities", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var initializedField = typeof(ResourceManager).GetField("_isInitialized", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var database = new Dictionary<string, ResourceData>();
            database[_testWood.resourceId] = _testWood;
            database[_testIron.resourceId] = _testIron;
            database[_testCoal.resourceId] = _testCoal;

            var counts = new Dictionary<string, long>();
            counts[_testWood.resourceId] = 0;
            counts[_testIron.resourceId] = 0;
            counts[_testCoal.resourceId] = 0;

            var capacities = new Dictionary<string, long>();

            databaseField.SetValue(_resourceManager, database);
            countsField.SetValue(_resourceManager, counts);
            capacitiesField.SetValue(_resourceManager, capacities);
            initializedField.SetValue(_resourceManager, true);
        }

        /// <summary>
        /// Teardown method called after each test.
        /// Cleans up test objects.
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            // Destroy test resources
            Object.DestroyImmediate(_testWood);
            Object.DestroyImmediate(_testIron);
            Object.DestroyImmediate(_testCoal);

            // Destroy manager object
            Object.DestroyImmediate(_managerObject);
        }

        #region AddResource Tests

        /// <summary>
        /// Test that AddResource increases the resource count correctly.
        /// </summary>
        [Test]
        public void AddResource_IncreasesCount()
        {
            // Arrange
            string resourceId = "test_wood";
            int amountToAdd = 50;

            // Act
            bool result = _resourceManager.AddResource(resourceId, amountToAdd);

            // Assert
            Assert.IsTrue(result, "AddResource should return true");
            Assert.AreEqual(50, _resourceManager.GetResourceCount(resourceId), 
                "Resource count should be 50 after adding 50");
        }

        /// <summary>
        /// Test that AddResource can be called multiple times and accumulates correctly.
        /// </summary>
        [Test]
        public void AddResource_MultipleTimes_Accumulates()
        {
            // Arrange
            string resourceId = "test_wood";

            // Act
            _resourceManager.AddResource(resourceId, 30);
            _resourceManager.AddResource(resourceId, 20);
            _resourceManager.AddResource(resourceId, 10);

            // Assert
            Assert.AreEqual(60, _resourceManager.GetResourceCount(resourceId), 
                "Resource count should accumulate to 60");
        }

        /// <summary>
        /// Test that AddResource returns false for invalid resource ID.
        /// </summary>
        [Test]
        public void AddResource_InvalidResourceId_ReturnsFalse()
        {
            // Act
            bool result = _resourceManager.AddResource("invalid_resource", 10);

            // Assert
            Assert.IsFalse(result, "AddResource should return false for invalid resource ID");
        }

        /// <summary>
        /// Test that AddResource returns false for negative amount.
        /// </summary>
        [Test]
        public void AddResource_NegativeAmount_ReturnsFalse()
        {
            // Act
            bool result = _resourceManager.AddResource("test_wood", -10);

            // Assert
            Assert.IsFalse(result, "AddResource should return false for negative amount");
        }

        #endregion

        #region TryConsumeResource Tests

        /// <summary>
        /// Test that TryConsumeResource with sufficient resources returns true and decreases count.
        /// </summary>
        [Test]
        public void TryConsumeResource_WithSufficientResources_ReturnsTrue()
        {
            // Arrange
            string resourceId = "test_iron";
            _resourceManager.AddResource(resourceId, 100);

            // Act
            bool result = _resourceManager.TryConsumeResource(resourceId, 50);

            // Assert
            Assert.IsTrue(result, "TryConsumeResource should return true when resources are sufficient");
            Assert.AreEqual(50, _resourceManager.GetResourceCount(resourceId), 
                "Resource count should be 50 after consuming 50 from 100");
        }

        /// <summary>
        /// Test that TryConsumeResource with insufficient resources returns false and doesn't change count.
        /// </summary>
        [Test]
        public void TryConsumeResource_WithInsufficientResources_ReturnsFalse()
        {
            // Arrange
            string resourceId = "test_iron";
            _resourceManager.AddResource(resourceId, 30);

            // Act
            bool result = _resourceManager.TryConsumeResource(resourceId, 50);

            // Assert
            Assert.IsFalse(result, "TryConsumeResource should return false when resources are insufficient");
            Assert.AreEqual(30, _resourceManager.GetResourceCount(resourceId), 
                "Resource count should remain unchanged when consumption fails");
        }

        /// <summary>
        /// Test that TryConsumeResource can consume exact amount available.
        /// </summary>
        [Test]
        public void TryConsumeResource_ExactAmount_ReturnsTrue()
        {
            // Arrange
            string resourceId = "test_coal";
            _resourceManager.AddResource(resourceId, 25);

            // Act
            bool result = _resourceManager.TryConsumeResource(resourceId, 25);

            // Assert
            Assert.IsTrue(result, "TryConsumeResource should return true when consuming exact amount");
            Assert.AreEqual(0, _resourceManager.GetResourceCount(resourceId), 
                "Resource count should be 0 after consuming all");
        }

        #endregion

        #region TryConsumeResources Tests

        /// <summary>
        /// Test that TryConsumeResources with atomic behavior consumes all or nothing.
        /// </summary>
        [Test]
        public void TryConsumeResources_AtomicBehavior_AllOrNothing()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 50);
            _resourceManager.AddResource("test_iron", 10);
            _resourceManager.AddResource("test_coal", 5);

            ResourceStack[] resources = new ResourceStack[]
            {
                new ResourceStack("test_wood", 30),
                new ResourceStack("test_iron", 5),
                new ResourceStack("test_coal", 10) // Insufficient - only have 5
            };

            // Act
            bool result = _resourceManager.TryConsumeResources(resources);

            // Assert
            Assert.IsFalse(result, "TryConsumeResources should return false when any resource is insufficient");
            Assert.AreEqual(50, _resourceManager.GetResourceCount("test_wood"), 
                "Wood count should remain unchanged (atomic behavior)");
            Assert.AreEqual(10, _resourceManager.GetResourceCount("test_iron"), 
                "Iron count should remain unchanged (atomic behavior)");
            Assert.AreEqual(5, _resourceManager.GetResourceCount("test_coal"), 
                "Coal count should remain unchanged (atomic behavior)");
        }

        /// <summary>
        /// Test that TryConsumeResources succeeds when all resources are sufficient.
        /// </summary>
        [Test]
        public void TryConsumeResources_AllSufficient_ReturnsTrue()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 50);
            _resourceManager.AddResource("test_iron", 30);
            _resourceManager.AddResource("test_coal", 20);

            ResourceStack[] resources = new ResourceStack[]
            {
                new ResourceStack("test_wood", 20),
                new ResourceStack("test_iron", 10),
                new ResourceStack("test_coal", 5)
            };

            // Act
            bool result = _resourceManager.TryConsumeResources(resources);

            // Assert
            Assert.IsTrue(result, "TryConsumeResources should return true when all resources are sufficient");
            Assert.AreEqual(30, _resourceManager.GetResourceCount("test_wood"), 
                "Wood count should be reduced by 20");
            Assert.AreEqual(20, _resourceManager.GetResourceCount("test_iron"), 
                "Iron count should be reduced by 10");
            Assert.AreEqual(15, _resourceManager.GetResourceCount("test_coal"), 
                "Coal count should be reduced by 5");
        }

        #endregion

        #region SetResourceCapacity Tests

        /// <summary>
        /// Test that SetResourceCapacity limits additions correctly.
        /// </summary>
        [Test]
        public void SetResourceCapacity_LimitsAdditions()
        {
            // Arrange
            string resourceId = "test_wood";
            _resourceManager.SetResourceCapacity(resourceId, 100);

            // Act
            _resourceManager.AddResource(resourceId, 150);

            // Assert
            Assert.AreEqual(100, _resourceManager.GetResourceCount(resourceId), 
                "Resource count should be capped at capacity limit");
        }

        /// <summary>
        /// Test that capacity of 0 means unlimited.
        /// </summary>
        [Test]
        public void SetResourceCapacity_Zero_MeansUnlimited()
        {
            // Arrange
            string resourceId = "test_iron";
            _resourceManager.SetResourceCapacity(resourceId, 0); // 0 = unlimited

            // Act
            _resourceManager.AddResource(resourceId, 1000);

            // Assert
            Assert.AreEqual(1000, _resourceManager.GetResourceCount(resourceId), 
                "Resource count should not be limited when capacity is 0");
        }

        /// <summary>
        /// Test that GetResourceCapacity returns the correct capacity.
        /// </summary>
        [Test]
        public void GetResourceCapacity_ReturnsCorrectValue()
        {
            // Arrange
            string resourceId = "test_coal";
            _resourceManager.SetResourceCapacity(resourceId, 250);

            // Act
            long capacity = _resourceManager.GetResourceCapacity(resourceId);

            // Assert
            Assert.AreEqual(250, capacity, "GetResourceCapacity should return the set capacity");
        }

        #endregion

        #region OnResourceChanged Event Tests

        /// <summary>
        /// Test that OnResourceChanged event fires correctly when adding resources.
        /// </summary>
        [Test]
        public void OnResourceChanged_FiresWhenResourceAdded()
        {
            // Arrange
            string firedResourceId = null;
            long firedAmount = 0;
            bool eventFired = false;

            _resourceManager.OnResourceChanged += (id, amount) =>
            {
                firedResourceId = id;
                firedAmount = amount;
                eventFired = true;
            };

            // Act
            _resourceManager.AddResource("test_wood", 50);

            // Assert
            Assert.IsTrue(eventFired, "OnResourceChanged event should fire");
            Assert.AreEqual("test_wood", firedResourceId, "Event should fire with correct resource ID");
            Assert.AreEqual(50, firedAmount, "Event should fire with correct amount");
        }

        /// <summary>
        /// Test that OnResourceChanged event fires correctly when consuming resources.
        /// </summary>
        [Test]
        public void OnResourceChanged_FiresWhenResourceConsumed()
        {
            // Arrange
            _resourceManager.AddResource("test_iron", 100);
            
            string firedResourceId = null;
            long firedAmount = 0;
            bool eventFired = false;

            _resourceManager.OnResourceChanged += (id, amount) =>
            {
                firedResourceId = id;
                firedAmount = amount;
                eventFired = true;
            };

            // Act
            _resourceManager.TryConsumeResource("test_iron", 30);

            // Assert
            Assert.IsTrue(eventFired, "OnResourceChanged event should fire");
            Assert.AreEqual("test_iron", firedResourceId, "Event should fire with correct resource ID");
            Assert.AreEqual(70, firedAmount, "Event should fire with correct remaining amount");
        }

        /// <summary>
        /// Test that OnResourceChanged event does not fire when consumption fails.
        /// </summary>
        [Test]
        public void OnResourceChanged_DoesNotFireWhenConsumptionFails()
        {
            // Arrange
            _resourceManager.AddResource("test_coal", 10);
            
            bool eventFired = false;
            _resourceManager.OnResourceChanged += (id, amount) => { eventFired = true; };

            // Act
            _resourceManager.TryConsumeResource("test_coal", 20); // Insufficient

            // Assert
            Assert.IsFalse(eventFired, "OnResourceChanged event should not fire when consumption fails");
        }

        #endregion

        #region HasResource Tests

        /// <summary>
        /// Test that HasResource validates correctly for sufficient resources.
        /// </summary>
        [Test]
        public void HasResource_WithSufficientAmount_ReturnsTrue()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 100);

            // Act
            bool result = _resourceManager.HasResource("test_wood", 50);

            // Assert
            Assert.IsTrue(result, "HasResource should return true when amount is sufficient");
        }

        /// <summary>
        /// Test that HasResource validates correctly for insufficient resources.
        /// </summary>
        [Test]
        public void HasResource_WithInsufficientAmount_ReturnsFalse()
        {
            // Arrange
            _resourceManager.AddResource("test_iron", 30);

            // Act
            bool result = _resourceManager.HasResource("test_iron", 50);

            // Assert
            Assert.IsFalse(result, "HasResource should return false when amount is insufficient");
        }

        /// <summary>
        /// Test that HasResource returns false for invalid resource ID.
        /// </summary>
        [Test]
        public void HasResource_InvalidResourceId_ReturnsFalse()
        {
            // Act
            bool result = _resourceManager.HasResource("invalid_resource", 10);

            // Assert
            Assert.IsFalse(result, "HasResource should return false for invalid resource ID");
        }

        /// <summary>
        /// Test that HasResource returns false for negative amount.
        /// </summary>
        [Test]
        public void HasResource_NegativeAmount_ReturnsFalse()
        {
            // Act
            bool result = _resourceManager.HasResource("test_wood", -10);

            // Assert
            Assert.IsFalse(result, "HasResource should return false for negative amount");
        }

        #endregion

        #region GetResourceData Tests

        /// <summary>
        /// Test that GetResourceData returns correct data for valid resource ID.
        /// </summary>
        [Test]
        public void GetResourceData_ValidResourceId_ReturnsCorrectData()
        {
            // Act
            ResourceData data = _resourceManager.GetResourceData("test_wood");

            // Assert
            Assert.IsNotNull(data, "GetResourceData should return non-null data");
            Assert.AreEqual("test_wood", data.resourceId, "Resource ID should match");
            Assert.AreEqual("Test Wood", data.displayName, "Display name should match");
        }

        /// <summary>
        /// Test that GetResourceData returns null for invalid resource ID.
        /// </summary>
        [Test]
        public void GetResourceData_InvalidResourceId_ReturnsNull()
        {
            // Act
            ResourceData data = _resourceManager.GetResourceData("invalid_resource");

            // Assert
            Assert.IsNull(data, "GetResourceData should return null for invalid resource ID");
        }

        /// <summary>
        /// Test that GetResourceData returns null for empty resource ID.
        /// </summary>
        [Test]
        public void GetResourceData_EmptyResourceId_ReturnsNull()
        {
            // Act
            ResourceData data = _resourceManager.GetResourceData("");

            // Assert
            Assert.IsNull(data, "GetResourceData should return null for empty resource ID");
        }

        #endregion

        #region ResetResources Tests

        /// <summary>
        /// Test that ResetResources clears all counts to zero.
        /// </summary>
        [Test]
        public void ResetResources_ClearsAllCounts()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 100);
            _resourceManager.AddResource("test_iron", 50);
            _resourceManager.AddResource("test_coal", 25);

            // Act
            _resourceManager.ResetResources();

            // Assert
            Assert.AreEqual(0, _resourceManager.GetResourceCount("test_wood"), 
                "Wood count should be 0 after reset");
            Assert.AreEqual(0, _resourceManager.GetResourceCount("test_iron"), 
                "Iron count should be 0 after reset");
            Assert.AreEqual(0, _resourceManager.GetResourceCount("test_coal"), 
                "Coal count should be 0 after reset");
        }

        /// <summary>
        /// Test that ResetResources fires OnResourceChanged events for reset resources.
        /// </summary>
        [Test]
        public void ResetResources_FiresEventsForResetResources()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 100);
            _resourceManager.AddResource("test_iron", 50);

            int eventCount = 0;
            _resourceManager.OnResourceChanged += (id, amount) =>
            {
                eventCount++;
                Assert.AreEqual(0, amount, "Reset event should have amount 0");
            };

            // Act
            _resourceManager.ResetResources();

            // Assert
            Assert.AreEqual(2, eventCount, "Should fire 2 events for 2 non-zero resources");
        }

        #endregion

        #region Save/Load Tests

        /// <summary>
        /// Test that save/load preserves resource counts correctly.
        /// </summary>
        [Test]
        public void SaveLoad_PreservesResourceCounts()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 150);
            _resourceManager.AddResource("test_iron", 75);
            _resourceManager.AddResource("test_coal", 30);

            // Act - Save
            ResourceManager.ResourceSaveData saveData = _resourceManager.GetSaveData();

            // Reset counts
            _resourceManager.ResetResources();
            Assert.AreEqual(0, _resourceManager.GetResourceCount("test_wood"), 
                "Wood should be 0 after reset");

            // Act - Load
            _resourceManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(150, _resourceManager.GetResourceCount("test_wood"), 
                "Wood count should be restored to 150");
            Assert.AreEqual(75, _resourceManager.GetResourceCount("test_iron"), 
                "Iron count should be restored to 75");
            Assert.AreEqual(30, _resourceManager.GetResourceCount("test_coal"), 
                "Coal count should be restored to 30");
        }

        /// <summary>
        /// Test that GetSaveData only includes non-zero resources.
        /// </summary>
        [Test]
        public void GetSaveData_OnlyIncludesNonZeroResources()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 100);
            _resourceManager.AddResource("test_iron", 0); // Zero - should not be saved
            _resourceManager.AddResource("test_coal", 50);

            // Act
            ResourceManager.ResourceSaveData saveData = _resourceManager.GetSaveData();

            // Assert
            Assert.AreEqual(2, saveData.resources.Length, 
                "Save data should only include 2 non-zero resources");
        }

        /// <summary>
        /// Test that LoadSaveData fires OnResourceChanged events for restored resources.
        /// </summary>
        [Test]
        public void LoadSaveData_FiresEventsForRestoredResources()
        {
            // Arrange
            _resourceManager.AddResource("test_wood", 100);
            _resourceManager.AddResource("test_iron", 50);
            ResourceManager.ResourceSaveData saveData = _resourceManager.GetSaveData();
            _resourceManager.ResetResources();

            int eventCount = 0;
            _resourceManager.OnResourceChanged += (id, amount) => { eventCount++; };

            // Act
            _resourceManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(2, eventCount, "Should fire 2 events for 2 restored resources");
        }

        /// <summary>
        /// Test that LoadSaveData handles unknown resource IDs gracefully.
        /// </summary>
        [Test]
        public void LoadSaveData_HandlesUnknownResourceIds()
        {
            // Arrange
            ResourceManager.ResourceSaveData saveData = new ResourceManager.ResourceSaveData
            {
                resources = new ResourceManager.ResourceEntry[]
                {
                    new ResourceManager.ResourceEntry { resourceId = "test_wood", amount = 100 },
                    new ResourceManager.ResourceEntry { resourceId = "unknown_resource", amount = 50 }
                }
            };

            // Act
            _resourceManager.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(100, _resourceManager.GetResourceCount("test_wood"), 
                "Known resource should be loaded");
            Assert.AreEqual(0, _resourceManager.GetResourceCount("unknown_resource"), 
                "Unknown resource should be ignored");
        }

        #endregion
    }
}
