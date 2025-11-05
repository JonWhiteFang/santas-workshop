using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Machines;
using SantasWorkshop.Data;
using System.Collections.Generic;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for MachineBase state machine functionality.
    /// Tests state transitions, event firing, power handling, and state validation.
    /// </summary>
    public class MachineStateTests
    {
        private GameObject machineObject;
        private TestMachine testMachine;
        private MachineData testMachineData;
        private Recipe testRecipe;

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
            testMachineData.inputPortCount = 1;
            testMachineData.outputPortCount = 1;
            testMachineData.bufferCapacity = 10;
            testMachineData.inputPortPositions = new Vector3[] { new Vector3(-0.5f, 0.5f, 0) };
            testMachineData.outputPortPositions = new Vector3[] { new Vector3(0.5f, 0.5f, 0) };
            testMachineData.availableRecipes = new List<Recipe>();

            // Create test recipe
            testRecipe = ScriptableObject.CreateInstance<Recipe>();
            testRecipe.recipeId = "test_recipe";
            testRecipe.recipeName = "Test Recipe";
            testRecipe.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "wood", amount = 1 }
            };
            testRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "plank", amount = 1 }
            };
            testRecipe.processingTime = 1f;
            testRecipe.powerConsumption = 10f;
            testRecipe.requiredTier = 1;

            testMachineData.availableRecipes.Add(testRecipe);

            // Create machine GameObject
            machineObject = new GameObject("TestMachine");
            testMachine = machineObject.AddComponent<TestMachine>();
            testMachine.TestSetMachineData(testMachineData);
            testMachine.Awake(); // Manually call Awake to initialize
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
            if (testRecipe != null)
            {
                Object.DestroyImmediate(testRecipe);
            }
        }

        #region State Transition Tests

        [Test]
        public void TransitionToState_ChangesState_Correctly()
        {
            // Arrange
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);

            // Act
            testMachine.TransitionToState(MachineState.Processing);

            // Assert
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);
        }

        [Test]
        public void TransitionToState_SameState_DoesNothing()
        {
            // Arrange
            testMachine.TransitionToState(MachineState.Processing);
            testMachine.ResetStateChangedCount();

            // Act
            testMachine.TransitionToState(MachineState.Processing);

            // Assert
            Assert.AreEqual(0, testMachine.StateChangedCount);
        }

        [Test]
        public void TransitionToState_FiresOnStateChangedEvent()
        {
            // Arrange
            MachineState oldState = MachineState.Idle;
            MachineState newState = MachineState.Idle;
            bool eventFired = false;

            testMachine.OnStateChanged += (old, newSt) =>
            {
                oldState = old;
                newState = newSt;
                eventFired = true;
            };

            // Act
            testMachine.TransitionToState(MachineState.Processing);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(MachineState.Idle, oldState);
            Assert.AreEqual(MachineState.Processing, newState);
        }

        [Test]
        public void TransitionToState_CallsOnStateEnter()
        {
            // Arrange
            testMachine.ResetEnterExitCounts();

            // Act
            testMachine.TransitionToState(MachineState.Processing);

            // Assert
            Assert.AreEqual(1, testMachine.EnterProcessingCount);
        }

        [Test]
        public void TransitionToState_CallsOnStateExit()
        {
            // Arrange
            testMachine.TransitionToState(MachineState.Processing);
            testMachine.ResetEnterExitCounts();

            // Act
            testMachine.TransitionToState(MachineState.Idle);

            // Assert
            Assert.AreEqual(1, testMachine.ExitProcessingCount);
        }

        [Test]
        public void TransitionToState_InvalidTransition_Blocked()
        {
            // Arrange
            testMachine.TransitionToState(MachineState.Disabled);

            // Act - Try to transition directly to Processing (should be blocked)
            testMachine.TransitionToState(MachineState.Processing);

            // Assert
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);
        }

        [Test]
        public void TransitionToState_FromDisabled_OnlyToIdle()
        {
            // Arrange
            testMachine.TransitionToState(MachineState.Disabled);

            // Act
            testMachine.TransitionToState(MachineState.Idle);

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);
        }

        #endregion

        #region Power State Tests

        [Test]
        public void SetPowered_False_TransitionsToNoPower()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);

            // Act
            testMachine.SetPowered(false);

            // Assert
            Assert.AreEqual(MachineState.NoPower, testMachine.CurrentState);
            Assert.IsFalse(testMachine.IsPowered);
        }

        [Test]
        public void SetPowered_True_RestoresPreviousState()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);
            testMachine.SetPowered(false);
            Assert.AreEqual(MachineState.NoPower, testMachine.CurrentState);

            // Act
            testMachine.SetPowered(true);

            // Assert
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);
            Assert.IsTrue(testMachine.IsPowered);
        }

        [Test]
        public void SetPowered_FiresOnPowerStatusChangedEvent()
        {
            // Arrange
            bool eventFired = false;
            bool poweredStatus = true;

            testMachine.OnPowerStatusChanged += (powered) =>
            {
                eventFired = true;
                poweredStatus = powered;
            };

            // Act
            testMachine.SetPowered(false);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsFalse(poweredStatus);
        }

        [Test]
        public void SetPowered_SameValue_DoesNotFireEvent()
        {
            // Arrange
            testMachine.SetPowered(true);
            int eventCount = 0;

            testMachine.OnPowerStatusChanged += (powered) =>
            {
                eventCount++;
            };

            // Act
            testMachine.SetPowered(true);

            // Assert
            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void SetPowered_WhileDisabled_DoesNotChangeState()
        {
            // Arrange
            testMachine.SetEnabled(false);
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);

            // Act
            testMachine.SetPowered(false);

            // Assert
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);
        }

        [Test]
        public void SetPowered_PreservesProcessingProgress()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);
            
            // Simulate some processing
            testMachine.TestUpdateStateMachine();
            float progressBeforePowerLoss = testMachine.ProcessingProgress;
            Assert.Greater(progressBeforePowerLoss, 0f);

            // Act - Lose power
            testMachine.SetPowered(false);
            Assert.AreEqual(MachineState.NoPower, testMachine.CurrentState);

            // Restore power
            testMachine.SetPowered(true);

            // Assert - Progress should be preserved
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);
            Assert.AreEqual(progressBeforePowerLoss, testMachine.ProcessingProgress);
        }

        #endregion

        #region State Validation Tests

        [Test]
        public void IsValidTransition_SameState_ReturnsTrue()
        {
            // Act
            bool isValid = testMachine.TestIsValidTransition(MachineState.Idle, MachineState.Idle);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidTransition_DisabledToProcessing_ReturnsFalse()
        {
            // Act
            bool isValid = testMachine.TestIsValidTransition(MachineState.Disabled, MachineState.Processing);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void IsValidTransition_DisabledToIdle_ReturnsTrue()
        {
            // Act
            bool isValid = testMachine.TestIsValidTransition(MachineState.Disabled, MachineState.Idle);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidTransition_ProcessingToWaitingForInput_ReturnsFalse()
        {
            // Act
            bool isValid = testMachine.TestIsValidTransition(MachineState.Processing, MachineState.WaitingForInput);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void IsValidTransition_IdleToProcessing_ReturnsTrue()
        {
            // Act
            bool isValid = testMachine.TestIsValidTransition(MachineState.Idle, MachineState.Processing);

            // Assert
            Assert.IsTrue(isValid);
        }

        #endregion

        #region State Update Tests

        [Test]
        public void UpdateIdle_WithRecipeAndInputs_TransitionsToProcessing()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Idle);

            // Act
            testMachine.TestUpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);
        }

        [Test]
        public void UpdateIdle_WithRecipeNoInputs_TransitionsToWaitingForInput()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.TransitionToState(MachineState.Idle);

            // Act
            testMachine.TestUpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.WaitingForInput, testMachine.CurrentState);
        }

        [Test]
        public void UpdateWaitingForInput_InputsAvailable_TransitionsToProcessing()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.TransitionToState(MachineState.WaitingForInput);

            // Act
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TestUpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.Processing, testMachine.CurrentState);
        }

        [Test]
        public void UpdateProcessing_CompletesAndTransitionsToIdle()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);

            // Act - Simulate processing completion
            for (int i = 0; i < 100; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (testMachine.CurrentState != MachineState.Processing)
                    break;
            }

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);
        }

        [Test]
        public void UpdateWaitingForOutput_SpaceAvailable_TransitionsToIdle()
        {
            // Arrange
            testMachine.TransitionToState(MachineState.WaitingForOutput);

            // Act
            testMachine.TestUpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);
        }

        #endregion

        #region Processing Event Tests

        [Test]
        public void OnEnterProcessing_FiresOnProcessingStartedEvent()
        {
            // Arrange
            bool eventFired = false;
            Recipe firedRecipe = null;

            testMachine.OnProcessingStarted += (recipe) =>
            {
                eventFired = true;
                firedRecipe = recipe;
            };

            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);

            // Act
            testMachine.TransitionToState(MachineState.Processing);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(testRecipe, firedRecipe);
        }

        [Test]
        public void CompleteProcessing_FiresOnProcessingCompletedEvent()
        {
            // Arrange
            bool eventFired = false;
            Recipe firedRecipe = null;

            testMachine.OnProcessingCompleted += (recipe) =>
            {
                eventFired = true;
                firedRecipe = recipe;
            };

            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.TransitionToState(MachineState.Processing);

            // Act - Complete processing
            for (int i = 0; i < 100; i++)
            {
                testMachine.TestUpdateStateMachine();
                if (eventFired)
                    break;
            }

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(testRecipe, firedRecipe);
        }

        #endregion

        #region Enable/Disable Tests

        [Test]
        public void SetEnabled_False_TransitionsToDisabled()
        {
            // Arrange
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);

            // Act
            testMachine.SetEnabled(false);

            // Assert
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);
        }

        [Test]
        public void SetEnabled_True_TransitionsToIdle()
        {
            // Arrange
            testMachine.SetEnabled(false);
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);

            // Act
            testMachine.SetEnabled(true);

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.CurrentState);
        }

        [Test]
        public void SetEnabled_SameValue_DoesNotChangeState()
        {
            // Arrange
            testMachine.ResetStateChangedCount();

            // Act
            testMachine.SetEnabled(true);

            // Assert
            Assert.AreEqual(0, testMachine.StateChangedCount);
        }

        [Test]
        public void UpdateStateMachine_WhileDisabled_DoesNotUpdate()
        {
            // Arrange
            testMachine.SetActiveRecipe(testRecipe);
            testMachine.AddToInputPort(0, "wood", 5);
            testMachine.SetEnabled(false);

            // Act
            testMachine.TestUpdateStateMachine();

            // Assert
            Assert.AreEqual(MachineState.Disabled, testMachine.CurrentState);
        }

        #endregion

        #region Test Helper Class

        /// <summary>
        /// Concrete implementation of MachineBase for testing state machine functionality.
        /// Exposes protected methods and tracks method calls.
        /// </summary>
        private class TestMachine : MachineBase
        {
            public int StateChangedCount { get; private set; }
            public int EnterProcessingCount { get; private set; }
            public int ExitProcessingCount { get; private set; }

            public void ResetStateChangedCount()
            {
                StateChangedCount = 0;
            }

            public void ResetEnterExitCounts()
            {
                EnterProcessingCount = 0;
                ExitProcessingCount = 0;
            }

            public void TestSetMachineData(MachineData data)
            {
                machineData = data;
            }

            public new void Awake()
            {
                base.Awake();
            }

            public bool TestIsValidTransition(MachineState from, MachineState to)
            {
                return IsValidTransition(from, to);
            }

            protected override void OnStateEnter(MachineState state)
            {
                base.OnStateEnter(state);
                if (state == MachineState.Processing)
                {
                    EnterProcessingCount++;
                }
            }

            protected override void OnStateExit(MachineState state)
            {
                base.OnStateExit(state);
                if (state == MachineState.Processing)
                {
                    ExitProcessingCount++;
                }
            }

            // Subscribe to OnStateChanged to track calls
            private void OnEnable()
            {
                OnStateChanged += (old, newState) => StateChangedCount++;
            }
        }

        #endregion
    }
}
