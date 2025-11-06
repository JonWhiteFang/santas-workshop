using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SantasWorkshop.Core;

namespace SantasWorkshop.Tests.Core
{
    /// <summary>
    /// Performance tests for TimeManager to ensure it meets performance requirements.
    /// Tests event scheduling with 1000+ events, tick overhead, and memory usage.
    /// </summary>
    [TestFixture]
    public class TimeManagerPerformanceTests
    {
        private GameObject _timeManagerObject;
        private TimeManager _timeManager;

        [SetUp]
        public void SetUp()
        {
            // Create TimeManager instance
            _timeManagerObject = new GameObject("TimeManager");
            _timeManager = _timeManagerObject.AddComponent<TimeManager>();
            
            // Wait for Awake to complete
            _timeManager.ResetForTesting();
        }

        [TearDown]
        public void TearDown()
        {
            if (_timeManagerObject != null)
            {
                Object.DestroyImmediate(_timeManagerObject);
            }
        }

        #region Event Scheduling Performance Tests

        [Test]
        public void EventScheduling_With1000Events_CompletesWithinPerformanceTarget()
        {
            // Arrange
            const int eventCount = 1000;
            const float maxSchedulingTimeMs = 100f; // 100ms for 1000 events
            var handles = new List<ScheduledEventHandle>();
            int callbackCount = 0;

            // Act - Measure scheduling time
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < eventCount; i++)
            {
                float delay = Random.Range(0.1f, 10f);
                var handle = _timeManager.ScheduleEvent(delay, () => callbackCount++);
                handles.Add(handle);
            }
            
            stopwatch.Stop();
            float schedulingTimeMs = (float)stopwatch.Elapsed.TotalMilliseconds;

            // Assert
            Assert.Less(schedulingTimeMs, maxSchedulingTimeMs, 
                $"Scheduling {eventCount} events took {schedulingTimeMs:F2}ms, exceeds target of {maxSchedulingTimeMs}ms");
            Assert.AreEqual(eventCount, handles.Count, "All events should be scheduled");
            
            UnityEngine.Debug.Log($"[Performance] Scheduled {eventCount} events in {schedulingTimeMs:F2}ms ({schedulingTimeMs/eventCount:F4}ms per event)");
        }

        [Test]
        public void EventProcessing_StaysUnder100EventsPerFrameLimit()
        {
            // Arrange
            const int eventCount = 500;
            int callbackCount = 0;
            int maxEventsInSingleFrame = 0;
            int frameCount = 0;

            // Schedule all events to trigger immediately
            for (int i = 0; i < eventCount; i++)
            {
                _timeManager.ScheduleEvent(0.001f, () => callbackCount++);
            }

            // Act - Simulate multiple frames
            for (int frame = 0; frame < 10; frame++)
            {
                int eventsBeforeFrame = callbackCount;
                
                // Simulate frame update (0.016s = ~60 FPS)
                SimulateFrame(0.016f);
                
                int eventsInFrame = callbackCount - eventsBeforeFrame;
                maxEventsInSingleFrame = Mathf.Max(maxEventsInSingleFrame, eventsInFrame);
                
                if (eventsInFrame > 0)
                    frameCount++;
                
                if (callbackCount >= eventCount)
                    break;
            }

            // Assert
            Assert.LessOrEqual(maxEventsInSingleFrame, 100, 
                "Event processing should not exceed 100 events per frame");
            Assert.AreEqual(eventCount, callbackCount, 
                "All events should eventually be processed");
            
            UnityEngine.Debug.Log($"[Performance] Processed {eventCount} events over {frameCount} frames (max {maxEventsInSingleFrame} per frame)");
        }

        [Test]
        public void EventCancellation_WithManyEvents_PerformsEfficiently()
        {
            // Arrange
            const int eventCount = 1000;
            const float maxCancellationTimeMs = 50f;
            var handles = new List<ScheduledEventHandle>();

            // Schedule events
            for (int i = 0; i < eventCount; i++)
            {
                var handle = _timeManager.ScheduleEvent(10f, () => { });
                handles.Add(handle);
            }

            // Act - Measure cancellation time
            var stopwatch = Stopwatch.StartNew();
            
            foreach (var handle in handles)
            {
                _timeManager.CancelScheduledEvent(handle);
            }
            
            stopwatch.Stop();
            float cancellationTimeMs = (float)stopwatch.Elapsed.TotalMilliseconds;

            // Assert
            Assert.Less(cancellationTimeMs, maxCancellationTimeMs, 
                $"Cancelling {eventCount} events took {cancellationTimeMs:F2}ms, exceeds target of {maxCancellationTimeMs}ms");
            
            UnityEngine.Debug.Log($"[Performance] Cancelled {eventCount} events in {cancellationTimeMs:F2}ms ({cancellationTimeMs/eventCount:F4}ms per event)");
        }

        #endregion

        #region Simulation Tick Performance Tests

