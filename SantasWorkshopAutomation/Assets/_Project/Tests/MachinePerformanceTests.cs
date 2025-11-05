using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using SantasWorkshop.Machines;
using SantasWorkshop.Data;

namespace SantasWorkshop.Tests
{
    /// <summary>
    /// Performance tests for Machine Framework.
    /// Tests machine update performance, state machine efficiency, and buffer operations.
    /// Target: <0.1ms per machine per frame for 100+ machines.
    /// </summary>
    public class MachinePerformanceTests
    {
        private const int MACHINE_COUNT = 100;
        private const float TARGET_UPDATE_TIME_MS = 0.1f; // Per machine
        private const float TARGET_TOTAL_TIME_MS = 16f; // 60 FPS frame budget
        private const int WARMUP_ITERATIONS = 10;
        private const int TEST_ITERATIONS = 100;

        private List<GameObject> machineObjects;
        private List<TestMachine> testMachines;
        private MachineData testMachineData;
        private Recipe testRecipe;

        [SetUp]
        public void Setup()
        {
            machineObjects = new List<GameObject>();
            testMachines = new List<TestMachine>();

            // Create test recipe
            testRecipe = ScriptableObject.CreateInstance<Recipe>();
            testRecipe.recipeId = "perf_test_recipe";
            testRecipe.recipeName = "Performance Test Recipe";
            testRecipe.inputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "wood", amount = 1 }
            };
            testRecipe.outputs = new ResourceStack[]
            {
                new ResourceStack { resourceId = "planks", amount = 2 }
            };
            testRecipe.processingTime = 1f;
            testRecipe.powerConsumption = 10f;
            testRecipe.requiredTier = 1;

            // Create test machine data
            testMachineData = ScriptableObject.CreateInstance<MachineData>();
            testMachineData.machineName = "Performance Test Machine";
            testMachineData.gridSize = new Vector2Int(1, 1);
            testMachineData.tier = 1;
            testMachineData.baseProcessingSpeed = 1f;
            testMachineData.basePowerConsumption = 10f;
            testMachineData.inputPortCount = 1;
            testMachineData.outputPortCount = 1;
            testMachineData.inputPortPositions = new Vector3[] { Vector3.left * 0.5f };
            testMachineData.outputPortPositions = new Vector3[] { Vector3.right * 0.5f };
            testMachineData.bufferCapacity = 10;
            testMachineData.availableRecipes = new List<Recipe> { testRecipe };

