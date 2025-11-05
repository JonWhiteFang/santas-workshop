using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Machines;
using System.Collections.Generic;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for InputPort and OutputPort classes.
    /// Tests buffer management, capacity limits, and save/load functionality.
    /// </summary>
    public class PortTests
    {
        #region InputPort Tests

        [Test]
        public void InputPort_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var port = new InputPort("test_port", new Vector3(1, 2, 3), 100);

            // Assert
            Assert.AreEqual("test_port", port.portId);
            Assert.AreEqual(new Vector3(1, 2, 3), port.localPosition);
            Assert.AreEqual(100, port.capacity);
            Assert.AreEqual(0, port.GetTotalAmount());
        }

        [Test]
        public void InputPort_AddResource_ValidAmount_ReturnsTrue()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("wood", 5);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(5, port.GetResourceAmount("wood"));
        }

        [Test]
        public void InputPort_AddResource_MultipleTypes_StoresCorrectly()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 20);

            // Act
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);
            port.AddResource("wood", 2);

            // Assert
            Assert.AreEqual(7, port.GetResourceAmount("wood"));
            Assert.AreEqual(3, port.GetResourceAmount("iron"));
            Assert.AreEqual(10, port.GetTotalAmount());
        }

        [Test]
        public void InputPort_AddResource_ExceedsCapacity_ReturnsFalse()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 8);

            // Act
            bool result = port.AddResource("wood", 5);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(8, port.GetResourceAmount("wood"));
        }

        [Test]
        public void InputPort_AddResource_ZeroAmount_ReturnsFalse()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("wood", 0);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
        }

        [Test]
        public void InputPort_AddResource_NegativeAmount_ReturnsFalse()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("wood", -5);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
        }

        [Test]
        public void InputPort_AddResource_EmptyResourceId_ReturnsFalse()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("", 5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void InputPort_AddResource_NullResourceId_ReturnsFalse()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource(null, 5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void InputPort_CanAcceptResource_WithSpace_ReturnsTrue()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            bool result = port.CanAcceptResource("iron", 3);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void InputPort_CanAcceptResource_WithoutSpace_ReturnsFalse()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 8);

            // Act
            bool result = port.CanAcceptResource("iron", 5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void InputPort_CanAcceptResource_ExactCapacity_ReturnsTrue()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 7);

            // Act
            bool result = port.CanAcceptResource("iron", 3);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void InputPort_RemoveResource_PartialAmount_ReturnsCorrectAmount()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int removed = port.RemoveResource("wood", 3);

            // Assert
            Assert.AreEqual(3, removed);
            Assert.AreEqual(2, port.GetResourceAmount("wood"));
        }

        [Test]
        public void InputPort_RemoveResource_MoreThanAvailable_ReturnsAvailableAmount()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int removed = port.RemoveResource("wood", 10);

            // Assert
            Assert.AreEqual(5, removed);
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
        }

        [Test]
        public void InputPort_RemoveResource_AllAmount_RemovesEntry()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int removed = port.RemoveResource("wood", 5);

            // Assert
            Assert.AreEqual(5, removed);
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
            Assert.AreEqual(0, port.GetTotalAmount());
        }

        [Test]
        public void InputPort_RemoveResource_NonExistentResource_ReturnsZero()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);

            // Act
            int removed = port.RemoveResource("wood", 5);

            // Assert
            Assert.AreEqual(0, removed);
        }

        [Test]
        public void InputPort_RemoveResource_ZeroAmount_ReturnsZero()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int removed = port.RemoveResource("wood", 0);

            // Assert
            Assert.AreEqual(0, removed);
            Assert.AreEqual(5, port.GetResourceAmount("wood"));
        }

        [Test]
        public void InputPort_GetTotalAmount_MultipleResources_ReturnsSum()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);
            port.AddResource("coal", 2);

            // Act
            int total = port.GetTotalAmount();

            // Assert
            Assert.AreEqual(10, total);
        }

        [Test]
        public void InputPort_GetAvailableSpace_ReturnsCorrectValue()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 3);

            // Act
            int available = port.GetAvailableSpace();

            // Assert
            Assert.AreEqual(7, available);
        }

        [Test]
        public void InputPort_GetAllResources_ReturnsReadOnlyDictionary()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);

            // Act
            var resources = port.GetAllResources();

            // Assert
            Assert.AreEqual(2, resources.Count);
            Assert.AreEqual(5, resources["wood"]);
            Assert.AreEqual(3, resources["iron"]);
        }

        [Test]
        public void InputPort_Clear_RemovesAllResources()
        {
            // Arrange
            var port = new InputPort("test", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);

            // Act
            port.Clear();

            // Assert
            Assert.AreEqual(0, port.GetTotalAmount());
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
            Assert.AreEqual(0, port.GetResourceAmount("iron"));
        }

        [Test]
        public void InputPort_GetSaveData_ReturnsCorrectData()
        {
            // Arrange
            var port = new InputPort("test_port", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);

            // Act
            var saveData = port.GetSaveData();

            // Assert
            Assert.AreEqual("test_port", saveData.portId);
            Assert.AreEqual(2, saveData.contents.Count);
            Assert.AreEqual(5, saveData.contents["wood"]);
            Assert.AreEqual(3, saveData.contents["iron"]);
        }

        [Test]
        public void InputPort_LoadSaveData_RestoresState()
        {
            // Arrange
            var port = new InputPort("test_port", Vector3.zero, 20);
            var saveData = new BufferSaveData
            {
                portId = "test_port",
                contents = new Dictionary<string, int>
                {
                    { "wood", 7 },
                    { "iron", 4 }
                }
            };

            // Act
            port.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(7, port.GetResourceAmount("wood"));
            Assert.AreEqual(4, port.GetResourceAmount("iron"));
            Assert.AreEqual(11, port.GetTotalAmount());
        }

        [Test]
        public void InputPort_SaveAndLoad_PreservesState()
        {
            // Arrange
            var port1 = new InputPort("test", Vector3.zero, 20);
            port1.AddResource("wood", 5);
            port1.AddResource("iron", 3);

            // Act
            var saveData = port1.GetSaveData();
            var port2 = new InputPort("test", Vector3.zero, 20);
            port2.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(port1.GetResourceAmount("wood"), port2.GetResourceAmount("wood"));
            Assert.AreEqual(port1.GetResourceAmount("iron"), port2.GetResourceAmount("iron"));
            Assert.AreEqual(port1.GetTotalAmount(), port2.GetTotalAmount());
        }

        #endregion

        #region OutputPort Tests

        [Test]
        public void OutputPort_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var port = new OutputPort("test_port", new Vector3(1, 2, 3), 100);

            // Assert
            Assert.AreEqual("test_port", port.portId);
            Assert.AreEqual(new Vector3(1, 2, 3), port.localPosition);
            Assert.AreEqual(100, port.capacity);
            Assert.AreEqual(0, port.GetTotalAmount());
        }

        [Test]
        public void OutputPort_AddResource_ValidAmount_ReturnsTrue()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("wood", 5);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(5, port.GetResourceAmount("wood"));
        }

        [Test]
        public void OutputPort_AddResource_MultipleTypes_StoresCorrectly()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 20);

            // Act
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);
            port.AddResource("wood", 2);

            // Assert
            Assert.AreEqual(7, port.GetResourceAmount("wood"));
            Assert.AreEqual(3, port.GetResourceAmount("iron"));
            Assert.AreEqual(10, port.GetTotalAmount());
        }

        [Test]
        public void OutputPort_AddResource_ExceedsCapacity_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 8);

            // Act
            bool result = port.AddResource("wood", 5);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(8, port.GetResourceAmount("wood"));
        }

        [Test]
        public void OutputPort_AddResource_ZeroAmount_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("wood", 0);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void OutputPort_AddResource_NegativeAmount_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("wood", -5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void OutputPort_AddResource_EmptyResourceId_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource("", 5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void OutputPort_AddResource_NullResourceId_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);

            // Act
            bool result = port.AddResource(null, 5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void OutputPort_CanAddResource_WithSpace_ReturnsTrue()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            bool result = port.CanAddResource("iron", 3);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void OutputPort_CanAddResource_WithoutSpace_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 8);

            // Act
            bool result = port.CanAddResource("iron", 5);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void OutputPort_ExtractResource_PartialAmount_ReturnsCorrectAmount()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int extracted = port.ExtractResource("wood", 3);

            // Assert
            Assert.AreEqual(3, extracted);
            Assert.AreEqual(2, port.GetResourceAmount("wood"));
        }

        [Test]
        public void OutputPort_ExtractResource_MoreThanAvailable_ReturnsAvailableAmount()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int extracted = port.ExtractResource("wood", 10);

            // Assert
            Assert.AreEqual(5, extracted);
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
        }

        [Test]
        public void OutputPort_ExtractResource_AllAmount_RemovesEntry()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int extracted = port.ExtractResource("wood", 5);

            // Assert
            Assert.AreEqual(5, extracted);
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
            Assert.AreEqual(0, port.GetTotalAmount());
        }

        [Test]
        public void OutputPort_ExtractResource_NonExistentResource_ReturnsZero()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);

            // Act
            int extracted = port.ExtractResource("wood", 5);

            // Assert
            Assert.AreEqual(0, extracted);
        }

        [Test]
        public void OutputPort_ExtractResource_ZeroAmount_ReturnsZero()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            int extracted = port.ExtractResource("wood", 0);

            // Assert
            Assert.AreEqual(0, extracted);
            Assert.AreEqual(5, port.GetResourceAmount("wood"));
        }

        [Test]
        public void OutputPort_HasResources_WithResources_ReturnsTrue()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);

            // Act
            bool hasResources = port.HasResources();

            // Assert
            Assert.IsTrue(hasResources);
        }

        [Test]
        public void OutputPort_HasResources_Empty_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);

            // Act
            bool hasResources = port.HasResources();

            // Assert
            Assert.IsFalse(hasResources);
        }

        [Test]
        public void OutputPort_HasResources_AfterExtractingAll_ReturnsFalse()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 5);
            port.ExtractResource("wood", 5);

            // Act
            bool hasResources = port.HasResources();

            // Assert
            Assert.IsFalse(hasResources);
        }

        [Test]
        public void OutputPort_GetTotalAmount_MultipleResources_ReturnsSum()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);
            port.AddResource("coal", 2);

            // Act
            int total = port.GetTotalAmount();

            // Assert
            Assert.AreEqual(10, total);
        }

        [Test]
        public void OutputPort_GetAvailableSpace_ReturnsCorrectValue()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 10);
            port.AddResource("wood", 3);

            // Act
            int available = port.GetAvailableSpace();

            // Assert
            Assert.AreEqual(7, available);
        }

        [Test]
        public void OutputPort_GetAllResources_ReturnsReadOnlyDictionary()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);

            // Act
            var resources = port.GetAllResources();

            // Assert
            Assert.AreEqual(2, resources.Count);
            Assert.AreEqual(5, resources["wood"]);
            Assert.AreEqual(3, resources["iron"]);
        }

        [Test]
        public void OutputPort_Clear_RemovesAllResources()
        {
            // Arrange
            var port = new OutputPort("test", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);

            // Act
            port.Clear();

            // Assert
            Assert.AreEqual(0, port.GetTotalAmount());
            Assert.AreEqual(0, port.GetResourceAmount("wood"));
            Assert.AreEqual(0, port.GetResourceAmount("iron"));
            Assert.IsFalse(port.HasResources());
        }

        [Test]
        public void OutputPort_GetSaveData_ReturnsCorrectData()
        {
            // Arrange
            var port = new OutputPort("test_port", Vector3.zero, 20);
            port.AddResource("wood", 5);
            port.AddResource("iron", 3);

            // Act
            var saveData = port.GetSaveData();

            // Assert
            Assert.AreEqual("test_port", saveData.portId);
            Assert.AreEqual(2, saveData.contents.Count);
            Assert.AreEqual(5, saveData.contents["wood"]);
            Assert.AreEqual(3, saveData.contents["iron"]);
        }

        [Test]
        public void OutputPort_LoadSaveData_RestoresState()
        {
            // Arrange
            var port = new OutputPort("test_port", Vector3.zero, 20);
            var saveData = new BufferSaveData
            {
                portId = "test_port",
                contents = new Dictionary<string, int>
                {
                    { "wood", 7 },
                    { "iron", 4 }
                }
            };

            // Act
            port.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(7, port.GetResourceAmount("wood"));
            Assert.AreEqual(4, port.GetResourceAmount("iron"));
            Assert.AreEqual(11, port.GetTotalAmount());
            Assert.IsTrue(port.HasResources());
        }

        [Test]
        public void OutputPort_SaveAndLoad_PreservesState()
        {
            // Arrange
            var port1 = new OutputPort("test", Vector3.zero, 20);
            port1.AddResource("wood", 5);
            port1.AddResource("iron", 3);

            // Act
            var saveData = port1.GetSaveData();
            var port2 = new OutputPort("test", Vector3.zero, 20);
            port2.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(port1.GetResourceAmount("wood"), port2.GetResourceAmount("wood"));
            Assert.AreEqual(port1.GetResourceAmount("iron"), port2.GetResourceAmount("iron"));
            Assert.AreEqual(port1.GetTotalAmount(), port2.GetTotalAmount());
            Assert.AreEqual(port1.HasResources(), port2.HasResources());
        }

        #endregion
    }
}
