using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;
using SantasWorkshop.Machines;
using SantasWorkshop.Data;
using SantasWorkshop.Core;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Integration tests for Machine Framework.
    /// Tests full processing cycles, multi-recipe switching, save/load, grid integration, and enable/disable functionality.
    /// </summary>
    public class MachineIntegrationTests
    {
        private GameObject machineObject;
        private TestProcessor testMachine;
        private MachineData testMachineData;
        private Recipe testRecipe1;
        private Recipe testRecipe2;
        private GameObject gridManagerObject;
        private GridManager gridManager;
        private GameObject resourceManagerObject;
        private ResourceManager resourceManager;

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

            // Create test recipes
            testRecipe1 = ScriptableObject.CreateInstance<Recipe>();
            testRecipe1.recipeId = "test_recipe_1";
            testRecipe1.recipeName = "Test Recipe 1";
            testRecipe1.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "wood", amount = 2 }
            };
            testRecipe1.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "planks", amount = 4 }
            };
            testRecipe1.processingTime = 2f;
            testRecipe1.powerConsumption = 10f;
            testRecipe1.requiredTier = 1;

            testRecipe2 = ScriptableObject.CreateInstance<Recipe>();
            testRecipe2.recipeId = "test_recipe_2";
            testRecipe2.recipeName = "Test Recipe 2";
            testRecipe2.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "iron_ore", amount = 1 }
            };
            testRecipe2.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "iron_ingot", amount = 1 }
            };
            testRecipe2.processingTime = 3f;
            testRecipe2.powerConsumption = 20f;
            testRecipe2.requiredTier = 1;

            // Create test machine data
            testMachineData = ScriptableObject.CreateInstance<MachineData>();
            testMachineData.machineName = "Test Processor";
            testMachineData.description = "A test processor machine";
            testMachineData.gridSize = new Vector2Int(2, 2);
            testMachineData.tier = 1;
            testMachineData.baseProcessingSpeed = 1f;
            testMachineData.basePowerConsumption = 10f;
            testMachineData.inputPortCount = 1;
            testMachineData.outputPortCount = 1;
            testMachineData.inputPortPositions = new Vector3[] { new Vector3(-0.5f, 0.5f, 0) };
            testMachineData.outputPortPositions = new Vector3[] { new Vector3(0.5f, 0.5f, 0) };
            testMachineData.bufferCapacity = 10;
            testMachineData.availableRecipes = new List<Recipe> { testRecipe1, testRecipe2 };

            // Create machine GameObject
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
            if (testMachineData != null)
                Object.DestroyImmediate(testMachineData);
            if (testRecipe1 != null)
                Object.DestroyImmediate(testRecipe1);
            if (testRecipe2 != null)
                Object.DestroyImmediate(testRecipe2);
        }

        #region Full Processing Cycle Tests

        [Test]
        public void FullProcessingCycle_AddInputs_Process_ExtractOutputs()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);

            // Add inputs to machine
            bool inputAdded = testMachine.AddToInputPort(0, "wood", 2);
            Assert.IsTrue(inputAdded, "Should be able to add inputs");

            // Act - Start processing
            testMachine.UpdateStateMachine();
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState, 
                "Machine should transition to Processing state");

            // Simulate processing time
            float elapsed = 0f;
            while (elapsed < testRecipe1.processingTime && testMachine.CurrentState == MachineState.Processing)
            {
                testMachine.SimulateUpdate(0.1f);
                elapsed += 0.1f;
            }

            // Assert - Processing complete
            Assert.AreNotEqual(MachineState.Processing, testMachine.CurrentState, 
                "Machine should complete processing");

            // Extract outputs
            int outputAmount = testMachine.GetOutputPortAmount(0, "planks");
            Assert.AreEqual(4, outputAmount, "Should have produced 4 planks");

            // Verify inputs consumed
            int remainingInput = testMachine.GetInputPortAmount(0, "wood");
            Assert.AreEqual(0, remainingInput, "Inputs should be consumed");
        }

        [Test]
        public void FullProcessingCycle_InsufficientInputs_WaitsForInput()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);

            // Add insufficient inputs
            testMachine.AddToInputPort(0, "wood", 1); // Need 2

            // Act
            testMachine.UpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.WaitingForInput, testMachine.CurrentState, 
                "Machine should wait for sufficient inputs");
        }

        [Test]
        public void FullProcessingCycle_FullOutputBuffer_WaitsForOutput()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);

            // Fill output buffer
            for (int i = 0; i < testMachineData.bufferCapacity; i++)
            {
                testMachine.AddToOutputPort(0, "planks", 1);
            }

            // Add inputs
            testMachine.AddToInputPort(0, "wood", 2);

            // Act
            testMachine.UpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.WaitingForOutput, testMachine.CurrentState, 
                "Machine should wait for output buffer space");
        }

        [Test]
        public void FullProcessingCycle_ContinuousProduction_ProcessesMultipleBatches()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);

            // Add inputs for 3 batches
            testMachine.AddToInputPort(0, "wood", 6);

            int completedBatches = 0;
            testMachine.OnProcessingCompleted += (recipe) => completedBatches++;

            // Act - Process for enough time for 3 batches
            float totalTime = testRecipe1.processingTime * 3f + 0.5f;
            float elapsed = 0f;
            while (elapsed < totalTime)
            {
                testMachine.SimulateUpdate(0.1f);
                elapsed += 0.1f;
            }

            // Assert
            Assert.AreEqual(3, completedBatches, "Should complete 3 batches");
            int outputAmount = testMachine.GetOutputPortAmount(0, "planks");
            Assert.AreEqual(12, outputAmount, "Should have produced 12 planks (3 batches × 4)");
        }

        #endregion

        #region Multi-Recipe Switching Tests

        [Test]
        public void MultiRecipe_SwitchRecipe_CancelsCurrentProcessing()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);
            testMachine.AddToInputPort(0, "wood", 2);

            // Start processing
            testMachine.UpdateStateMachine();
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);

            // Process partially
            testMachine.SimulateUpdate(0.5f);
            float progressBefore = testMachine.ProcessingProgress;
            Assert.Greater(progressBefore, 0f, "Should have some progress");

            // Act - Switch recipe
            testMachine.SetActiveRecipe(testRecipe2);

            // Assert
            Assert.AreEqual(0f, testMachine.ProcessingProgress, 
                "Progress should be reset when switching recipes");
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState, 
                "Should transition to Idle after recipe switch");
        }

        [Test]
        public void MultiRecipe_SwitchRecipe_PreservesInputs()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);
            testMachine.AddToInputPort(0, "wood", 2);

            // Start processing
            testMachine.UpdateStateMachine();
            testMachine.SimulateUpdate(0.5f);

            // Act - Switch recipe
            testMachine.SetActiveRecipe(testRecipe2);

            // Assert - Inputs should still be in buffer (not consumed yet)
            int remainingWood = testMachine.GetInputPortAmount(0, "wood");
            Assert.AreEqual(2, remainingWood, 
                "Inputs should be preserved when switching recipes mid-processing");
        }

        [Test]
        public void MultiRecipe_SwitchToNewRecipe_ProcessesCorrectly()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);

            // Act - Switch to recipe 2
            testMachine.SetActiveRecipe(testRecipe2);
            testMachine.AddToInputPort(0, "iron_ore", 1);

            // Process
            testMachine.UpdateStateMachine();
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);

            float elapsed = 0f;
            while (elapsed < testRecipe2.processingTime && testMachine.CurrentState == MachineState.Processing)
            {
                testMachine.SimulateUpdate(0.1f);
                elapsed += 0.1f;
            }

            // Assert
            int outputAmount = testMachine.GetOutputPortAmount(0, "iron_ingot");
            Assert.AreEqual(1, outputAmount, "Should produce iron ingot from recipe 2");
        }

        #endregion

        #region Save/Load Tests

        [Test]
        public void SaveLoad_PreservesBasicState()
        {
            // Arrange
            testMachine.SetGridPosition(new Vector3Int(5, 0, 10));
            testMachine.SetRotation(2);
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);

            // Act - Save
            MachineSaveData saveData = testMachine.GetSaveData();

            // Create new machine and load
            GameObject newMachineObject = new GameObject("LoadedMachine");
            TestProcessor loadedMachine = newMachineObject.AddComponent<TestProcessor>();
            loadedMachine.SetMachineData(testMachineData);
            loadedMachine.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(new Vector3Int(5, 0, 10), loadedMachine.GridPosition, 
                "Grid position should be preserved");
            Assert.AreEqual(2, loadedMachine.Rotation, 
                "Rotation should be preserved");
            Assert.IsTrue(loadedMachine.IsPowered, 
                "Power state should be preserved");

            // Cleanup
            Object.DestroyImmediate(newMachineObject);
        }

        [Test]
        public void SaveLoad_PreservesProcessingProgress()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);
            testMachine.AddToInputPort(0, "wood", 2);

            // Start processing and advance partially
            testMachine.UpdateStateMachine();
            testMachine.SimulateUpdate(1f); // Process for 1 second
            float progressBefore = testMachine.ProcessingProgress;
            Assert.Greater(progressBefore, 0f, "Should have some progress");

            // Act - Save and load
            MachineSaveData saveData = testMachine.GetSaveData();

            GameObject newMachineObject = new GameObject("LoadedMachine");
            TestProcessor loadedMachine = newMachineObject.AddComponent<TestProcessor>();
            loadedMachine.SetMachineData(testMachineData);
            loadedMachine.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(progressBefore, loadedMachine.ProcessingProgress, 0.01f, 
                "Processing progress should be preserved");
            Assert.AreEqual(MachineState.Processing, loadedMachine.CurrentState, 
                "Processing state should be preserved");

            // Cleanup
            Object.DestroyImmediate(newMachineObject);
        }

        [Test]
        public void SaveLoad_PreservesBufferContents()
        {
            // Arrange
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.AddToOutputPort(0, "planks", 3);

            // Act - Save and load
            MachineSaveData saveData = testMachine.GetSaveData();

            GameObject newMachineObject = new GameObject("LoadedMachine");
            TestProcessor loadedMachine = newMachineObject.AddComponent<TestProcessor>();
            loadedMachine.SetMachineData(testMachineData);
            loadedMachine.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(5, loadedMachine.GetInputPortAmount(0, "wood"), 
                "Input buffer contents should be preserved");
            Assert.AreEqual(3, loadedMachine.GetOutputPortAmount(0, "planks"), 
                "Output buffer contents should be preserved");

            // Cleanup
            Object.DestroyImmediate(newMachineObject);
        }

        [Test]
        public void SaveLoad_PreservesEnabledState()
        {
            // Arrange
            testMachine.SetEnabled(false);

            // Act - Save and load
            MachineSaveData saveData = testMachine.GetSaveData();

            GameObject newMachineObject = new GameObject("LoadedMachine");
            TestProcessor loadedMachine = newMachineObject.AddComponent<TestProcessor>();
            loadedMachine.SetMachineData(testMachineData);
            loadedMachine.LoadSaveData(saveData);

            // Assert
            Assert.AreEqual(MachineState.Disabled, loadedMachine.CurrentState, 
                "Disabled state should be preserved");

            // Cleanup
            Object.DestroyImmediate(newMachineObject);
        }

        #endregion

        #region Grid Integration Tests

        [Test]
        public void GridIntegration_SetGridPosition_StoresPosition()
        {
            // Arrange
            Vector3Int position = new Vector3Int(10, 0, 15);

            // Act
            testMachine.SetGridPosition(position);

            // Assert
            Assert.AreEqual(position, testMachine.GridPosition, 
                "Grid position should be stored correctly");
        }

        [Test]
        public void GridIntegration_GetOccupiedCells_ReturnsCorrectCells()
        {
            // Arrange
            testMachine.SetGridPosition(new Vector3Int(5, 0, 5));
            // Machine is 2x2

            // Act
            List<Vector3Int> occupiedCells = testMachine.GetOccupiedCells();

            // Assert
            Assert.AreEqual(4, occupiedCells.Count, "Should occupy 4 cells (2x2)");
            Assert.Contains(new Vector3Int(5, 0, 5), occupiedCells);
            Assert.Contains(new Vector3Int(6, 0, 5), occupiedCells);
            Assert.Contains(new Vector3Int(5, 0, 6), occupiedCells);
            Assert.Contains(new Vector3Int(6, 0, 6), occupiedCells);
        }

        [Test]
        public void GridIntegration_SetRotation_UpdatesRotation()
        {
            // Arrange & Act
            testMachine.SetRotation(0);
            Assert.AreEqual(0, testMachine.Rotation);

            testMachine.SetRotation(1);
            Assert.AreEqual(1, testMachine.Rotation);

            testMachine.SetRotation(2);
            Assert.AreEqual(2, testMachine.Rotation);

            testMachine.SetRotation(3);
            Assert.AreEqual(3, testMachine.Rotation);
        }

        [Test]
        public void GridIntegration_SetRotation_ClampsToValidRange()
        {
            // Act & Assert
            testMachine.SetRotation(-1);
            Assert.AreEqual(0, testMachine.Rotation, "Should clamp negative to 0");

            testMachine.SetRotation(5);
            Assert.AreEqual(3, testMachine.Rotation, "Should clamp above 3 to 3");
        }

        [Test]
        public void GridIntegration_Rotation_UpdatesVisualTransform()
        {
            // Arrange
            testMachine.SetRotation(0);
            Quaternion rotation0 = testMachine.transform.rotation;

            // Act
            testMachine.SetRotation(1);
            Quaternion rotation1 = testMachine.transform.rotation;

            testMachine.SetRotation(2);
            Quaternion rotation2 = testMachine.transform.rotation;

            // Assert
            Assert.AreNotEqual(rotation0, rotation1, "Rotation 0 and 1 should differ");
            Assert.AreNotEqual(rotation1, rotation2, "Rotation 1 and 2 should differ");
            Assert.AreEqual(Quaternion.Euler(0, 90f, 0), rotation1, 
                "Rotation 1 should be 90 degrees");
            Assert.AreEqual(Quaternion.Euler(0, 180f, 0), rotation2, 
                "Rotation 2 should be 180 degrees");
        }

        #endregion

        #region Enable/Disable Tests

        [Test]
        public void EnableDisable_SetEnabled_False_TransitionsToDisabled()
        {
            // Arrange
            testMachine.SetPowered(true);
            testMachine.SetActiveRecipe(testRecipe1);

            // Act
            testMachine.SetEnabled(false);

            // Assert
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState, 
                "Should transition to Disabled state");
        }

        [Test]
        public void EnableDisable_SetEnabled_True_TransitionsToIdle()
        {
            // Arrange
            testMachine.SetEnabled(false);
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);

            // Act
            testMachine.SetEnabled(true);

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState, 
                "Should transition to Idle state when re-enabled");
        }

        [Test]
        public void EnableDisable_WhileDisabled_DoesNotProcess()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);
            testMachine.AddToInputPort(0, "wood", 2);
            testMachine.SetEnabled(false);

            // Act
            testMachine.UpdateStateMachine();
            testMachine.SimulateUpdate(5f); // Try to process

            // Assert
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState, 
                "Should remain disabled");
            Assert.AreEqual(0f, testMachine.ProcessingProgress, 
                "Should not make any progress while disabled");
        }

        [Test]
        public void EnableDisable_WhileProcessing_PausesProgress()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);
            testMachine.AddToInputPort(0, "wood", 2);

            // Start processing
            testMachine.UpdateStateMachine();
            testMachine.SimulateUpdate(0.5f);
            float progressBefore = testMachine.ProcessingProgress;
            Assert.Greater(progressBefore, 0f);

            // Act - Disable
            testMachine.SetEnabled(false);
            testMachine.SimulateUpdate(1f); // Try to continue processing

            // Assert
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);
            // Progress should be preserved but not advanced
            Assert.AreEqual(progressBefore, testMachine.ProcessingProgress, 0.01f, 
                "Progress should be preserved when disabled");
        }

        [Test]
        public void EnableDisable_ReEnable_ResumesProcessing()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe1);
            testMachine.SetPowered(true);
            testMachine.AddToInputPort(0, "wood", 2);

            // Start processing and disable
            testMachine.UpdateStateMachine();
            testMachine.SimulateUpdate(0.5f);
            testMachine.SetEnabled(false);

            // Act - Re-enable
            testMachine.SetEnabled(true);
            testMachine.UpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState, 
                "Should resume processing when re-enabled");
        }

        #endregion

        #region Test Helper Class

        /// <summary>
        /// Concrete test implementation of MachineBase for integration testing.
        /// </summary>
        private class TestProcessor : MachineBase
        {
            public void SetMachineData(MachineData data)
            {
                machineData = data;
                InitializeFromData();
            }

            public void SimulateUpdate(float deltaTime)
            {
                // Manually call Update logic
                if (!isEnabled) return;
                UpdateStateMachine();

                // Simulate time passing for processing
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

            public bool AddToOutputPort(int portIndex, string resourceId, int amount)
            {
                if (portIndex < 0 || portIndex >= outputPorts.Count)
                    return false;

                return outputPorts[portIndex].AddResource(resourceId, amount);
            }

            public int GetInputPortAmount(int portIndex, string resourceId)
            {
                if (portIndex < 0 || portIndex >= inputPorts.Count)
                    return 0;

                return inputPorts[portIndex].GetResourceAmount(resourceId);
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