            // Create machines
            for (int i = 0; i < MACHINE_COUNT; i++)
            {
                GameObject machineObj = new GameObject($"PerfTestMachine_{i}");
                TestMachine machine = machineObj.AddComponent<TestMachine>();
                machine.SetMachineData(testMachineData);
                machine.SetPowered(true);
                machine.SetActiveRecipe(testRecipe);

                machineObjects.Add(machineObj);
                testMachines.Add(machine);
            }
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var obj in machineObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }

            if (testMachineData != null)
                Object.DestroyImmediate(testMachineData);
            if (testRecipe != null)
                Object.DestroyImmediate(testRecipe);

            machineObjects.Clear();
            testMachines.Clear();
        }

        #region Update Performance Tests

        [Test]
        public void Performance_100Machines_UpdatePerFrame()
        {
            // Arrange - Set all machines to different states
            for (int i = 0; i < testMachines.Count; i++)
            {
                var machine = testMachines[i];
                
                // Distribute machines across states
                switch (i % 5)
                {
                    case 0: // Idle
                        break;
                    case 1: // WaitingForInput
                        machine.TransitionToState(MachineState.WaitingForInput);
                        break;
                    case 2: // Processing
                        machine.AddToInputPort(0, "wood", 1);
                        machine.UpdateStateMachine();
                        break;
                    case 3: // WaitingForOutput
                        machine.TransitionToState(MachineState.WaitingForOutput);
                        break;
                    case 4: // NoPower
                        machine.SetPowered(false);
                        break;
                }
            }

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                foreach (var machine in testMachines)
                {
                    machine.SimulateUpdate(0.016f);
                }
            }

            // Act - Measure update time
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int iteration = 0; iteration < TEST_ITERATIONS; iteration++)
            {
                foreach (var machine in testMachines)
                {
                    machine.SimulateUpdate(0.016f);
                }
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerIteration = totalMs / TEST_ITERATIONS;
            double avgMsPerMachine = avgMsPerIteration / MACHINE_COUNT;

            // Assert
            UnityEngine.Debug.Log($"Performance Test Results:");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS} iterations");
            UnityEngine.Debug.Log($"  Avg per iteration: {avgMsPerIteration:F2}ms ({MACHINE_COUNT} machines)");
            UnityEngine.Debug.Log($"  Avg per machine: {avgMsPerMachine:F4}ms");
            UnityEngine.Debug.Log($"  Target per machine: {TARGET_UPDATE_TIME_MS:F4}ms");
            UnityEngine.Debug.Log($"  Target per frame: {TARGET_TOTAL_TIME_MS:F2}ms");

            Assert.Less(avgMsPerMachine, TARGET_UPDATE_TIME_MS, 
                $"Average update time per machine ({avgMsPerMachine:F4}ms) should be less than {TARGET_UPDATE_TIME_MS}ms");

            Assert.Less(avgMsPerIteration, TARGET_TOTAL_TIME_MS, 
                $"Total update time for {MACHINE_COUNT} machines ({avgMsPerIteration:F2}ms) should be less than {TARGET_TOTAL_TIME_MS}ms (60 FPS budget)");
        }

        [Test]
        public void Performance_StateMachine_UpdateTime()
        {
            // Arrange - Single machine for precise measurement
            var machine = testMachines[0];
            machine.AddToInputPort(0, "wood", 1);
            machine.UpdateStateMachine();

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                machine.UpdateStateMachine();
            }

            // Act - Measure state machine update
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < TEST_ITERATIONS * 100; i++)
            {
                machine.UpdateStateMachine();
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerUpdate = totalMs / (TEST_ITERATIONS * 100);

            // Assert
            UnityEngine.Debug.Log($"State Machine Performance:");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS * 100} updates");
            UnityEngine.Debug.Log($"  Avg per update: {avgMsPerUpdate:F6}ms");
            UnityEngine.Debug.Log($"  Target: <0.05ms");

            Assert.Less(avgMsPerUpdate, 0.05f, 
                $"State machine update time ({avgMsPerUpdate:F6}ms) should be less than 0.05ms");
        }

        #endregion

        #region Buffer Operation Performance Tests

        [Test]
        public void Performance_BufferOperations_AddResource()
        {
            // Arrange
            var machine = testMachines[0];

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                machine.AddToInputPort(0, "wood", 1);
                machine.ClearInputPort(0);
            }

            // Act - Measure add operations
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < TEST_ITERATIONS * 100; i++)
            {
                machine.AddToInputPort(0, "wood", 1);
                machine.ClearInputPort(0);
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerOperation = totalMs / (TEST_ITERATIONS * 100);

            // Assert
            UnityEngine.Debug.Log($"Buffer Add Performance:");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS * 100} operations");
            UnityEngine.Debug.Log($"  Avg per operation: {avgMsPerOperation:F6}ms");
            UnityEngine.Debug.Log($"  Target: <0.01ms");

            Assert.Less(avgMsPerOperation, 0.01f, 
                $"Buffer add operation time ({avgMsPerOperation:F6}ms) should be less than 0.01ms");
        }

        [Test]
        public void Performance_BufferOperations_RemoveResource()
        {
            // Arrange
            var machine = testMachines[0];

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                machine.AddToInputPort(0, "wood", 5);
                machine.RemoveFromInputPort(0, "wood", 5);
            }

            // Act - Measure remove operations
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < TEST_ITERATIONS * 100; i++)
            {
                machine.AddToInputPort(0, "wood", 5);
                machine.RemoveFromInputPort(0, "wood", 5);
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerOperation = totalMs / (TEST_ITERATIONS * 100);

            // Assert
            UnityEngine.Debug.Log($"Buffer Remove Performance:");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS * 100} operations");
            UnityEngine.Debug.Log($"  Avg per operation: {avgMsPerOperation:F6}ms");
            UnityEngine.Debug.Log($"  Target: <0.01ms");

            Assert.Less(avgMsPerOperation, 0.01f, 
                $"Buffer remove operation time ({avgMsPerOperation:F6}ms) should be less than 0.01ms");
        }

        [Test]
        public void Performance_BufferOperations_QueryAmount()
        {
            // Arrange
            var machine = testMachines[0];
            machine.AddToInputPort(0, "wood", 5);

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                int amount = machine.GetInputPortAmount(0, "wood");
            }

            // Act - Measure query operations
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < TEST_ITERATIONS * 1000; i++)
            {
                int amount = machine.GetInputPortAmount(0, "wood");
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerOperation = totalMs / (TEST_ITERATIONS * 1000);

            // Assert
            UnityEngine.Debug.Log($"Buffer Query Performance:");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS * 1000} operations");
            UnityEngine.Debug.Log($"  Avg per operation: {avgMsPerOperation:F6}ms");
            UnityEngine.Debug.Log($"  Target: <0.01ms");

            Assert.Less(avgMsPerOperation, 0.01f, 
                $"Buffer query operation time ({avgMsPerOperation:F6}ms) should be less than 0.01ms");
        }

        #endregion

        #region Recipe Processing Performance Tests

        [Test]
        public void Performance_RecipeValidation_CanProcessRecipe()
        {
            // Arrange
            var machine = testMachines[0];
            machine.AddToInputPort(0, "wood", 1);

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                bool canProcess = machine.CanProcessRecipe(testRecipe);
            }

            // Act - Measure validation
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < TEST_ITERATIONS * 100; i++)
            {
                bool canProcess = machine.CanProcessRecipe(testRecipe);
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerValidation = totalMs / (TEST_ITERATIONS * 100);

            // Assert
            UnityEngine.Debug.Log($"Recipe Validation Performance:");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS * 100} validations");
            UnityEngine.Debug.Log($"  Avg per validation: {avgMsPerValidation:F6}ms");
            UnityEngine.Debug.Log($"  Target: <0.02ms");

            Assert.Less(avgMsPerValidation, 0.02f, 
                $"Recipe validation time ({avgMsPerValidation:F6}ms) should be less than 0.02ms");
        }

        [Test]
        public void Performance_Processing_CompleteRecipe()
        {
            // Arrange
            var machine = testMachines[0];

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                machine.AddToInputPort(0, "wood", 1);
                machine.UpdateStateMachine();
                machine.SimulateUpdate(testRecipe.processingTime);
                machine.ClearOutputPort(0);
            }

            // Act - Measure complete processing cycle
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                machine.AddToInputPort(0, "wood", 1);
                machine.UpdateStateMachine();
                machine.SimulateUpdate(testRecipe.processingTime);
                machine.ClearOutputPort(0);
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerCycle = totalMs / TEST_ITERATIONS;

            // Assert
            UnityEngine.Debug.Log($"Complete Processing Cycle Performance:");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS} cycles");
            UnityEngine.Debug.Log($"  Avg per cycle: {avgMsPerCycle:F4}ms");
            UnityEngine.Debug.Log($"  Target: <0.1ms");

            Assert.Less(avgMsPerCycle, 0.1f, 
                $"Complete processing cycle time ({avgMsPerCycle:F4}ms) should be less than 0.1ms");
        }

        #endregion

        #region Memory and Allocation Tests

        [Test]
        public void Performance_NoGarbageAllocation_DuringUpdate()
        {
            // Arrange
            var machine = testMachines[0];
            machine.AddToInputPort(0, "wood", 1);
            machine.UpdateStateMachine();

            // Force garbage collection before test
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long memoryBefore = System.GC.GetTotalMemory(false);

            // Act - Run many updates
            for (int i = 0; i < 1000; i++)
            {
                machine.SimulateUpdate(0.016f);
            }

            long memoryAfter = System.GC.GetTotalMemory(false);
            long allocatedBytes = memoryAfter - memoryBefore;

            // Assert
            UnityEngine.Debug.Log($"Memory Allocation Test:");
            UnityEngine.Debug.Log($"  Memory before: {memoryBefore / 1024}KB");
            UnityEngine.Debug.Log($"  Memory after: {memoryAfter / 1024}KB");
            UnityEngine.Debug.Log($"  Allocated: {allocatedBytes / 1024}KB");
            UnityEngine.Debug.Log($"  Target: <10KB for 1000 updates");

            Assert.Less(allocatedBytes, 10 * 1024, 
                $"Memory allocation ({allocatedBytes / 1024}KB) should be minimal (<10KB) for 1000 updates");
        }

        #endregion

        #region Scalability Tests

        [Test]
        public void Performance_Scalability_200Machines()
        {
            // Arrange - Create additional machines
            List<GameObject> extraObjects = new List<GameObject>();
            List<TestMachine> extraMachines = new List<TestMachine>();

            for (int i = 0; i < 100; i++)
            {
                GameObject machineObj = new GameObject($"ExtraMachine_{i}");
                TestMachine machine = machineObj.AddComponent<TestMachine>();
                machine.SetMachineData(testMachineData);
                machine.SetPowered(true);
                machine.SetActiveRecipe(testRecipe);
                machine.AddToInputPort(0, "wood", 1);
                machine.UpdateStateMachine();

                extraObjects.Add(machineObj);
                extraMachines.Add(machine);
            }

            var allMachines = new List<TestMachine>(testMachines);
            allMachines.AddRange(extraMachines);

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                foreach (var machine in allMachines)
                {
                    machine.SimulateUpdate(0.016f);
                }
            }

            // Act - Measure with 200 machines
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int iteration = 0; iteration < TEST_ITERATIONS; iteration++)
            {
                foreach (var machine in allMachines)
                {
                    machine.SimulateUpdate(0.016f);
                }
            }

            stopwatch.Stop();

            // Calculate metrics
            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgMsPerIteration = totalMs / TEST_ITERATIONS;
            double avgMsPerMachine = avgMsPerIteration / allMachines.Count;

            // Assert
            UnityEngine.Debug.Log($"Scalability Test (200 machines):");
            UnityEngine.Debug.Log($"  Total time: {totalMs:F2}ms for {TEST_ITERATIONS} iterations");
            UnityEngine.Debug.Log($"  Avg per iteration: {avgMsPerIteration:F2}ms");
            UnityEngine.Debug.Log($"  Avg per machine: {avgMsPerMachine:F4}ms");
            UnityEngine.Debug.Log($"  Target per frame: {TARGET_TOTAL_TIME_MS:F2}ms");

            Assert.Less(avgMsPerIteration, TARGET_TOTAL_TIME_MS, 
                $"Update time for 200 machines ({avgMsPerIteration:F2}ms) should be less than {TARGET_TOTAL_TIME_MS}ms");

            // Cleanup
            foreach (var obj in extraObjects)
            {
                Object.DestroyImmediate(obj);
            }
        }

        #endregion

        #region Test Helper Class

        private class TestMachine : MachineBase
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

            public int RemoveFromInputPort(int portIndex, string resourceId, int amount)
            {
                if (portIndex < 0 || portIndex >= inputPorts.Count)
                    return 0;
                return inputPorts[portIndex].RemoveResource(resourceId, amount);
            }

            public void ClearInputPort(int portIndex)
            {
                if (portIndex < 0 || portIndex >= inputPorts.Count)
                    return;
                
                // Clear all resources from port
                var port = inputPorts[portIndex];
                var buffer = port.GetType().GetField("buffer", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(port) as System.Collections.Generic.Dictionary<string, int>;
                buffer?.Clear();
            }

            public void ClearOutputPort(int portIndex)
            {
                if (portIndex < 0 || portIndex >= outputPorts.Count)
                    return;
                
                // Clear all resources from port
                var port = outputPorts[portIndex];
                var buffer = port.GetType().GetField("buffer", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(port) as System.Collections.Generic.Dictionary<string, int>;
                buffer?.Clear();
            }

            public void UpdateStateMachine()
            {
                base.UpdateStateMachine();
            }
        }

        #endregion
    }
}
