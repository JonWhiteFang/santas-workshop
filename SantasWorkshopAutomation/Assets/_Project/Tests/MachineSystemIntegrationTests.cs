using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using SantasWorkshop.Machines;
using SantasWorkshop.Data;
using SantasWorkshop.Core;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Integration tests for Machine Framework with existing systems.
    /// Tests integration with ResourceManager, GridManager, and PlacementController.
    /// </summary>
    public class MachineSystemIntegrationTests
    {
        private GameObject gridManagerObject;
        private GridManager gridManager;
        private GameObject resourceManagerObject;
        private ResourceManager resourceManager;
        private GameObject placementControllerObject;
        private PlacementController placementController;
        private GameObject machineObject;
        private TestProcessor testMachine;
        private MachineData testMachineData;
        private Recipe testRecipe;

        [SetUp]
        public void Setup()
        {
            // Create GridManager
            gridManagerObject = new GameObject("GridManager");
            gridManager = gridManagerObject.AddComponent<GridManager>();
            gridManager.Initialize(50, 50, 1f);

            // Create ResourceManager
            resourceManagerObject = new GameObject("ResourceManager");
            resourceManager = resourceManagerObject.AddComponent<ResourceManager>();

            // Create PlacementController
            placementControllerObject = new GameObject("PlacementController");
            placementController = placementControllerObject.AddComponent<PlacementController>();

            // Create test recipe
            testRecipe = ScriptableObject.CreateInstance<Recipe>();
            testRecipe.recipeId = "test_recipe";
            testRecipe.recipeName = "Test Recipe";
            testRecipe.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "wood", amount = 2 }
            };
            testRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "planks", amount = 4 }
            };
            testRecipe.processingTime = 2f;
            testRecipe.powerConsumption = 10f;
            testRecipe.requiredTier = 1;

            // Create test machine data
            testMachineData = ScriptableObject.CreateInstance<MachineData>();
            testMachineData.machineName = "Test Processor";
            testMachineData.gridSize = new Vector2Int(2, 2);
            testMachineData.tier = 1;
            testMachineData.baseProcessingSpeed = 1f;
            testMachineData.basePowerConsumption = 10f;
            testMachineData.inputPortCount = 1;
            testMachineData.outputPortCount = 1;
            testMachineData.inputPortPositions = new Vector3[] { Vector3.left * 0.5f };
            testMachineData.outputPortPositions = new Vector3[] { Vector3.right * 0.5f };
            testMachineData.bufferCapacity = 10;
            testMachineData.availableRecipes = new List<Recipe> { testRecipe };

            // Create machine
            machineObject = new GameObject("TestProcessor");
            testMachine = machineObject.AddComponent<TestProcessor>();
            testMachine.SetMachineData(testMachineData);
        }

        [TearDown]
        public void Teardown()
        {
            if (machineObject != null)
                Object.DestroyImmediate(machineObject);
            if (gridManagerObject != null)
                Object.DestroyImmediate(gridManagerObject);
            if (resourceManagerObject != null)
                Object.DestroyImmediate(resourceManagerObject);
            if (placementControllerObject != null)
                Object.DestroyImmediate(placementControllerObject);
            if (testMachineData != null)
                Object.DestroyImmediate(testMachineData);
            if (testRecipe != null)
                Object.DestroyImmediate(testRecipe);
        }

        #region ResourceManager Integration Tests

        [Test]
        public void ResourceManager_Integration_MachineExists()
        {
            // Assert
            Assert.IsNotNull(resourceManager, "ResourceManager should exist");
            Assert.IsNotNull(ResourceManager.Instance, "ResourceManager singleton should be accessible");
        }

        [Test]
        public void ResourceManager_Integration_CanAddResources()
        {
            // Arrange
            string resourceId = "wood";
            int amount = 10;

            // Act
            resourceManager.AddResource(resourceId, amount);

            // Assert
            int storedAmount = resourceManager.GetResourceAmount(resourceId);
            Assert.AreEqual(amount, storedAmount, 
                "ResourceManager should store the added resources");
        }

        [Test]
        public void ResourceManager_Integration_CanConsumeResources()
        {
            // Arrange
            string resourceId = "wood";
            resourceManager.AddResource(resourceId, 10);

            // Act
            bool consumed = resourceManager.TryConsumeResource(resourceId, 5);

            // Assert
            Assert.IsTrue(consumed, "Should be able to consume resources");
            Assert.AreEqual(5, resourceManager.GetResourceAmount(resourceId), 
                "Remaining amount should be correct");
        }

        [Test]
        public void ResourceManager_Integration_MachineCanQueryResources()
        {
            // Arrange
            resourceManager.AddResource("wood", 10);

            // Act
            int amount = resourceManager.GetResourceAmount("wood");

            // Assert
            Assert.AreEqual(10, amount, 
                "Machine should be able to query resource amounts from ResourceManager");
        }

        #endregion

        #region GridManager Integration Tests

        [Test]
        public void GridManager_Integration_MachineExists()
        {
            // Assert
            Assert.IsNotNull(gridManager, "GridManager should exist");
            Assert.IsNotNull(GridManager.Instance, "GridManager singleton should be accessible");
        }

        [Test]
        public void GridManager_Integration_CanOccupyCells()
        {
            // Arrange
            Vector3Int position = new Vector3Int(5, 0, 5);
            Vector2Int size = new Vector2Int(2, 2);

            // Act
            bool canPlace = gridManager.CanPlaceAt(position, size);
            Assert.IsTrue(canPlace, "Should be able to place at empty position");

            gridManager.OccupyCells(position, size, machineObject);

            // Assert
            bool isOccupied = gridManager.IsCellOccupied(position);
            Assert.IsTrue(isOccupied, "Cell should be occupied after placement");
        }

        [Test]
        public void GridManager_Integration_MachineRegistersOccupiedCells()
        {
            // Arrange
            Vector3Int position = new Vector3Int(10, 0, 10);
            testMachine.SetGridPosition(position);

            // Act
            List<Vector3Int> occupiedCells = testMachine.GetOccupiedCells();

            // Assert
            Assert.AreEqual(4, occupiedCells.Count, "2x2 machine should occupy 4 cells");
            Assert.Contains(new Vector3Int(10, 0, 10), occupiedCells);
            Assert.Contains(new Vector3Int(11, 0, 10), occupiedCells);
            Assert.Contains(new Vector3Int(10, 0, 11), occupiedCells);
            Assert.Contains(new Vector3Int(11, 0, 11), occupiedCells);
        }

        [Test]
        public void GridManager_Integration_MachineFreesGridCellsOnDestroy()
        {
            // Arrange
            Vector3Int position = new Vector3Int(15, 0, 15);
            Vector2Int size = new Vector2Int(2, 2);
            
            gridManager.OccupyCells(position, size, machineObject);
            Assert.IsTrue(gridManager.IsCellOccupied(position), "Cell should be occupied");

            // Act
            gridManager.FreeCells(position, size);

            // Assert
            bool isOccupied = gridManager.IsCellOccupied(position);
            Assert.IsFalse(isOccupied, "Cell should be free after machine is destroyed");
        }

        [Test]
        public void GridManager_Integration_CanConvertGridToWorld()
        {
            // Arrange
            Vector3Int gridPos = new Vector3Int(5, 0, 10);

            // Act
            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            // Assert
            Assert.AreEqual(5.5f, worldPos.x, 0.01f, "World X should be grid X + 0.5");
            Assert.AreEqual(0f, worldPos.y, 0.01f, "World Y should be 0");
            Assert.AreEqual(10.5f, worldPos.z, 0.01f, "World Z should be grid Z + 0.5");
        }

        [Test]
        public void GridManager_Integration_CanConvertWorldToGrid()
        {
            // Arrange
            Vector3 worldPos = new Vector3(5.7f, 0f, 10.3f);

            // Act
            Vector3Int gridPos = gridManager.WorldToGrid(worldPos);

            // Assert
            Assert.AreEqual(new Vector3Int(5, 0, 10), gridPos, 
                "Should convert world position to grid position correctly");
        }

        #endregion

        #region PlacementController Integration Tests

        [Test]
        public void PlacementController_Integration_Exists()
        {
            // Assert
            Assert.IsNotNull(placementController, "PlacementController should exist");
            Assert.IsNotNull(PlacementController.Instance, 
                "PlacementController singleton should be accessible");
        }

        [Test]
        public void PlacementController_Integration_CanValidatePlacement()
        {
            // Arrange
            Vector3Int position = new Vector3Int(20, 0, 20);
            Vector2Int size = new Vector2Int(2, 2);

            // Act
            bool canPlace = placementController.CanPlaceAt(position, size);

            // Assert
            Assert.IsTrue(canPlace, 
                "PlacementController should validate placement at empty position");
        }

        [Test]
        public void PlacementController_Integration_PreventsDuplicatePlacement()
        {
            // Arrange
            Vector3Int position = new Vector3Int(25, 0, 25);
            Vector2Int size = new Vector2Int(2, 2);

            // Occupy cells
            gridManager.OccupyCells(position, size, machineObject);

            // Act
            bool canPlace = placementController.CanPlaceAt(position, size);

            // Assert
            Assert.IsFalse(canPlace, 
                "PlacementController should prevent placement at occupied position");
        }

        #endregion

        #region Full System Integration Tests

        [Test]
        public void FullIntegration_PlaceMachine_ProcessResources_ExtractOutput()
        {
            // Arrange - Place machine on grid
            Vector3Int gridPosition = new Vector3Int(30, 0, 30);
            testMachine.SetGridPosition(gridPosition);
            gridManager.OccupyCells(gridPosition, testMachineData.gridSize, machineObject);

            // Add resources to ResourceManager
            resourceManager.AddResource("wood", 10);

            // Setup machine
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.SetPowered(true);

            // Act - Add inputs from ResourceManager
            bool hasResources = resourceManager.TryConsumeResource("wood", 2);
            Assert.IsTrue(hasResources, "Should have resources in ResourceManager");

            testMachine.AddToInputPort(0, "wood", 2);

            // Process
            testMachine.UpdateStateMachine();
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);

            // Simulate processing
            float elapsed = 0f;
            while (elapsed < testRecipe.processingTime && testMachine.CurrentState == MachineState.Processing)
            {
                testMachine.SimulateUpdate(0.1f);
                elapsed += 0.1f;
            }

            // Extract outputs and add to ResourceManager
            int outputAmount = testMachine.GetOutputPortAmount(0, "planks");
            Assert.AreEqual(4, outputAmount, "Should have produced planks");

            resourceManager.AddResource("planks", outputAmount);

            // Assert - Verify full cycle
            Assert.AreEqual(8, resourceManager.GetResourceAmount("wood"), 
                "Should have consumed 2 wood from ResourceManager");
            Assert.AreEqual(4, resourceManager.GetResourceAmount("planks"), 
                "Should have added 4 planks to ResourceManager");
            Assert.IsTrue(gridManager.IsCellOccupied(gridPosition), 
                "Machine should still occupy grid cells");
        }

        [Test]
        public void FullIntegration_MultipleMachines_OnGrid()
        {
            // Arrange - Create second machine
            GameObject machine2Object = new GameObject("TestProcessor2");
            TestProcessor machine2 = machine2Object.AddComponent<TestProcessor>();
            machine2.SetMachineData(testMachineData);

            // Place machines on grid
            Vector3Int pos1 = new Vector3Int(35, 0, 35);
            Vector3Int pos2 = new Vector3Int(38, 0, 35); // 3 cells away (2x2 machine + 1 gap)

            testMachine.SetGridPosition(pos1);
            machine2.SetGridPosition(pos2);

            gridManager.OccupyCells(pos1, testMachineData.gridSize, machineObject);
            gridManager.OccupyCells(pos2, testMachineData.gridSize, machine2Object);

            // Act - Verify both machines are placed
            bool pos1Occupied = gridManager.IsCellOccupied(pos1);
            bool pos2Occupied = gridManager.IsCellOccupied(pos2);

            // Assert
            Assert.IsTrue(pos1Occupied, "First machine should occupy its position");
            Assert.IsTrue(pos2Occupied, "Second machine should occupy its position");

            // Verify they don't overlap
            List<Vector3Int> cells1 = testMachine.GetOccupiedCells();
            List<Vector3Int> cells2 = machine2.GetOccupiedCells();

            foreach (var cell in cells1)
            {
                Assert.IsFalse(cells2.Contains(cell), 
                    "Machines should not overlap on grid");
            }

            // Cleanup
            Object.DestroyImmediate(machine2Object);
        }

        [Test]
        public void FullIntegration_MachineRotation_UpdatesGridOccupation()
        {
            // Arrange
            Vector3Int position = new Vector3Int(40, 0, 40);
            testMachine.SetGridPosition(position);

            // Act - Rotate machine
            testMachine.SetRotation(0);
            List<Vector3Int> cells0 = testMachine.GetOccupiedCells();

            testMachine.SetRotation(1);
            List<Vector3Int> cells1 = testMachine.GetOccupiedCells();

            // Assert - Cells should be the same for a square machine
            Assert.AreEqual(cells0.Count, cells1.Count, 
                "Square machine should occupy same number of cells after rotation");

            // Verify rotation updated transform
            Assert.AreEqual(Quaternion.Euler(0, 90f, 0), testMachine.transform.rotation, 
                "Transform should reflect rotation");
        }

        #endregion

        #region Test Helper Class

        private class TestProcessor : MachineBase
        {
            public void SetMachineData(MachineData data)
            {
                machineData = data;
                InitializeFromData();
            }

            public void SimulateUpdate(float deltaTime)
            {
                if (!isEnabled) return;
                UpdateStateMachine();

                if (currentState == MachineState.Processing)
                {
                    processingTimeRemaining -= deltaTime;
                    processingProgress = 1f - (processingTimeRemaining / (activeRecipe.processingTime / speedMultiplier));

                    if (processingTimeRemaining <= 0f)
                    {
                        CompleteProcessing();
                    }
                }
            }

            public bool AddToInputPort(int portIndex, string resourceId, int amount)
            {
                if (portIndex < 0 || portIndex >= inputPorts.Count)
                    return false;
                return inputPorts[portIndex].AddResource(resourceId, amount);
            }

            public int GetOutputPortAmount(int portIndex, string resourceId)
            {
                if (portIndex < 0 || portIndex >= outputPorts.Count)
                    return 0;
                return outputPorts[portIndex].GetResourceAmount(resourceId);
            }

            public void UpdateStateMachine()
            {
                base.UpdateStateMachine();
            }
        }

        #endregion
    }
}
