using NUnit.Framework;
using UnityEngine;
using SantasWorkshop.Machines;
using SantasWorkshop.Data;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Unit tests for MachineBase core functionality.
    /// Tests machine lifecycle, state management, and power handling.
    /// </summary>
    public class MachineBaseTests
    {
        private GameObject machineObject;
        private TestMachine testMachine;
        private MachineData testMachineData;

        [SetUp]
        public void Setup()
        {
            // Create test machine data
            testMachineData = ScriptableObject.CreateInstance<MachineData>();
            testMachineData.machineId = "test_machine";
            testMachineData.displayName = "Test Machine";
            testMachineData.powerConsumption = 10f;

            // Create a GameObject with TestMachine component
            machineObject = new GameObject("TestMachine");
            testMachine = machineObject.AddComponent<TestMachine>();
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up the GameObject and ScriptableObject after each test
            if (machineObject != null)
            {
                Object.DestroyImmediate(machineObject);
            }
            if (testMachineData != null)
            {
                Object.DestroyImmediate(testMachineData);
            }
        }

        #region Initialize Tests

        [Test]
        public void Initialize_SetsCorrectState()
        {
            // Act
            testMachine.Initialize(testMachineData);

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.State, 
                "Machine should be in Idle state after initialization");
        }

        [Test]
        public void Initialize_SetsMachineId()
        {
            // Act
            testMachine.Initialize(testMachineData);

            // Assert
            Assert.AreEqual("test_machine", testMachine.MachineId, 
                "Machine ID should match the data");
        }

        [Test]
        public void Initialize_SetsPowerConsumption()
        {
            // Act
            testMachine.Initialize(testMachineData);

            // Assert
            Assert.AreEqual(10f, testMachine.PowerConsumption, 
                "Power consumption should match the data");
        }

        [Test]
        public void Initialize_WithNullData_HandlesGracefully()
        {
            // Act
            testMachine.Initialize(null);

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.State, 
                "Machine should still be in Idle state");
            Assert.AreEqual(0f, testMachine.PowerConsumption, 
                "Power consumption should be 0 with null data");
        }

        #endregion

        #region Shutdown Tests

        [Test]
        public void Shutdown_ChangesStateToOffline()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            Assert.AreEqual(MachineState.Idle, testMachine.State, "Precondition: Should start in Idle");

            // Act
            testMachine.Shutdown();

            // Assert
            Assert.AreEqual(MachineState.Offline, testMachine.State, 
                "Machine should be in Offline state after shutdown");
        }

        [Test]
        public void Shutdown_SetsPoweredToFalse()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.SetPowered(true);
            Assert.IsTrue(testMachine.IsPowered, "Precondition: Should be powered");

            // Act
            testMachine.Shutdown();

            // Assert
            Assert.IsFalse(testMachine.IsPowered, 
                "Machine should not be powered after shutdown");
        }

        [Test]
        public void Shutdown_FromWorkingState_ChangesToOffline()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.SetPowered(true);
            testMachine.SetTestState(MachineState.Working);
            Assert.AreEqual(MachineState.Working, testMachine.State, "Precondition: Should be Working");

            // Act
            testMachine.Shutdown();

            // Assert
            Assert.AreEqual(MachineState.Offline, testMachine.State, 
                "Machine should be Offline after shutdown from Working state");
        }

        #endregion

        #region Power State Tests

        [Test]
        public void SetPowered_True_SetsPoweredState()
        {
            // Arrange
            testMachine.Initialize(testMachineData);

            // Act
            testMachine.SetPowered(true);

            // Assert
            Assert.IsTrue(testMachine.IsPowered, 
                "Machine should be powered after SetPowered(true)");
        }

        [Test]
        public void SetPowered_False_ClearsPoweredState()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.SetPowered(true);

            // Act
            testMachine.SetPowered(false);

            // Assert
            Assert.IsFalse(testMachine.IsPowered, 
                "Machine should not be powered after SetPowered(false)");
        }

        [Test]
        public void SetPowered_False_WhileWorking_ChangesToIdle()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.SetPowered(true);
            testMachine.SetTestState(MachineState.Working);
            Assert.AreEqual(MachineState.Working, testMachine.State, "Precondition: Should be Working");

            // Act
            testMachine.SetPowered(false);

            // Assert
            Assert.AreEqual(MachineState.Idle, testMachine.State, 
                "Machine should change to Idle when power is removed while Working");
        }

        [Test]
        public void SetPowered_SameValue_DoesNotTriggerChange()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.SetPowered(true);
            testMachine.ResetPowerChangedCount();

            // Act
            testMachine.SetPowered(true);

            // Assert
            Assert.AreEqual(0, testMachine.PowerChangedCount, 
                "OnPowerChanged should not be called when setting same value");
        }

        [Test]
        public void SetPowered_DifferentValue_TriggersChange()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.SetPowered(false);
            testMachine.ResetPowerChangedCount();

            // Act
            testMachine.SetPowered(true);

            // Assert
            Assert.AreEqual(1, testMachine.PowerChangedCount, 
                "OnPowerChanged should be called once when changing power state");
        }

        #endregion

        #region State Management Tests

        [Test]
        public void State_ReturnsCurrentState()
        {
            // Arrange
            testMachine.Initialize(testMachineData);

            // Act
            MachineState state = testMachine.State;

            // Assert
            Assert.AreEqual(MachineState.Idle, state, 
                "State property should return current state");
        }

        [Test]
        public void SetState_ChangesState()
        {
            // Arrange
            testMachine.Initialize(testMachineData);

            // Act
            testMachine.SetTestState(MachineState.Working);

            // Assert
            Assert.AreEqual(MachineState.Working, testMachine.State, 
                "State should change to Working");
        }

        [Test]
        public void SetState_TriggersStateChanged()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.ResetStateChangedCount();

            // Act
            testMachine.SetTestState(MachineState.Working);

            // Assert
            Assert.AreEqual(1, testMachine.StateChangedCount, 
                "OnStateChanged should be called once");
        }

        [Test]
        public void SetState_SameState_DoesNotTriggerChange()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            testMachine.SetTestState(MachineState.Working);
            testMachine.ResetStateChangedCount();

            // Act
            testMachine.SetTestState(MachineState.Working);

            // Assert
            Assert.AreEqual(0, testMachine.StateChangedCount, 
                "OnStateChanged should not be called when setting same state");
        }

        #endregion

        #region Tick Tests

        [Test]
        public void Tick_IsCalled()
        {
            // Arrange
            testMachine.Initialize(testMachineData);

            // Act
            testMachine.Tick(0.016f);

            // Assert
            Assert.AreEqual(1, testMachine.TickCount, 
                "Tick should be called once");
        }

        [Test]
        public void Tick_ReceivesDeltaTime()
        {
            // Arrange
            testMachine.Initialize(testMachineData);
            float expectedDelta = 0.016f;

            // Act
            testMachine.Tick(expectedDelta);

            // Assert
            Assert.AreEqual(expectedDelta, testMachine.LastDeltaTime, 0.0001f, 
                "Tick should receive correct delta time");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void FullLifecycle_InitializeTickShutdown()
        {
            // Initialize
            testMachine.Initialize(testMachineData);
            Assert.AreEqual(MachineState.Idle, testMachine.State);

            // Power on
            testMachine.SetPowered(true);
            Assert.IsTrue(testMachine.IsPowered);

            // Start working
            testMachine.SetTestState(MachineState.Working);
            Assert.AreEqual(MachineState.Working, testMachine.State);

            // Tick
            testMachine.Tick(0.016f);
            Assert.AreEqual(1, testMachine.TickCount);

            // Shutdown
            testMachine.Shutdown();
            Assert.AreEqual(MachineState.Offline, testMachine.State);
            Assert.IsFalse(testMachine.IsPowered);
        }

        #endregion

        #region Test Helper Class

        /// <summary>
        /// Concrete implementation of MachineBase for testing purposes.
        /// Exposes protected methods and tracks method calls.
        /// </summary>
        private class TestMachine : MachineBase
        {
            public int TickCount { get; private set; }
            public float LastDeltaTime { get; private set; }
            public int PowerChangedCount { get; private set; }
            public int StateChangedCount { get; private set; }

            public override void Tick(float deltaTime)
            {
                TickCount++;
                LastDeltaTime = deltaTime;
            }

            protected override void OnPowerChanged(bool powered)
            {
                base.OnPowerChanged(powered);
                PowerChangedCount++;
            }

            protected override void OnStateChanged(MachineState previousState, MachineState newState)
            {
                base.OnStateChanged(previousState, newState);
                StateChangedCount++;
            }

            // Test helper methods to expose protected functionality
            public void SetTestState(MachineState state)
            {
                SetState(state);
            }

            public void ResetPowerChangedCount()
            {
                PowerChangedCount = 0;
            }

            public void ResetStateChangedCount()
            {
                StateChangedCount = 0;
            }
        }

        #endregion
    }
}