        [Test]
        public void SimulationTick_WithNoSubscribers_HasMinimalOverhead()
        {
            // Arrange
            const int frameCount = 100;
            const float maxAverageFrameTimeMs = 0.5f;
            
            _timeManager.SetTimeSpeed(1f);
            _timeManager.Resume();

            // Act - Measure frame processing time with no subscribers
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < frameCount; i++)
            {
                SimulateFrame(0.016f); // ~60 FPS
            }
            
            stopwatch.Stop();
            float totalTimeMs = (float)stopwatch.Elapsed.TotalMilliseconds;
            float averageFrameTimeMs = totalTimeMs / frameCount;

            // Assert
            Assert.Less(averageFrameTimeMs, maxAverageFrameTimeMs, 
                $"Average frame time {averageFrameTimeMs:F4}ms exceeds target of {maxAverageFrameTimeMs}ms with no subscribers");
            
            UnityEngine.Debug.Log($"[Performance] {frameCount} frames with no tick subscribers: {totalTimeMs:F2}ms total, {averageFrameTimeMs:F4}ms average");
        }

        [Test]
        public void SimulationTick_With10Subscribers_HasAcceptableOverhead()
        {
            // Arrange
            const int subscriberCount = 10;
            const int frameCount = 100;
            const float maxAverageFrameTimeMs = 1f;
            int tickCount = 0;

            // Subscribe 10 handlers
            for (int i = 0; i < subscriberCount; i++)
            {
                TimeManager.OnSimulationTick += () => tickCount++;
            }

            _timeManager.SetTimeSpeed(1f);
            _timeManager.Resume();

            // Act - Measure frame processing time
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < frameCount; i++)
            {
                SimulateFrame(0.016f); // ~60 FPS
            }
            
            stopwatch.Stop();
            float totalTimeMs = (float)stopwatch.Elapsed.TotalMilliseconds;
            float averageFrameTimeMs = totalTimeMs / frameCount;

            // Assert
            Assert.Less(averageFrameTimeMs, maxAverageFrameTimeMs, 
                $"Average frame time {averageFrameTimeMs:F4}ms exceeds target of {maxAverageFrameTimeMs}ms with {subscriberCount} subscribers");
            Assert.Greater(tickCount, 0, "Ticks should have been processed");
            
            UnityEngine.Debug.Log($"[Performance] {frameCount} frames with {subscriberCount} tick subscribers: {totalTimeMs:F2}ms total, {averageFrameTimeMs:F4}ms average, {tickCount} ticks");
        }

        [Test]
        public void SimulationTick_With100Subscribers_RemainsPerformant()
        {
            // Arrange
            const int subscriberCount = 100;
            const int frameCount = 100;
            const float maxAverageFrameTimeMs = 5f; // More lenient for 100 subscribers
            int tickCount = 0;

            // Subscribe 100 handlers
            for (int i = 0; i < subscriberCount; i++)
            {
                TimeManager.OnSimulationTick += () => tickCount++;
            }

            _timeManager.SetTimeSpeed(1f);
            _timeManager.Resume();

            // Act - Measure frame processing time
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < frameCount; i++)
            {
                SimulateFrame(0.016f); // ~60 FPS
            }
            
            stopwatch.Stop();
            float totalTimeMs = (float)stopwatch.Elapsed.TotalMilliseconds;
            float averageFrameTimeMs = totalTimeMs / frameCount;

            // Assert
            Assert.Less(averageFrameTimeMs, maxAverageFrameTimeMs, 
                $"Average frame time {averageFrameTimeMs:F4}ms exceeds target of {maxAverageFrameTimeMs}ms with {subscriberCount} subscribers");
            Assert.Greater(tickCount, 0, "Ticks should have been processed");
            
            UnityEngine.Debug.Log($"[Performance] {frameCount} frames with {subscriberCount} tick subscribers: {totalTimeMs:F2}ms total, {averageFrameTimeMs:F4}ms average, {tickCount} ticks");
        }

        [Test]
        public void SimulationTick_TimingAccuracy_RemainsConsistentOverTime()
        {
            // Arrange
            const int testDurationSeconds = 10;
            const float tickRate = 10f; // 10 Hz
            const float expectedTicksPerSecond = 10f;
            const float tolerancePercent = 5f; // 5% tolerance
            
            _timeManager.TickRate = tickRate;
            _timeManager.SetTimeSpeed(1f);
            _timeManager.Resume();

            int tickCount = 0;
            TimeManager.OnSimulationTick += () => tickCount++;

            // Act - Simulate time passing
            float simulatedTime = 0f;
            while (simulatedTime < testDurationSeconds)
            {
                float deltaTime = 0.016f; // ~60 FPS
                SimulateFrame(deltaTime);
                simulatedTime += deltaTime;
            }

            // Calculate actual tick rate
            float actualTicksPerSecond = tickCount / simulatedTime;
            float deviationPercent = Mathf.Abs(actualTicksPerSecond - expectedTicksPerSecond) / expectedTicksPerSecond * 100f;

            // Assert
            Assert.Less(deviationPercent, tolerancePercent, 
                $"Tick rate deviation {deviationPercent:F2}% exceeds tolerance of {tolerancePercent}%");
            
            UnityEngine.Debug.Log($"[Performance] Tick timing over {testDurationSeconds}s: Expected {expectedTicksPerSecond * testDurationSeconds} ticks, got {tickCount} ticks ({actualTicksPerSecond:F2} Hz, {deviationPercent:F2}% deviation)");
        }

        #endregion

        #region Memory Usage Tests

        [Test]
        public void MemoryUsage_WithManyScheduledEvents_RemainsReasonable()
        {
            // Arrange
            const int eventCount = 1000;
            const long maxMemoryIncreaseKB = 1024; // 1 MB max increase
            
            // Force garbage collection before test
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long memoryBefore = System.GC.GetTotalMemory(false) / 1024; // KB

            // Act - Schedule many events
            for (int i = 0; i < eventCount; i++)
            {
                _timeManager.ScheduleEvent(Random.Range(1f, 100f), () => { });
            }

            // Force garbage collection after scheduling
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long memoryAfter = System.GC.GetTotalMemory(false) / 1024; // KB
            long memoryIncrease = memoryAfter - memoryBefore;

            // Assert
            Assert.Less(memoryIncrease, maxMemoryIncreaseKB, 
                $"Memory increase {memoryIncrease}KB exceeds target of {maxMemoryIncreaseKB}KB for {eventCount} events");
            
            UnityEngine.Debug.Log($"[Performance] Memory usage with {eventCount} events: {memoryIncrease}KB increase ({memoryIncrease/(float)eventCount:F2}KB per event)");
        }

        [Test]
        public void MemoryUsage_EventCleanup_ReleasesMemory()
        {
            // Arrange
            const int eventCount = 500;
            int callbackCount = 0;
            
            // Force garbage collection before test
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long memoryBefore = System.GC.GetTotalMemory(false) / 1024; // KB

            // Schedule events that will trigger immediately
            for (int i = 0; i < eventCount; i++)
            {
                _timeManager.ScheduleEvent(0.001f, () => callbackCount++);
            }

            long memoryAfterScheduling = System.GC.GetTotalMemory(false) / 1024; // KB

            // Act - Process all events
            for (int frame = 0; frame < 10; frame++)
            {
                SimulateFrame(0.016f);
                if (callbackCount >= eventCount)
                    break;
            }

            // Force garbage collection after processing
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            long memoryAfterProcessing = System.GC.GetTotalMemory(false) / 1024; // KB

            // Assert
            Assert.AreEqual(eventCount, callbackCount, "All events should be processed");
            
            long memoryReclaimed = memoryAfterScheduling - memoryAfterProcessing;
            
            UnityEngine.Debug.Log($"[Performance] Memory cleanup: {memoryAfterScheduling - memoryBefore}KB allocated, {memoryReclaimed}KB reclaimed after processing");
            
            // Memory should be reclaimed (or at least not grow significantly)
            Assert.LessOrEqual(memoryAfterProcessing, memoryAfterScheduling + 100, 
                "Memory should not grow significantly after event processing");
        }

        #endregion

        #region Stress Tests

        [Test]
        public void StressTest_MixedOperations_RemainsStable()
        {
            // Arrange
            const int iterations = 100;
            const int eventsPerIteration = 10;
            int totalCallbacks = 0;
            var activeHandles = new List<ScheduledEventHandle>();

            _timeManager.SetTimeSpeed(1f);
            _timeManager.Resume();

            // Act - Perform mixed operations
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                // Schedule some events
                for (int j = 0; j < eventsPerIteration; j++)
                {
                    float delay = Random.Range(0.1f, 5f);
                    var handle = _timeManager.ScheduleEvent(delay, () => totalCallbacks++);
                    activeHandles.Add(handle);
                }

                // Cancel some random events
                if (activeHandles.Count > 20)
                {
                    int cancelCount = Random.Range(1, 5);
                    for (int j = 0; j < cancelCount && activeHandles.Count > 0; j++)
                    {
                        int index = Random.Range(0, activeHandles.Count);
                        _timeManager.CancelScheduledEvent(activeHandles[index]);
                        activeHandles.RemoveAt(index);
                    }
                }

                // Change time speed occasionally
                if (i % 20 == 0)
                {
                    _timeManager.SetTimeSpeed(Random.Range(1f, 5f));
                }

                // Simulate frame
                SimulateFrame(0.016f);
            }
            
            stopwatch.Stop();
            float totalTimeMs = (float)stopwatch.Elapsed.TotalMilliseconds;

            // Assert
            Assert.Greater(totalCallbacks, 0, "Some events should have been processed");
            Assert.Less(totalTimeMs, 1000f, "Stress test should complete within 1 second");
            
            UnityEngine.Debug.Log($"[Performance] Stress test: {iterations} iterations with mixed operations completed in {totalTimeMs:F2}ms, {totalCallbacks} callbacks executed");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Simulates a single frame update by manually calling Update via reflection.
        /// </summary>
        private void SimulateFrame(float deltaTime)
        {
            // Set Time.deltaTime for this frame
            typeof(Time).GetField("deltaTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, deltaTime);

            // Call Update method via reflection
            var updateMethod = typeof(TimeManager).GetMethod("Update", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateMethod?.Invoke(_timeManager, null);
        }

        #endregion
    }
}
