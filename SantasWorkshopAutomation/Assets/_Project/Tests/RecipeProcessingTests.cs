using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Machines;
using SantasWorkshop.Data;
using System.Collections.Generic;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for MachineBase recipe processing functionality.
    /// Tests recipe validation, processing logic, input/output handling, and progress tracking.
    /// </summary>
    public class RecipeProcessingTests
    {
        private GameObject machineObject;
        private TestMachine testMachine;
        private MachineData testMachineData;
        private Recipe simpleRecipe;
        private Recipe complexRecipe;
        private Recipe highTierRecipe;

        [SetUp]
        public void Setup()
        {
            // Create test machine data
            testMachineData = ScriptableObject.CreateInstance<MachineData>();
            testMachineData.machineName = "Test Machine";
            testMachineData.tier = 1;
            testMachineData.gridSize = new Vector2Int(1, 1);
            testMachineData.basePowerConsumption = 10f;
            testMachineData.baseProcessingSpeed = 1f;
            testMachineData.inputPortCount = 2;
            testMachineData.outputPortCount = 2;
            testMachineData.bufferCapacity = 20;
            testMachineData.inputPortPositions = new Vector3[] 
            { 
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
            testMachineData.outputPortPositions = new Vector3[] 
            { 
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0.5f)
            };
            testMachineData.availableRecipes = new List<Recipe>();

            // Create simple recipe (1 input, 1 output)
            simpleRecipe = ScriptableObject.CreateInstance<Recipe>();
            simpleRecipe.recipeId = "simple_recipe";
            simpleRecipe.recipeName = "Simple Recipe";
            simpleRecipe.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "wood", amount = 2 }
            };
            simpleRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "plank", amount = 4 }
            };
            simpleRecipe.processingTime = 2f;
            simpleRecipe.powerConsumption = 10f;
            simpleRecipe.requiredTier = 1;

            // Create complex recipe (multiple inputs, multiple outputs)
            complexRecipe = ScriptableObject.CreateInstance<Recipe>();
            complexRecipe.recipeId = "complex_recipe";
            complexRecipe.recipeName = "Complex Recipe";
            complexRecipe.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "wood", amount = 2 },
                new ResourceStack { resourceId = "iron", amount = 1 }
            };
            complexRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "gear", amount = 1 },
                new ResourceStack { resourceId = "scrap", amount = 1 }
            };
            complexRecipe.processingTime = 3f;
            simpleRecipe.powerConsumption = 20f;
            complexRecipe.requiredTier = 1;

            // Create high tier recipe
            highTierRecipe = ScriptableObject.CreateInstance<Recipe>();
            highTierRecipe.recipeId = "high_tier_recipe";
            highTierRecipe.recipeName = "High Tier Recipe";
            highTierRecipe.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "steel", amount = 1 }
            };
            highTierRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "advanced_gear", amount = 1 }
            };
            highTierRecipe.processingTime = 1f;
            highTierRecipe.powerConsumption = 30f;
            highTierRecipe.requiredTier = 2;

            testMachineData.availableRecipes.Add(simpleRecipe);
            testMachineData.availableRecipes.Add(complexRecipe);
            testMachineData.availableRecipes.Add(highTierRecipe);

            // Create machine GameObject
            machineObject = new GameObject("TestMachine");
            testMachine = machineObject.AddComponent<TestMachine>();
            testMachine.TestSetMachineData(testMachineData);
            testMachine.Awake();
        }

        [TearDown]
        public void Teardown()
        {
            if (machineObject != null)
            {
                Object.DestroyImmediate(machineObject);
            }
            if (testMachineData != null)
            {
                Object.DestroyImmediate(testMachineData);
            }
            if (simpleRecipe != null)
            {
                Object.DestroyImmediate(simpleRecipe);
            }
            if (complexRecipe != null)
            {
                Object.DestroyImmediate(complexRecipe);
            }
            if (highTierRecipe != null)
            {
                Object.DestroyImmediate(highTierRecipe);
            }
        }

        #region SetActiveRecipe Tests

        [Test]
        public void SetActiveRecipe_ValidRecipe_SetsRecipe()
        {
            // Act
            testMachine.SetActiveRecipe(simpleRecipe);

            // Assert
            Assert.AreEqual(simpleRecipe, testMachine.TestGetActiveRecipe());
        }

        [Test]
        public void SetActiveRecipe_Null_ClearsRecipe()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);

            // Act
            testMachine.SetActiveRecipe(null);

            // Assert
            Assert.IsNull(testMachine.TestGetActiveRecipe());
        }

        [Test]
        public void SetActiveRecipe_UpdatesPowerConsumption()
        {
            // Act
            testMachine.SetActiveRecipe(simpleRecipe);

            // Assert
            Assert.AreEqual(simpleRecipe.powerConsumption, testMachine.TestGetPowerConsumption());
        }

        [Test]
        public void SetActiveRecipe_WhileProcessing_CancelsProcessing()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);

            // Act
            testMachine.SetActiveRecipe(complexRecipe);

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);
            Assert.AreEqual(0f, testMachine.ProcessingProgress);
        }

        [Test]
        public void SetActiveRecipe_UnavailableRecipe_DoesNotSet()
        {
            // Arrange
            Recipe unavailableRecipe = ScriptableObject.CreateInstance<Recipe>();
            unavailableRecipe.recipeId = "unavailable";
            unavailableRecipe.recipeName = "Unavailable";
            unavailableRecipe.inputs = new ResourceStack[] { new ResourceStack { resourceId = "wood", amount = 1 } };
            unavailableRecipe.outputs = new ResourceStack[] { new ResourceStack { resourceId = "plank", amount = 1 } };
            unavailableRecipe.processingTime = 1f;
            unavailableRecipe.powerConsumption = 10f;
            unavailableRecipe.requiredTier = 1;

            // Act
            testMachine.SetActiveRecipe(unavailableRecipe);

            // Assert
            Assert.IsNull(testMachine.TestGetActiveRecipe());

            // Cleanup
            Object.DestroyImmediate(unavailableRecipe);
        }

        [Test]
        public void SetActiveRecipe_HigherTierThanMachine_DoesNotSet()
        {
            // Arrange
            Assert.AreEqual(1, testMachine.Tier);

            // Act
            testMachine.SetActiveRecipe(highTierRecipe);

            // Assert
            Assert.IsNull(testMachine.TestGetActiveRecipe());
        }

        #endregion

        #region CanProcessRecipe Tests

        [Test]
        public void CanProcessRecipe_WithSufficientInputs_ReturnsTrue()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);

            // Act
            bool canProcess = testMachine.TestCanProcessRecipe(simpleRecipe);

            // Assert
            Assert.IsTrue(canProcess);
        }

        [Test]
        public void CanProcessRecipe_WithInsufficientInputs_ReturnsFalse()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 1); // Need 2

            // Act
            bool canProcess = testMachine.TestCanProcessRecipe(simpleRecipe);

            // Assert
            Assert.IsFalse(canProcess);
        }

        [Test]
        public void CanProcessRecipe_WithoutPower_ReturnsFalse()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.SetPowered(false);

            // Act
            bool canProcess = testMachine.TestCanProcessRecipe(simpleRecipe);

            // Assert
            Assert.IsFalse(canProcess);
        }

        [Test]
        public void CanProcessRecipe_WhileDisabled_ReturnsFalse()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.SetEnabled(false);

            // Act
            bool canProcess = testMachine.TestCanProcessRecipe(simpleRecipe);

            // Assert
            Assert.IsFalse(canProcess);
        }

        [Test]
        public void CanProcessRecipe_WithFullOutputBuffer_ReturnsFalse()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            
            // Fill output buffer
            testMachine.TestAddToOutputBuffer("plank", 20); // Buffer capacity is 20

            // Act
            bool canProcess = testMachine.TestCanProcessRecipe(simpleRecipe);

            // Assert
            Assert.IsFalse(canProcess);
        }

        [Test]
        public void CanProcessRecipe_NullRecipe_ReturnsFalse()
        {
            // Act
            bool canProcess = testMachine.TestCanProcessRecipe(null);

            // Assert
            Assert.IsFalse(canProcess);
        }

        [Test]
        public void CanProcessRecipe_ComplexRecipe_AllInputsRequired()
        {
            // Arrange
            testMachine.SetActiveRecipe(complexRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            // Missing iron

            // Act
            bool canProcess = testMachine.TestCanProcessRecipe(complexRecipe);

            // Assert
            Assert.IsFalse(canProcess);

            // Add iron
            testMachine.AddToInputPort(0, "iron", 2);
            canProcess = testMachine.TestCanProcessRecipe(complexRecipe);
            Assert.IsTrue(canProcess);
        }

        #endregion

        #region Processing Progress Tests

        [Test]
        public void ProcessingProgress_StartsAtZero()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);

            // Act
            testMachine.TransitionToState(MachineState.Processing);

            // Assert
            Assert.AreEqual(0f, testMachine.ProcessingProgress);
        }

        [Test]
        public void ProcessingProgress_IncreasesOverTime()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);

            // Act
            testMachine.TestUpdateStateMachine();

            // Assert
            Assert.Greater(testMachine.ProcessingProgress, 0f);
        }

        [Test]
        public void ProcessingProgress_ReachesOne_WhenComplete()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);

            // Act - Simulate processing completion
            for (int i = 0; i < 200; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.ProcessingProgress >= 1f || testMachine.CurrentState != MachineState.Processing)
                    break;
            }

            // Assert
            Assert.GreaterOrEqual(testMachine.ProcessingProgress, 1f);
        }

        [Test]
        public void EstimatedTimeRemaining_DecreasesOverTime()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);
            float initialTime = testMachine.EstimatedTimeRemaining;

            // Act
            testMachine.TestUpdateStateMachine();

            // Assert
            Assert.Less(testMachine.EstimatedTimeRemaining, initialTime);
        }

        [Test]
        public void ProcessingProgress_ResetsAfterCompletion()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 10); // Enough for multiple cycles

            // Act - Complete one cycle
            testMachine.TransitionToState(MachineState.Processing);
            for (int i = 0; i < 200; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.ProcessingProgress >= 1f)
                    break;
            }

            // Continue to next cycle
            testMachine.TestUpdateStateMachine();

            // Assert - Should have reset and started new cycle
            Assert.Less(testMachine.ProcessingProgress, 1f);
        }

        #endregion

        #region Input/Output Processing Tests

        [Test]
        public void CompleteProcessing_ConsumesInputs()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            int initialAmount = testMachine.TestGetInputBufferAmount("wood");

            // Act - Complete processing
            testMachine.TransitionToState(MachineState.Processing);
            for (int i = 0; i < 200; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.CurrentState != MachineState.Processing)
                    break;
            }

            // Assert
            int finalAmount = testMachine.TestGetInputBufferAmount("wood");
            Assert.AreEqual(initialAmount - simpleRecipe.inputs[0].amount, finalAmount);
        }

        [Test]
        public void CompleteProcessing_ProducesOutputs()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);

            // Act - Complete processing
            testMachine.TransitionToState(MachineState.Processing);
            for (int i = 0; i < 200; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.CurrentState != MachineState.Processing)
                    break;
            }

            // Assert
            int outputAmount = testMachine.TestGetOutputBufferAmount("plank");
            Assert.AreEqual(simpleRecipe.outputs[0].amount, outputAmount);
        }

        [Test]
        public void CompleteProcessing_ComplexRecipe_ConsumesAllInputs()
        {
            // Arrange
            testMachine.SetActiveRecipe(complexRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.AddToInputPort(0, "iron", 5);

            // Act - Complete processing
            testMachine.TransitionToState(MachineState.Processing);
            for (int i = 0; i < 300; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.CurrentState != MachineState.Processing)
                    break;
            }

            // Assert
            Assert.AreEqual(3, testMachine.TestGetInputBufferAmount("wood")); // 5 - 2
            Assert.AreEqual(4, testMachine.TestGetInputBufferAmount("iron")); // 5 - 1
        }

        [Test]
        public void CompleteProcessing_ComplexRecipe_ProducesAllOutputs()
        {
            // Arrange
            testMachine.SetActiveRecipe(complexRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.AddToInputPort(0, "iron", 5);

            // Act - Complete processing
            testMachine.TransitionToState(MachineState.Processing);
            for (int i = 0; i < 300; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.CurrentState != MachineState.Processing)
                    break;
            }

            // Assert
            Assert.AreEqual(1, testMachine.TestGetOutputBufferAmount("gear"));
            Assert.AreEqual(1, testMachine.TestGetOutputBufferAmount("scrap"));
        }

        [Test]
        public void CompleteProcessing_ContinuesIfInputsAvailable()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 10); // Enough for 5 cycles

            // Act - Complete one cycle
            testMachine.TransitionToState(MachineState.Processing);
            for (int i = 0; i < 200; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.ProcessingProgress >= 1f)
                    break;
            }

            // Continue
            testMachine.TestUpdateStateMachine();

            // Assert - Should still be processing
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);
        }

        [Test]
        public void CompleteProcessing_StopsIfNoInputs()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 2); // Exactly enough for 1 cycle

            // Act - Complete processing
            testMachine.TransitionToState(MachineState.Processing);
            for (int i = 0; i < 200; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.CurrentState != MachineState.Processing)
                    break;
            }

            // Assert - Should transition to Idle
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);
        }

        #endregion

        #region Recipe Switching Tests

        [Test]
        public void SwitchRecipe_WhileIdle_SwitchesSuccessfully()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            Assert.AreEqual(simpleRecipe, testMachine.TestGetActiveRecipe());

            // Act
            testMachine.SetActiveRecipe(complexRecipe);

            // Assert
            Assert.AreEqual(complexRecipe, testMachine.TestGetActiveRecipe());
        }

        [Test]
        public void SwitchRecipe_WhileProcessing_CancelsAndSwitches()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);
            testMachine.TestUpdateStateMachine(); // Make some progress
            Assert.Greater(testMachine.ProcessingProgress, 0f);

            // Act
            testMachine.SetActiveRecipe(complexRecipe);

            // Assert
            Assert.AreEqual(complexRecipe, testMachine.TestGetActiveRecipe());
            Assert.AreEqual(0f, testMachine.ProcessingProgress);
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);
        }

        [Test]
        public void SwitchRecipe_InputsNotConsumed()
        {
            // Arrange
            testMachine.SetActiveRecipe(simpleRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);
            testMachine.TestUpdateStateMachine();
            int woodBeforeSwitch = testMachine.TestGetInputBufferAmount("wood");

            // Act
            testMachine.SetActiveRecipe(complexRecipe);

            // Assert - Inputs should not be consumed when canceling
            Assert.AreEqual(woodBeforeSwitch, testMachine.TestGetInputBufferAmount("wood"));
        }

        #endregion

        #region Recipe Validation Tests

        [Test]
        public void GetAvailableRecipes_ReturnsAllRecipes()
        {
            // Act
            var recipes = testMachine.GetAvailableRecipes();

            // Assert
            Assert.AreEqual(3, recipes.Count);
            Assert.Contains(simpleRecipe, recipes);
            Assert.Contains(complexRecipe, recipes);
            Assert.Contains(highTierRecipe, recipes);
        }

        [Test]
        public void IsRecipeAvailable_ForAvailableRecipe_ReturnsTrue()
        {
            // Act
            bool isAvailable = testMachine.TestIsRecipeAvailable(simpleRecipe);

            // Assert
            Assert.IsTrue(isAvailable);
        }

        [Test]
        public void IsRecipeAvailable_ForUnavailableRecipe_ReturnsFalse()
        {
            // Arrange
            Recipe unavailableRecipe = ScriptableObject.CreateInstance<Recipe>();
            unavailableRecipe.recipeId = "unavailable";

            // Act
            bool isAvailable = testMachine.TestIsRecipeAvailable(unavailableRecipe);

            // Assert
            Assert.IsFalse(isAvailable);

            // Cleanup
            Object.DestroyImmediate(unavailableRecipe);
        }

        #endregion

        #region Test Helper Class

        /// <summary>
        /// Concrete implementation of MachineBase for testing recipe processing.
        /// Exposes protected methods for testing.
        /// </summary>
        private class TestMachine : MachineBase
        {
            public void TestSetMachineData(MachineData data)
            {
                machineData = data;
            }

            public new void Awake()
            {
                base.Awake();
            }

            public Recipe TestGetActiveRecipe()
            {
                return activeRecipe;
            }

            public float TestGetPowerConsumption()
            {
                return powerConsumption;
            }

            public bool TestCanProcessRecipe(Recipe recipe)
            {
                return CanProcessRecipe(recipe);
            }

            public int TestGetInputBufferAmount(string resourceId)
            {
                return GetInputBufferAmount(resourceId);
            }

            public int TestGetOutputBufferAmount(string resourceId)
            {
                int total = 0;
                foreach (var port in outputPorts)
                {
                    total += port.GetResourceAmount(resourceId);
                }
                return total;
            }

            public bool TestAddToOutputBuffer(string resourceId, int amount)
            {
                return AddToOutputBuffer(resourceId, amount);
            }

            public bool TestIsRecipeAvailable(Recipe recipe)
            {
                return IsRecipeAvailable(recipe);
            }
        }

        #endregion
    }
}
